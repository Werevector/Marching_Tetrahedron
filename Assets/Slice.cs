// DICOM Library is "Fellow Oak Dicom" from https://github.com/fo-dicom/fo-dicom

// todo: cast dicom image directly to unity 2d texture: https://github.com/fo-dicom/fo-dicom/wiki/Image-rendering

// fo-dicom in unity does not support jpeg encoded files:  https://github.com/fo-dicom/fo-dicom/wiki/Unity


using System;
using System.Collections;
using System.Collections.Generic;

using Dicom;
using System.Linq;
using Dicom.Imaging;
using Dicom.Imaging.Render;
using Dicom.IO.Buffer;
using UnityEngine;
using System.IO;
using Dicom.IO;
using Dicom.IO.Reader;
using UnityEngine.Assertions;

public class SliceInfo
{
    // member variables are named exactly after DICOM tag names
    public string PatientId;
    public float SliceThickness;       // millimeter
    public int Rows;
    public int Columns;
    public Vector2 PixelSpacing;       // millimeter
    public int BitsAllocated;
    public int BitsStored;
    public float RescaleIntercept;
    public float RescaleSlope;
    // varies from file to file within one scan
    public float SliceLocation;
    public Vector3 ImageOrientationPatient;
    public float SmallestImagePixelValue;
    public float LargestImagePixelValue;

    public SliceInfo(DicomFile dicomfile)
    {
        PatientId = dicomfile.Dataset.Get<string>(DicomTag.PatientID);

        Rows = dicomfile.Dataset.Get<int>(DicomTag.Rows);       // 512
        Columns = dicomfile.Dataset.Get<int>(DicomTag.Columns);  // 512
        BitsAllocated = dicomfile.Dataset.Get<int>(DicomTag.BitsAllocated); // 16
        BitsStored = dicomfile.Dataset.Get<int>(DicomTag.BitsStored); //  12
        RescaleIntercept = dicomfile.Dataset.Get<float>(DicomTag.RescaleIntercept); //  -1024
        RescaleSlope = dicomfile.Dataset.Get<float>(DicomTag.RescaleSlope); // 1     Hounsfield = val*slope + intercept : http://www.idlcoyote.com/fileio_tips/hounsfield.html             
        SmallestImagePixelValue = dicomfile.Dataset.Get<float>(DicomTag.SmallestImagePixelValue); //  20   
        LargestImagePixelValue = dicomfile.Dataset.Get<float>(DicomTag.LargestImagePixelValue);  // 172     
        // See http://dicom.nema.org/medical/dicom/2014c/output/chtml/part03/sect_C.7.6.2.html for tags below
        PixelSpacing.x = 0.3f;// dicomfile.Dataset.Get<float>(DicomTag.PixelSpacing, 0);     // millimeter  0.296875/0.296875  (512*0.296875mm = 152mm = 15,2kvadratcm slice størrelse)
        PixelSpacing.y = 0.3f; // dicomfile.Dataset.Get<float>(DicomTag.PixelSpacing, 1);
        // image orientation // 1/0/0/0/1/0
        ImageOrientationPatient.x = dicomfile.Dataset.Get<float>(DicomTag.ImagePositionPatient, 0);     //-75.8515625
        ImageOrientationPatient.y = dicomfile.Dataset.Get<float>(DicomTag.ImagePositionPatient, 1);     //-171.8515625 
        ImageOrientationPatient.z = dicomfile.Dataset.Get<float>(DicomTag.ImagePositionPatient, 2);     //332.7
        SliceThickness = dicomfile.Dataset.Get<float>(DicomTag.SliceThickness);      // millimeter 0.6
        SliceLocation = dicomfile.Dataset.Get<float>(DicomTag.SliceLocation); // 332.7     
    }
}


class Slice : IComparable   // IComparable so it can be sorted by sort()
{
    public DicomFile dicomFile;
    public SliceInfo sliceInfo;
    public ushort[] slicePixels = null;         // todo: lag getter og setter i c# stil,   er den null by default?

    public int CompareTo(object obj)  // for IComparable so it can be sorted by sort()
    {
        Slice otherSlice = obj as Slice;
        return sliceInfo.SliceLocation.CompareTo(otherSlice.sliceInfo.SliceLocation);
    }

    public Slice(string filename)
    {
        dicomFile = DicomFile.Open(filename);
        sliceInfo = new SliceInfo(dicomFile);
        slicePixels = null; // slicePixels remains empty until loadPixels() is called
    }

    private void loadPixels()
    {
        // code found at https://groups.google.com/forum/#!topic/fo-dicom/EQtF5-7_PAU
        DicomPixelData pxd = DicomPixelData.Create(dicomFile.Dataset);
        IByteBuffer buffer = pxd.GetFrame(0);
        slicePixels = Dicom.IO.ByteConverter.ToArray<ushort>(buffer);
        //byte[] bSlicePixels = Dicom.IO.ByteConverter.ToArray<byte>(buffer);

        // alternative:
        //var header = DicomPixelData.Create(dicomFile.Dataset);
        //var pixelData = PixelDataFactory.Create(header, 0);
        //ushort[] pixels = ((GrayscalePixelDataU16)pixelData).Data;
    }

    public ushort[] getPixels()
    {
        if (slicePixels == null)
            loadPixels();
        return slicePixels;
    }

    public void releasePixels()
    {
        slicePixels = null;
    }

    public static void initDicom()
    {
        var dict = new DicomDictionary();
        dict.Load(@".\Assets\dicom\DICOM Dictionary.xml", DicomDictionaryFormat.XML);
        DicomDictionary.Default = dict;
    }

}

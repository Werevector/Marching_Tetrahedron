using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;

public class quadScript : MonoBehaviour {

    #region MemberVariables
    Slice[] _slices;

    int mCurrentSlice = 0;
    float mCurrentIsoValue = 2500.0f;

    meshScript mOutlineMeshScript;
    List<Vector3> mOutlineVertices;
    List<int> mOutlineIndices;

    void Start () {
       
        Slice.initDicom();

        string dicomfilepath = Application.dataPath + @"\..\dicomdata\";
        _slices = processSlices(dicomfilepath);
        setTexture(_slices[0]);

        mOutlineVertices = new List<Vector3>();
        mOutlineIndices = new List<int>();

        mOutlineMeshScript = GameObject.Find("GameObjectMesh").GetComponent<meshScript>();
    }

    Slice[] processSlices(string dicomfilepath)
    {
        string[] dicomfilenames = Directory.GetFiles(dicomfilepath, "*.IMA"); 
  
        int numSlices = dicomfilenames.Length;

        Slice[] slices = new Slice[numSlices];

        float max = -1;
        float min = 99999;
        for (int i = 0; i < numSlices; i++)
        {
            string filename = dicomfilenames[i];
            slices[i] = new Slice(filename);
            SliceInfo info = slices[i].sliceInfo;
            if (info.LargestImagePixelValue > max) max = info.LargestImagePixelValue;
            if (info.SmallestImagePixelValue < min) min = info.SmallestImagePixelValue;
            // Del dataen på max før den settes inn i tekstur
            // alternativet er å dele på 2^dicombitdepth,  men det ville blitt 4096 i dette tilfelle

        }
        print("Number of slices read:" + numSlices);
        print("Max intensity in all slices:" + max);
        print("Min intensity in all slices:" + min);
        Array.Sort(slices);
        
        return slices;
    }
    #endregion

    void setTexture(Slice slice)
    {
        int s = 0;
        int xdim = slice.sliceInfo.Rows;
        int ydim = slice.sliceInfo.Columns;

        var texture = new Texture2D(xdim, ydim, TextureFormat.RGB24, false);     // garbage collector will tackle that it is new'ed 

        ushort[] pixels = slice.getPixels();
        
        for (int y = 0; y < ydim; y++)
            for (int x = 0; x < xdim; x++)
            {
                float val = pixelval(new Vector2(x, y), xdim, pixels);
                float v = val / mCurrentIsoValue;
                texture.SetPixel(x, y, new UnityEngine.Color(v, v, v));
            }

        texture.filterMode = FilterMode.Point;
        texture.Apply();  // Apply all SetPixel calls
        GetComponent<Renderer>().material.mainTexture = texture;
    }


    ushort pixelval(Vector2 p, int xdim, ushort[] pixels)
    {
        return pixels[(int)p.x + (int)p.y * xdim];
    }

    ushort pixelval(float x, float y, int xdim, ushort[] pixels)
    {
        return pixels[(int)x + (int)y * xdim];
    }

    void addOutlineVerts(float x1, float y1, float x2, float y2, int xdim, int ydim)
    {
        if (mOutlineIndices.Count < 65000)
        {
            mOutlineVertices.Add(new Vector3(x1 / xdim - 0.5f, y1 / ydim - 0.5f, 0.0f));
            mOutlineIndices.Add(mOutlineVertices.Count - 1);
            mOutlineVertices.Add(new Vector3(x2 / xdim - 0.5f, y2 / ydim - 0.5f, 0.0f));
            mOutlineIndices.Add(mOutlineVertices.Count - 1);
        }
    }

    void generateOutlineMesh(Slice slice)
    {
        
        int xdim = slice.sliceInfo.Rows;
        int ydim = slice.sliceInfo.Columns;

        mOutlineVertices.Clear();
        mOutlineIndices.Clear();

        var texture = new Texture2D(xdim, ydim, TextureFormat.RGB24, false);     // garbage collector will tackle that it is new'ed 

        ushort[] pixels = slice.getPixels();

        for (int y = 0; y < ydim; y++)
        {
            for (int x = 0; x < xdim; x++)
            {
                float val = pixelval(new Vector2(x, y), xdim, pixels);

                if (val > 0)
                {
                    bool middleIso = val > mCurrentIsoValue ? true : false;


                    //left
                    if (x - 1 > 0)
                    {
                        float left = pixelval(new Vector2(x - 1, y), xdim, pixels);
                        bool leftIso = left > mCurrentIsoValue ? true : false;
                        if (middleIso != leftIso)
                        {
                            addOutlineVerts(x, y, x, y+1, xdim, ydim);
                        }
                    }

                    //right
                    if (x + 1 < xdim)
                    {
                        float right = pixelval(new Vector2(x + 1, y), xdim, pixels);
                        bool rightIso = right > mCurrentIsoValue ? true : false;
                        if (middleIso != rightIso)
                        {
                            addOutlineVerts(x+1, y, x + 1, y+1, xdim, ydim);
                        }
                    }

                    //up
                    if (y - 1 > 0)
                    {
                        float up = pixelval(new Vector2(x, y - 1), xdim, pixels);
                        bool upIso = up > mCurrentIsoValue ? true : false;
                        if (middleIso != upIso)
                        {
                            addOutlineVerts(x, y, x+1, y, xdim, ydim);
                        }
                    }

                    //down
                    if (y + 1 < ydim)
                    {
                        float down = pixelval(new Vector2(x, y + 1), xdim, pixels);
                        bool downIso = down > mCurrentIsoValue ? true : false;
                        if (middleIso != downIso)
                        {
                            addOutlineVerts(x, y+1, x+1, y + 1, xdim, ydim);
                        }
                    }

                }
            }
        }
        mOutlineMeshScript.createMeshGeometry(mOutlineVertices, mOutlineIndices);
    }

   
    public void slicePosSliderChange(float val)
    {
        int showSlice = (int)((val * 354) - 1);
        mCurrentSlice = showSlice;
        setTexture(_slices[showSlice]);
        generateOutlineMesh(_slices[mCurrentSlice]);
        print("slicePosSliderChange:" + val); 
    }
    
    public void sliceIsoSliderChange(float val)
    {

        mCurrentIsoValue = val * 2500.0f;
        setTexture(_slices[mCurrentSlice]);
        generateOutlineMesh(_slices[mCurrentSlice]);
    }
    
    public void buttonPushed()
    {
        generateOutlineMesh(_slices[mCurrentSlice]);
    }

}

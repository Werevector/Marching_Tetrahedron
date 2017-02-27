using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class TetrahedraMarcher : MonoBehaviour {

    #region MemberVariables
    Slice[] mSlices;

    int mCurrentSliceIndex = 0;
    float mCurrentIsoValue = 1000.0f;

    meshScript mMesh;
    List<Vector3> mOutlineVertices;
    List<int> mOutlineIndices;
    #endregion

    void Start () {
        Slice.initDicom();
        string dicomfilepath = Application.dataPath + @"\..\dicomdata\";
        mSlices = Slice.ProcessSlices(dicomfilepath);
        mOutlineVertices = new List<Vector3>();
        mOutlineIndices = new List<int>();
        mMesh = GameObject.Find("GameObjectMesh").GetComponent<meshScript>();
    }

    void GenerateSurfaceMesh ()
    {
        for (int i = 0; i < mSlices.Length-1; i++)
        {
            EvaluateSlice(i);
        }
    }

    void EvaluateSlice(int index)
    {
        Slice sliceA = mSlices[index];
        Slice sliceB = mSlices[index + 1];
        int width = sliceA.sliceInfo.Rows;
        int height = sliceA.sliceInfo.Columns;

        ushort[] pixelsA = sliceA.getPixels();
        ushort[] pixelsB = sliceB.getPixels();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {

            }
        }
    }

    void EvaluateCube(ushort[] pA, ushort[] pB, int x, int y)
    {
        
        //Cube Point Values
        bool c0, c1, c2, c3, c4, c5, c6, c7;
        //c0 = EvaluatePointByIso()
    }

    void EvaluateTetrahedron()
    {

    }

    void CreateTriangle()
    {

    }

    void CreateQuad()
    {

    }

    //Is point over/under the ISO value
    bool EvaluatePointByIso(ushort v)
    {
        return v > mCurrentIsoValue;
    }

    



    #region utility functions
    ushort PixelValue(Vector2 p, int xdim, ushort[] pixels)
    {
        return pixels[(int)p.x + (int)p.y * xdim];
    }
    ushort PixelVal(float x, float y, int xdim, ushort[] pixels)
    {
        return pixels[(int)x + (int)y * xdim];
    }
    #endregion


}

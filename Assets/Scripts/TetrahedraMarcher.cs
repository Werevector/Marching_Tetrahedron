using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class TetrahedraMarcher : MonoBehaviour {

    #region MemberVariables
    Slice[] mSlices;

    int mCurrentSliceIndex = 0;
    float mCurrentIsoValue = 2500.0f/3;

    meshScript mMesh;
    List<Vector3> mMeshVertices;
    List<int> mMeshIndices;
    Vector3 debugCenter;
    #endregion

    enum THCase
    {
        NOT,
        A,B,C,D,E,F,G
    }

    void Start () {
        //Slice.initDicom();
        //string dicomfilepath = Application.dataPath + @"\..\dicomdata\";
        //mSlices = Slice.ProcessSlices(dicomfilepath);
        mMeshVertices = new List<Vector3>();
        mMeshIndices = new List<int>();
        mMesh = GameObject.Find("GameObjectMesh").GetComponent<meshScript>();
    }

    void GenerateSurfaceMesh ()
    {
        //for (int i = 0; i < mSlices.Length - 1; i++)
        //{
        //    EvaluateSlice(i);
        //}
        for (int i = 0; i < 30; i++)
        {
            EvaluateSlice(i);
        }
    }

    void EvaluateSlice(int index)
    {
        //Slice sliceA = mSlices[index];
        //Slice sliceB = mSlices[index + 1];
        //int width = sliceA.sliceInfo.Rows;
        //int height = sliceA.sliceInfo.Columns;
        int width = 100;
        int height = 100;
        debugCenter = new Vector3(width / 2, height / 2, 30 / 2);

        //ushort[] pixelsA = sliceA.getPixels();
        //ushort[] pixelsB = sliceB.getPixels();

        ushort[] pixelsA = new ushort[10];
        ushort[] pixelsB = new ushort[10];

        for (int x = 0; x < width-1; x++)
        {
            for (int y = 0; y < height-1; y++)
            {
                EvaluateCube(pixelsA, pixelsB, x, y, index, width);
            }
        } 
    }

    void EvaluateCube(ushort[] pA, ushort[] pB, int x, int y, int z, int width)
    {
        /*
           2-------3  pB
          /|      /|  |
         / |     / |  |  z = slice index
        6--|----7  |  |
        |  0----|--1  pA
        | /     | /
        4-------5    x,y = c0 | x+1,y = c1 | etc
        */

        bool c0, c1, c2, c3, c4, c5, c6, c7;

        ////pA = Bottom square
        //c0 = EvaluatePointByIso(PixelValue(x, y, width, pA));
        //c1 = EvaluatePointByIso(PixelValue(x + 1, y, width, pA));
        //c4 = EvaluatePointByIso(PixelValue(x, y + 1, width, pA));
        //c5 = EvaluatePointByIso(PixelValue(x + 1, y + 1, width, pA));

        //////pB = Top square
        //c2 = EvaluatePointByIso(PixelValue(x, y, width, pB));
        //c3 = EvaluatePointByIso(PixelValue(x + 1, y, width, pB));
        //c6 = EvaluatePointByIso(PixelValue(x, y + 1, width, pB));
        //c7 = EvaluatePointByIso(PixelValue(x + 1, y + 1, width, pB));

        c0 = EvaluatePointByIso(DebugPixelValue(x, y, z, width, pA));
        c1 = EvaluatePointByIso(DebugPixelValue(x + 1, y, z, width, pA));
        c4 = EvaluatePointByIso(DebugPixelValue(x, y + 1, z, width, pA));
        c5 = EvaluatePointByIso(DebugPixelValue(x + 1, y + 1, z, width, pA));

        c2 = EvaluatePointByIso(DebugPixelValue(x, y, z, width, pB));
        c3 = EvaluatePointByIso(DebugPixelValue(x + 1, y, z, width, pB));
        c6 = EvaluatePointByIso(DebugPixelValue(x, y + 1, z, width, pB));
        c7 = EvaluatePointByIso(DebugPixelValue(x + 1, y + 1, z, width, pB));

        //c0 = EvaluatePointByIso(DebugPixelValueSph(x, y, z, width, pA));
        //c1 = EvaluatePointByIso(DebugPixelValueSph(x + 1, y, z, width, pA));
        //c4 = EvaluatePointByIso(DebugPixelValueSph(x, y + 1, z, width, pA));
        //c5 = EvaluatePointByIso(DebugPixelValueSph(x + 1, y + 1, z, width, pA));

        //c2 = EvaluatePointByIso(DebugPixelValueSph(x, y, z, width, pB));
        //c3 = EvaluatePointByIso(DebugPixelValueSph(x + 1, y, z, width, pB));
        //c6 = EvaluatePointByIso(DebugPixelValueSph(x, y + 1, z, width, pB));
        //c7 = EvaluatePointByIso(DebugPixelValueSph(x + 1, y + 1, z, width, pB));

        Vector3 p0 = new Vector3(x, y, z);
        Vector3 p1 = new Vector3(x + 1, y, z);
        Vector3 p4 = new Vector3(x, y + 1, z);
        Vector3 p5 = new Vector3(x + 1, y + 1, z);

        Vector3 p2 = new Vector3(x, y, z + 1);
        Vector3 p3 = new Vector3(x + 1, y, z + 1);
        Vector3 p6 = new Vector3(x, y + 1, z + 1);
        Vector3 p7 = new Vector3(x + 1, y + 1, z + 1);

        EvaluateTetrahedron(p4, p6, p0, p7, c4, c6, c0, c7);
        EvaluateTetrahedron(p6, p0, p7, p2, c6, c0, c7, c2);
        EvaluateTetrahedron(p0, p7, p2, p3, c0, c7, c2, c3);
        EvaluateTetrahedron(p4, p5, p7, p0, c4, c5, c7, c0);
        EvaluateTetrahedron(p1, p7, p0, p3, c1, c7, c0, c3);
        EvaluateTetrahedron(p0, p5, p7, p1, c0, c5, c7, c1);
    }

    void EvaluateTetrahedron(Vector3 aP, Vector3 bP, Vector3 cP, Vector3 dP,
                             bool a, bool b, bool c, bool d)
    {
        Vector3 abP = (aP + bP) / 2;
        Vector3 acP = (aP + cP) / 2;
        Vector3 adP = (aP + dP) / 2;

        Vector3 bdP = (bP + dP) / 2;
        Vector3 bcP = (bP + cP) / 2;
        Vector3 cdP = (cP + dP) / 2;

        THCase tCase = FindTetraCase(a, b, c, d);
        switch (tCase)
        {
            case THCase.NOT:
                break;
            case THCase.A:
                CreateTriangle(adP, bdP, cdP);
                break;
            case THCase.B:
                CreateTriangle(acP, cdP, bcP);
                break;
            case THCase.C:
                CreateTriangle(abP, bcP, bdP);
                break;
            case THCase.D:
                CreateTriangle(abP, acP, adP);
                break;
            case THCase.E:
                CreateQuad(acP, adP, bdP, bcP);
                break;
            case THCase.F:
                CreateQuad(abP, bcP, cdP, adP);
                break;
            case THCase.G:
                
                CreateQuad(abP, bdP, cdP, acP);
                break;
            default:
                break;
        }
    }

    void CreateTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        mMeshVertices.Add(a);
        mMeshIndices.Add(mMeshVertices.Count - 1);
        mMeshVertices.Add(b);
        mMeshIndices.Add(mMeshVertices.Count - 1);
        mMeshVertices.Add(c);
        mMeshIndices.Add(mMeshVertices.Count - 1); 
    }

    void CreateQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        CreateTriangle(a, b, c);
        CreateTriangle(a, c, d);
    }

    //Is point over/under the ISO value
    bool EvaluatePointByIso(ushort v)
    {
        return v > mCurrentIsoValue;
    }

    
    THCase FindTetraCase (bool a, bool b, bool c, bool d)
    {
        THCase tCase = THCase.NOT;

        if ((a && b && c && d) || (!a && !b && !c && !d))
            return tCase;

        //find the correct case for the bools
        if ((!a && !b && !c) && d  || (a && b && c) && !d)
        {
            tCase = THCase.A;
            return tCase;
        }

        if ((!a && !b) && c && !d || (a && b) && !c && d)
        {
            tCase = THCase.B;
            return tCase;
        }

        if (!a && b && (!c && !d) || a && !b && (c && d))
        {
            tCase = THCase.C;
            return tCase;
        }
        if (!a && (b && c && d) || a && (!b && !c && !d))
        {
            tCase = THCase.D;
            return tCase;
        }
        if ((!a && !b) && (c && d) || (a && b) && (!c && !d))
        {
            tCase = THCase.E;
            return tCase;
        }
        if (!a && b && !c && d || a && !b && c && !d)
        {
            tCase = THCase.F;
            return tCase;
        }
        if (!a && (b && c) && !d || a && (!b && !c) && d)
        {
            tCase = THCase.G;
            return tCase;
        }

        return tCase;
    }



    public void buttonPushed()
    {
        float t1 = Time.realtimeSinceStartup;
        
        GenerateSurfaceMesh();
        mMesh.toFile("testMesh.obj", mMeshVertices, mMeshIndices);
        
        float t2 = Time.realtimeSinceStartup;
        float t3 = t2 - t1;
        print("Time: " + t3/60 + " minutes");
    }

    #region utility functions
    ushort PixelValue(Vector2 p, int xdim, ushort[] pixels)
    {
        return pixels[(int)p.x + (int)p.y * xdim];
    }
    ushort PixelValue(float x, float y, int xdim, ushort[] pixels)
    {
        return pixels[(int)x + (int)y * xdim];
    }

    ushort DebugPixelValue(float x, float y, int index, int xdim, ushort[] pixels)
    {
        ushort val = (ushort)(mCurrentIsoValue - 100);
        if((x > 20 && x < 40) && (y > 20 && y < 40) && (index > 5 && index < 20))
            val = (ushort)(mCurrentIsoValue + 100);
        return val;
    }

    ushort DebugPixelValueSph(float x, float y, int index, int xdim, ushort[] pixels)
    {
        ushort val = (ushort)(mCurrentIsoValue - 100);
        Vector3 pt = new Vector3(x, y, index);
        if (Vector3.Distance(pt, debugCenter) > 10)
        {
            val = (ushort)(mCurrentIsoValue + 100);
        }
        return val;
    }
    #endregion
}

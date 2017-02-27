using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;

public class quadScript : MonoBehaviour {

    // forklar klassen at dicom har et "levende" dictionary som leses fra xml ved initDicom
    // at slices må sorteres, og det basert på en tag, men at pixeldata lesing er en separat operasjon, derfor har vi nullpeker til pixeldata
    // at dicomfile lagres slik at fil ikke må leses enda en gang når pixeldata hentes
    
    Slice[] _slices;
    int _currSlice = 0;
    meshScript _outlineScript;
    List<Vector3> _outlineVerts;
    List<int> _outlineIndices;
    float isoValue = 2500.0f;

    // Use this for initialization
    void Start () {
       
        Slice.initDicom();

        string dicomfilepath = Application.dataPath + @"\..\dicomdata\"; // Application.dataPath is in the assets folder, but these files are "managed", so we go one level up
        _slices = processSlices(dicomfilepath);
        setTexture(_slices[0]);

        _outlineVerts = new List<Vector3>();
        _outlineIndices = new List<int>();

        _outlineScript = GameObject.Find("GameObjectMesh").GetComponent<meshScript>();
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        vertices.Add(new Vector3(-0.5f,-0.5f,0));
        vertices.Add(new Vector3(0.5f,0.5f,0));
        indices.Add(0);
        indices.Add(1);
        _outlineScript.createMeshGeometry(vertices, indices);
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
                float v = val / isoValue;
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

    Vector2 vec2(float x, float y)
    {
        return new Vector2(x, y);
    }


    // Update is called once per frame
    void Update () {
        
      
    }

    void addOutlineVerts(float x1, float y1, float x2, float y2, int xdim, int ydim)
    {
        if (_outlineIndices.Count < 65000)
        {
            _outlineVerts.Add(new Vector3(x1 / xdim - 0.5f, y1 / ydim - 0.5f, 0.0f));
            _outlineIndices.Add(_outlineVerts.Count - 1);
            _outlineVerts.Add(new Vector3(x2 / xdim - 0.5f, y2 / ydim - 0.5f, 0.0f));
            _outlineIndices.Add(_outlineVerts.Count - 1);
        }

    }

    void generateOutlineMesh(Slice slice)
    {
        
        int xdim = slice.sliceInfo.Rows;
        int ydim = slice.sliceInfo.Columns;

        _outlineVerts.Clear();
        _outlineIndices.Clear();

        var texture = new Texture2D(xdim, ydim, TextureFormat.RGB24, false);     // garbage collector will tackle that it is new'ed 

        ushort[] pixels = slice.getPixels();

        for (int y = 0; y < ydim; y++)
        {
            for (int x = 0; x < xdim; x++)
            {
                float val = pixelval(new Vector2(x, y), xdim, pixels);

                if (val > 0)
                {
                    bool middleIso = val > isoValue ? true : false;


                    //left
                    if (x - 1 > 0)
                    {
                        float left = pixelval(new Vector2(x - 1, y), xdim, pixels);
                        bool leftIso = left > isoValue ? true : false;
                        if (middleIso != leftIso)
                        {
                            addOutlineVerts(x, y, x, y+1, xdim, ydim);
                        }
                    }

                    //right
                    if (x + 1 < xdim)
                    {
                        float right = pixelval(new Vector2(x + 1, y), xdim, pixels);
                        bool rightIso = right > isoValue ? true : false;
                        if (middleIso != rightIso)
                        {
                            addOutlineVerts(x+1, y, x + 1, y+1, xdim, ydim);
                        }
                    }

                    //up
                    if (y - 1 > 0)
                    {
                        float up = pixelval(new Vector2(x, y - 1), xdim, pixels);
                        bool upIso = up > isoValue ? true : false;
                        if (middleIso != upIso)
                        {
                            addOutlineVerts(x, y, x+1, y, xdim, ydim);
                        }
                    }

                    //down
                    if (y + 1 < ydim)
                    {
                        float down = pixelval(new Vector2(x, y + 1), xdim, pixels);
                        bool downIso = down > isoValue ? true : false;
                        if (middleIso != downIso)
                        {
                            addOutlineVerts(x, y+1, x+1, y + 1, xdim, ydim);
                        }
                    }

                }
            }
        }
        _outlineScript.createMeshGeometry(_outlineVerts, _outlineIndices);
    }

   
    public void slicePosSliderChange(float val)
    {
        int showSlice = (int)((val * 354) - 1);
        _currSlice = showSlice;
        setTexture(_slices[showSlice]);
        generateOutlineMesh(_slices[_currSlice]);
        print("slicePosSliderChange:" + val); 
    }
    
    public void sliceIsoSliderChange(float val)
    {

        isoValue = val * 2500.0f;
        setTexture(_slices[_currSlice]);
        generateOutlineMesh(_slices[_currSlice]);
    }
    
    public void buttonPushed()
    {
        generateOutlineMesh(_slices[_currSlice]);
    }

}

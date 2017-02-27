using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Collections;
using System.IO;
using System.Text;

public class meshScript : MonoBehaviour
{

    void Start()
    {
        MeshFilter meshFilter = (MeshFilter)gameObject.AddComponent(typeof(MeshFilter));
        MeshRenderer renderer = gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
    }
 
    public void createMeshGeometry(List<Vector3> vertices, List<int> indices)
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
     
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
        // mesh.MarkDynamic();  // https://docs.unity3d.com/ScriptReference/Mesh.MarkDynamic.html
        // For iterative mesh additions without reloading the old mesh data   https://docs.unity3d.com/ScriptReference/Mesh.CombineMeshes.html
        //mesh.Optimize();  //https://docs.unity3d.com/ScriptReference/Mesh.Optimize.html

        mesh.RecalculateBounds();
        
        //mesh.RecalculateNormals();
    }


    public string MeshToString(MeshFilter mf)
    {
        Mesh m = mf.mesh;
        StringBuilder sb = new StringBuilder();

        sb.Append("g ").Append(mf.name).Append("\n");
        foreach (Vector3 v in m.vertices)
            sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));

        sb.Append("\n");
        foreach (Vector3 v in m.normals)
            sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));

        sb.Append("\n");
        foreach (Vector3 v in m.uv)
            sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
               
        int[] triangles = m.GetIndices(0);
        for (int i = 0; i < triangles.Length; i += 3)
            sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",  triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));

        return sb.ToString();
    }

    public void MeshToFile(string filename)
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        using (StreamWriter sw = new StreamWriter(filename))
            sw.Write(MeshToString(mf));
        print("Mesh saved to file: " + filename);
    }

    public void toFile(string filename, List<Vector3> vertices, List<int> indices)
    {
        StreamWriter stream = new StreamWriter(filename);
        stream.WriteLine("g "+ "Mesh");
      
        foreach (Vector3 v in vertices)
            stream.WriteLine(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));

        for (int i = 0; i < indices.Count; i += 3)
            stream.WriteLine(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",  indices[i] + 1, indices[i + 1] + 1, indices[i + 2] + 1));
           
        stream.Close();
        print("Mesh saved to file: " + filename);
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AStarDebug {

    static Dictionary<int, MeshFilter> triangleObjs = new Dictionary<int, MeshFilter>();

    static public void DrawTriangle(int id,Int3 a,Int3 b,Int3 c)
    {
        return;
#if !UNITY_EDITOR
        return;
#endif
        MeshFilter mf = null;
        Mesh mesh;
        if (!triangleObjs.TryGetValue(id, out mf))
        {
            mf = new GameObject("AStarDebug_" + id).AddComponent<MeshFilter>();
            mf.transform.position = new Vector3(0,0.05f,0);
            mf.transform.rotation = Quaternion.identity;
            triangleObjs[id] = mf;
            mesh = new Mesh(); 
            mf.gameObject.AddComponent<MeshRenderer>();
        }
        mesh = mf.mesh;
        mesh.vertices = new Vector3 []{ (Vector3)a, (Vector3)b, (Vector3)c };
        mesh.SetIndices(new int[] {0,1,2 }, MeshTopology.Triangles,0);
        mesh.UploadMeshData(false); 
    } 
}

using UnityEngine;
using System.Collections;

public class EditorHelper 
{ 
    public static void SetEditorShader(GameObject tgo)
    {
        if (RuntimePlatform.WindowsEditor == Application.platform || RuntimePlatform.OSXEditor == Application.platform)
        {
            if (tgo == null) return;
            foreach (Renderer element in tgo.GetComponentsInChildren<Renderer>(true))
            {
                foreach (Material mat in element.sharedMaterials)
                {
                    if (mat == null)
                    { 
                        continue;
                    }
                    var shader = Shader.Find("" + mat.shader.name);
                    if (shader == null)
                    {
                        Logger.err("SetEditorShader:"+ mat.shader.name);
                    }
                    mat.shader = shader;
                }
            }
 
            //针对地形组件
            foreach (var terrain in tgo.GetComponentsInChildren<Terrain>(true))
            {
                foreach (var tree in terrain.terrainData.treePrototypes)
                {
                    if (tree.prefab != null)
                    {
                        foreach (Renderer element in tree.prefab.GetComponentsInChildren<Renderer>(true))
                        {
                            foreach (Material mat in element.sharedMaterials)
                            {
                                if (mat == null)
                                {
                                    continue;
                                }
                                var shader = Shader.Find("" + mat.shader.name);
                                if (shader == null)
                                {
                                    Logger.err("SetEditorShader:" + mat.shader.name);
                                }
                                mat.shader = shader; 
                            }
                        } 
                    } 
                }
                terrain.terrainData.RefreshPrototypes();
                terrain.Flush();
            }
        }
    }
 
}

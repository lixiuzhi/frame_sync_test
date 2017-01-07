using UnityEngine;
using System.Collections;

public class GlassEffect : MonoBehaviour {

    public RenderTexture rtt = null;

    Camera cam;

    bool enable = false;

    int FrameNumber = 0;

    public Material mat;

    public void EnableGlass()
    {
        if (rtt == null)
        {
            rtt = RenderTexture.GetTemporary((int)(Screen.width / 6f), (int)(Screen.height /6f));
            rtt.anisoLevel = 4;
            rtt.antiAliasing = 1;
            rtt.filterMode = FilterMode.Bilinear;
            rtt.wrapMode = TextureWrapMode.Clamp;
            cam = GetComponent<Camera>();
            mat = new Material(Shader.Find("LXZ/GlassBlur"));
        }
        enable = true;
        var oldCamTarget = cam.targetTexture;
        var oldActiveRtt = RenderTexture.active;

        CameraClearFlags oldFlags = cam.clearFlags;
        cam.targetTexture = rtt;

        RenderTexture.active = rtt;

        FrameNumber = 0;
        cam.clearFlags = CameraClearFlags.Depth; 
        cam.Render();
        cam.targetTexture = oldCamTarget;
        cam.clearFlags = oldFlags;
        RenderTexture.active = oldActiveRtt;
    }

    public void DisableGlass()
    {
        enable = false; 
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            EnableGlass();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            DisableGlass();
        }
    }
#endif

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
         if (enable && FrameNumber>1)
             Graphics.Blit(rtt, dest, mat);
        else
            Graphics.Blit(src, dest);

        if (FrameNumber < 10)
            FrameNumber++;
    }
}

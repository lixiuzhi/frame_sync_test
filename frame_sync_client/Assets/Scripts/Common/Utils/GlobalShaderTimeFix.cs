using UnityEngine;
using System.Collections;

public class GlobalShaderTimeFix : MonoBehaviour {

    float fixTime = 0;
	void LateUpdate ()
    {
        fixTime += Time.deltaTime;
        if (fixTime > 25)
        {
            fixTime = 0;
        }
        Shader.SetGlobalFloat("_FixTime", fixTime);
	}
}

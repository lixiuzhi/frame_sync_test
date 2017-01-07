using UnityEngine;
using System.Collections;

public class CameraController : SingletonTemplate<CameraController> {

    Transform battleTargetTrans = null;
    Camera battleCamera;

    Vector3 battleCamTargetPos;
    Vector3 battleCamPosOffset = new Vector3(15.49f, 16.22f, -9.67f);
    Vector3 battleCamRotOffset = new Vector3(36, -60f, 0);

    public void InitBattleCamera(Transform trans)
    {
        battleTargetTrans = trans;
        if (battleCamera == null)
        {
            battleCamera = Camera.main;
        }
        battleCamTargetPos = battleTargetTrans.position;
    }


    public Camera GetCurCamera()
    {
        return battleCamera;
    }

	public void Update ()
    {
        if (battleTargetTrans != null)
        {
            battleCamTargetPos = battleTargetTrans.position + battleCamPosOffset;
        }

        if (battleCamera != null)
        {
            battleCamera.transform.position = Vector3.Lerp(battleCamera.transform.position, battleCamTargetPos,1.2f);
            battleCamera.transform.rotation =Quaternion.Euler(battleCamRotOffset); 
        }
    }
}

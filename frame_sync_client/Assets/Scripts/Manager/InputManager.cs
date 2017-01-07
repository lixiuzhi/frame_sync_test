using UnityEngine;
using System.Collections;

public class InputManager : SingletonTemplate<InputManager> {

    bool isBattleJoystickMove = false;
    float battleJoystickMoveDegree = 0;
    float oldDegree=0;
    bool isKeyOperator = false;

    public Int3 moveDir = Int3.zero;

    public void Update()
    {
        if (isBattleJoystickMove)
        {
            moveDir = (Int3)(Quaternion.AngleAxis((int)battleJoystickMoveDegree + Camera.main.transform.eulerAngles.y, Vector3.up) * Vector3.right);
            moveDir.Normalize(); 
        }
      // if (GameManager.Singleton.CurrentState == GameState.Battle || GameManager.Singleton.CurrentState == GameState.MainCity)
        { 
            float degree = 0;
            int up = 0;
            int right = 0;
            if (Input.GetKey(KeyCode.W))
            {
                isKeyOperator = true;
                up = 1;
            }
            if (Input.GetKey(KeyCode.S))
            {
                isKeyOperator = true;
                up += -1;
            }
            if (Input.GetKey(KeyCode.A))
            {
                isKeyOperator = true;
                right = -1;
            }
            if (Input.GetKey(KeyCode.D))
            {
                isKeyOperator = true;
                right += 1;
            }

            if (up == 1)
            {
                degree = -90;
            }
            else if (up == -1)
            {
                degree = 90;
            }

            if (right == 1)
            {
                degree = degree / 2;
            }
            else if (right == -1)
            {
                if (up == 1)
                {
                    degree -= 45;
                }

                if (up == -1)
                {
                    degree += 45;
                }
                if (up == 0)
                {
                    degree = 180;
                }
            }
            if (isKeyOperator)
            { 
                if (oldDegree < 0)
                {
                    oldDegree = 360 + oldDegree;
                }
                if (degree < 0)
                {
                    degree = 360 + degree;
                }

                oldDegree = Mathf.LerpAngle(oldDegree, degree, 0.2f);

                if (oldDegree > 180)
                {
                    oldDegree = oldDegree - 360;
                } 

                BattleJoystickMove(oldDegree);
            }

            if (up == 0 && right == 0 && isKeyOperator)
            {
                isKeyOperator = false;
                BattleJoystickEnd();
            }
        }
    }
  
    public void FixedUpdate()
    {

    }
        
    public void BattleJoystickMove(float degree)
    {
        oldDegree = degree;
        battleJoystickMoveDegree = degree;
        isBattleJoystickMove = true;
    }

    public void BattleJoystickEnd()
    {
        isBattleJoystickMove = false;
        moveDir = Int3.zero;
        //var moveCom = ActorManager.Singleton.GetPlayer().MovementCom;
        //if (moveCom != null)
        //    moveCom.StopMove();
    }
}

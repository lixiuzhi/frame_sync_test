using UnityEngine;
using System.Collections;

public class InputManager : SingletonTemplate<InputManager>
{

    bool isKeyOperator = false;

    public Int3 moveDir = Int3.zero;

    public void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            isKeyOperator = true;
            moveDir = (Int3)Camera.main.transform.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            isKeyOperator = true;
            moveDir = (Int3)(-Camera.main.transform.forward);
        }
        if (Input.GetKey(KeyCode.A))
        {
            isKeyOperator = true;
            moveDir = (Int3)(-Camera.main.transform.right);
        }
        if (Input.GetKey(KeyCode.D))
        {
            isKeyOperator = true;
            moveDir = (Int3)(Camera.main.transform.right);
        }

        if (!isKeyOperator)
            moveDir = Int3.zero;

        moveDir.y = 0;
        moveDir.Normalize();
    }
}

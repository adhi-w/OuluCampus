using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Controller : IController
{
    public float speed_Mult;
    public float rotate_Mult;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float leftSpeed, rightSpeed;
        if (Input.GetKey(KeyCode.W))
        {
            if(Input.GetKey(KeyCode.A))
            {
                leftSpeed = 1-(rotate_Mult);
                rightSpeed = 1;
            }
            else if(Input.GetKey(KeyCode.D))
            {
                leftSpeed = 1;
                rightSpeed = 1 - (rotate_Mult);
            }
            else
            {
                leftSpeed = 1;
                rightSpeed = 1;
            }
        }
        else if (Input.GetKey(KeyCode.S))
        {
            if (Input.GetKey(KeyCode.A))
            {
                leftSpeed = -1+rotate_Mult;
                rightSpeed = -1;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                leftSpeed = -1;
                rightSpeed = -1+rotate_Mult;
            }
            else
            {
                leftSpeed = -1;
                rightSpeed = -1;
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.A))
            {
                leftSpeed = -rotate_Mult;
                rightSpeed = rotate_Mult;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                leftSpeed = rotate_Mult;
                rightSpeed = -rotate_Mult;
            }
            else
            {
                leftSpeed = 0.0f;
                rightSpeed = 0.0f;
            }
        }
        setMotor.Invoke(1,leftSpeed * speed_Mult);
        setMotor.Invoke(0,rightSpeed * speed_Mult);
    }

}

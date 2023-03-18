using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ros_Robot_Controller : IController
{
    //This script gets the wheel speeds from Cmd_vel_sub and sends them to robot
    public float leftSpeed,rightSpeed;
    public float speed_Mult;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        setMotor.Invoke(1,leftSpeed * speed_Mult);
        setMotor.Invoke(0,rightSpeed * speed_Mult);
    }
}

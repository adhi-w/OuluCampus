using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cmd_vel_sub : LifeNode
{
    //This script subscribes to cmd_vel topic and calculates the rotating speeds for the wheels
    ZeroMQ zeroMQ;
    Ros_Robot_Controller controller;
    subscribeEvent subscribe;

    float WheelRadius;
    float WheelBase;

    public float linearMultiplier = 0.25f;
    public float angularMultiplier = 0.1f;
    public override void init()
    {
        //Initializing. Subscribe to topic and get the variables for wheels
        base.init();
        zeroMQ = ZeroMQ.Instance;
        controller = GetComponent<Ros_Robot_Controller>();
        subscribe = zeroMQ.Add_Subscriber("cmd_vel", "Twist");
        subscribe.AddListener(subscribe_handler);
        WheelRadius = Variables.wheelRadius;
        WheelBase = Variables.wheelBase*2;
    }

    public override void begin()
    {
        base.begin();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void subscribe_handler(RosMessage message)
    {
        //When we get a message apply the speed on the wheel according to the message speeds
        message.Cast(null, twistData => {
            
            Vector3 localVelocity = twistData.linear.toUnity();
            Vector3 localAngularVelocity = twistData.angular.toUnity();
            controller.leftSpeed = (localVelocity.z - localAngularVelocity.y * WheelBase / 2.0f) / WheelRadius * Mathf.Rad2Deg;
            controller.rightSpeed = (localVelocity.z + localAngularVelocity.y * WheelBase / 2.0f) / WheelRadius* Mathf.Rad2Deg;
        });
    }
}

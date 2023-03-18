using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModels;
namespace UnityPhysics
{
    public class WheelOdometry : LifeNode
{
    //This script updates the robot transform and odometry(not used atm)
    public Rigidbody lW, rW, base_link;

    public Vector3 position;
    public Quaternion rotation;

    float WheelRadius;
    float WheelBase;
    public bool useAccurate; //if this is disabled we will use wheel rotation to calculate the position of the robot. If enabled we use exact position in unity-scene for position
    public TransformSystem ts;
    public TransformStamped stamped;

    public publishEvent publisher;
    ZeroMQ zmq;
    public bool sendBaseLink = false;

    public Vector3 last_position,linear_velocity;
    public Vector3 last_orientation, angular_velocity;

    Vector3 startPosition;
    float startRotation;

    public Imu imu;


    // Start is called before the first frame update
    public override void init()
    {
        WheelRadius = Variables.wheelRadius;
        WheelBase = Variables.wheelBase*2;
        zmq = ZeroMQ.Instance;
        stamped = new TransformStamped();
        stamped.header.stamp = zmq.time;
        stamped.header.frame_id = "odom";
        stamped.child_frame_id = "base_footprint";
        stamped.transform.translation = position.toRos();
        stamped.transform.rotation = rotation.toRos();
        if(sendBaseLink)
            ts.AddTransform.Invoke(stamped);
        publisher = zmq.Add_Publisher("odom", "Odometry");
        base.init();
        startPosition = transform.position;
        startRotation = transform.eulerAngles.y;
    }

    public override void begin(){
        
        base.begin();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        position = transform.position;
        rotation = transform.rotation;
        if(began)
        {
            if(useAccurate)
        {
            
            linear_velocity = transform.InverseTransformDirection(base_link.velocity);
            last_position = position;
            //Get angular velocity from imu
            angular_velocity = base_link.angularVelocity;
            
        }
        /*else
        {
            //Calculate wheel speeds
            float rightAngularSpeed = (rW.transform.parent.worldToLocalMatrix * rW.angularVelocity).x;
            float leftAngularSpeed = (lW.transform.parent.worldToLocalMatrix * lW.angularVelocity).x;

            float rightLinearSpeed = rightAngularSpeed * WheelRadius;
            float leftLinearSpeed = leftAngularSpeed * WheelRadius;
            //Debug.Log(rW.transform.worldToLocalMatrix * rW.angularVelocity);
            if (rightLinearSpeed == leftLinearSpeed)//If we go straight just add linear speed to position
            {
                position.z -= rightLinearSpeed * Time.fixedDeltaTime * Mathf.Cos(rotation.y * Mathf.Deg2Rad);
                position.x -= rightLinearSpeed * Time.fixedDeltaTime * Mathf.Sin(rotation.y * Mathf.Deg2Rad);
            }
            else //Otherwise calculate rotation and linear movement
            {
                float r = Variables.wheelBase * (rightLinearSpeed + leftLinearSpeed) / ((rightLinearSpeed - leftLinearSpeed));
                float w = (rightLinearSpeed - leftLinearSpeed) / WheelBase;
                float linearVelocity = (rightLinearSpeed+leftLinearSpeed)/2;
                //Debug.Log("R: " + r + ", W: " + w);
                //position.z -= (r * Mathf.Cos(w * Time.fixedDeltaTime) * Mathf.Sin(rotation.y * Mathf.Deg2Rad) + r * Mathf.Cos(rotation.y * Mathf.Deg2Rad) * Mathf.Sin(w * Time.fixedDeltaTime) - r * Mathf.Sin(rotation.y * Mathf.Deg2Rad));
                //position.x -= (r * Mathf.Sin(w * Time.fixedDeltaTime) * Mathf.Sin(rotation.y * Mathf.Deg2Rad) - r * Mathf.Cos(rotation.y * Mathf.Deg2Rad) * Mathf.Cos(w * Time.fixedDeltaTime) + r * Mathf.Cos(rotation.y * Mathf.Deg2Rad));
                Vector2 ICC = new Vector2(position.z-r*Mathf.Sin(rotation.y),position.x + r*Mathf.Cos(rotation.y));
                float dtheta = w*Time.fixedDeltaTime;
                Vector2 vel = Vector2.zero;
                position.z = (Mathf.Cos(dtheta)*(position.z-ICC.x)-Mathf.Sin(dtheta)*(position.x-ICC.y)) + ICC.x;
                position.x = (Mathf.Sin(dtheta) * (position.z-ICC.x) + Mathf.Cos(dtheta)*(position.x-ICC.y)) + ICC.y;
                
                rotation.y -= w * Time.fixedDeltaTime;
                //position.z += linearVelocity*Time.fixedDeltaTime * Mathf.Cos(rotation.y);
                //position.x += linearVelocity*Time.fixedDeltaTime * Mathf.Sin(rotation.y);


            }
            linear_velocity.z = (rightLinearSpeed+leftLinearSpeed)/2;
            last_position = position;
            //Get angular velocity from imu
            angular_velocity.y = (rightLinearSpeed-leftLinearSpeed)/(Variables.wheelBase*2);
            //Debug.Log(rightAngularSpeed + ", " + rightAngularSpeed);
        }*/

        
        //Update transform
        stamped.header.stamp = zmq.time;
        stamped.transform.translation = position.toRos();
        stamped.transform.rotation = rotation.toRos();
        if (sendBaseLink)
            ts.UpdateTransform.Invoke(stamped);
        //Update odometer
        
        }
        
    }

    public override void updateCycle()
    {
        OdometryData data = new OdometryData();
        data.header.stamp = zmq.time;
        data.header.frame_id = "map";
        data.child_frame_id = "base_footprint";
        data.pose.pose.position = position.toRos();
        data.pose.pose.orientation = rotation.toRos();
        data.twist.twist.linear = linear_velocity.toRos();
        data.twist.twist.angular = angular_velocity.toRos();
        publisher.Invoke(new RosMessage(DataPresets.Odometry, data));
    }
}

}

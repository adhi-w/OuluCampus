using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModels;
namespace UnityPhysics
{
    public class Imu : LifeNode
{
    public Vector3 last_linear_position,last_linear_velocity,linear_acceleration;
    public Quaternion orientation;
    public Vector3 angular_velocity, last_angle;


    ZeroMQ zmq;
    public publishEvent publish;
    public ImuData data;
    public TransformStamped trSt;
    public TransformSystem ts;

    public bool useSimplified = false;
    public Rigidbody body;
    public bool sendTransform;

    // Start is called before the first frame update
    public override void init()
    {
        base.init();
        zmq = ZeroMQ.Instance;
        orientation = transform.rotation;
        angular_velocity = Vector3.zero;
        last_linear_position = transform.position;
        last_linear_velocity = Vector3.zero;
        linear_acceleration = Vector3.zero;
        last_angle = ToEulerAngles(orientation);
        data = new ImuData();
        data.header.frame_id = "imu_link";
        data.header.stamp = zmq.time;

        trSt = new TransformStamped();
        trSt.transform = transform.localToRos();
        trSt.header.stamp = zmq.time;
        trSt.header.frame_id = "base_link";
        trSt.child_frame_id = "imu_link";
        if(sendTransform)
            ts.AddTransform.Invoke(trSt);
        

        publish = zmq.Add_Publisher("imu", "Imu");
        

    }

    public override void begin()
    {
        base.begin();
    }
    private void Update()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(began)
        {
            if(useSimplified == false)
            {
                Vector3 linear_velocity = (transform.position - last_linear_position) / Time.fixedDeltaTime;
                linear_acceleration = (linear_velocity - last_linear_velocity) / Time.fixedDeltaTime + new Vector3(0, -9.81f, 0);

                last_linear_velocity = linear_velocity;



                orientation = transform.rotation;
                Vector3 angle = ToEulerAngles(orientation);
                //Debug.Log(angle);
                angular_velocity = angle - last_angle;

                angular_velocity.x = clamp(angular_velocity.x);
                angular_velocity.y = clamp(angular_velocity.y);
                angular_velocity.z = clamp(angular_velocity.z);
                angular_velocity /= Time.fixedDeltaTime;
                last_angle = angle;

                data.header.stamp = zmq.time;

                data.angular_velocity = angular_velocity.toRos();
                data.linear_acceleration = linear_acceleration.toRos();
                data.orientation = orientation.toRos();
            }
            else
            {
                orientation = transform.rotation;
                Vector3 linear_velocity = body.GetPointVelocity(transform.position);
                linear_acceleration = (linear_velocity - last_linear_velocity) / Time.fixedDeltaTime + new Vector3(0, -9.81f, 0);
                last_linear_velocity = linear_velocity;

                angular_velocity = body.angularVelocity;

                data.header.stamp = zmq.time;

                data.angular_velocity = angular_velocity.toRos();
                data.linear_acceleration = linear_acceleration.toRos();
                data.orientation = orientation.toRos();
            }
            

            trSt.transform = transform.localToRos();
            trSt.header.stamp = zmq.time;
            if(sendTransform)
                ts.UpdateTransform.Invoke(trSt);
        }
        
    }

    public float clamp(float f)
    {
        return f > 180 ? -360 + f : f < -180 ? 360 + f : f;
    }
    public Quaternion conjugate(Quaternion q)
    {
        return new Quaternion(-q.x, -q.y, -q.z, q.w);
    }

    Vector3 ToEulerAngles(Quaternion q)
    {
        Vector3 angles;

        // roll (x-axis rotation)
        float sinr_cosp = 2 * (q.w * q.x + q.y * q.z);
        float cosr_cosp = 1 - 2 * (q.x * q.x + q.y * q.y);
        angles.x = Mathf.Atan2(sinr_cosp, cosr_cosp) * Mathf.Rad2Deg;

        // pitch (y-axis rotation)
        float sinp = 2 * (q.w * q.y - q.z * q.x);
        if (Mathf.Abs(sinp) >= 1)
            angles.y = Mathf.PI / 2 * Mathf.Sign(sinp) * Mathf.Rad2Deg; // use 90 degrees if out of range
        else
            angles.y = Mathf.Asin(sinp) * Mathf.Rad2Deg;

        // yaw (z-axis rotation)
        float siny_cosp = 2 * (q.w * q.z + q.x * q.y);
        float cosy_cosp = 1 - 2 * (q.y * q.y + q.z * q.z);
        angles.z = Mathf.Atan2(siny_cosp, cosy_cosp)*Mathf.Rad2Deg;

        return angles;
    }

    public override void updateCycle()
    {
        publish.Invoke(new RosMessage(DataPresets.Imu, data));
    }
}

}

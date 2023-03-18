using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModels;

public class JoystickPosePublisher : LifeNode
{
    public publishEvent publisher;
    [Range(-100,100)]
    public float vertical,horizontal;
    public bool button;

    public Transform Robot;
    public Rigidbody Robot_Rigidbody;
    public Vector3 position = Vector3.zero;
    
    public float _Scale = 1.0f;

    public override void init()
    {
        base.init();
        publisher = ZeroMQ.Instance.Add_Publisher("worldpos","PoseStamped");
    }

    public override void begin()
    {
        base.begin();
    }

    void Update()
    {
        //float h = Input.GetAxis("Oculus_CrossPlatform_PrimaryThumbstickHorizontal");
        //float v = Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickVertical");
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float speed = Robot_Rigidbody.velocity.z;
        float rotation = Robot_Rigidbody.angularVelocity.y;
        Vector3 pos = getForwardPosition(Robot.position, Robot.forward, speed, rotation, 1.0f);
        Vector3 newForward = Quaternion.AngleAxis(rotation*1.0f, Vector3.up) * Robot.forward;
        Vector3 newRight = Quaternion.AngleAxis(rotation*1.0f, Vector3.up) * Robot.right;
        position = pos + newForward * v * _Scale + newRight * h*_Scale;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(Robot.position, position);
    }
    public override void updateCycle()
    {
        if((Robot.position-position).sqrMagnitude>0.25f)
        {
            PoseStampedData data = new PoseStampedData();
            data.pose.position = position.toRos();
            data.header.frame_id="map";
            data.header.stamp = ZeroMQ.Instance.time;
            publisher.Invoke(new RosMessage(DataPresets.poseStampedData, data));    
        }
    }

    public Vector3 getForwardPosition(Vector3 position, Vector3 direction, float speed, float rotation, float time)
    {
        for(int i=0; i<10; i++)
        {
            position += direction * speed / 10 * time;
            direction = Quaternion.AngleAxis(rotation / 10 * time, Vector3.up) * direction;
        }
        return position;
    }
}

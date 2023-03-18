using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModels;

public class InitialPose : LifeNode
{
    //This script sends initial pose for the amcl. The pose is sent in the startup and can be also sent at any time by pressing send button in the inspector
    public Transform robot; //Transform whose pose will be sent to amcl
    publishEvent publisher;
    ZeroMQ zmq;
    // Start is called before the first frame update
    public override void init()
    {
        //Initialize. Add publisher to ROS
        base.init();
        zmq = ZeroMQ.Instance;
        publisher = zmq.Add_Publisher("initialpose", "PoseWithCovarianceStamped");
    }

    public override void begin()
    {
        //Begin. Sends initial pose
        base.begin();
        sendPose();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void sendPose()
    {
        //Get robots position and add it to ROS package. Covariance is copied from message that was sent by Rviz
        PoseWithCovarianceStampedData data = new PoseWithCovarianceStampedData();
        data.pose.pose.position = robot.position.toRos();
        data.pose.pose.orientation = robot.rotation.toRos();
        data.header.frame_id = "map";
        data.header.stamp = zmq.time;
        data.pose.covariance = new double[] { 0.25f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.25f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0685389194520094f};
        RosMessage msg = new RosMessage(DataPresets.PoseWithCovarianceStamped, data);
        publisher.Invoke(msg);

        
    }
}

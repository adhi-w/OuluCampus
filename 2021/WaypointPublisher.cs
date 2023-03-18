using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModels;
public class WaypointPublisher : LifeNode
{
    //This script sends waypoints to the ROS
    public static WaypointPublisher instance_;

    public publishEvent publisher;
    public Transform[] waypoints;
    public int wp_id;
    
    void Awake()
    {
        instance_ = this;
    }
    public override void init()
    {
        base.init();
        publisher = ZeroMQ.Instance.Add_Publisher("goal_pose","PoseStamped");
    }

    public override void begin()
    {
        base.begin();
    }
    
    public void sendWaypoint(bool repeat = false)
    {
        int id = wp_id;
        if(repeat)
            id--;
        if(began && wp_id<waypoints.Length)
        {
            Transform point = waypoints[id];
            Debug.Log(point.rotation);
            PoseStampedData data = new PoseStampedData();
            data.header.frame_id = "map";
            data.header.stamp = ZeroMQ.Instance.time;
            data.pose.position = point.position.toRos();
            data.pose.orientation = point.rotation.toRos();
            Debug.Log(data.pose.orientation);
            RosMessage message = new RosMessage(DataPresets.poseStampedData, data);
            publisher.Invoke(message);
            if(!repeat)
                wp_id++;
        }
    }
}

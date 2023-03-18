using DataModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
public class LaserScan : LifeNode
{
    //This script simulates the laser scan and sends it to "scan" topic for ros
    public LaserData laserData;
    bool scanning;
    publishEvent pe;
    ZeroMQ zmq;
    public RosMessage message;
    public TransformStamped trSt;
    public TransformSystem ts;

    public bool sendTransform;

    public LayerMask mask;

    public bool visualize;
    public override void init(){
        //These values are similar to the values on the real robots laser scanner
        zmq = ZeroMQ.Instance;
        float angle = 270 * Mathf.Deg2Rad;
        laserData.angle_min = -angle/2;
        laserData.angle_max = angle/2;
        laserData.angle_increment = 0.25f * Mathf.Deg2Rad;
        laserData.time_increment = 0.0f;
        laserData.scan_time = 0.000f;
        laserData.range_min = 0.06f;
        laserData.range_max = 20f;
        laserData.ranges = new float[1081];
        laserData.header.frame_id = "base_scan";
        pe = zmq.Add_Publisher("scan", "LaserScan");
        trSt = new TransformStamped();
        trSt.header.stamp = zmq.time;
        trSt.header.frame_id = "base_link";
        trSt.child_frame_id = "base_scan";
        trSt.transform = transform.localToRos();
        if(sendTransform)
            ts.AddTransform.Invoke(trSt);

        message = new RosMessage(DataPresets.laserData,laserData);
        base.init();
    }

    public override void begin()
    {
        base.begin();
    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(transform.localRotation);

        
        /*transformsData.transforms = new TransformStamped[] { trSt };
        
        tps.Invoke(message);*/
        
        //StartCoroutine("sendTransform");
    }

    // Update is called once per frame
    void Update()
    {
        if(began && visualize)
        {
            float startAngle = 0;
            Ray ray = new Ray(); ;
            ray.origin = transform.position;
            for (int i = 0; i <= 1080; i++)
            {
                float a = startAngle - i * 0.25f;
                Quaternion q = Quaternion.AngleAxis(a, transform.up);
                ray.direction = q * transform.forward;
                Debug.DrawRay(ray.origin, ray.direction * laserData.ranges[i]);
            }
        }
        
    }

    private void FixedUpdate()
    {
        /*trSt.header.stamp.sec = Mathf.FloorToInt(Time.time);
        trSt.header.stamp.nsec = Mathf.FloorToInt((Time.time % 1) * 1000000000);
        trSt.transform = transform.localToRos();
        ts.UpdateTransform.Invoke(trSt);*/
    }
    public IEnumerator Scan()
    {
        //Use raycast for simulating the scan
        scanning = true;
        laserData.header.stamp = zmq.time;
        float startAngle = 0;

        laserData.header.frame_id = "base_scan";

        RaycastHit hit;
        Ray ray = new Ray(); ;
        
        for(int i = 0; i<=1080; i++)
        {
            float a = startAngle - i * 0.25f;
            Quaternion q = Quaternion.AngleAxis(a, transform.up);
            ray.direction = q*transform.forward;
            ray.origin = transform.position;
            if(Physics.Raycast(ray, out hit, laserData.range_max,mask))
            {
                laserData.ranges[i] = hit.distance;
            }
            else
            {
                laserData.ranges[i] = laserData.range_max;
            }
        }

        //Send laserdata
        message = new RosMessage(DataPresets.laserData, laserData);
        pe.Invoke(message);
        scanning = false;
        yield return null;
    }

    public override void updateCycle()
    {
        StartCoroutine("Scan");
    }



    /*public IEnumerator sendTransform()
    {
        while(true)
        {
            trSt.header.stamp.sec = Mathf.FloorToInt(Time.time);
            trSt.header.stamp.nsec = Mathf.FloorToInt((Time.time % 1) * 1000000000);
            trSt.header.frame_id = "map";
            trSt.child_frame_id = "laser";
            trSt.transform = transform.toRos();
            transformsData.transforms = new TransformStamped[] { trSt };
            message = new RosMessage(DataPresets.TFMessage, transformsData);

            tp.Invoke(message);
            message = new RosMessage(DataPresets.laserData, laserData);
            yield return new WaitForSeconds(0.25f);
        }
    }*/
}

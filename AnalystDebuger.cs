using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModels;
using System;

public class AnalystDebuger : LifeNode
{
    ZeroMQ zeroMQ;
    subscribeEvent subscribe;
    public override void init()
    {
        base.init();
        zeroMQ = ZeroMQ.Instance;
        subscribe = zeroMQ.Add_Subscriber("nav_analyst", "Analyst");
        subscribe.AddListener(subscribe_handler);
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

    public void subscribe_handler(RosMessage msg)
    {
        Analyst analystmsg = msg.cast<Analyst>();
        Debug.Log(String.Format("Total cost of the path: {0}\nTotal length of the path: {1}\nTotal duration of the path: {2}.{3:000}",analystmsg.total_cost, analystmsg.total_length, analystmsg.total_time.sec, analystmsg.total_time.nanosec));

    }
}

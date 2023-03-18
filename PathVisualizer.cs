using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModels;
public class PathVisualizer : LifeNode
{
    ZeroMQ zmq;
    LineRenderer lineRenderer;
    subscribeEvent subscribe;

    Vector3[] points = new Vector3[1];
    bool dirty;

     public override void init(){
         zmq = ZeroMQ.Instance;
         lineRenderer = GetComponent<LineRenderer>();
         base.init();
     }

    public override void begin()
    {
        subscribe = zmq.Add_Subscriber("plan", "Path");
        subscribe.AddListener(subscribe_handler);
        base.begin();

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(dirty)
        {
            dirty=false;
            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);
        }
    }

    void subscribe_handler(RosMessage message)
    {
        PathData data = message.cast<PathData>();
        points = new Vector3[data.poses.Length];
        int i=0;
        foreach(PoseStampedData pose in data.poses)
        {
            points[i] = pose.pose.position.toUnity();
            points[i].y+=0.2f;
            i++;
        }
        dirty=true;
    }
}

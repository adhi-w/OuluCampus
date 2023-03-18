using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Events;
using System.Linq;
public class TransformSystem : LifeNode
{
    //This script keeps track of the transforms and sends them to python.
    public class TransformAction : UnityEvent<TransformStamped> {};

    public TransformAction AddTransform, RemoveTransform, UpdateTransform, AddStaticTransform; //You can add, remove and update transforms using these actions. Static transforms aren't working at the moment
    public TFMessageData transformsData;
    public List<TransformStamped> transforms = new List<TransformStamped>();
    public List<TransformStamped> staticTransforms = new List<TransformStamped>();

    public List<TransformStamped> updatedTransforms = new List<TransformStamped>();
    publishEvent publisher,staticPublisher;
    ZeroMQ zmq;

    public RosMessage message;
    // Start is called before the first frame update
    public override void init()
    {
        //Initialize everything.
        base.init();
        AddTransform = new TransformAction();
        RemoveTransform = new TransformAction();
        UpdateTransform = new TransformAction();
        AddStaticTransform = new TransformAction();
        AddTransform.AddListener(addTransform);
        RemoveTransform.AddListener(removeTransform);
        UpdateTransform.AddListener(updateTransform);
        AddStaticTransform.AddListener(addStaticTransform);
        zmq = ZeroMQ.Instance;
        transformsData = new TFMessageData();
        publisher = zmq.Add_Publisher("tf", "TFMessage");
        staticPublisher = zmq.Add_Publisher("tf_static", "TFMessage");

        
    }

    public override void begin()
    {
        base.begin();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void updateCycle()
    {
        transformsData.transforms = updatedTransforms.ToArray();
        message = new RosMessage(DataPresets.TFMessage, transformsData);
        publisher.Invoke(message);
        updatedTransforms.Clear();
    }

    public void sendStaticTransforms(){
        transformsData.transforms = staticTransforms.ToArray();
        message = new RosMessage(DataPresets.TFMessage, transformsData);
        staticPublisher.Invoke(message);
    }

    public void addTransform(TransformStamped tr)
    {
        //Add new transform to the list
        TransformStamped found = transforms.Where(obj => obj.child_frame_id == tr.child_frame_id && obj.header.frame_id == tr.header.frame_id).SingleOrDefault();

        if (found == null)
        {
            transforms.Add(tr);
            updatedTransforms.Add(tr);
        }
        else
            updateTransform(tr);
    }

    public void addStaticTransform(TransformStamped tr)
    {
        TransformStamped found = staticTransforms.Where(obj => obj.child_frame_id == tr.child_frame_id && obj.header.frame_id == tr.header.frame_id).SingleOrDefault();

        if (found == null)
            staticTransforms.Add(tr);
    }

    public void removeTransform(TransformStamped tr)
    {
        //Remove transform
        TransformStamped found = transforms.Where(obj => obj.child_frame_id == tr.child_frame_id && obj.header.frame_id == tr.header.frame_id).SingleOrDefault();

        if (found == null)
        {
            print("Didn't find transform to be removed: " + tr.header.frame_id + "->" + tr.child_frame_id);
        }
        else
            transforms.Remove(found);

        found = updatedTransforms.Where(obj => obj.child_frame_id == tr.child_frame_id && obj.header.frame_id == tr.header.frame_id).SingleOrDefault();

        if (found != null)
        {
            transforms.Remove(found);
        }

        
    }

    public void updateTransform(TransformStamped tr)
    {
        //Updates transform in list
        TransformStamped found = transforms.Where(obj => obj.child_frame_id == tr.child_frame_id && obj.header.frame_id == tr.header.frame_id).SingleOrDefault();
        if(found == null)
        {
            print("Error finding transform");
            return;
        }
        found.transform = tr.transform;
        found.header = tr.header;

        TransformStamped found2 = updatedTransforms.Where(obj => obj.child_frame_id == tr.child_frame_id && obj.header.frame_id == tr.header.frame_id).SingleOrDefault();
        if(found2 == null)
            updatedTransforms.Add(tr);
        else
        {
            found2.transform = tr.transform;
            found2.header = tr.header;
        }
    }


}



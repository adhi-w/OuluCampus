using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetMQ;
using System.Threading;
using NetMQ.Sockets;
using System;
using UnityEditor;
using UnityEngine.Events;
using System.Net.Sockets;

public class subscribeEvent : UnityEvent<RosMessage> { };
public class publishEvent : UnityEvent<RosMessage> { };
public class ZeroMQ : MonoBehaviour
{
    //This script connects to python package using ZeroMQ
    private static ZeroMQ _instance; //Basic singleton pattern

    public static ZeroMQ Instance{
        get{
            return _instance;
        }
    }
    private PairSocket client;
    private NetMQTimer messageTimer;
    private Thread receiverThread;
    private bool stopThread;
    private UnityEngine.Object thisLock_ = new UnityEngine.Object();
    private UnityEngine.Object subLock_ = new UnityEngine.Object();


    public Queue<string> messages = new Queue<string>();
    public Queue<RosMessage> subMessages = new Queue<RosMessage>();
    private Dictionary<string, subscribeEvent> subscriptions = new Dictionary<string, subscribeEvent>();
    private List<publishEvent> publishers = new List<publishEvent>();
    public string ip = "tcp://192.168.0.102:5555";
    public TimeStamp time;
    // Start is called before the first frame update
    void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
        }
        ConnectToServer();
    }
    private void Start() {
        subscriptions["clock"] = new subscribeEvent();
        subscriptions["clock"].AddListener(timer);
    }

    // Update is called once per frame
    void Update()
    {
        //Debugging
        if (Input.GetKeyDown(KeyCode.Space))
            messages.Enqueue("This is test");
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
        
    }

    private void ConnectToServer()
    {
        //Start server thread
        receiverThread = new Thread(new ThreadStart(ListenForData));
        receiverThread.IsBackground = true;
        receiverThread.Start();

    }

    private void ListenForData()
    {
        //Connect the ZeroMQ
        AsyncIO.ForceDotNet.Force();
        NetMQConfig.Cleanup();
        try
        {
            messageTimer = new NetMQTimer(10);
            messageTimer.Elapsed +=  (s, a) =>
            {
                //Did we receive any subscriptions
                
                //Is there any messages to send
                while (messages.Count > 0)
                {

                    var toSend = messages.Dequeue();
                    //Debug.Log(toSend);
                    //Debug.Log(messages.Count);
                    try
                    {
                        client.SendFrame(toSend);
                    }
                    catch
                    {
                        Debug.LogWarning("Error when sending data: "+toSend);
                    }
                    
                    //Debug.Log("Sending message");

                }
            };
            using (client = new PairSocket())
            using (NetMQPoller poller = new NetMQPoller { client, messageTimer})
            {
                client.Connect(ip);
                Debug.Log("Connected");
                //Poller will call this function when we have a message from python
                client.ReceiveReady += (s, a) =>
                {
                    
                        bool more;
                        string str = a.Socket.ReceiveFrameString(out more);
                        //Debug.Log(str);
                        RosMessage message = RosMessage.FromJson(str);
                        //What kind of message received
                        switch (message.command)
                        {
                            case "subscription":
                                //When we get subcription message add it to queue
                                //subMessages.Enqueue(message);
                                //if(message.topic!="clock")
                                 //   Debug.Log("Got Subscription on: "+message.topic);
                                if(subscriptions.ContainsKey(message.topic))
                                    subscriptions[message.topic]?.Invoke(message);
                                //if (subscriptions.ContainsKey(message.topic))
                                //    subscriptions[message.topic].Invoke(message);
                                break;
                        }
                    
                    
                };
                //Start the poller
                poller.RunAsync();

                //Loop until the end
                while (!stopThread)
                {
                        
                        
                    
                }
                //poller.Stop();
                //client.Close();

                //receiverThread.Abort();
            }
        }
        catch (SocketException e) 
        {
            Debug.Log("There was an error");
            if (e.SocketErrorCode != SocketError.WouldBlock)
                        throw;
        }
        
        
        NetMQConfig.Cleanup();


    }

    public void timer(RosMessage message)
    {
        message.Cast(null, null, clockData => {
            time = clockData.time;
        });
    }
    public void SendJsonMessage(RosMessage msg)
    {
        //Send RosMessage as JSON
        messages.Enqueue(msg.ToJson());
    }



    void OnApplicationQuit()
    {
        stopThread = true;
    }

    private void OnDestroy()
    {
        stopThread = true;
    }

    public subscribeEvent Add_Subscriber(string topic, string type)
    {
        //Sends subscription message to the python and adds the subscription to dictionary. Returns subscription event that will be invoked when we get message from the python
        subscribeEvent tmp = new subscribeEvent();
        Debug.Log("Adding subscriber");
        RosMessage msg = RosMessage.AddSubscriber(type, topic);
        SendJsonMessage(msg);
        subscriptions.Add(topic, tmp);
        return tmp;
    }

    public void Spin()
    {
        //Start spinning
        SendJsonMessage(RosMessage.Spin());
    }

    public publishEvent Add_Publisher(string topic, string type)
    {
        //Sends publisher message to python and adds publisher to list. Returns publish event that can be used to send messages to this topic.
        publishEvent tmp = new publishEvent();
        Debug.Log("added publisher "+topic);
        RosMessage msg = RosMessage.AddPublisher(type, topic);
        SendJsonMessage(msg);
        tmp.AddListener((RosMessage _msg) => Publish(topic, _msg));
        publishers.Add(tmp);
        return tmp;
    }

    public void Publish(string topic, RosMessage msg)
    {
        //Publish message to specific topic. This will be connected to the publish events
        msg.topic = topic;
        SendJsonMessage(msg);
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
public class SystemScript : MonoBehaviour
{
    //This script controls the LifeNode-scripts
    ZeroMQ zmq;
    //public string startBat = "C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs\\Visual Studio 2019\\Visual Studio Tools\\VC\\x64 Native Tools Command Prompt for VS 2019";
    public string startFile = "C:/opt/start.bat"; //File to start the ROS-nodes from
    public bool runLocally = false;
    public List<LifeNode> nodes = new List<LifeNode>(); //List of LifeNode-scripts we want to start
    public bool started, initialized,began;
    // Start is called before the first frame update
    void Start()
    {
        zmq = ZeroMQ.Instance;
        //On startup start the process for ROS-nodes
        if(runLocally)
        {
            using(Process process = new Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = startFile;
                process.StartInfo.WorkingDirectory = "C:/opt/";
                process.Start();
            }
        }
        
    }

    public void init()
    {
        //Initialize all LifeNodes in the list
        foreach(LifeNode n in nodes)
        {
            n.init();
            UnityEngine.Debug.Log(n);
        }
        started = true;
    }
    // Update is called once per frame
    void Update()
    {
        if(started)
        {
            if(!initialized)
            {
                //Wait until all LifeNodes are ready
                bool ready = true;
                foreach(LifeNode n in nodes)
                {
                    if(!n.initialized)
                        ready = false;
                }
                //After everything is ready let all nodes to start working
                if(ready)
                {
                    initialized = true;
                    foreach(LifeNode n in nodes)
                    {
                        n.begin();
                    }
                }
                    
            }
            else
            {
                if(!began)
                {
                    //Wait for all the nodes to be started
                    bool ready = true;
                    foreach(LifeNode n in nodes)
                    {
                        if(!n.began)
                            ready = false;
                    }
                    //When everything is ready start spinning ROS
                    if(ready)
                    {
                        Spin();
                        began=true;
                    }
                }
            }
        }
    }

    public void Spin()
    {
        zmq.Spin();
    }
}

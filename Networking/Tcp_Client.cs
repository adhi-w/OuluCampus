using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;


public class Tcp_Client : MonoBehaviour
{
    private TcpClient client;
    private Thread receiveThread;
    public string message;
    public bool send;

    
    // Start is called before the first frame update
    void Awake()
    {
        ConnectToServer();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (send)
        {
            SendTcpMessage(message);
            send = false;
        }
    }


    private void ConnectToServer()
    {
        try
        {
            AsyncIO.ForceDotNet.Force();
            receiveThread = new Thread(new ThreadStart(ListenForData));
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.Log("On client connect exception " + e);
        }
    }

    private void ListenForData()
    {
        try
        {
            client = new TcpClient("localhost", 9999);
            Byte[] bytes = new Byte[1024];
            while (true)
            {
                // Get a stream object for reading 				
                using (NetworkStream stream = client.GetStream())
                {
                    int length;
                    // Read incomming stream into byte arrary. 					
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var incommingData = new byte[length];
                        Array.Copy(bytes, 0, incommingData, 0, length);
                        // Convert byte array to string message. 						
                        string serverMessage = Encoding.ASCII.GetString(incommingData);
                        //Debug.Log("server message received as: " + serverMessage);
                        Debug.Log(serverMessage);
                        RosMessage msg = RosMessage.FromJson(serverMessage);
                        
                        /*switch(parts[0])
                        {
                            case "subscription":
                                if(subscriptions.ContainsKey(parts[1])) //if there is subscription with given type
                                {
                                    subscriptions[parts[1]].Invoke(parts[2]);
                                }
                                break;
                        }*/
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    private void SendTcpMessage(string message)
    {
        if (client == null)
        {
            return;
        }
        try
        {
            // Get a stream object for writing. 			
            NetworkStream stream = client.GetStream();
            if (stream.CanWrite)
            {
                // Convert string message to byte array.                 
                byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(message+"\n");
                // Write byte array to socketConnection stream.                 
                stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                Debug.Log("Sending message: " + message);
                //Debug.Log("Client sent his message - should be received by server");
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    public void SendJsonMessage(RosMessage msg)
    {
        SendTcpMessage(msg.ToJson());
    }

    private void OnDisable()
    {
        SendTcpMessage("close");
        client.Close();
        receiveThread.Abort();
    }

    private void OnApplicationQuit()
    {
        SendTcpMessage("close");
        client.Close();
        receiveThread.Abort();
    }

    private void OnDestroy()
    {
        SendTcpMessage("close");
        client.Close();
        receiveThread.Abort();
    }

    
}

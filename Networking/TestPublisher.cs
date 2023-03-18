using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class TestPublisher : MonoBehaviour
{
    public ZeroMQ client;
    subscribeEvent sE;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            sE = client.Add_Subscriber("chatter", "String");
            sE.AddListener(subscribe);
            client.Spin();
        }


    }

    public void subscribe(RosMessage msg)
    {
        msg.Cast(LaserData: data =>
        {
            //this will execute for laser data
        },
            TwistData: data =>
            {
                //this will execute for twist data
            } );
    }
}

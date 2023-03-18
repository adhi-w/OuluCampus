using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Universal_Trigger : MonoBehaviour
{
    //This script triggers the UnityEvent when the robot hits the trigger
    public UnityEvent Event;
    public bool triggered;
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag=="Player" && triggered==false)
        {
            triggered=true;
            Event.Invoke();
        }
    }
}

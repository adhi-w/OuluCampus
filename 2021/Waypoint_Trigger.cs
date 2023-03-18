using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint_Trigger : MonoBehaviour
{
    public bool triggered = false;
    public float timer=0f;
    public int repeatTimes = 0;
    public float repeatTime = 0.2f;
    // Start is called before the first frame update
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag=="Player" && triggered==false)
        {
            triggered=true;
            StartCoroutine(delayer(timer));
        }
    }

    IEnumerator delayer(float time)
    {
        yield return new WaitForSeconds(time);
        WaypointPublisher.instance_.sendWaypoint(false);
        while(repeatTimes>0)
        {
            yield return new WaitForSeconds(repeatTime);
            WaypointPublisher.instance_.sendWaypoint(true);
            repeatTimes--;
        }
        
    }
}

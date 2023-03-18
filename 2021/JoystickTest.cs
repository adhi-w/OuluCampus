using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModels;

public class JoystickTest : LifeNode
{
    public publishEvent publisher;
    [Range(-1,1)]
    public float vertical,horizontal;
    public bool button;

    public override void init()
    {
        base.init();
        publisher = ZeroMQ.Instance.Add_Publisher("joy","Joy");
    }

    public override void begin()
    {
        base.begin();
    }
    public override void updateCycle()
    {
        Joy data = new Joy();
        data.header.frame_id="map";
        data.header.stamp = ZeroMQ.Instance.time;
        data.axes[0] = horizontal;
        data.axes[1] = vertical;
        data.buttons[0] = button ? 1 : 0;
        publisher.Invoke(new RosMessage(DataPresets.joyData, data));
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;
public class MotorEvent : UnityEvent<int, float> { } // int id: id of the motor to control, float value: target value for the motor
public abstract class IController : MonoBehaviour
{
    public MotorEvent setMotor; 


}

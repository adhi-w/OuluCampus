using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace UnityPhysics
{
    public class Motion_Primitive_Controller : IController
{

    public class PrimitiveListEvent : UnityEvent<List<Primitive>, bool> { };

    public Transform robotTransform;
    public Rigidbody robotRigidbody;

    //[SerializeField]
    //private PID positionPID, rotationPID;
    public PID leftWheelPID, rightWheelPID;

    public float distanceTolerance,angleTolerance,angularSpeedTolerance, linearSpeedTolerance;

    public Primitive currentPrimitive;
    
    
    public float leftSpeed, rightSpeed;

    public float wheelRadius;
    public float axisRadius;
    [HideInInspector]
    public float axisCirc;
    [HideInInspector]
    public float wheelCirc;
    public float leftDistance;
    public Rigidbody leftWheelRigidbody, rightWheelRigidbody;

    [SerializeField]
    public List<Primitive> primitiveList = new List<Primitive>();
    public PrimitiveListEvent setPrimitives = new PrimitiveListEvent();

    public List<PrimitiveInspector> inspectorList = new List<PrimitiveInspector>();
    // Start is called before the first frame update
    void Start()
    {
        currentPrimitive = new P_None();
        setPrimitives.AddListener(nextPrimitive);
        wheelRadius = Variables.wheelRadius*100;
        axisRadius = Variables.wheelBase;
        wheelCirc = 2 * Mathf.PI * wheelRadius;
        axisCirc = 2 * Mathf.PI * axisRadius;
        UnityEngine.Debug.Log("Asdf");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawSphere(targetPosition, 0.1f);
        //Gizmos.DrawRay(new Ray(robotTransform.position, targetAngle));
    }

    private void FixedUpdate()
    {
        if(!(currentPrimitive is P_None))
        {
            currentPrimitive.performMotion();
            leftDistance = currentPrimitive.leftDistance;
        }

        setMotor.Invoke(0, rightSpeed);
        setMotor.Invoke(1, leftSpeed);
        
    }

    public void nextPrimitive(List<Primitive> prList = null, bool start=true)
    {
        if (prList != null)
            primitiveList = prList;
        //Debug.Log("Time: " + Time.time);
        //Debug.Log(primitiveList.Count);
        if(primitiveList.Count==0 || start == false)
        {
            currentPrimitive = new P_None();
            return;
        }

        Primitive next = primitiveList[0];
        primitiveList.RemoveAt(0);
        currentPrimitive = next;
        currentPrimitive.Reset(this);

    }
    public List<Primitive> InspectorConverter(List<PrimitiveInspector> list)
    {
        List<Primitive> outList = new List<Primitive>();
        foreach (PrimitiveInspector p in list)
        {
            if (p.type == PrimitiveInspector.PrimitiveType.P_DriveStraight)
                outList.Add(new P_DriveStraight(p.value, null));
            if (p.type == PrimitiveInspector.PrimitiveType.P_TurnInPlace)
                outList.Add(new P_TurnInPlace(p.value, null));
        }
        return outList;
    }



}
}


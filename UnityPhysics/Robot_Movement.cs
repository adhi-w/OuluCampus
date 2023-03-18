using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityPhysics
{
    public class Robot_Movement : MonoBehaviour
{
    public float pFactor, iFactor, dFactor;
    

    public MotorEvent motorEvent = new MotorEvent();  // id 0 = right motor, id 1 = left motor, value = target angular velocity of the motor
    public IController controller;

    public float maxTorque = 100; //Maximum torque the motors can apply

    [SerializeField]
    private HingeJoint leftJoint, rightJoint; //Joints of the wheels
    [SerializeField]
    private Rigidbody leftWheelRigid, rightWheelRigid; //Rigidbodies of the wheels

    

    private float leftTargetSpeed, rightTargetSpeed; //Target speeds gotten from the controller

    private PID pidLeft, pidRight; //PID-controllers of the wheels

    public LineRenderer left, right; //Debugging lines
    private List<Vector3> leftList = new List<Vector3>();
    private List<Vector3> rightList = new List<Vector3>();
    public float visMult;


    public bool motorEN = false; //Are we using the motors in joints to control the wheels (not in use)

    

    private Rigidbody pxRigidbody; //Rigidbody of the robot itself

    private JointMotor leftMotor, rightMotor; //Motor variable we use to edit the joint motors. (For some reason PhysX4 motors in hinges aren't variables you can change on the fly

    public float maxSpeed;

    public float leftSpeed, rightSpeed;
    private float lastLeftSpeed,lastRightSpeed;
    public bool usePID;

    // Start is called before the first frame update
    void Start()
    {
        motorEvent.AddListener(setMotorSpeed);//Add listener for motors and assign the controller
        controller.setMotor = motorEvent;

        pidLeft = new PID(pFactor, iFactor, dFactor); //Create the PID-controllers
        pidRight = new PID(pFactor, iFactor, dFactor);

        pxRigidbody = GetComponent<Rigidbody>(); //Get the rigidbody
        lastLeftSpeed = -10;
        lastRightSpeed = -10;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        pidLeft.pFactor = pFactor; //Update the PID values
        pidLeft.iFactor = iFactor;
        pidLeft.dFactor = dFactor;
        pidRight.pFactor = pFactor;
        pidRight.iFactor = iFactor;
        pidRight.dFactor = dFactor;
        
        


        float rightCurrentSpeed = (rightWheelRigid.transform.worldToLocalMatrix * rightWheelRigid.angularVelocity).x; //Get current angular velocity over X-axis of the wheels
        float leftCurrentSpeed = (leftWheelRigid.transform.worldToLocalMatrix * leftWheelRigid.angularVelocity).x;

        float rightCorrection =  pidRight.Update(rightTargetSpeed, rightCurrentSpeed, Time.fixedDeltaTime); //Calculate the correction values using PID-controllers
        float leftCorrection =  pidLeft.Update(leftTargetSpeed, leftCurrentSpeed, Time.fixedDeltaTime);

        
        if(usePID)
        {
            leftSpeed = leftCurrentSpeed;
            rightSpeed = rightCurrentSpeed;
            leftSpeed += leftCorrection;
            rightSpeed += rightCorrection;
        }
        else
        {
            leftSpeed = leftTargetSpeed;
            rightSpeed = rightTargetSpeed;
        }
        leftSpeed = Mathf.Clamp(leftSpeed, -maxSpeed, maxSpeed);
        rightSpeed = Mathf.Clamp(rightSpeed, -maxSpeed, maxSpeed);
        
        if (!motorEN)
        {
            /*rightWheelRigid.AddTorque(pxRigidbody.transform.localToWorldMatrix * new Vector3(rightCorrection, 0, 0), ForceMode.VelocityChange);
            leftWheelRigid.AddTorque(pxRigidbody.transform.localToWorldMatrix * new Vector3(leftCorrection, 0, 0), ForceMode.VelocityChange);

            leftMotor.force = 0;
            leftMotor.targetVelocity = 0;
            rightMotor.force = 0;
            rightMotor.targetVelocity = 0;
            */
            leftJoint.motor = leftMotor;
            rightJoint.motor = rightMotor;
            rightWheelRigid.AddTorque(pxRigidbody.transform.localToWorldMatrix * new Vector3(rightCorrection, 0, 0), ForceMode.VelocityChange);
            leftWheelRigid.AddTorque(pxRigidbody.transform.localToWorldMatrix * new Vector3(leftCorrection, 0, 0), ForceMode.VelocityChange);
        }
        else
        {
            
            //motor.maxTorque = maxTorque; //Set the motor speeds
            //motor.targetSpeed = Mathf.Min(Mathf.Max((leftTargetSpeed+leftCorrection)*60,-maxSpeed),maxSpeed);
            //motor.targetSpeed = rightSpeed;
            /*if(rightSpeed == 0)
            {
                limit.minAngle = 0;
                limit.maxAngle = 0;
            }
            else
            {
                limit.minAngle = -180;
                limit.maxAngle = 180;
            }*/
            //rightJoint.limit = limit;
            if(lastRightSpeed!=rightSpeed)
            {
                
                rightMotor.targetVelocity = rightSpeed;
                rightMotor.force = maxTorque;
                rightMotor.freeSpin = false;
                
                rightJoint.motor = rightMotor;
            }
            

            /*if(leftSpeed == 0)
            {
                limit.minAngle = 0;
                limit.maxAngle = 0;
            }
            else
            {
                limit.minAngle = -180;
                limit.maxAngle = 180;
            }*/
            //leftJoint.limit = limit;
                

            if(lastLeftSpeed!=leftSpeed)
            {
                
                leftMotor.targetVelocity = leftSpeed;
                leftMotor.force = maxTorque;
                leftMotor.freeSpin = false;
                leftJoint.motor = leftMotor;
                //Debug.Log("Updating motor");
            }

            
            lastLeftSpeed = leftSpeed;
            lastRightSpeed = rightSpeed;
            //motor.targetSpeed = Mathf.Min(Mathf.Max((rightTargetSpeed+rightCorrection)*60,-maxSpeed),maxSpeed);
            

            

        }


        //Debug.Log(leftCurrentSpeed + "\t" + leftTargetSpeed+ "\t" + leftCorrection + "\t" + (leftCurrentSpeed + leftCorrection));
        //Debug.Log(pxRigidbody.velocity.magnitude);
        if(left!=null && right!=null){
        if(left.gameObject.activeSelf || right.gameObject.activeSelf)
        {
            addPoint(leftList, pxRigidbody.velocity.magnitude);//Add visualizer points and show the lines
            addPoint(rightList, pxRigidbody.angularVelocity.y);
            visualize(left, leftList);
            visualize(right, rightList);
        }}
        
    }

    private void setMotorSpeed(int id , float speed) //Function for controlling the motors
    {
        if (id == 0)
            rightTargetSpeed = speed;
        else if(id == 1)
            leftTargetSpeed = speed;
    }

    public void addPoint(List<Vector3> L, float v)//Add point into the visualizer list
    {
        if (L.Count > 312)
            L.Clear();
        L.Add(new Vector3(L.Count*0.01f,v*visMult,0));
        if (L.Count % 50 == 0)
        {
            L.Add(new Vector3((L.Count - 1) * 0.01f, -0.5f, 0));
            L.Add(new Vector3((L.Count - 2) * 0.01f, v * visMult, 0));
        }
            
    }
    public void visualize(LineRenderer LR, List<Vector3> L)//Set visualizer points into the linerenderers
    {
        LR.positionCount = L.Count;
        LR.SetPositions(L.ToArray());
    }
}

}

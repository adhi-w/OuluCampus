using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityPhysics
{
    [System.Serializable]
    public abstract class Primitive
    {
        public float value;
        protected Motion_Primitive_Controller controller;
        public float leftDistance;
        public float rightDistance;
        protected float targetDistanceLeft;
        protected float targetDistanceRight;
        protected PID leftWheelPID;
        protected PID rightWheelPID;

        protected float lastTime;
        public abstract float timeToPerform();

        public abstract void performMotion();

        public abstract void Reset(Motion_Primitive_Controller controller = null);
    }
    [System.Serializable]
    public class P_DriveStraight : Primitive
    {
        public float timeStarted;
        public P_DriveStraight(float distance, Motion_Primitive_Controller c)
        {
            value = distance;
            controller = c;
            Debug.Log("Hazaaaa");
            Reset();
        }

        public override void Reset(Motion_Primitive_Controller controller = null)
        {
            if (controller != null)
                this.controller = controller;

            if(this.controller!=null)
            {
                leftWheelPID = new PID(this.controller.leftWheelPID);
                rightWheelPID = new PID(this.controller.rightWheelPID);
            }
            
            leftDistance = 0;
            rightDistance = 0;
            targetDistanceRight = value;
            targetDistanceLeft = value;
            lastTime = Time.time;
            timeStarted = -1;
        }
        public override float timeToPerform()
        {
            return Mathf.Abs(value) * 1.2f;
        }

        public override void performMotion()
        {
            if (timeStarted == -1)
                timeStarted = Time.time;
            float timeTaken = Time.time-lastTime;
            lastTime = Time.time;
            float rightAngularSpeed = controller.robotTransform.InverseTransformVector(controller.rightWheelRigidbody.angularVelocity).x;
            float leftAngularSpeed = (controller.robotTransform.InverseTransformVector(controller.leftWheelRigidbody.angularVelocity)).x;

            
            float rightLinearSpeed = rightAngularSpeed * Variables.wheelRadius;
            float leftLinearSpeed = leftAngularSpeed * Variables.wheelRadius;
            leftDistance +=  leftLinearSpeed * Time.fixedDeltaTime;
            rightDistance += rightLinearSpeed * Time.fixedDeltaTime;

            if (Mathf.Abs(targetDistanceLeft - leftDistance) < controller.distanceTolerance && Mathf.Abs(targetDistanceRight - rightDistance) < controller.distanceTolerance && controller.robotRigidbody.velocity.sqrMagnitude < controller.linearSpeedTolerance * controller.linearSpeedTolerance)
            {
                controller.nextPrimitive();
                controller.leftSpeed = 0;
                controller.rightSpeed = 0;
                Debug.Log("Time used for driving forward: " + (Time.time - timeStarted));
                return;
            }

            float leftError = leftWheelPID.Update(targetDistanceLeft, leftDistance, Time.fixedDeltaTime);
            float rightError = rightWheelPID.Update(targetDistanceRight, rightDistance, Time.fixedDeltaTime);

            //Debug.Log(robotRigidbody.velocity.magnitude+", "+robotRigidbody.angularVelocity.magnitude);
            controller.leftSpeed = leftError;
            controller.rightSpeed = rightError;
        }
    }

    [System.Serializable]
    public class P_None : Primitive
    {
        public override float timeToPerform()
        {
            return 0;
        }
        public override void performMotion()
        {
            throw new System.NotImplementedException();
        }

        public override void Reset(Motion_Primitive_Controller controller = null)
        {
            throw new System.NotImplementedException();
        }
    }

    [System.Serializable]
    public class P_TurnInPlace : Primitive
    {
        public float timeStarted;
        public P_TurnInPlace(float angle, Motion_Primitive_Controller c)
        {
            value = angle;
            controller = c;
            Reset();
        }

        public override void Reset(Motion_Primitive_Controller controller = null)
        {
            if (controller != null)
                this.controller = controller;

            float distance = 0;
            if (this.controller != null)
            {
                leftWheelPID = new PID(this.controller.leftWheelPID);
                rightWheelPID = new PID(this.controller.rightWheelPID);
                distance = value * Mathf.Deg2Rad * Variables.wheelBase;
            }
            leftDistance = 0;
            rightDistance = 0;
            targetDistanceRight = -distance;
            targetDistanceLeft = distance;
            timeStarted = -1;
            lastTime = Time.time;
        }
        public override float timeToPerform()
        {
            return Mathf.Abs(value)*0.014f;
        }
        public override void performMotion()
        {
            if (timeStarted == -1)
                timeStarted = Time.time;
            float timeTaken = Time.time-lastTime;
            lastTime = Time.time;
            float rightAngularSpeed = controller.robotTransform.InverseTransformVector(controller.rightWheelRigidbody.angularVelocity).x;
            float leftAngularSpeed = (controller.robotTransform.InverseTransformVector(controller.leftWheelRigidbody.angularVelocity)).x;

            
            float rightLinearSpeed = rightAngularSpeed * Variables.wheelRadius;
            float leftLinearSpeed = leftAngularSpeed * Variables.wheelRadius;
            leftDistance +=  leftLinearSpeed * Time.fixedDeltaTime;
            rightDistance += rightLinearSpeed * Time.fixedDeltaTime;
            if (Mathf.Abs(targetDistanceLeft - leftDistance) < controller.distanceTolerance && Mathf.Abs(targetDistanceRight - rightDistance) < controller.distanceTolerance && controller.robotRigidbody.angularVelocity.sqrMagnitude < controller.angularSpeedTolerance * controller.angularSpeedTolerance)
            {
                controller.nextPrimitive();
                controller.leftSpeed = 0;
                controller.rightSpeed = 0;
                Debug.Log("Time used for rotation: " + (Time.time - timeStarted));
                return;
            }

            float leftTurnError = leftWheelPID.Update(targetDistanceLeft, leftDistance, Time.fixedDeltaTime);
            float rightTurnError = rightWheelPID.Update(targetDistanceRight, rightDistance, Time.fixedDeltaTime);
            //Debug.Log(robotRigidbody.velocity.magnitude + ", " + robotRigidbody.angularVelocity.magnitude);
            //Debug.Log(targetDistanceLeft+"\t"+leftDistance+"\t"+leftError);
            controller.leftSpeed = leftTurnError;
            controller.rightSpeed = rightTurnError;
        }
    }

    [System.Serializable]
    public class PrimitiveInspector
    {
        public enum PrimitiveType { P_DriveStraight, P_TurnInPlace, P_None };
        public PrimitiveType type;
        public float value;

        public PrimitiveInspector(PrimitiveType type, float v = 0)
        {
            this.type = type;
            value = v;
        }
    }
}






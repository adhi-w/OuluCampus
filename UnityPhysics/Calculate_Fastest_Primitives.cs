using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
namespace UnityPhysics
{
    public class Calculate_Fastest_Primitives : MonoBehaviour
{
    public Motion_Primitive_Controller controller;
    public Vector3 targetPosition;
    public Vector3 targetAngle;

    public Vector3 mousePositionOnPlane;

    public enum TargetingState { PositionSelect, RotationSelect, Ready };
    public TargetingState state;

    public Transform mouseVisualizer,positionVisualizer, lastPositionVisualizer;
    public Transform robotTransform;
    public List<PrimitiveWord> words = new List<PrimitiveWord>();
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        mousePositionOnPlane = Mouse2Plane();
        mouseVisualizer.position = mousePositionOnPlane;
        switch(state)
        {
            case TargetingState.PositionSelect:
                positionVisualizer.position = mousePositionOnPlane;
                if (Input.GetMouseButtonDown(0))
                {
                    targetPosition = mousePositionOnPlane;
                    state = TargetingState.RotationSelect;
                }
                else if(Input.GetMouseButtonDown(1))
                {
                    state = TargetingState.RotationSelect;
                }
                    
                break;
            case TargetingState.RotationSelect:
                positionVisualizer.position = targetPosition;
                Vector3 rotation = new Vector3(0, Vector3.SignedAngle(Vector3.forward, (mousePositionOnPlane - positionVisualizer.position),Vector3.up), 0);
                positionVisualizer.rotation = Quaternion.Euler(rotation);
                if (Input.GetMouseButtonDown(0))
                {
                    targetAngle = rotation;
                    state = TargetingState.Ready;
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    state = TargetingState.Ready;
                }

                break;
            case TargetingState.Ready:
                positionVisualizer.position = targetPosition;
                positionVisualizer.rotation = Quaternion.Euler(targetAngle);
                lastPositionVisualizer.position = targetPosition;
                lastPositionVisualizer.rotation = Quaternion.Euler(targetAngle);
                words = calculateFastest(targetPosition, targetAngle);
                controller.nextPrimitive(words[0].convertedPrimitives);
                state = TargetingState.PositionSelect;
                break;
        }
    }


    public Vector3 Mouse2Plane()
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        // create a ray from the mousePosition
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // plane.Raycast returns the distance from the ray start to the hit point
        float distance;
        if (plane.Raycast(ray, out distance))
        {
            // some point of the plane was hit - get its coordinates
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    public List<PrimitiveWord> calculateFastest(Vector3 position, Vector3 angle)
    {
        
        List<PrimitiveWord> words = new List<PrimitiveWord>();
        Vector3 robotPosition = robotTransform.position;
        Vector3 robotAngle = robotTransform.rotation.eulerAngles;
        Quaternion r = Quaternion.AngleAxis(angle.y, Vector3.up);
        Vector3 forward = Vector3.forward;
        forward = r * forward;
        robotAngle.x = 0;
        robotAngle.z = 0;
        Vector3 between = position - robotPosition;
        between.y = 0;
        float angleBetween = Vector3.SignedAngle(robotTransform.forward, forward, Vector3.up);
        float distanceBetween = between.magnitude;
        float angleTolerance = 0.01f;
        float distanceTolerance = 0.01f;
        angle.Normalize();
        robotAngle.Normalize();
        robotPosition.y = 0;

        if(position.Close(robotPosition, distanceTolerance) && robotTransform.forward.Close(forward, angleTolerance))
        {
            PrimitiveWord tmp = new PrimitiveWord("");
            tmp.primitives.Add(new PrimitiveInspector(PrimitiveInspector.PrimitiveType.P_None));
            tmp.Convert(controller);
            tmp.calculateTotalTime();
            words.Add(tmp);
        }
        else if(position.Close(robotPosition, distanceTolerance))
        {
            PrimitiveWord tmp = new PrimitiveWord("A");
            tmp.primitives.Add(new PrimitiveInspector(PrimitiveInspector.PrimitiveType.P_TurnInPlace,angleBetween));
            tmp.Convert(controller);
            tmp.calculateTotalTime();
            words.Add(tmp);
        }
        else if(robotTransform.forward.Close(between.normalized, angleTolerance))
        {
            PrimitiveWord tmp = new PrimitiveWord("B");
            tmp.primitives.Add(new PrimitiveInspector(PrimitiveInspector.PrimitiveType.P_DriveStraight, distanceBetween));
            if (!forward.Close(robotTransform.forward, angleTolerance))
            {
                tmp.type = "C";
                tmp.primitives.Add(new PrimitiveInspector(PrimitiveInspector.PrimitiveType.P_TurnInPlace, angleBetween));
            }
                
            tmp.Convert(controller);
            tmp.calculateTotalTime();
            words.Add(tmp);
        }
        else
        {
            PrimitiveWord tmp = new PrimitiveWord("D");
            tmp.primitives.Add(new PrimitiveInspector(PrimitiveInspector.PrimitiveType.P_TurnInPlace, Vector3.SignedAngle(robotTransform.forward, between, Vector3.up)));
            tmp.primitives.Add(new PrimitiveInspector(PrimitiveInspector.PrimitiveType.P_DriveStraight, distanceBetween));
            tmp.primitives.Add(new PrimitiveInspector(PrimitiveInspector.PrimitiveType.P_TurnInPlace, Vector3.SignedAngle(between, forward, Vector3.up)));
            tmp.Convert(controller);
            tmp.calculateTotalTime();
            words.Add(tmp);
            
            
            


            

        }

        Vector3 oppositeAngle = -robotTransform.forward;
        Debug.Log("OppositeAngle: " + oppositeAngle);
        Debug.Log("Angle: " + between.normalized);
        if (oppositeAngle.Close(between.normalized, angleTolerance) && forward.Close(between.normalized, angleTolerance))
        {
            PrimitiveWord tmp = new PrimitiveWord("E");
            tmp.primitives.Add(new PrimitiveInspector(PrimitiveInspector.PrimitiveType.P_DriveStraight, -distanceBetween / 2));
            tmp.primitives.Add(new PrimitiveInspector(PrimitiveInspector.PrimitiveType.P_TurnInPlace, 180));
            tmp.primitives.Add(new PrimitiveInspector(PrimitiveInspector.PrimitiveType.P_DriveStraight, distanceBetween / 2));
            tmp.Convert(controller);
            tmp.calculateTotalTime();
            words.Add(tmp);
        }
        else
        {

            bool found = false;

            Vector3 intersection = GetIntersectionPointCoordinates(robotPosition, robotPosition + robotTransform.forward, targetPosition, targetPosition + forward, out found);
            if (found)
            {
                PrimitiveWord tmp = new PrimitiveWord("G");
                float distance = (intersection - robotPosition).magnitude;
                distance *= Mathf.Sign(Vector3.Dot(robotTransform.forward, (intersection - robotPosition)));
                tmp.primitives.Add(new PrimitiveInspector(PrimitiveInspector.PrimitiveType.P_DriveStraight, distance));
                Debug.Log((intersection - robotPosition) + "\t" + forward + "\t" + Vector3.SignedAngle(robotTransform.forward, forward, Vector3.up));
                tmp.primitives.Add(new PrimitiveInspector(PrimitiveInspector.PrimitiveType.P_TurnInPlace, Vector3.SignedAngle(robotTransform.forward, forward, Vector3.up)));
                distance = (targetPosition - intersection).magnitude;
                distance *= Mathf.Sign(Vector3.Dot(forward, (targetPosition - intersection)));
                tmp.primitives.Add(new PrimitiveInspector(PrimitiveInspector.PrimitiveType.P_DriveStraight, distance));
                tmp.Convert(controller);
                tmp.calculateTotalTime();
                words.Add(tmp);
            }
        }

        words = words.OrderBy(w => w.totalTime).ToList();
        return words;
    }

    public Vector3 GetIntersectionPointCoordinates(Vector3 A1, Vector3 A2, Vector3 B1, Vector3 B2, out bool found)
    {
        float tmp = (B2.x - B1.x) * (A2.z - A1.z) - (B2.z - B1.z) * (A2.x - A1.x);

        if (tmp == 0)
        {
            // No solution!
            found = false;
            return Vector3.zero;
        }

        float mu = ((A1.x - B1.x) * (A2.z - A1.z) - (A1.z - B1.z) * (A2.x - A1.x)) / tmp;

        found = true;

        return new Vector3(
            B1.x + (B2.x - B1.x) * mu,0,
            B1.z + (B2.z - B1.z) * mu
        );
    }
    [System.Serializable]
    public class PrimitiveWord
    {
        public List<PrimitiveInspector> primitives;
        public List<Primitive> convertedPrimitives;
        public float totalTime;
        public string type;

        public PrimitiveWord(string t)
        {
            this.primitives = new List<PrimitiveInspector>();
            type = t;
        }

        public void Convert(Motion_Primitive_Controller c)
        {
            convertedPrimitives = c.InspectorConverter(primitives);
        }

        public void calculateTotalTime()
        {
            if(convertedPrimitives!=null)
            {
                totalTime = 0;
                foreach(Primitive p in convertedPrimitives)
                {
                    totalTime += p.timeToPerform();
                }
            }
        }
    }
}

}

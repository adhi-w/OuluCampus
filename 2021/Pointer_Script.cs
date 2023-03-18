using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Pointer_Script : MonoBehaviour
{
    public Transform leftHand, rightHand, center;
    public Transform lineTransform;
    public LineRenderer lineRenderer;

    public Vector3 positionOffset = new Vector3();
    public Transform parentTransform;
    public bool rightHanded = true;
    public bool pointingType = false;

    public GameObject line,ControllerLeft, ControllerRight;
    public bool canPoint = false;
    public bool hasPointed = false;
    public Transform pointTarget;
    public float realAngle, currentAngle, guessedAngle;
    public bool debug = false;

    string buttonName = "Oculus_CrossPlatform_Button2";
    string cancelButtonName = "Oculus_CrossPlatform_Button1";
    private IEnumerator coroutine;
    public bool VibrateBoth = false;

    public bool hasSaved = false;
    // Start is called before the first frame update
    void Start()
    {
        SetHand(rightHanded);
        line.SetActive(false);
        ControllerLeft.SetActive(false);
        ControllerRight.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //Put the pointers position into the controller + offset
        transform.position = parentTransform.TransformPoint(positionOffset);
        if(!pointingType)
        {
            //Use controller orientation for the direction
            transform.rotation = parentTransform.rotation;

            //Calculate the real angle
            Vector3 vector = pointTarget.position-transform.position;
            realAngle = Mathf.Atan2( vector.x, vector.z ) * Mathf.Rad2Deg;
            //Calculate current angle
            vector = transform.forward;
            currentAngle = Mathf.Atan2( vector.x, vector.z ) * Mathf.Rad2Deg;
        }
        else
        {
            //Take the direction from the direction of the controller from the head of the user
            Vector3 vector = transform.position - center.position;
            float angle = Mathf.Atan2( vector.x, vector.z );
            currentAngle = angle * Mathf.Rad2Deg;
            Quaternion direction = new Quaternion(0, Mathf.Sin(angle/2), 0, Mathf.Cos(angle/2));
            transform.rotation = direction;
            //Calculate the real angle
            vector = pointTarget.position-center.position;
            realAngle = Mathf.Atan2( vector.x, vector.z ) * Mathf.Rad2Deg;
        }

        if(canPoint)
        {
            if(!hasPointed)
            {
                lineTransform.position = transform.position;
                lineTransform.rotation = transform.rotation;
                lineRenderer.startColor = Color.green;
                lineRenderer.endColor = Color.green;
            
            
                if(Input.GetButtonDown(buttonName))
                {
                    
                    Debug.Log("Real Angle: " + realAngle + "\t Current Angle: " + currentAngle + "\t Difference: " + angleDistance(realAngle, currentAngle));
                    guessedAngle = angleDistance(realAngle, currentAngle);
                    if(coroutine!=null)
                        StopCoroutine(coroutine);
                    coroutine = doVibrate(rightHanded, 0.5f, 0.5f, 0.5f);
                    StartCoroutine(coroutine);
                    hasPointed = true;
                }
            }
            else
            {
                lineRenderer.startColor = Color.red;
                lineRenderer.endColor = Color.red;
                if(Input.GetButtonDown(cancelButtonName))
                {
                    hasPointed = false;
                    if(coroutine!=null)
                        StopCoroutine(coroutine);
                    coroutine = doVibrate(rightHanded, 0.5f, 0.5f, 0.25f);
                    StartCoroutine(coroutine);
                }
            }

            
        }
    }

    void OnDrawGizmos()
    {
        //Draw the real angle in editor view
        if(debug)
        {
            Vector3 direction = new Vector3(Mathf.Sin(realAngle * Mathf.Deg2Rad), 0, Mathf.Cos(realAngle * Mathf.Deg2Rad));
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, direction);
        }
    }

    public void SetHand(bool isRight = true)
    {
        //Set the handness of the user. True if right handed false if left handed
        if(isRight)
        {
            parentTransform = rightHand;
            positionOffset.x = Mathf.Abs(positionOffset.x);
            buttonName = "Oculus_CrossPlatform_Button4"; //Button2 is button A on the right controller
            cancelButtonName = "Oculus_CrossPlatform_Button3";
            rightHanded = true;
        }
        else
        {
            parentTransform = leftHand;
            positionOffset.x = -Mathf.Abs(positionOffset.x);
            buttonName = "Oculus_CrossPlatform_Button2"; //Button4 is button X on the left controller
            cancelButtonName = "Oculus_CrossPlatform_Button1";
            rightHanded = false;
        }
            
    }

    public void saveAngle(string file)
    {
        using (StreamWriter sw = new StreamWriter(file))
        {
            sw.WriteLine(guessedAngle);
        }
        hasSaved = true;
    }

    public void letPoint()
    {
        //Let the user to point the direction
        canPoint = true;
        line.SetActive(true);
        ControllerLeft.SetActive(true);
        ControllerRight.SetActive(true);
    }

    public float angleDistance(float alpha, float beta) {
        //Calculate correct difference between the angles
        float phi = Mathf.Abs(beta - alpha) % 360;       // This is either the distance or 360 - distance
        float distance = phi > 180 ? 360 - phi : phi;
        return distance;
    }

    public IEnumerator doVibrate(bool right, float freq, float ampl, float time)
    {
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.All);
        yield return null;
        OVRInput.Controller ctrl = OVRInput.Controller.RTouch;
        if(!right)
            ctrl = OVRInput.Controller.LTouch;
        if(VibrateBoth)
            ctrl = OVRInput.Controller.All;
        OVRInput.SetControllerVibration(freq, ampl, ctrl);
        yield return new WaitForSeconds(time);
        OVRInput.SetControllerVibration(0, 0, ctrl);
    }
}

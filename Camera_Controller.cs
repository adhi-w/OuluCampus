using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Controller : IController
{

    float panAngle, tiltAngle;

    public float panSpeed, tiltSpeed;

    public bool resetCameras = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Add arrow keys to target pan and tilt angles
        panAngle += (Input.GetKey(KeyCode.RightArrow) ? panSpeed : 0) - (Input.GetKey(KeyCode.LeftArrow) ? panSpeed : 0);
        tiltAngle += (Input.GetKey(KeyCode.UpArrow) ? tiltSpeed : 0) - (Input.GetKey(KeyCode.DownArrow) ? tiltSpeed : 0);

        //Invoke unity event with the desired angles
        setMotor.Invoke(0, panAngle);
        setMotor.Invoke(1, tiltAngle);
    }

    private void OnValidate()
    {
        //If resetCameras was toggled in editor view this will reset the cameras to 0
        if(resetCameras)
        {
            panAngle = 0;
            tiltAngle = 0;
            resetCameras = false;
        }
    }
}

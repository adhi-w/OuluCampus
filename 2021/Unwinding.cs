using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unwinding : MonoBehaviour
{
    public Transform parent, self;
    Quaternion previous;
    public bool _X, _Y, _Z;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //Calculate the rotation difference
        Quaternion rotation = parent.rotation * Quaternion.Inverse(previous);
        //Only disable rotation on chosen axis
        if(!_X)
            rotation.x=0;
        if(!_Y)
            rotation.y=0; 
        if(!_Z)
            rotation.z=0;
        //Normalize the quaternion and rotate to inverse direction
        rotation = Quaternion.Normalize(rotation);
        self.localRotation = self.localRotation*Quaternion.Inverse(rotation);
        previous = parent.rotation;

    }
}

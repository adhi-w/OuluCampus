using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LifeNode : MonoBehaviour {
    public float frequency = 1f;
    public bool doCycle = false;
    public virtual void init()
    {
        initialized = true;
    }
    public virtual void begin(){
        began = true;

        if(doCycle)
        {
            StartCoroutine("cycle");
        }
            
    }

    public virtual void updateCycle() {
        
    }

    public virtual IEnumerator cycle()
    {
        WaitForSeconds wait = new WaitForSeconds(1/frequency);
        while(true)
        {
            yield return wait;
            updateCycle();

        }
    }
    public bool began,initialized;
}
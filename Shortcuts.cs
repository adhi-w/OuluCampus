using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Shortcuts : MonoBehaviour
{
    public List<Shortcut> cuts = new List<Shortcut>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach(Shortcut s in cuts)
            s.Check();
    }
    [System.Serializable]
    public class Shortcut
    {
        public KeyCode key;
        public UnityEvent events;

        public Shortcut()
        {
            events = new UnityEvent();
        }

        public void Check()
        {
            if(Input.GetKeyDown(key))
            {
                events?.Invoke();
                
            }
        }
    }
}

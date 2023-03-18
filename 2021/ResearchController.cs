using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class ResearchController : MonoBehaviour
{
    public Pointer_Script pointer;
    public Tracker.Tracker headTracker;
    public Tracker.TrackingController trackingController;
    public Tracker.FollowerController followerController;
    public Unwinding unwinding;

    private Rect windowRect = new Rect(20,20,150,190);
    private string Username = "Subject";
    private bool unWindingToggle = false;
    private string fileBase, filePath;
    public bool running = false;
    public bool canSave = false; // default: false

    void Start()
    {
        
    }

    void OnGUI()
    {
        windowRect = GUI.Window(0, windowRect, WindowFunction, "Controls");
    }

    void WindowFunction(int windowID)
    {
        GUI.enabled = !running;
        Username = GUI.TextField (new Rect (5, 20, 140, 30), Username);
        unWindingToggle = GUI.Toggle(new Rect(5,55,140,30), unWindingToggle, "Unwinding");
        
        if(GUI.Button(new Rect(5,85,140,30), "Start"))
        {
            running = true;
            unwinding._Y=unWindingToggle;

            filePath = System.IO.Path.Combine(Application.persistentDataPath, Username);
            try
            {
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
            
            }
            catch (IOException ex)
            {
                Debug.LogError(ex.Message);
            }
            trackingController.basePath = Username;

            fileBase = unWindingToggle ? "UW_"+Username : "W_"+Username;
            
            headTracker.filename = fileBase + "_head_tracking.trk";
            trackingController.startTracking();
            followerController.startTracking();
            canSave = true; /// default: should be commented
        }

        GUI.enabled = canSave;
        if(GUI.Button(new Rect(5,120,140,30), "Save results"))
        {
            trackingController.stopTracking();
            trackingController.saveTracking();
            pointer.saveAngle(filePath + "/" + fileBase + "_direction.txt");
        }

        GUI.enabled = headTracker.hasSaved && pointer.hasSaved;
        if(GUI.Button(new Rect(5,155,140,30), "Exit"))
        {
            Application.Quit();
        }
        GUI.enabled = true;
    }

    public void OnApplicationQuit()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(pointer.hasPointed)
            canSave = true;
        if(Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }
}

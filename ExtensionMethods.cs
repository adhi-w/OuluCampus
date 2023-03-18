using DataModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    public static bool Close(this Vector3 vect, Vector3 target, float d, bool separate = false)
    {
        Vector3 b = target - vect;
        if (separate)
            return (b.x <= d && b.y <= d && b.z <= d);
        else
            return b.sqrMagnitude <= d * d;
    }

    public static bool Close(this float f, float target, float d)
    {
        return (target - f) <= d;
    }

    public static Vector3 toUnity(this V3 v)
    {
        return new Vector3(-(float)v.y, (float)v.z, (float)v.x); 
    }

    public static V3 toRos(this Vector3 v)
    {
        return new V3(v.z, -v.x, v.y);
    }

    public static Quaternion toUnity(this V4 v)
    {
        return new Quaternion((float)v.y, -(float)v.z, -(float)v.x, (float)v.w);
    }

    public static V4 toRos(this Quaternion q)
    {
        
        Quaternion q2 = new Quaternion(-q.z, q.x, -q.y, q.w);
        q2.Normalize();
        return new V4(q2.x,q2.y,q2.z,q2.w);
    }

    public static R_Transform toRos(this Transform t)
    {
        R_Transform r = new R_Transform();
        r.translation = t.position.toRos();
        r.rotation = t.rotation.toRos();
        return r;
    }

    public static R_Transform localToRos(this Transform t)
    {
        R_Transform r = new R_Transform();
        r.translation = t.localPosition.toRos();
        r.rotation = t.localRotation.toRos();
        return r;
    }

    public static void fromRos(this Transform t, R_Transform r)
    {
        t.position = r.translation.toUnity();
        t.rotation = r.rotation.toUnity();
    }
} 

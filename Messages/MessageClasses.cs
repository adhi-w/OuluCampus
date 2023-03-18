using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DataModels;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.AI;

/*ADDING A NEW DATATYPE
 1. add data model to DataModels namespace
 2. add a data preset to DataPresets
 3. append it to the 'all' list
 4. Add a new named argument to the Cast method with name of the preset but with the first letter capitalized
 5. Either copy the if template or use the attached python script
 */


[System.Serializable]
public class PoseWithCovariance
{
    public Ros_Pose pose;
    public double[] covariance;

    public PoseWithCovariance()
    {
        covariance = new double[36];
        pose = new Ros_Pose();
    }
}

[System.Serializable]
public class Ros_Pose
{
    public V3 position;
    public V4 orientation;

    public Ros_Pose()
    {
        position = new V3();
        orientation = new V4();
    }
    
}

[System.Serializable]
public class TwistWithCovariance
{
    public Ros_Twist twist;
    public double[] covariance;

    public TwistWithCovariance()
    {
        covariance = new double[36];
        twist = new Ros_Twist();
    }
}

[System.Serializable]
public class Ros_Twist
{
    public V3 linear;
    public V3 angular;

    public Ros_Twist()
    {
        linear = new V3();
        angular = new V3();
    }
}
[System.Serializable]
public class RegionOfInterest
{
    public int x_offset;
    public int y_offset;
    public int height;
    public int width;
    public bool do_rectify;
    public RegionOfInterest()
    {

    }
}
[System.Serializable]
public class V3
{
    public double x, y, z;

    public V3(float xx, float yy, float zz)
    {
        x = (double)xx;
        y = (double)yy;
        z = (double)zz;
    }
    public V3()
    {

    }
}
[System.Serializable]
public class V4
{
    public double x, y, z, w;
    public V4(float xx, float yy, float zz, float ww)
    {
        x = (double)xx;
        y = (double)yy;
        z = (double)zz;
        w = (double)ww;
    }

    public V4()
    {
        w = 1;
    }

    public override string ToString()
    {
        return "(" + this.x + "," + this.y + "," + this.z + "," + this.w + ")";
    }
}
[System.Serializable]
public class Header
{

    public TimeStamp stamp;
    public string frame_id;

    public Header()
    {
        
    }
}
[System.Serializable]
public class TimeStamp
{
    public int sec;
    public int nsec;

    public TimeStamp()
    {
        sec = 0;
        nsec = 0;
    }
    public static TimeStamp now()
    {
        TimeStamp toReturn = new TimeStamp();
        toReturn.sec = Mathf.FloorToInt(Time.time);
        toReturn.nsec = Mathf.FloorToInt((Time.time % 1) * 1000000000);
        return toReturn;
    }
}
[System.Serializable]
public class Duration
{
    public int sec;
    public int nanosec;

    public Duration()
    {
        this.sec = 0;
        this.nanosec = 0;
    }

}
[System.Serializable]
public class R_Transform
{
    public V3 translation;
    public V4 rotation;

    public R_Transform()
    {
        translation = new V3();
        rotation = new V4();
    }
}

[System.Serializable]
public class TransformStamped
{
    public Header header;
    public R_Transform transform;
    public string child_frame_id;

    public TransformStamped()
    {
        header = new Header();
        transform = new R_Transform();
    }
}

namespace DataModels
{
    
    
    [System.Serializable]
    public class TwistData : DataModel
    {
        public V3 linear, angular;
    }
    [System.Serializable]
    public abstract class DataModel
    {
        
    }
    [System.Serializable]
    public class LaserData : DataModel
    {
        public Header header;// timestamp in the header is the acquisition time of 
                             // the first ray in the scan.
                             //
                             // in frame frame_id, angles are measured around 
                             // the positive Z axis (counterclockwise, if Z is up)
                             // with zero angle being forward along the x axis
        public float angle_min;
        public float angle_max;
        public float angle_increment;
        public float time_increment;
        public float scan_time;
        public float range_min;
        public float range_max;
        public float[] ranges;
        public float[] intensities;
        
    }
    [System.Serializable]
    public class StringData : DataModel
    {
        public string data;
    }
    [System.Serializable]
    public class ImageData : DataModel
    {
        public Header header; //Header frame_id should be optical frame of camera
                             // origin of frame should be optical center of camera
                             // +x should point to the right in the image
                             // +y should point down in the image
                             // +z should point into to plane of the image
                             // If the frame_id here and the frame_id of the CameraInfo
                             // message associated with the image conflict
                             // the behavior is undefined

        public int height;
        public int width;

        public string encoding; //taken from the list of strings in include/sensor_msgs/image_encodings.h

        public byte is_bigendian;
        public int step; //Full row length in bytes
        public byte[] data;


        public void setData(Texture2D tex)
        {
            encoding = "rgba8";
            step = tex.width;
            width = tex.width;
            height = tex.height;
            data = tex.GetRawTextureData();
            is_bigendian = 1;
        }
    }

    [System.Serializable]
    public class CameraInfoData : DataModel
    {
        public Header header;
        public int height;
        public int width;

        public string distortion_model = "plumb_bob";

        double[] D = { 0, 0, 0, 0, 0 };

        double[] R = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        double[] P = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public int binning_x = 0;
        public int binning_y = 0;

        public RegionOfInterest roi;
    }

    public class TFMessageData : DataModel
    {
        public TransformStamped[] transforms;
        public TFMessageData()
        {
            transforms = new TransformStamped[1];
        }
    }

    public class ImuData : DataModel
    {
        public Header header;
        public V4 orientation;
        public double[] orientation_covariance;
        public V3 angular_velocity;
        public double[] angular_velocity_covariance;
        public V3 linear_acceleration;
        public double[] linear_acceleration_covariance;

        public ImuData()
        {
            header = new Header();
            orientation = new V4();
            orientation_covariance = new double[9];

            angular_velocity = new V3();
            angular_velocity_covariance = new double[9];

            linear_acceleration = new V3();
            linear_acceleration_covariance = new double[9];

        }
    }

    public class OdometryData : DataModel
    {
        public Header header;
        public string child_frame_id;
        public PoseWithCovariance pose;
        public TwistWithCovariance twist;

        public OdometryData()
        {
            header = new Header();
            pose = new PoseWithCovariance();
            twist = new TwistWithCovariance();
        }
    }
    
    public class PoseWithCovarianceStampedData : DataModel
    {
        public Header header;
        public PoseWithCovariance pose;

        public PoseWithCovarianceStampedData()
        {
            header = new Header();
            pose = new PoseWithCovariance();
        }
    }

    public class PoseStampedData : DataModel
    {
        public Header header;
        public Ros_Pose pose;

        public PoseStampedData(){
            header = new Header();
            pose = new Ros_Pose();
        }
    }

    public class ClockData : DataModel
    {
        public TimeStamp time;

        public ClockData()
        {
            time = new TimeStamp();
        }
    }

    public class PathData : DataModel
    {
        public Header header;
        public PoseStampedData[] poses;

        public PathData()
        {
            header = new Header();
            poses = new PoseStampedData[1];
        }
    }
    [System.Serializable]
    public class Analyst : DataModel
    {
        public double total_cost;
        public double total_length;
        public Duration total_time;

        public Analyst()
        {
            total_cost = 0;
            total_length = 0;
            total_time = new Duration();
        }
    }
    [System.Serializable]
    public class Joy : DataModel
    {
        public Header header;
        public float[] axes;
        public int[] buttons;

        public Joy()
        {
            header = new Header();
            axes = new float[2];
            buttons = new int[1];
        }
    }

    

}

public class DataPresets
{
    public static MessageMetadata laserData = new MessageMetadata(
        "LaserScan", "publish",
        typeof(LaserData)
        
        );
    public static MessageMetadata twistData = new MessageMetadata(
        "Twist", "publish",
        typeof(TwistData)
      
    );
    public static MessageMetadata stringData = new MessageMetadata(
        "String", "publish",
        typeof(StringData)
    );

    public static MessageMetadata imageData = new MessageMetadata(
        "Image", "publish",
        typeof(ImageData)
        );

    public static MessageMetadata cameraInfoData = new MessageMetadata(
        "CameraInfo", "publish",
        typeof(CameraInfoData)
        );

    public static MessageMetadata TFMessage = new MessageMetadata(
        "TFMessage", "publish",
        typeof(TFMessageData)
        );

    public static MessageMetadata Imu = new MessageMetadata(
        "Imu", "publish",
        typeof(ImuData)
        );
    public static MessageMetadata Odometry = new MessageMetadata(
        "Odometry", "publish",
        typeof(OdometryData)
        );
    public static MessageMetadata PoseWithCovarianceStamped = new MessageMetadata(
        "PoseWithCovarianceStamped", "publish",
        typeof(PoseWithCovarianceStampedData)
        );
    public static MessageMetadata clockData = new MessageMetadata(
        "Clock", "subscription",
        typeof(ClockData)
    );

    public static MessageMetadata poseStampedData = new MessageMetadata(
        "PoseStamped", "publish",
        typeof(ClockData)
    );

    public static MessageMetadata pathData = new MessageMetadata(
        "Path", "publish",
        typeof(PathData)
    );

    public static MessageMetadata analystData = new MessageMetadata(
        "Analyst", "publish",
        typeof(Analyst)
    );
    public static MessageMetadata joyData = new MessageMetadata(
        "Joy", "publish",
        typeof(Joy)
    );
    public static List<MessageMetadata> all = new List<MessageMetadata>(){laserData,twistData,stringData, imageData, cameraInfoData, TFMessage, Imu, Odometry, PoseWithCovarianceStamped, clockData, poseStampedData, pathData, analystData, joyData };
}
public class MessageMetadata{
    public string type;
    public string command;
    public Type dataModel;
    public Func<string, DataModel> fromJson;
    public MessageMetadata(string type, string command,Type dataModelType){
        this.type=type;
        this.dataModel = dataModelType;
        this.fromJson = this.fromJson;
        this.command=command;
    }
}



public class RosMessage{
  
    [System.Serializable]
    class SerializationModel
    {
        
        public JObject data;
        public string type;
        public string command;
        public string topic;
    }


  
    public void Cast(Action<LaserData> LaserData = null, Action<TwistData> TwistData =null, Action<ClockData> ClockData = null, Action<PathData> PathData = null, Action other = null)
    {
        /*
         python one-liner script for generating these. Paste directly into terminal:
         
python3 -c "print('\n'.join([\
'if ({0} != null && metadata.dataModel == DataPresets.{1}.dataModel)\
{0}(data as {0});'.format(x,x[0].lower() + x[1:])\
for x in ['LaserData', 'TwistData']]))" 
         
         */
        
        if (LaserData != null && metadata.dataModel == DataPresets.laserData.dataModel)LaserData(data.ToObject<LaserData>());
        if (TwistData != null && metadata.dataModel == DataPresets.twistData.dataModel)TwistData(data.ToObject<TwistData>());
        if (ClockData != null && metadata.dataModel == DataPresets.clockData.dataModel)ClockData(data.ToObject<ClockData>());
        if (PathData != null && metadata.dataModel == DataPresets.pathData.dataModel)PathData(data.ToObject<PathData>());
        if (other!=null) other();




    }

    public T cast<T>()
    {
        return data.ToObject<T>();
    }
    
    protected JObject data;
    public string topic = "";
    private MessageMetadata metadata;
    public string type{
        get { return metadata.type; }
    }

   
    public string command{
        get {return metadata.command; }
        set {metadata.command = value;}
    }

  
  
    public RosMessage(MessageMetadata metadataPreset, JObject data){
        this.metadata=metadataPreset;
        this.data = data;
    }
    public RosMessage(MessageMetadata metadataPreset, dynamic data){
        this.metadata=metadataPreset;
        this.data = JObject.FromObject(data);
    }
    private RosMessage(string type, string command, string topic)
    {
        this.metadata = new MessageMetadata(type,command, null);
        this.topic = topic;
    }

    public static RosMessage AddPublisher(string type, string topic)
    {
        return new RosMessage(type, "add_publisher", topic);
    }

    public void ApplyData(dynamic data)
    {
        this.data = JObject.FromObject(data);
    }
    public static RosMessage AddSubscriber(string type, string topic)
    {
        return new RosMessage(type, "add_subscriber", topic);
    }

    

    public static RosMessage Spin()
    {
        return new RosMessage("", "spin", "");
    }
    public string ToJson()
    {
        var tmp = new SerializationModel(){command = this.command, data = this.data, type = this.type, topic = this.topic};
        return JsonConvert.SerializeObject(tmp);
    }

    public static RosMessage FromJson(string json)
    {
       var i= JsonConvert.DeserializeObject<SerializationModel>(json);
       var preset = DataPresets.all.First(d => d.type == i.type);
       var u= new RosMessage(preset, i.data);

       u.command = i.command;
       u.topic = i.topic;
       return u;
    }

    public static void Test()
    {
        var jsonString =
            @"{""type"":""LaserScan"", ""command"":""publish"", ""topic"":""exampleTopic"", ""data"":{""angle_min"":12.5}}";
        var msg = FromJson(jsonString);
        msg.Cast(laserData => { Debug.Log("laser data ineed: " + laserData.angle_min.ToString()); });
        Debug.Log(msg.ToJson());


    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public class SaveAndLoad
{
    public static LogData Load(int id)
    {
        LogData data;

        if (File.Exists(Application.persistentDataPath + "/" + id + ".dat"))
        {
            try
            {
                using (Stream stream = File.OpenRead(Application.persistentDataPath + "/" + id + ".dat"))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    data = (LogData)formatter.Deserialize(stream);
                }

                Debug.Log("Loaded data for ID: " + id);

                return data;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
        else
        {
            Debug.Log("No data for ID: " + id);
        }

        return null;
    }

    public static void Save(int id, LogData data)
    {
        using (Stream stream = File.OpenWrite(Application.persistentDataPath + "/" + id + ".dat"))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, data);

            Debug.Log("Saved data for ID: " + id);
        }
    }
}

[Serializable]
public class LogData
{
    [Serializable]
    public class MotionInfo
    {
        float _time;
        public float Time { get { return _time; } }
        float[] _position;
        public Vector3 Position { get { return new Vector3(_position[0], _position[1], _position[2]); } }
        float[] _rotation;
        public Quaternion Rotation { get { return new Quaternion(_rotation[0], _rotation[1], _rotation[2], _rotation[3]); } }

        public MotionInfo(float time, Vector3 position, Quaternion rotation)
        {
            _time = time;

            _position = new float[3] { position.x, position.y, position.z };

            _rotation = new float[4] { rotation.x, rotation.y, rotation.z, rotation.w };
        }
    }

    List<MotionInfo> _motionInfos = new List<MotionInfo>();
    public MotionInfo[] MotionInfos { get { return _motionInfos.ToArray(); } }

    public void AddMotionInfo(float time, Vector3 position, Quaternion rotation)
    {
        _motionInfos.Add(new MotionInfo(time, position, rotation));
    }
}
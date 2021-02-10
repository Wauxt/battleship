using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SavePatternSystem : MonoBehaviour
{
    public void Save()
    {
        int i = 0;
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath
          + "/MySaveData.dat");
        SavePattern data = new SavePattern();
        data.position[i] = new SVector3();
        data.rotation[i] = new SQuaternion();
        GameObject tr;
        
        for (i = 0; i < 10; i++)
        {
            tr = GameObject.Find("Ship" + i.ToString());
            data.position[i] = tr.transform.localPosition;
            data.rotation[i] = tr.transform.localRotation;
        }
        bf.Serialize(file, data);
        file.Close();
        Debug.Log("Game data saved!");        
    }

    public void Load()
    {
        int i = 0;
        if (File.Exists(Application.persistentDataPath
    + "/MySaveData.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file =
              File.Open(Application.persistentDataPath
              + "/MySaveData.dat", FileMode.Open);
            SavePattern data = (SavePattern)bf.Deserialize(file);
            file.Close();
            GameObject tr;            

            for (i = 0; i < 10; i++)
            {
                tr = GameObject.Find("Ship" + i.ToString());
                tr.transform.localPosition = new Vector3(data.position[i].x, data.position[i].y, data.position[i].z);
                tr.transform.localRotation = new Quaternion(0, data.rotation[i].y, 0, data.rotation[i].w);
                tr.transform.localScale = new Vector3(1f, 0.4f, 1f);
            }

            Debug.Log("Game data loaded!");
        }
        else
            Debug.Log("There is no save data!");
    }

    [Serializable]
    class SavePattern
    {
        public SVector3[] position;
        public SQuaternion[] rotation;

        public SavePattern()
        {
            position = new SVector3[10];
            rotation = new SQuaternion[10];
        }
    }

    [System.Serializable]
    public struct SVector3
    {
        public float x;
        public float y;
        public float z;
        public SVector3(float rX, float rY, float rZ)
        {
            x = rX;
            y = rY;
            z = rZ;
        }
        public override string ToString()
        {
            return String.Format("[{0}, {1}, {2}]", x, y, z);
        }
        public static implicit operator Vector3(SVector3 rValue)
        {
            return new Vector3(rValue.x, rValue.y, rValue.z);
        }
        public static implicit operator SVector3(Vector3 rValue)
        {
            return new SVector3(rValue.x, rValue.y, rValue.z);
        }
    }

    [System.Serializable]
    public struct SQuaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;
        public SQuaternion(float rX, float rY, float rZ, float rW)
        {
            x = rX;
            y = rY;
            z = rZ;
            w = rW;
        }
        public override string ToString()
        {
            return String.Format("[{0}, {1}, {2}, {3}]", x, y, z, w);
        }
        public static implicit operator Quaternion(SQuaternion rValue)
        {
            return new Quaternion(rValue.x, rValue.y, rValue.z, rValue.w);
        }
        public static implicit operator SQuaternion(Quaternion rValue)
        {
            return new SQuaternion(rValue.x, rValue.y, rValue.z, rValue.w);
        }
    }
}
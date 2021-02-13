using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using SFB;

public class SavePatternSystem : MonoBehaviour
{
    [SerializeField]
    private GameObject modalPanel = null;

    [SerializeField]
    private ShipsGrid shipsGrid = null;

    public void Save()
    {
        int i = 0;
        BinaryFormatter bf = new BinaryFormatter();
        
        var extensionList = new[] {
            new ExtensionFilter("Data", "dat"),
        };
        var path = StandaloneFileBrowser.SaveFilePanel(" Сохранить файл ", "C:\\Users\\user\\Desktop\\mysave", "MySave",extensionList);
        if (path.Length != 0)
        {
            FileStream file = File.Create(path);
            SavePattern data = new SavePattern();
            data.position[i] = new SVector3();
            data.rotation[i] = new SQuaternion();
            GameObject tr;

            for (i = 0; i < 10; i++)
            {
                tr = shipsGrid.transform.GetChild(i).gameObject;

                data.position[i] = tr.transform.localPosition;
                data.rotation[i] = tr.transform.localRotation;
            }
            bf.Serialize(file, data);
            file.Close();

        }
        Debug.Log("Game data saved!");        
    }

    public void Load()
    {
        var extensions = new[] {
            new ExtensionFilter("Data", "dat"),
        };
        var path = StandaloneFileBrowser.OpenFilePanel("Open File", "C:\\Users\\user\\Desktop\\", extensions, false);
        if (path.Length!=0)
        {            
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file =
            File.Open(path[0], FileMode.Open);
            try
            {
                SavePattern data = (SavePattern)bf.Deserialize(file);
                GameObject tr;
                
                for (int i = 0; i < 10; i++)
                {                    
                    tr = shipsGrid.transform.GetChild(i).gameObject;
                    tr.transform.localPosition = new Vector3(data.position[i].x, data.position[i].y, data.position[i].z);
                    tr.transform.localRotation = new Quaternion(0, data.rotation[i].y, 0, data.rotation[i].w);
                    if (data.position[i].x >= 0 && data.position[i].x <= 9.55f)
                        tr.transform.localScale = new Vector3(1f, 0.4f, 1f);
                    else
                        tr.transform.localScale = new Vector3(0.5f, 0.2f, 0.5f);
                }
                if (!PlacementIsLegit())
                {
                    modalPanel.SetActive(true);
                    shipsGrid.gameObject.transform.GetChild(0).gameObject.GetComponent<Ship>().DeleteAllShips();
                }
                else
                {
                    shipsGrid.SwitchReadyState();
                }
            }
            catch(Exception e)
            {
                modalPanel.SetActive(true);
                Debug.Log(e.StackTrace);
            }
            file.Close();
        }
    }

    public void Modal_Panel() 
    {
        //GameObject.Find("Modal Panel").SetActive(false);
        modalPanel.SetActive(false);
    }

    /// <summary>
    public bool PlacementIsLegit()
    {
        for (int i = 0; i < 10; i++)
        {
            Ship curShip = shipsGrid.transform.GetChild(i).gameObject.GetComponent<Ship>();
            for (int j = 0; j < curShip.deckAmount; j++)
            {
                if (curShip.DeckOverlappsAnyOtherShip(curShip.gameObject.transform.GetChild(j)))
                {
                    return false;
                }
            }
        }

        return true;
    }
    /// </summary>

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
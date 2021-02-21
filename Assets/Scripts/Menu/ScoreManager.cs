using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;
public class ScoreManager : MonoBehaviour
{
    [SerializeField] private GameObject scoreList = null;
    [SerializeField] private GameObject scoreEntryPrefab = null;

    public const string privateCode = "Bkbb-mZf30CUQHzbrVt-5ghz58on7kQ0SF6kLdpGa99g";
    public const string publicCode = "603126ed8f40bb39ec1ed838";
    public const string webURL = "http://dreamlo.com/lb/";

    void OnEnable()
    {
        foreach (Transform child in scoreList.transform)
        {
            Destroy(child.gameObject);
        }
    }    

    public void GetLeaderboard()
    {
        foreach (Transform child in scoreList.transform)
        {
            Destroy(child.gameObject);
        }
        StartCoroutine(GetScores());
    }

    IEnumerator GetScores()
    {
        WWW get = new WWW(webURL + publicCode + "/pipe/");

        yield return get;

        if (string.IsNullOrEmpty(get.error))
        {
            print(get.text);
            ShowEntries(get.text);
        }
        else
        {
            print("error. " + get.error);
        }
    }

    void ShowEntries(string textStream)
    {
        string[] entries = textStream.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (string entry in entries)
        {
            string[] entryInfo = entry.Split(new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);

            string name = entryInfo[0];

            int winCount = int.Parse(entryInfo[1]);
            int matchCount = int.Parse(entryInfo[2]);

            int hitCount = int.Parse(entryInfo[3]);
            int shotCount = int.Parse(entryInfo[4]);

            GameObject entryObject = Instantiate(scoreEntryPrefab, scoreList.transform);

            entryObject.transform.Find("Name").GetComponent<Text>().text = name;
            entryObject.transform.Find("Winrate").GetComponent<Text>().text = ((double)winCount * 100 / (double)matchCount).ToString("0.00") + "%";
            entryObject.transform.Find("Accuracy").GetComponent<Text>().text = ((double)hitCount * 100 / (double)shotCount).ToString("0.00") + "%";

        }
    }

    //        To send:
    //              UnityWebRequest www = new UnityWebRequest(webURL + privateCode + "/add/" + UnityWebRequest.EscapeURL(username) + "/" + score);
    //              yield return www.SendWebRequest();


    //        To receive:
    //              UnityWebRequest www = new UnityWebRequest(webURL + publicCode + "/pipe/");
    //              www.downloadHandler = new DownloadHandlerBuffer();
    //              yield return www.SendWebRequest();
    //              print(www.downloadHandler.text);

}

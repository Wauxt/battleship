using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScoreList : MonoBehaviour
{
    public GameObject playerScoreEntryPrefab;
    ScoreManager scoreManager;
    int lastChangeCounter;
    public string sortingScoreType = "winRate";
    void Start()
    {
        scoreManager = GameObject.FindObjectOfType<ScoreManager>();
        lastChangeCounter = scoreManager.GetChangeCounter();
        DummyChangeIfNotEmpty();
    }
    void DummyChangeIfNotEmpty()
    {
        string[] names = scoreManager.GetPlayerNames();
        if (names != null && names.Length > 0)
            scoreManager.ChangeScore(names[0], "winRate", 0);
    }
    public void setSortByName()
    {
        sortingScoreType = "name";
        DummyChangeIfNotEmpty();
    }
    public void setSortByWinRate()
    {
        sortingScoreType = "winRate";
        DummyChangeIfNotEmpty();
    }
    public void setSortByAccuracy()
    {
        sortingScoreType = "accuracy";
        DummyChangeIfNotEmpty();
    }

    // Update is called once per frame
    void Update()
    {
        if (scoreManager == null)
        {
            return;
        }
        if (scoreManager.GetChangeCounter() == lastChangeCounter)
        {
            return;
        }

        lastChangeCounter = scoreManager.GetChangeCounter();

        while (this.transform.childCount > 0)
        {
            Transform c = this.transform.GetChild(0);
            c.SetParent(null);
            Destroy(c.gameObject);
        }

        string[] names = scoreManager.GetPlayerNames(sortingScoreType);
        foreach (string name in names)
        {
            GameObject go = (GameObject)Instantiate(playerScoreEntryPrefab);
            go.transform.SetParent(this.transform);
            go.transform.Find("Name").GetComponent<Text>().text = name;
            go.transform.Find("Winrate").GetComponent<Text>().text = (scoreManager.GetScore(name, "winRate") * 100f).ToString() + "%";
            go.transform.Find("Accuracy").GetComponent<Text>().text = scoreManager.GetScore(name, "accuracy").ToString();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class ScoreManager : MonoBehaviour
{
    Dictionary<string, Dictionary<string, float> > playerScores;
    int changeCounter = 0;    
    //db implementation incoming i guess;

    void Start()
    {
        SetScore("wauxt", "winRate", 0.5f);
        SetScore("wauxt", "accuracy", 0.173f);

        SetScore("sworduck", "winRate", 0.36f);
        SetScore("sworduck", "accuracy", 0.18f);

        SetScore("sproof", "winRate", 0.5f);
        SetScore("sproof", "accuracy", 0.21f);        
    }
    public void AddADummyEntry()
    {
        Random random = new Random();
        int length = Random.Range(5, 10);
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefjklmnopqrstuvwxyz";
        string name = new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Range(0,s.Length-1)]).ToArray());
        float winRate = Random.Range(0f, 0.3f);
        float accuracy = Random.Range(0.05f, 0.30f);

        SetScore(name, "winRate", winRate);
        SetScore(name, "accuracy", accuracy);
    }
    public void ClearDummyScores()
    {
        playerScores.Clear();
        Start();
    }
    void Init()
    {
        if (playerScores != null)
            return;

        playerScores = new Dictionary<string, Dictionary<string, float>>();
    }
    public float GetScore(string username, string scoreType)
    {
        Init();

        

        if (playerScores.ContainsKey(username) == false)
        {
            return 0;
        }
        if (playerScores[username].ContainsKey(scoreType) == false)
        {
            return 0;
        }
        return playerScores[username][scoreType];
    }
    public void SetScore(string username, string scoreType, float value)
    {
        Init();

        changeCounter++;

        if (playerScores.ContainsKey(username) == false)
        {
            playerScores[username] = new Dictionary<string, float>();
        }
        playerScores[username][scoreType] = value;
    }
    public void ChangeScore(string username, string scoreType, float amount)
    {
        Init();
        float currScore = GetScore(username, scoreType);
        SetScore(username, scoreType, currScore + amount);
    }
    public string[] GetPlayerNames()
    {
        Init();
        return playerScores.Keys.ToArray();
    }
    public string[] GetPlayerNames(string sortingScoreType)
    {
        Init();
        
        return playerScores.Keys.OrderByDescending(n => GetScore(n, sortingScoreType)).ToArray();
    }
    public int GetChangeCounter()
    {
        return changeCounter;
    }

   
}

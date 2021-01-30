using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class NetworkGamePlayer : NetworkBehaviour
{
    

    [SyncVar]
    public string displayName = "Загрузка...";

    private int playerNumber = 0;       

    private NetworkManagerBS room;
    
    private NetworkManagerBS Room
    {
        get
        {
            if (room != null)
            {
                return room;
            }
            return room = NetworkManager.singleton as NetworkManagerBS;
        }
    }    

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);
        Room.GamePlayers.Add(this);
        NetworkManagerBS.OnIngameDisconnect += IngameDisconnect;
        playerNumber = Room.GamePlayers.Count == 1 ? 1 : Room.GamePlayers.Count == 2 ? 2 : 0;
        
    }
    public void IngameDisconnect()
    {        
        if (isServer)
        {
            Room.StopHost();
        }
        else
        {
            Room.StopClient();
        }
        SceneManager.LoadScene("Menu");
    }

    public override void OnStopClient()
    {
        Room.GamePlayers.Remove(this); 
        // clear() or remove(this) ????      
    }


    [Server]
    public void SetDisplayName(string name)
    {
        displayName = name;
    }
}

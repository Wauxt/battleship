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
    [SyncVar] public string displayName = "Загрузка...";
    [SyncVar] public bool placementIsReady = false;

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
        playerNumber = Room.GamePlayers.Count == 1 ? 1 : Room.GamePlayers.Count == 2 ? 2 : 0;
    }

    public override void OnStopClient()
    {
        Room.GamePlayers.Remove(this);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
    }

    [Server]
    public void SetDisplayName(string name) => displayName = name;

    [Command]
    public void CmdReadyUp()
    {        
        placementIsReady = true;        
        StartCoroutine(UpdateStuff(1f));        
    }

    private IEnumerator UpdateStuff(float count)
    {
        yield return new WaitForSeconds(count);
        GameObject.Find("GameManager").GetComponent<OnlineGameManager>().RpcUpdateLoadingRings();
    }

}

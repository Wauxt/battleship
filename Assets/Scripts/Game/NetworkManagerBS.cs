using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using Mirror;

public class NetworkManagerBS : NetworkManager
{
    [Header("Room")]
    [SerializeField] private NetworkRoomPlayer roomPlayerPrefab = null;

    [Header("Game")]
    [SerializeField] private NetworkGamePlayer gamePlayerPrefab = null;
    [SerializeField] private GameObject playerSpawnSystem = null;

    private int minPlayers = 2;


    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action OnClientHostedTheServer;
    
    //public static event Action OnIngameDisconnect;

    public static event Action<NetworkConnection> OnServerReadied;

    public List<NetworkRoomPlayer> RoomPlayers { get; } = new List<NetworkRoomPlayer>();
    public List<NetworkGamePlayer> GamePlayers { get; } = new List<NetworkGamePlayer>();


    public override void OnStartServer()
    {
        spawnPrefabs.Clear();
        spawnPrefabs = Resources.LoadAll<GameObject>("NetworkIdentified").ToList();
    }
    public override void OnStopServer() => RoomPlayers.Clear();

    public override void OnStartClient()
    {
        spawnPrefabs.Clear();
        spawnPrefabs = Resources.LoadAll<GameObject>("NetworkIdentified").ToList();
        foreach (var prefab in spawnPrefabs)
        {
            ClientScene.RegisterPrefab(prefab);
        }
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        if (numPlayers <= 1)
        {
            OnClientHostedTheServer?.Invoke();
        }
        else
        {
            OnClientConnected?.Invoke();
        }
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        OnClientDisconnected?.Invoke();
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        if (numPlayers >= maxConnections)
        {
            conn.Disconnect();
            return;
        }
    }
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (conn.identity != null)
        {
            var player = conn.identity.GetComponent<NetworkRoomPlayer>();
            RoomPlayers.Remove(player);

            NotifyPlayersOfReadyState();
        }

        base.OnServerDisconnect(conn);
    }

    public void NotifyPlayersOfReadyState()
    {
        foreach (var player in RoomPlayers)
        {
            player.HandleReadyToStart(IsReadyToStart());
        }
    }

    private bool IsReadyToStart()
    {
        if (numPlayers < minPlayers)
        {
            return false;
        }
        foreach (var player in RoomPlayers)
        {
            if (!player.IsReady)
            {
                return false;
            }
        }
        return true;
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        bool isLeader = RoomPlayers.Count == 0;

        NetworkRoomPlayer roomPlayerInstance = Instantiate(roomPlayerPrefab);
        roomPlayerInstance.IsLeader = isLeader;
        NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
    }

    public void StartGame()
    {
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            if (!IsReadyToStart()) { return; }

            ServerChangeScene("Game_PVP");
        }
    }

    public override void ServerChangeScene(string newSceneName)
    {
        //yet only from menu to game
        if (SceneManager.GetActiveScene().name == "Menu" && newSceneName == "Game_PVP")
        {
            for (int i = RoomPlayers.Count - 1; i >= 0; i--)
            {
                var conn = RoomPlayers[i].connectionToClient;
                var gameplayerInstance = Instantiate(gamePlayerPrefab);
                gameplayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);

                NetworkServer.Destroy(conn.identity.gameObject);
                NetworkServer.ReplacePlayerForConnection(conn, gameplayerInstance.gameObject, true);
            }
        }

        base.ServerChangeScene(newSceneName);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName == "Game_PVP")
        {
            GameObject playerSpawnSystemInstance = Instantiate(playerSpawnSystem);            
            NetworkServer.Spawn(playerSpawnSystemInstance);            
        }
    }

    public void IngameDisconnect()
    {
        if (mode == NetworkManagerMode.ClientOnly)
        {
            StopClient();
        }
        else if(mode == NetworkManagerMode.Host)
        {
            StopHost();
        }
        Destroy(gameObject);
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);
        OnServerReadied?.Invoke(conn);
    }

}

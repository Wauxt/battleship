using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;

public class OnlineGameManager : NetworkBehaviour
{
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

    [Header("Players")]
    [SerializeField] private GameObject ownPlayer = null;
    [SerializeField] private GameObject opponentPlayer = null;
    [SerializeField] private GameObject ownGamePlayer = null;
    [SerializeField] private GameObject opponentGamePlayer = null;

    [Header("Terrain")]
    [SerializeField] private GameObject ownTerrain = null;
    [SerializeField] private GameObject opponentTerrain = null;

    [Header("Grids")]
    [SerializeField] private GameObject ownGrid = null;
    [SerializeField] private GameObject opponentGrid = null;

    [Header("UI")]
    [SerializeField] private GameObject ownCanvas = null;
    [SerializeField] private GameObject opponentCanvas = null;
    [SerializeField] private GameObject sharedCanvas = null;

    
    public override void OnStartClient()
    {
        sharedCanvas = GameObject.Find("SharedCanvas");

        if (isServer && isClient) // host/left/player01
        {
            Debug.Log("\"Hello! I'm the host (player_01, AKA The Left Player)\" - said the OGM");

            ownGrid = GameObject.Find("Grid_01");
            opponentGrid = GameObject.Find("Grid_02");

            ownCanvas = GameObject.Find("Canvas_01");
            opponentCanvas = GameObject.Find("Canvas_02");

            ownTerrain = GameObject.Find("Terrain_01");
            opponentTerrain = GameObject.Find("Terrain_02");

            sharedCanvas.transform.Find("Coords").Find("Right").gameObject.SetActive(false);
            sharedCanvas.transform.Find("LoadingRings").Find("Left").gameObject.SetActive(false);
        }
        else if (isClientOnly) // // clientonly/right/player02
        {           
            Debug.Log("\"Hello! I'm the client (player_02, AKA The Right Player)\" - said the OGM");

            ownGrid = GameObject.Find("Grid_02");
            opponentGrid = GameObject.Find("Grid_01");

            ownCanvas = GameObject.Find("Canvas_02");
            opponentCanvas = GameObject.Find("Canvas_01");

            ownTerrain = GameObject.Find("Terrain_02");
            opponentTerrain = GameObject.Find("Terrain_01");

            sharedCanvas.transform.Find("Coords").Find("Left").gameObject.SetActive(false);
            sharedCanvas.transform.Find("LoadingRings").Find("Right").gameObject.SetActive(false);
        }

        /////////
        
        GameObject[] players = new GameObject[2];
        players = GameObject.FindGameObjectsWithTag("GamePlayer");

        foreach (GameObject player in players)
        {
            if (player.GetComponent<NetworkIdentity>().hasAuthority)
            {
                ownGamePlayer = player;
            }
            else
            {
                opponentGamePlayer = player;
            }
        }

        // destroy opponent's ships on this client (it is possible, that the grid itself wont be neccessary)
        foreach (Transform child in opponentGrid.transform)
        {
            Destroy(child.gameObject);
        }

        // disable opponent's buttons and fleet terrain
        opponentCanvas.transform.Find("Panel").gameObject.SetActive(false);
        opponentTerrain.gameObject.SetActive(false);        

    }
    [ClientRpc]
    public void RpcUpdateLoadingRings()
    {
        NetworkGamePlayer gamePlayer = opponentGamePlayer.GetComponent<NetworkGamePlayer>();

        if (!gamePlayer.netIdentity.isServer) // meaning, that our opponent is in the left
        {
            sharedCanvas.transform.Find("LoadingRings").Find("Left").gameObject.SetActive(!gamePlayer.placementIsReady);
        }
        else // meaning, that our opponent is in the right
        {
            sharedCanvas.transform.Find("LoadingRings").Find("Right").gameObject.SetActive(!gamePlayer.placementIsReady);
        }
    }    

    public void GoBackToMainMenu() => Room.IngameDisconnect();
    [ClientRpc] public void DebugStuf() => Debug.Log("bullshit");
    public void ReadyToBattle()
    {
        sharedCanvas.transform.Find("Coords").Find("Left").gameObject.SetActive(true);
        sharedCanvas.transform.Find("Coords").Find("Right").gameObject.SetActive(true);

        ownCanvas.transform.Find("Panel").gameObject.SetActive(false);

        opponentTerrain.gameObject.SetActive(true);        

        ownGamePlayer.GetComponent<NetworkGamePlayer>().CmdReadyUp();        

            GameObject[] players = new GameObject[2];
        players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (player.GetComponent<NetworkIdentity>().hasAuthority)
            {
                ownPlayer = player;
            }
            else
            {
                opponentPlayer = player;
            }
        }

        StartCoroutine(PlayerLerpToCenter(ownPlayer));

        
    }     
    

    private IEnumerator PlayerLerpToCenter(GameObject player)
    {
        Vector3 startPos = player.transform.localPosition;
        Vector3 endPos = new Vector3(500f, 120f, 110f);
        float time = .5f;
        float elapsedTime = 0;

        while (elapsedTime < time)
        {
            ownPlayer.transform.position = Vector3.Lerp(startPos, endPos, (elapsedTime / time));
            ownPlayer.transform.GetChild(0).GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView = Mathf.Lerp(60, 75, elapsedTime / time);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    

}

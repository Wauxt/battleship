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
    public enum Side
    {
        Left,
        Right,
        Default
    }

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
    [SerializeField] private GameObject battlefield = null;


    [SyncVar] private Side whoseTurn = Side.Default;
    [SyncVar] private string placement_01 = "";
    [SyncVar] private string placement_02 = "";
    

    public Side WhoseTurn { get { return whoseTurn; } set { whoseTurn = value; } }
    public string Placement_01 { get { return placement_01; } set { placement_01 = value; } }
    public string Placement_02 { get { return placement_02; } set { placement_02 = value; } }

    public override void OnStartClient()
    {
        sharedCanvas = GameObject.Find("SharedCanvas");

        if (isServer && isClient) // meaning we are host's OGM
        {
            ownGrid = GameObject.Find("Grid_01");
            opponentGrid = GameObject.Find("Grid_02");

            ownCanvas = GameObject.Find("Canvas_01");
            opponentCanvas = GameObject.Find("Canvas_02");

            ownTerrain = GameObject.Find("Terrain_01");
            opponentTerrain = GameObject.Find("Terrain_02");

            sharedCanvas.transform.Find("Coords").Find("Right").gameObject.SetActive(false);
            sharedCanvas.transform.Find("LoadingRings").Find("Left").gameObject.SetActive(false);

            battlefield = sharedCanvas.transform.Find("Battlefields").Find("Field (right)").gameObject;            
        }
        else if (isClientOnly) // meaning we are clientOnly's OGM
        {
            ownGrid = GameObject.Find("Grid_02");
            opponentGrid = GameObject.Find("Grid_01");

            ownCanvas = GameObject.Find("Canvas_02");
            opponentCanvas = GameObject.Find("Canvas_01");

            ownTerrain = GameObject.Find("Terrain_02");
            opponentTerrain = GameObject.Find("Terrain_01");

            sharedCanvas.transform.Find("Coords").Find("Left").gameObject.SetActive(false);
            sharedCanvas.transform.Find("LoadingRings").Find("Right").gameObject.SetActive(false);

            battlefield = sharedCanvas.transform.Find("Battlefields").Find("Field (left)").gameObject;            
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

        if (gamePlayer.mySide == Side.Left) // meaning, that our opponent is in the left
        {
            sharedCanvas.transform.Find("LoadingRings").Find("Left").gameObject.SetActive(!gamePlayer.placementIsReady);
        }
        else if (gamePlayer.mySide == Side.Right) // meaning, that our opponent is in the right
        {
            sharedCanvas.transform.Find("LoadingRings").Find("Right").gameObject.SetActive(!gamePlayer.placementIsReady);
        }
    }
    
    [ClientRpc]
    public void RpcUpdateBattleFields()
    {
        if (whoseTurn == ownGamePlayer.GetComponent<NetworkGamePlayer>().mySide)
        {
            battlefield.SetActive(opponentGamePlayer.GetComponent<NetworkGamePlayer>().placementIsReady);
        }
    }
    public void GoBackToMainMenu() => Room.IngameDisconnect();
    public void ReadyToBattle()
    {
        sharedCanvas.transform.Find("Coords").Find("Left").gameObject.SetActive(true);
        sharedCanvas.transform.Find("Coords").Find("Right").gameObject.SetActive(true);

        ownCanvas.transform.Find("Panel").gameObject.SetActive(false);

        opponentTerrain.gameObject.SetActive(true);

        ownGamePlayer.GetComponent<NetworkGamePlayer>().CmdReadyUp();

        string myplacement = ownGamePlayer.GetComponent<NetworkGamePlayer>().GetMyPlacement();
        CmdSetPlacement(ownGamePlayer.GetComponent<NetworkGamePlayer>().mySide, myplacement);

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

    [Command(ignoreAuthority = true)]
    public void CmdSetPlacement(Side side, string placement)
    {
        if (side == Side.Left)
        {
            Placement_01 = placement;
        }
        else if (side == Side.Right)
        {
            Placement_02 = placement;
        }
    }

    [Command(ignoreAuthority = true)]
    public void CmdShootAndUpdateCell(Side shooter, int row, int column)
    {        
        int index = 10 * row + column;        
        

        if (shooter == Side.Left) // meaning, we should check RIGHT side (or placement_02)
        {
            if (Placement_02[index] == '0')
            {
                Debug.Log("left player: miss " + index);
                Placement_02 = ReplaceAtIndex(index, '3', Placement_02);
            }
            else if (Placement_02[index] == '1')
            {
                Debug.Log("left player: hit! " + index);
                Placement_02 = ReplaceAtIndex(index, '4', Placement_02);
                Room.SpawnDeckHit(new Vector3(515 + (column * 10), 0, 145 - (row * 10)));
            }
        }
        else if (shooter == Side.Right) // meaning, we should check LEFT side (or placement_01)
        {
            if (Placement_01[index] == '0')
            {
                Debug.Log("right player: miss " + index);
                Placement_01 = ReplaceAtIndex(index, '3', Placement_01);
            }
            else if (Placement_01[index] == '1')
            {
                Debug.Log("right player: hit! " + index);
                Placement_01 = ReplaceAtIndex(index, '4', Placement_01);
                Room.SpawnDeckHit(new Vector3(500, 0, 100));
            }
        }
    }
    public void ShootCell(Button contextButton)
    {
        contextButton.interactable = false;
        int row = int.Parse(contextButton.gameObject.transform.parent.gameObject.name.Substring(5, 1));
        int column = int.Parse(contextButton.gameObject.name.Substring(6, 1));        

        CmdShootAndUpdateCell(WhoseTurn, row, column);
    }
    
    static string ReplaceAtIndex(int i, char newChar, string str)
    {
        char[] charArr = str.ToCharArray();
        charArr[i] = newChar;
        return string.Join("", charArr);
    }
}

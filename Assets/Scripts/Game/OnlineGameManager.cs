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
    public void RpcUpdateBattleFieldsAfterPlacement()
    {
        if (whoseTurn == ownGamePlayer.GetComponent<NetworkGamePlayer>().mySide)
        {
            battlefield.SetActive(opponentGamePlayer.GetComponent<NetworkGamePlayer>().placementIsReady);
        }
    }

    [ClientRpc]
    public void RpcUpdateBattleFields()
    {
        if (whoseTurn == ownGamePlayer.GetComponent<NetworkGamePlayer>().mySide)
        {
            battlefield.SetActive(true);
        }
        else
        {
            battlefield.SetActive(false);
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
    public void CmdShootAndUpdateCell(Side shooter, int row, int column) // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! //
    {        
         
        string targetPlacement = shooter == Side.Left ? Placement_02 : Placement_01;
        int[,] targetCells = new int[10, 10];
        

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                targetCells[i, j] = int.Parse(targetPlacement[i * 10 + j].ToString());
            }
        }

        targetCells[row, column] = targetCells[row, column] == 0 ? 3 : 4;

        bool hit = targetCells[row, column] == 4;
        Room.SpawnMarkerHitOrMiss(shooter, row, column, hit);
        
        if (hit)
        {            
            Room.SpawnDeckHit(new Vector3((shooter == Side.Left ? 515 : 395) + (column*10), 0, 145 - (row * 10)));

            if (!AnyAliveDeckAround(targetCells, row, column)) // if that was the last deck
            {
                // find all decks of that ship
                int i = row;
                int j = column;

                if (targetCells[row >= 1 ? row - 1 : row + 1, column] == 4 || targetCells[row <= 8 ? row + 1 : row-1, column] == 4) // vertical ?
                {
                    // find top deck, or go up untill row != 1
                    while (targetCells[i, j] == 4 && row != 1)
                    {
                        if (i > 1) { i--; }
                        else if (i < 1) { i++; }
                    }

                    // splash, go down, repeat
                    while (targetCells[i,j] == 4 && i <= 8)
                    {
                        if (j >= 1) // if there are still some cells to the left
                        {
                            targetCells[i - 1, j - 1] = 3;
                            Room.SpawnMarkerHitOrMiss(shooter, i-1, j - 1, false);
                            
                            targetCells[i, j - 1] = 3;
                            Room.SpawnMarkerHitOrMiss(shooter, i, j - 1, false);

                            targetCells[i+1, j - 1] = 3;
                            Room.SpawnMarkerHitOrMiss(shooter, i+1, j - 1, false);
                        }                        
                        if (j <= 8) // if there are still some cells to the right
                        {
                            targetCells[i - 1, j + 1] = 3;
                            Room.SpawnMarkerHitOrMiss(shooter, i - 1, j + 1, false);

                            targetCells[i, j + 1] = 3;
                            Room.SpawnMarkerHitOrMiss(shooter, i, j + 1, false);

                            targetCells[i, j + 1] = 3;
                            Room.SpawnMarkerHitOrMiss(shooter, i + 1, j + 1, false);
                        }
                        if (targetCells[i-1,j] == 0)
                        {
                            targetCells[i - 1, j] = 3;
                            Room.SpawnMarkerHitOrMiss(shooter, i - 1, j, false);
                        }
                        if (targetCells[i + 1, j] == 0)
                        {
                            targetCells[i + 1, j] = 3;
                            Room.SpawnMarkerHitOrMiss(shooter, i + 1, j, false);                            
                        }
                    }

                    

                }
                else if (targetCells[row, column >= 1 ? column - 1 : column + 1] == 4 || targetCells[row, column <= 8 ? column + 1 : column - 1] == 4) // horizontal ?
                {

                }


                // splash everything around it
            }            
        }

        targetPlacement = "";
        for (int i = 0; i< 10; i++)
        {
            for (int j = 0; j<10; j++)
            {
                targetPlacement += targetCells[i, j].ToString();
            }
        }

        if (shooter == Side.Left)
        {
            Placement_02 = targetPlacement;
        }
        else if (shooter == Side.Right)
        {
            Placement_01 = targetPlacement;
        }        
    }

    [Server]
    public bool AnyAliveDeckAround(int[,] targetCells, int row, int column)
    {
        for (int i = (row >= 1) ? row - 1 : row; i < ((row <= 8) ? row + 1 : row); i++)
        {
            for (int j = (column >= 1) ? column - 1 : column; j < ((column <= 8) ? column + 1 : column); j++)
            {
                if (targetCells[i, j] == 1)
                {
                    return true;
                }
            }
        }
        return false;
    }


    public void ShootCell(Button contextButton)
    {
        if (WhoseTurn != ownGamePlayer.GetComponent<NetworkGamePlayer>().mySide)
        {
            return;
        }

        int row = int.Parse(contextButton.gameObject.transform.parent.gameObject.name.Substring(5, 1));
        int column = int.Parse(contextButton.gameObject.name.Substring(6, 1));
        string targetPlacement = WhoseTurn == Side.Left ? Placement_02 : Placement_01;
        
        if (targetPlacement[10*row + column] == '3' || targetPlacement[10 * row + column] == '4')
        {
            return;
        }

        CmdShootAndUpdateCell(WhoseTurn, row, column);
        //CmdSwitchTurn();
    }

    [Command(ignoreAuthority = true)]
    public void CmdSwitchTurn() 
    { 
        WhoseTurn = WhoseTurn == Side.Left ? Side.Right : Side.Left;
        StartCoroutine(UpdateFieldsAfterSeconds(.5f));
    }

    private IEnumerator UpdateFieldsAfterSeconds(float count)
    {
        yield return new WaitForSeconds(count);
        RpcUpdateBattleFields();
    }
}

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
    [SerializeField] private GameObject myPlayer = null;
    [SerializeField] private GameObject opponent = null;

    [Header("Grids")]
    [SerializeField] private GameObject ownGrid = null;
    [SerializeField] private GameObject opponentGrid = null;

    [Header("UI")]
    [SerializeField] private GameObject sharedCanvas = null;

    
    public override void OnStartClient()
    {
        if (isServer && isClient) 
        {
            Debug.Log("\"Hello! I'm the host (player_01, AKA The Left Player)\" - said the OGM");

            ownGrid = GameObject.Find("Grid_01");
            opponentGrid = GameObject.Find("Grid_02");
        }
        else if (isClientOnly)
        {           
            Debug.Log("\"Hello! I'm the client (player_02, AKA The Right Player)\" - said the OGM");
            ownGrid = GameObject.Find("Grid_01");
            opponentGrid = GameObject.Find("Grid_02");
        }


    }
    public void GoBackToMainMenu() => Room.IngameDisconnect();
    public void ReadyToBattle()
    {
        GameObject[] players = new GameObject[2];
        players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {            
            if (player.GetComponent<NetworkIdentity>().hasAuthority)
            {
                myPlayer = player;                
            }            
        }       
        
        StartCoroutine(PlayerLerpToCenter(myPlayer));


        /*
         и удалить лишние кнопки, а кнопку "выход" тоже передвинуть куданибудь 
        (а лучше просто новую создать, она ведь для всех игроков будет работать, да?)
        еще можно каким нибудь макаром вывести имена игроков



        */

    }

    private IEnumerator PlayerLerpToCenter(GameObject player)
    {
        Vector3 startPos = player.transform.localPosition;
        Vector3 endPos = new Vector3(500f, 120f, 110f);
        float time = .5f;
        float elapsedTime = 0;

        while (elapsedTime < time)
        {
            myPlayer.transform.position = Vector3.Lerp(startPos, endPos, (elapsedTime / time));
            myPlayer.transform.GetChild(0).GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView = Mathf.Lerp(60, 75, elapsedTime / time);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    

}

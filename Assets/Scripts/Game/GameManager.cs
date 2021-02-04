using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class GameManager : MonoBehaviour
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
    public void GoBackToMainMenu() => Room.IngameDisconnect();

    public void ReadyToBattle()
    {
        if (SceneManager.GetActiveScene().name == "Game_PVP")
        {
            //
        }
        // if online => add networkIdentity, add ogm as OGM, ogm.ReadyToPvp(), return

        // else:
        
        // move camera to center, change fov to 75
        
        // make animation of opponent loading
        // save placement, send it to OGM

        // wait for opponent to make his placement, then start game

    }
}

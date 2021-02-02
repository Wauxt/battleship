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
    public void GoBackToMainMenu()
    {
        Room.IngameDisconnect();
        SceneManager.LoadScene("Menu");
    }    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public void GoBackToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}

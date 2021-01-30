using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour
{
    public void OfflineStartButton()
    {
        GameObject.Find("Main Camera").GetComponent<Animator>().SetTrigger("hasToMoveUp");
        gameObject.SetActive(false);
    }
    void Start()
    {        
        ReloadMenu();
    }
    public void BackInMenu()
    {        
        ReloadMenu();
    }
    public void ReloadMenu()
    {
        GameObject buttons = transform.Find("Buttons").gameObject;
        GameObject authField = transform.Find("AuthField").gameObject;
        GameObject logo = transform.Find("Logo").gameObject;

        logo.SetActive(true);
        authField.SetActive(!Authorization.isAuthorized);

        buttons.SetActive(false);
        buttons.SetActive(true);
    }
    public void Quit()
    {
        Debug.Log("Quittin\'");
        Application.Quit();
    }    
}

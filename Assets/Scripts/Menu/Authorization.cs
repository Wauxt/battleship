using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Authorization : MonoBehaviour
{
    public static string nickname;    
    public static bool isAuthorized;
       
    GameObject authField;
    GameObject errorPanel;
    GameObject textField;
    GameObject logo;
    

    void Start()
    {
        authField = transform.Find("AuthField").gameObject;
        errorPanel = authField.transform.Find("ErrorPanel").gameObject;
        textField = authField.transform.Find("InputField").gameObject.transform.Find("Text").gameObject;
        logo = transform.Find("Logo").gameObject;        
    }
    
    bool IsAllLettersOrDigits(string s)
    {
        foreach (char c in s)
        {
            if (!char.IsLetterOrDigit(c))
                return false;
        }
        return true;
    }

    public void Authorize(GameObject textField)
    {
        string name = textField.GetComponent<Text>().text;

        if ((string.IsNullOrEmpty(name)) || (name.Length < 5))
        {
            errorPanel.SetActive(true);
            errorPanel.transform.Find("MinLength").gameObject.SetActive(true);
            errorPanel.transform.Find("WrongSymbals").gameObject.SetActive(false);
            return;
        }
        else if (!IsAllLettersOrDigits(name))
        {
            errorPanel.SetActive(true);
            errorPanel.transform.Find("MinLength").gameObject.SetActive(false);
            errorPanel.transform.Find("WrongSymbals").gameObject.SetActive(true);
            return;
        }
        errorPanel.SetActive(false);

        nickname = name;
        isAuthorized = true;        
        
        GetComponent<MainMenu>().ReloadMenu();        
        logo.GetComponent<Animator>().SetBool("isAuthorized", true);        
    }    
}
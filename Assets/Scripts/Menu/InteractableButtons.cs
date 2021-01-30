using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractableButtons : MonoBehaviour
{    
    void Start()
    {        
        
    }

    // Update is called once per frame
    void Update()
    {        
                
    }
    void OnEnable() 
    {
        GetComponent<UnityEngine.UI.Button>().interactable = Authorization.isAuthorized;
        if (Authorization.isAuthorized)
        {
            GetComponentInChildren<TMP_Text>().color = Color.white;
        }
        else
        {
            GetComponentInChildren<TMP_Text>().color = Color.grey;
        }
    }
}

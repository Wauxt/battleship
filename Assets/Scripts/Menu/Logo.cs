using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Logo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnEnable()
    {
        if (Authorization.isAuthorized)
        {
            GetComponent<Animator>().SetBool("isAuthorized", true);
        }
    }
    void OnDisable()
    {
        GetComponent<Image>().color = new Color(255, 255, 255, 0);
    }
}

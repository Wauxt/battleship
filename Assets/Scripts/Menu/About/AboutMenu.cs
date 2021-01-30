using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AboutMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public void OpenRules()
    {
        Application.OpenURL("Rules.html");
    }
    public void OpenAbout()
    {
        Application.OpenURL("About.html");
    }
}

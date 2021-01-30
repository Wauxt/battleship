using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Captions : MonoBehaviour
{
    // Start is called before the first frame update
    public void ShowCaption()
    {
        GameObject Low = transform.Find("Text 0").gameObject;
        GameObject Medium = transform.Find("Text 1").gameObject;
        GameObject High = transform.Find("Text 2").gameObject;

        switch (Difficulty.difficultyValue)
        {
            case 0:
                {
                    Low.SetActive(true);
                    Medium.SetActive(false);
                    High.SetActive(false);
                    break;
                }
            case 1:
                {
                    Low.SetActive(false);
                    Medium.SetActive(true);
                    High.SetActive(false);
                    break;
                }
            case 2:
                {
                    Low.SetActive(false);
                    Medium.SetActive(false);
                    High.SetActive(true);
                    break;
                }
        }            
    }
}

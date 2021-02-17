using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using SFB;
using Mirror;
using System.Collections;

public static class LoadFromMenu
{
    public static IEnumerator LoadSaveFileFromMenu()
    {
        var extensions = new[] {
            new ExtensionFilter("Data", "dat"),
        };
        var path = StandaloneFileBrowser.OpenFilePanel("Open File", "C:\\Users\\user\\Desktop\\", extensions, false);

        SceneManager.LoadScene("Game_PVE");
        yield return new WaitForSeconds(4f);

        GameObject gm = GameObject.Find("GameManager");
        //gm.GetComponent<SaveSystem>().LoadGame(path);

    }
}

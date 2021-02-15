using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;
using TMPro;

public class GameManager : MonoBehaviour
{
    public enum Side
    {
        Left,
        Right,
        Default
    }

    // easy = random  
    // medium = diagon + coasts
    // hard = random + diag + coasts

    [Header("Players")]
    [SerializeField] private GameObject ownPlayer = null;
    [SerializeField] private GameObject opponentPlayer = null;

    [Header("Terrain")]
    [SerializeField] private GameObject ownTerrain = null;
    [SerializeField] private GameObject opponentTerrain = null;

    [Header("Grids")]
    [SerializeField] private GameObject ownGrid = null;
    [SerializeField] private GameObject opponentGrid = null;

    [Header("UI")]
    [SerializeField] private GameObject ownCanvas = null; // non overlay canvas

    //[SerializeField] private GameObject opponentCanvas = null;
    [SerializeField] private GameObject sharedCanvas = null;

    [SerializeField] private GameObject overlayHitMarkers = null;
    [SerializeField] private GameObject overlayEndGame = null;

    [SerializeField] private TMP_Text ownNameInfo = null;
    [SerializeField] private TMP_Text opponentNameInfo = null;
    [SerializeField] private TMP_Text whoseTurnInfo = null;


    [Header("Game")]
    [SerializeField] private GameObject battlefield = null;
    [SerializeField] private GameObject hitDeckPrefab = null;
    [SerializeField] private GameObject missSplashPrefab = null;

    private Side whoseTurn = Side.Default;
    private string placement_01 = "";
    private string placement_02 = "";
    private int shotCount_01 = 0;
    private int shotCount_02 = 0;
    private int hitCount_01 = 0;
    private int hitCount_02 = 0;

    //private NetworkManagerBS room;
    //private NetworkManagerBS Room
    //{
    //    get
    //    {
    //        if (room != null)
    //        {
    //            return room;
    //        }
    //        return room = NetworkManager.singleton as NetworkManagerBS;
    //    }
    //}

    public Side WhoseTurn { get { return whoseTurn; } set { whoseTurn = value; } }
    public string Placement_01 { get { return placement_01; } set { placement_01 = value; } }
    public string Placement_02 { get { return placement_02; } set { placement_02 = value; } }



    public void Start()
    {
        opponentGrid.GetComponent<ShipsGrid>().AutoPlacement_Random();
    }

    public void UpdateBattleFields()
    {
        if (WhoseTurn == Side.Left)
        {
            battlefield.SetActive(true);
            whoseTurnInfo.text = "<color #ffffff>Ваш ход</color>";
        }
        else
        {
            battlefield.SetActive(false);
            whoseTurnInfo.text = "<color #999999>Ход соперника</color>";
        }
    }
    public void GoBackToMainMenu() => SceneManager.LoadScene("Menu");
    public void ReadyToBattle()
    {
        sharedCanvas.transform.Find("Coords").Find("Left").gameObject.SetActive(true);
        sharedCanvas.transform.Find("Coords").Find("Right").gameObject.SetActive(true);

        GameObject barPanel = GameObject.Find("MainCamera").transform.Find("Canvas").Find("Bar").Find("Panel").gameObject; // bar
        barPanel.SetActive(true);
        barPanel.transform.Find("Save").gameObject.GetComponent<Button>().interactable = true;
        barPanel.transform.Find("Save").transform.Find("Text").gameObject.GetComponent<TMP_Text>().text = "Сохранить";

        ownCanvas.transform.Find("Panel").gameObject.SetActive(false);

        opponentTerrain.gameObject.SetActive(true);

        sharedCanvas.transform.Find("NameTags").gameObject.SetActive(true);
        ownNameInfo.text = "<color #ff00ff>" + Authorization.nickname + "</color>";
        opponentNameInfo.text = "<color #ff0000>Противник (" + (Difficulty.difficultyValue == 2 ? "тяжело" : Difficulty.difficultyValue == 1 ? "средне" : "легко") + ")</color>";

        string myplacement = "";
        int[,] cells = new int[10, 10];

        if (true)
        {
            cells = ownGrid.GetComponent<ShipsGrid>().GridCells;
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    myplacement += cells[i, j];
                }
            }
            Placement_01 = myplacement;

            string opponentPlacement = "";

            opponentGrid.GetComponent<ShipsGrid>().EndPlacement();

            cells = opponentGrid.GetComponent<ShipsGrid>().GridCells;
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    opponentPlacement += cells[i, j];
                }
            }
            Placement_02 = opponentPlacement;

            foreach (Transform child in opponentGrid.transform)
            {
                child.gameObject.SetActive(false);
            }
        }//// set placements

        WhoseTurn = Side.Left;

        battlefield.SetActive(true);

        StartCoroutine(PlayerLerpToCenter());
    }

    private IEnumerator PlayerLerpToCenter()
    {
        Vector3 startPos = ownPlayer.transform.localPosition;
        Vector3 endPos = new Vector3(500f, 120f, 110f);
        float time = .5f;
        float elapsedTime = 0;

        while (elapsedTime < time)
        {
            ownPlayer.transform.position = Vector3.Lerp(startPos, endPos, (elapsedTime / time));
            ownPlayer.GetComponent<Camera>().fieldOfView = Mathf.Lerp(60, 75, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public void ShootCell(Button contextButton)
    {
        if (WhoseTurn != Side.Left)
        {
            return;
        }

        int row = int.Parse(contextButton.gameObject.transform.parent.gameObject.name.Substring(5, 1));
        int column = int.Parse(contextButton.gameObject.name.Substring(6, 1));
        string targetPlacement = WhoseTurn == Side.Left ? Placement_02 : Placement_01;

        if (targetPlacement[10 * row + column] == '3' || targetPlacement[10 * row + column] == '4')
        {
            return;
        }
        bool hit = targetPlacement[10 * row + column] == '1';       
                
        CmdShootAndUpdateCell(WhoseTurn, row, column);

        //if (!hit)
        //{
        //    CmdSwitchTurn();
        //}
    }    
    public void CmdShootAndUpdateCell(Side shooter, int row, int column) // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! //
    {
        string targetPlacement = shooter == Side.Left ? Placement_02 : Placement_01;
        int[,] targetCells = new int[10, 10];

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                targetCells[i, j] = int.Parse(targetPlacement[i * 10 + j].ToString());
            }
        }
        bool hit = targetCells[row, column] == 1;

        overlayHitMarkers.GetComponent<Animator>().SetTrigger(!hit ? "Miss" : shooter == Side.Left ? "Hit" : "GotHit");

        targetCells[row, column] = hit ? 4 : 3;

        SpawnMarkerHitOrMiss(shooter, row, column, hit);

        if (hit)
        {
            if (ThatWasTheLastDeck(targetCells.Clone() as int[,], row, column)) // if that was the last deck
            {
                int i = row;
                int j = column;

                // check if ship was placed horizontally or vertically                 
                bool shipWasVertical = (i >= 1 && targetCells[i - 1, j] == 4) || (i <= 8 && targetCells[i + 1, j] == 4);
                bool shipWasHorizontal = (j >= 1 && targetCells[i, j - 1] == 4) || (j <= 8 && targetCells[i, j + 1] == 4);


                // find first from the top/left deck                
                if (shipWasVertical)
                {
                    while (i > 1 && targetCells[i - 1, j] == 4) { i--; }
                }
                else if (shipWasHorizontal)
                {
                    while (j > 1 && targetCells[i, j - 1] == 4) { j--; }
                }

                bool areWeDoneYet = false;

                // repeat for all decks:
                while (!areWeDoneYet)
                {
                    if (i >= 1 && j >= 1 && targetCells[i - 1, j - 1] == 0)
                    {
                        targetCells[i - 1, j - 1] = 3; // top left
                        SpawnMarkerHitOrMiss(shooter, i - 1, j - 1, false);
                    }

                    if (i >= 1 && targetCells[i - 1, j] == 0)
                    {
                        targetCells[i - 1, j] = 3; // top
                        SpawnMarkerHitOrMiss(shooter, i - 1, j, false);
                    }

                    if (i >= 1 && j <= 8 && targetCells[i - 1, j + 1] == 0)
                    {
                        targetCells[i - 1, j + 1] = 3; // top right     
                        SpawnMarkerHitOrMiss(shooter, i - 1, j + 1, false);
                    }

                    if (j <= 8 && targetCells[i, j + 1] == 0)
                    {
                        targetCells[i, j + 1] = 3; // right
                        SpawnMarkerHitOrMiss(shooter, i, j + 1, false);
                    }

                    if (i <= 8 && j <= 8 && targetCells[i + 1, j + 1] == 0)
                    {
                        targetCells[i + 1, j + 1] = 3; // bot right
                        SpawnMarkerHitOrMiss(shooter, i + 1, j + 1, false);
                    }

                    if (i <= 8 && targetCells[i + 1, j] == 0)
                    {
                        targetCells[i + 1, j] = 3; // bot
                        SpawnMarkerHitOrMiss(shooter, i + 1, j, false);
                    }

                    if (i <= 8 && j >= 1 && targetCells[i + 1, j - 1] == 0)
                    {
                        targetCells[i + 1, j - 1] = 3; // bot left
                        SpawnMarkerHitOrMiss(shooter, i + 1, j - 1, false);
                    }

                    if (j >= 1 && targetCells[i, j - 1] == 0)
                    {
                        targetCells[i, j - 1] = 3; // left
                        SpawnMarkerHitOrMiss(shooter, i, j - 1, false);
                    }


                    if (shipWasVertical && i < 8 && targetCells[i + 1, j] == 4)
                    {
                        i++;
                    }
                    else if (shipWasHorizontal && j < 8 && targetCells[i, j + 1] == 4)
                    {
                        j++;
                    }
                    else
                    {
                        areWeDoneYet = true;
                    }

                }
            }
        }

        // for stat
        if (shooter == Side.Left)
        {
            shotCount_01++;
            if (hit)
            {
                hitCount_01++;
            }
        }
        else if (shooter == Side.Right)
        {
            shotCount_02++;
            if (hit)
            {
                hitCount_02++;
            }
        }

        targetPlacement = "";
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                targetPlacement += targetCells[i, j].ToString();
            }
        }

        if (shooter == Side.Left)
        {
            Placement_02 = targetPlacement;
        }
        else if (shooter == Side.Right)
        {
            Placement_01 = targetPlacement;
        }

        if (hitCount_01 == 20)
        {
            StartCoroutine(AnnounceWinnerAfterSeconds(Side.Left, 1f));
        }
        else if (hitCount_02 == 20)
        {
            StartCoroutine(AnnounceWinnerAfterSeconds(Side.Right, 1f));
        }
    }
    public bool ThatWasTheLastDeck(int[,] targetCells, int row, int column)
    {
        int i = row;
        int j = column;

        bool shipWasVertical = (i >= 1 && targetCells[i - 1, j] == 4) || (i <= 8 && targetCells[i + 1, j] == 4);
        bool shipWasHorizontal = (j >= 1 && targetCells[i, j - 1] == 4) || (j <= 8 && targetCells[i, j + 1] == 4);

        // find first from the top/left deck                
        if (shipWasVertical)
        {
            while (i > 1 && targetCells[i - 1, j] == 4) { i--; }
        }
        else if (shipWasHorizontal)
        {
            while (j > 1 && targetCells[i, j - 1] == 4) { j--; }
        }

        bool areWeDoneYet = false;

        // repeat for all decks:
        while (!areWeDoneYet)
        {
            if (i >= 1 && j >= 1 && targetCells[i - 1, j - 1] == 1)
                return false;

            if (i >= 1 && targetCells[i - 1, j] == 1)
                return false;

            if (i >= 1 && j <= 8 && targetCells[i - 1, j + 1] == 1)
                return false;

            if (j <= 8 && targetCells[i, j + 1] == 1)
                return false;

            if (i <= 8 && j <= 8 && targetCells[i + 1, j + 1] == 1)
                return false;

            if (i <= 8 && targetCells[i + 1, j] == 1)
                return false;

            if (i <= 8 && j >= 1 && targetCells[i + 1, j - 1] == 1)
                return false;

            if (j >= 1 && targetCells[i, j - 1] == 1)
                return false;



            if (shipWasVertical && i < 8 && targetCells[i + 1, j] == 4)
            {
                i++;
            }
            else if (shipWasHorizontal && j < 8 && targetCells[i, j + 1] == 4)
            {
                j++;
            }
            else
            {
                areWeDoneYet = true;
            }
        }

        return true;
    }
    public void SpawnMarkerHitOrMiss(Side shooter, int row, int column, bool hit)
    {
        Vector3 position = new Vector3((shooter == Side.Left ? 515 : 395) + (column * 10), 0, 145 - (row * 10));
        GameObject marker = hit ? Instantiate(hitDeckPrefab) : Instantiate(missSplashPrefab);
        marker.transform.position = position;
    }
    private IEnumerator AnnounceWinnerAfterSeconds(Side winner, float count)
    {
        battlefield.SetActive(false);
        yield return new WaitForSeconds(count);
        RpcAnnounceWinner(winner);
    }
    public void RpcAnnounceWinner(Side winner)
    {
        overlayEndGame.SetActive(true);
        overlayEndGame.GetComponent<Animator>().SetTrigger("GameEnded");

        TMP_Text winloose = overlayEndGame.transform.Find("Panel").transform.Find("WinLoose").GetComponent<TMP_Text>();
        Image winlooseBackground = overlayEndGame.transform.Find("Panel").transform.Find("WinLooseBackground").GetComponent<Image>();
        TMP_Text name_01 = overlayEndGame.transform.Find("Panel").transform.Find("Stats").Find("Name_01").GetComponent<TMP_Text>();
        TMP_Text name_02 = overlayEndGame.transform.Find("Panel").transform.Find("Stats").Find("Name_02").GetComponent<TMP_Text>();
        TMP_Text acc_01 = overlayEndGame.transform.Find("Panel").transform.Find("Stats").Find("Acc_01").GetComponent<TMP_Text>();
        TMP_Text acc_02 = overlayEndGame.transform.Find("Panel").transform.Find("Stats").Find("Acc_02").GetComponent<TMP_Text>();

        float accuracy_01 = shotCount_01 > 0 ? (float)hitCount_01 * 100 / (float)shotCount_01 : 0;
        float accuracy_02 = shotCount_02 > 0 ? (float)hitCount_02 * 100 / (float)shotCount_02 : 0;

        if (winner == Side.Left)
        {
            winloose.text = "<color #ffffff>Победа</color>";
            winlooseBackground.color = new Color(0f, 160f / 255f, 50f / 255f, 30f / 255f);
        }
        else
        {
            winloose.text = "<color #ffffff>Поражение</color>";
            winlooseBackground.color = new Color(160f / 255f, 10f / 255f, 0f, 30f / 255f);
        }

        acc_01.text = accuracy_01.ToString("0.000") + "%";
        acc_02.text = accuracy_02.ToString("0.000") + "%";

        name_01.text = Authorization.nickname;
        name_02.text = "Противник (" + (Difficulty.difficultyValue == 2 ? "тяжело" : Difficulty.difficultyValue == 1 ? "средне" : "легко") + ")";
    }
}

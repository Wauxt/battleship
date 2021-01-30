using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ShipsGrid : MonoBehaviour
{
    [SerializeField] private Camera playerCamera = null;
    [SerializeField] private Canvas controlCanvas = null;

    private Button startButton;    
    private GameObject autoPlacementPanel;
    private List<BoxCollider> shipsColliders;

    int[,] gridCells;
        

    public Camera PlayerCamera() { return playerCamera; }
    public Canvas ControlCanvas() { return controlCanvas; }
    public List<BoxCollider> ShipsColliders() { return shipsColliders; }
    
    public int[,] GetGridCells()
    {
        int[,] result = new int[10, 10];
        return result;
    }
    

    void Awake()
    {
        autoPlacementPanel = controlCanvas.gameObject.transform.Find("Panel").Find("AutoPlacementPanel").gameObject;
        autoPlacementPanel.SetActive(false);

        startButton = controlCanvas.gameObject.transform.Find("Panel").Find("Start").gameObject.GetComponent<Button>();

        gridCells = new int[10, 10];

        shipsColliders = new List<BoxCollider>();

        for (int i = 0; i < transform.childCount; i++)
        {
            shipsColliders.Add(transform.GetChild(i).GetComponent<BoxCollider>());
        }
    }

    public void ToggleAutoPlacementPanel()
    { 
        if (!autoPlacementPanel.activeSelf)        
            autoPlacementPanel.SetActive(true);        
        else        
            autoPlacementPanel.SetActive(false);        
    }

    public bool AllShipsAreInsideGrid()
    {
        for (int i = 0; i < shipsColliders.Count; i++)
        {
            Ship curShip = shipsColliders[i].gameObject.GetComponent<Ship>();
            if (!curShip.ShipIsInsideGrid())
            {
                return false;
            }
        }
        return true;
    }

    public void ToggleReadyButton()
    {
        if (!AllShipsAreInsideGrid())
        {
            startButton.interactable = false;
            startButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "<color=grey>Начать игру</color>";
        }
        else
        {
            startButton.interactable = true;
            startButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "<color=white>Начать игру</color>";
        }
    }

    public void EndPlacement()
    {
        // have to check and send current placement
        
        
        gameObject.GetComponent<BoxCollider>().enabled = false;
        for (int i = 0; i < shipsColliders.Count; i++)
        {
            shipsColliders[i].enabled = false;
        }

        
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                // for now lets say tat the cell is empty
                gridCells[i, j] = 0;
                Vector3 pos = transform.position + new Vector3(5, 0, -5)+ new Vector3(j * 10, 0, -i * 10);
                Collider[] col = Physics.OverlapSphere(pos, 1f);

                // cince gridcollider is disabled we can simply check with Physics.CheckSphere (?)
                if (Physics.CheckSphere(pos , 1f))
                {
                    gridCells[i, j] = 1;
                    Debug.Log("a deck!");
                }

            }
        }

        gameObject.GetComponent<BoxCollider>().enabled = true;
        for (int i = 0; i < shipsColliders.Count; i++)
        {
            shipsColliders[i].enabled = true;
        }




    }

    public void AutoPlacement_Random()
    {
        transform.GetChild(0).GetComponent<Ship>().DeleteAllShips();

        //all cells are available
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                gridCells[i, j] = 0;
            }
        }

        for (int i = 0; i < shipsColliders.Count; i++)
        {
            SetRandomPosition(i);
        }

        ToggleReadyButton();
    }

    public void AutoPlacement_AntiDiagonal()
    {
        transform.GetChild(0).GetComponent<Ship>().DeleteAllShips();

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                gridCells[i, j] = 0;

                if ((i == j) || (i == 9 - j))
                {
                    gridCells[i, j] = 1; // removing diagonal cells
                }
            }
        }      

        for (int i = 0; i < shipsColliders.Count; i++)
        {
            SetRandomPosition(i);
        }

        ToggleReadyButton();
    }

    public void AutoPlacement_Coasts()
    {
        transform.GetChild(0).GetComponent<Ship>().DeleteAllShips();

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                gridCells[i, j] = 1; 
                if (i == 0 || i == 9 || j == 0 || j == 9)
                {
                    gridCells[i, j] = 0; // setting coasts free
                }
            }
        }        

        for (int i = 0; i < shipsColliders.Count; i++)
        {
            if (shipsColliders[i].gameObject.GetComponent<Ship>().deckAmount > 1)
                SetRandomPosition(i);
        }

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                if (i < 2 || i > 7 || j < 2 || j > 7)
                {
                    gridCells[i, j] = 1; // removing coasts
                }
                else
                {
                    gridCells[i, j] = 0; // setting free everything in between
                }
            }
        }

        for (int i = 0; i < shipsColliders.Count; i++)
        {
            if (shipsColliders[i].gameObject.GetComponent<Ship>().deckAmount == 1)
                SetRandomPosition(i);
        }

        ToggleReadyButton();
    }
    void SetRandomPosition(int i)
    {
        GameObject ship = shipsColliders[i].gameObject;
        int deckAmount = ship.GetComponent<Ship>().deckAmount;

        float Yangle = Mathf.Round(Random.value) * 90;

        try
        {
            Quaternion newRotation = Quaternion.Euler(0, Yangle, 0);
            Vector3 newPosition = GetPosition_Random(newRotation.eulerAngles.y < 45 ? "horizontal" : "vertical", deckAmount);

            ship.transform.localScale = new Vector3(1, 0.4f, 1);
            ship.transform.rotation = newRotation;
            ship.transform.localPosition = newPosition;

            ship.GetComponent<Ship>().CurPosition = ship.transform.localPosition;
            ship.GetComponent<Ship>().CurRotation = ship.transform.rotation;
        }
        catch
        {
            try
            {
                Yangle = Yangle > 45 ? 0 : 90; // if doesnt fit => turn 90

                Quaternion newRotation = Quaternion.Euler(0, Yangle, 0);
                Vector3 newPosition = GetPosition_Random(newRotation.eulerAngles.y < 45 ? "horizontal" : "vertical", deckAmount);

                ship.transform.localScale = new Vector3(1, 0.4f, 1);
                ship.transform.rotation = newRotation;
                ship.transform.localPosition = newPosition;

                ship.GetComponent<Ship>().CurPosition = ship.transform.localPosition;
                ship.GetComponent<Ship>().CurRotation = ship.transform.rotation;
            }
            catch 
            {
                ship.GetComponent<Ship>().DeleteShip();
            }            
        }
    }
    Vector3 GetPosition_Random(string rotation, int deckAmount)
    {

        List<Vector2Int> availableCells = new List<Vector2Int>();
        if (deckAmount > 1)
        {
            if (rotation == "horizontal")
            {
                for (int y = 0; y < 10; y++)
                {
                    int x = 0;
                    int start = -1;
                    while (x < 10)
                    {
                        if (gridCells[y, x] == 0)
                        {
                            if (start == -1)
                            {
                                start = x;
                            }
                            else
                            {
                                if (x - start + 1 >= deckAmount)
                                {
                                    availableCells.Add(new Vector2Int(start, y));
                                    start++;
                                }
                            }
                        }
                        else if (gridCells[y, x] == 1)
                        {
                            start = -1;
                        }
                        x++;
                    }
                }
            }
            else if (rotation == "vertical")
            {
                for (int x = 0; x < 10; x++)
                {
                    int y = 0;
                    int start = -1;
                    while (y < 10)
                    {
                        if (gridCells[y, x] == 0)
                        {
                            if (start == -1)
                            {
                                start = y;
                            }
                            else
                            {
                                if (y - start + 1 >= deckAmount)
                                {
                                    availableCells.Add(new Vector2Int(x, start));
                                    start++;
                                }
                            }
                        }
                        else if (gridCells[y, x] == 1)
                        {
                            start = -1;
                        }
                        y++;
                    }
                }
            }
        }
        else
        {
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    if (gridCells[y, x] == 0)
                    {
                        availableCells.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        Vector2Int newCoords = availableCells[Random.Range(0, availableCells.Count)];

        Vector3 result = new Vector3(newCoords.x + .5f, .2f, -newCoords.y - .5f);


        int firstX, lastX, firstY, lastY, width, height;

        width = rotation == "horizontal" ? deckAmount : 1;
        height = rotation == "vertical" ? deckAmount : 1;

        firstX = newCoords.x > 0 ? newCoords.x - 1 : 0;
        lastX = (newCoords.x + width - 1) < 9 ? newCoords.x + width : 9;

        firstY = newCoords.y > 0 ? newCoords.y - 1 : 0;
        lastY = (newCoords.y + height - 1) < 9 ? newCoords.y + height : 9;

        for (int y = firstY; y <= lastY; y++)
        {
            for (int x = firstX; x <= lastX; x++)
            {
                gridCells[y, x] = 1;
            }
        }

        return result;
    }

    

}

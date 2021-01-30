using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StaticGridScriptIDK
{
    private int width;
    private int height;
    private int[,] gridArray;
    private float cellSize;
    private Vector3 topLeftPos;

    public StaticGridScriptIDK(int width, int height, float cellSize, Transform gridGameObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        topLeftPos = gridGameObject.position;

        gridArray = new int[width, height];

        // Draw gizmos:

        for (int x = 0; x<gridArray.GetLength(0); x++)
        {
            for (int y=0; y<gridArray.GetLength(1); y++)
            {                
                Debug.DrawLine(GetWorldPosition(x, y)+ topLeftPos, GetWorldPosition(x, y + 1) + topLeftPos, Color.white, 100f);
                Debug.DrawLine(GetWorldPosition(x, y) + topLeftPos, GetWorldPosition(x + 1, y) + topLeftPos, Color.white, 100f);
            }
        }
        Debug.DrawLine(GetWorldPosition(0, height) + topLeftPos, GetWorldPosition(width, height) + topLeftPos, Color.green, 100f);
        Debug.DrawLine(GetWorldPosition(width, 0) + topLeftPos, GetWorldPosition(width, height) + topLeftPos, Color.green, 100f);
    }
    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x * cellSize, 0, -y*cellSize);
    }    

}

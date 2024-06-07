using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// This script is responsible for instantiating the grid tiles.
/// </summary>
public class InstantiateGrid : MonoBehaviour
{
    [SerializeField] private GameObject gridPrefab;

    [SerializeField] private float gridSize = 5;

    private List<GameObject> gridList = new List<GameObject>(); 
    public float GridSize => gridSize;
    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    void Start()
    {
   //
        // Instantiates grid tiles for the map layout
        // Each tile is 32x32 so we instantiate them by
        // that interval, and center them by adding 16 to its
        // coordinates
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                GameObject temp = Instantiate(gridPrefab, transform);
                temp.transform.position = new Vector2(16 + (i- (gridSize/2)) * 32, 16 + (j - (gridSize / 2)) * 32);
                gridList.Add(temp);
            }
        }
    }

   
    public void SetGrid(int size)
    {
        foreach (GameObject temp in gridList)
        {
            Destroy(temp);
        }

        gridList.Clear();

        gridSize = size;

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                GameObject temp = Instantiate(gridPrefab, transform);
                temp.transform.position = new Vector2(16 + (i - (gridSize / 2)) * 32, 16 + (j - (gridSize / 2)) * 32);
                gridList.Add(temp);
            }
        }
    }
}

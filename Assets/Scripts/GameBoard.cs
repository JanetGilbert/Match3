using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimalType { Cat, Chick, Fox, Mouse, Pig, Rabbit };

[System.Serializable]
public struct AnimalDef
{
    public AnimalType type;
    public Sprite sprite;
}



public class GameBoard : MonoBehaviour
{


    // Set in Editor
    public Animal animalPrefab; // Prefab of animal game piece.
    public int gridX; // Horizontal grid size.
    public int gridY; // Vertical grid size.
    public float gridSpacing; // Space between animals in grid.

    public AnimalDef [] animalDefs;

    private Animal [,] animalGrid;

    void Start()
    {
        InitGrid();

       
    }

    
    void Update()
    {
        
    }

    private void InitGrid()
    {
        float animalPosX = transform.position.x;
        float animalPosY = transform.position.y;

        // Create random grid.
        animalGrid = new Animal[gridX, gridY];

        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                animalGrid[x, y] = Instantiate(animalPrefab, new Vector3(animalPosX, animalPosY, 0), Quaternion.identity);
                animalGrid[x, y].name = "Animal_" + x + "_" + y;
                animalGrid[x, y].transform.parent = this.transform; // Set the Board as the parent of the grid items.

                animalPosY += gridSpacing;
            }

            animalPosY = transform.position.y;
            animalPosX += gridSpacing;
        }

        // Fit to screen.
        CameraFit cameraFit = Camera.main.GetComponent<CameraFit>();
        float halfGridWidth = gridX * gridSpacing * 0.5f;
        float halfGridHeight = gridY * gridSpacing * 0.5f;
        float halfSpacing = gridSpacing / 2.0f;

        Bounds bounds = new Bounds();
        bounds.center = transform.position + new Vector3(halfGridWidth - halfSpacing, halfGridHeight - halfSpacing);
        bounds.extents = new Vector3(halfGridWidth, halfGridHeight, 0.0f);

        cameraFit.Fit(bounds);
    }
}

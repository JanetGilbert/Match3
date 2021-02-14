using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// For mapping the sprites to the animal types.
[System.Serializable]
public struct AnimalDef
{
    public AnimalType type;
    public Sprite sprite;
}

public enum BoardState {Idle, Swapping, Matching, Falling};


public class GameBoard : MonoBehaviour
{


    // Set in Editor
    public Animal animalPrefab; // Prefab of animal game piece.
    public int gridX; // Horizontal grid size.
    public int gridY; // Vertical grid size.
    public float gridSpacing; // Space between animals in grid.

    public AnimalDef [] animalDefs;

    private Animal [,] animalGrid;

    // For checking matches
    private bool[,] scratchGrid; // Grid to ensure we don't check a square twice.
    private List<Animal> matchList; // Animals in current match
    private List<Animal> removeList; // Matched animals to remove

    // State
    private Animal selectedAnimal;
    private BoardState state;

    // Constants
    private const int MATCH_SIZE = 3;

    void Start()
    {
        InitGrid();
        state = BoardState.Idle;
        matchList = new List<Animal>();
        removeList = new List<Animal>();

    }

    
    void Update()
    {
        if (state == BoardState.Swapping)
        {
            bool stillSwapping = false;

            for (int x = 0; x < gridX; x++)
            {
                for (int y = 0; y < gridY; y++)
                {
                    if (animalGrid[x, y].State == AnimalState.Moving)
                    {
                        stillSwapping = true;
                    }
                }
            }

            if (!stillSwapping)
            {
                state = BoardState.Idle;

                RemoveAllMatches();
                //AddNewAnimals();
                state = BoardState.Falling;
            }
        }
    }

    private void InitGrid()
    {
        float animalPosX = transform.position.x;
        float animalPosY = transform.position.y;

        scratchGrid = new bool[gridX, gridY];

        // Create random grid.
        animalGrid = new Animal[gridX, gridY];

        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                animalGrid[x, y] = Instantiate(animalPrefab, new Vector3(animalPosX, animalPosY, 0), Quaternion.identity, this.transform);
                animalGrid[x, y].name = "Animal_" + x + "_" + y;
                animalGrid[x, y].X = x;
                animalGrid[x, y].Y = y;

                animalPosY += gridSpacing;
            }

            animalPosY = transform.position.y;
            animalPosX += gridSpacing;
        }

        GenerateBoard();

        // Fit grid to screen.
        CameraFit cameraFit = Camera.main.GetComponent<CameraFit>();
        cameraFit.Fit(new Rect(transform.position.x - (gridSpacing / 2.0f),
                               transform.position.y - (gridSpacing / 2.0f),
                                gridX * gridSpacing,
                                gridY * gridSpacing));
    }

    private void GenerateBoard()
    {
        int numAnimalTypes = AnimalType.GetNames(typeof(AnimalType)).Length;

        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                animalGrid[x, y].Type = (AnimalType)Random.Range(0, numAnimalTypes);
            }
        }
    }

    private void RemoveAllMatches()
    {
        bool removed = false;

        removeList.Clear();

        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                ClearMatchScratch();
                CheckMatch(x, y, animalGrid[x, y].Type);
                if (matchList.Count > MATCH_SIZE)
                {
                    removed = true;
                    foreach (Animal a in matchList)
                    {
                        removeList.Add(a);
                    }
                }
            }
        }


        foreach (Animal a in removeList)
        {
            a.StartRemoving();
        }

        if (removeList.Count > 0)
        {
            state = BoardState.Matching;
        }
        else
        {
            state = BoardState.Idle;
        }

        removeList.Clear();
    }

    private void ClearMatchScratch()
    {

        matchList.Clear();

        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                scratchGrid[x, y] = false;
            }
        }
    }

    // Flood-fill algorithm
    private void CheckMatch(int x, int y, AnimalType typeMatch)
    {
        if (x < 0 || y < 0 || x >= gridX || y >= gridY)
        {
            return; // Off the board
        }

        if (animalGrid[x, y].Type != typeMatch)
        {
            return; // Not a match
        }

        if (scratchGrid[x, y])
        {
            return; // Already checked this square
        }

        scratchGrid[x, y] = true;

        CheckMatch(x - 1, y, typeMatch); // Recursively call this function.
        CheckMatch(x + 1, y, typeMatch);
        CheckMatch(x, y  - 1, typeMatch);
        CheckMatch(x, y + 1, typeMatch);

        matchList.Add(animalGrid[x, y]);

    }

    public void Select(int x, int y)
    {
        if (state != BoardState.Idle)
        {
            return; // Can't interact with board unless in idle state.
        }

        if (animalGrid[x, y] == selectedAnimal)
        {
            return;
        }

        if (selectedAnimal != null)
        {
            Swap(animalGrid[x, y], selectedAnimal);
            selectedAnimal.Selected(false);
            selectedAnimal = null;
        }
        else
        {
            selectedAnimal = animalGrid[x, y];
            selectedAnimal.Selected(true);
        }
    }

    public void Swap(Animal a1, Animal a2)
    {
        a2.MoveTo(a1);
        a1.MoveTo(a2);

        int x1 = a1.X;
        int y1 = a1.Y;
        int x2 = a2.X;
        int y2 = a2.Y;

        Animal temp = a1;
        animalGrid[x1, y1] = a2;
        animalGrid[x2, y2] = temp;

        animalGrid[x1, y1].X = x1;
        animalGrid[x1, y1].Y = y1;

        animalGrid[x2, y2].X = x2;
        animalGrid[x2, y2].Y = y2;

        state = BoardState.Swapping;

    }

    public Sprite GetAnimalSprite(AnimalType type)
    {
        foreach (AnimalDef def in animalDefs)
        {
            if (def.type == type)
            {
                return def.sprite;
            }
        }

        return null;
    }
}

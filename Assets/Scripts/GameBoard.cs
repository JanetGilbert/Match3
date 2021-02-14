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

public enum BoardState {Idle, Matching, Falling};


public class GameBoard : MonoBehaviour
{


    // Set in Editor
    public Animal animalPrefab; // Prefab of animal game piece.
    public int gridX; // Horizontal grid size.
    public int gridY; // Vertical grid size.
    public float gridSpacing; // Space between animals in grid.

    public AnimalDef[] animalDefs;

    private Animal[,] animalGrid;

    // For checking matches
    private bool[,] scratchGrid; // Grid to ensure we don't check a square twice.
    private List<Animal> matchList; // Animals in current match

    // State
    // private Animal selectedAnimal;
    private BoardState state;

    // Constants
    private const int MATCH_SIZE = 3;

    void Start()
    {
        InitGrid();
        state = BoardState.Idle;
        matchList = new List<Animal>();

    }


    void Update()
    {
        if (state == BoardState.Matching)
        {
            bool stillMatching = false;

            for (int x = 0; x < gridX; x++)
            {
                for (int y = 0; y < gridY; y++)
                {
                    if (animalGrid[x, y] != null && animalGrid[x, y].State == AnimalState.Matching)
                    {
                        if (animalGrid[x, y].IsLerpFinished())
                        {
                            Destroy(animalGrid[x, y].gameObject);
                            animalGrid[x, y] = null;
                        }
                        else
                        {
                            stillMatching = true;
                        }
                    }
                }
            }

            if (!stillMatching)
            {
                SetBoardFalling();
                FillEmptySquares();
                state = BoardState.Falling;
            }
        }
        else if (state == BoardState.Falling)
        {
            bool stillFalling = false;

            for (int x = 0; x < gridX; x++)
            {
                for (int y = 0; y < gridY; y++)
                {
                    if (animalGrid[x, y] != null)
                    {
                        if (animalGrid[x, y].State == AnimalState.Moving && !animalGrid[x, y].IsLerpFinished())
                        {
                            stillFalling = true;
                        }
                    }
                }
            }

            if (!stillFalling)
            {
                
                state = BoardState.Idle;
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

    private Animal MakeRandomAnimal(int x, int y)
    {
        Animal newAnimal  = Instantiate(animalPrefab, new Vector3(x, y, 0), Quaternion.identity, this.transform);
        newAnimal.name = "Animal_" + x + "_" + y;
        newAnimal.X = x;
        newAnimal.Y = y;

        int numAnimalTypes = AnimalType.GetNames(typeof(AnimalType)).Length;
        newAnimal.Type = (AnimalType)Random.Range(0, numAnimalTypes);

        newAnimal.transform.position = transform.position + new Vector3(x * gridSpacing, y * gridSpacing, 0.0f);
        newAnimal.transform.parent = transform;

        return newAnimal;
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

        if (animalGrid[x, y] == null)
        {
            return; // Empty square
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
        CheckMatch(x, y - 1, typeMatch);
        CheckMatch(x, y + 1, typeMatch);

        matchList.Add(animalGrid[x, y]);

    }

    public void Select(int x, int y)
    {
        if (state != BoardState.Idle)
        {
            return; // Can't interact with board unless in idle state.
        }

        ClearMatchScratch();
        CheckMatch(x, y, animalGrid[x, y].Type);

        if (matchList.Count >= MATCH_SIZE)
        {
            foreach (Animal a in matchList)
            {
                a.StartRemoving();
            }

            state = BoardState.Matching;
        }
        else
        {
            state = BoardState.Idle;
        }
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

    private void SetBoardFalling()
    {
        state = BoardState.Falling;

        for (int x = 0; x < gridX; x++)
        {
            int fallDistance = 0;

            for (int y = 0; y < gridY; y++)
            {
                if (animalGrid[x, y] == null)
                {
                    fallDistance++;
                }
                else if (fallDistance > 0)
                {
                    Vector3 start = animalGrid[x, y].transform.position;
                    Vector3 dest = start - new Vector3(0.0f, fallDistance * gridSpacing, 0.0f);
                    animalGrid[x, y].SetFalling(fallDistance, start, dest);
                    SwapAnimalsInGrid(x, y, x, y - fallDistance);
                }
            }
        }
    }

    private void FillEmptySquares()
    {
        for (int x = 0; x < gridX; x++)
        {
            int fallDistance = 0;
            // Calculate distance to fall
            for (int y = 0; y < gridY; y++)
            {
                if (animalGrid[x, y] == null)
                {
                    fallDistance++;
                }
            }

            for (int y = 0; y < gridY; y++)
            {
                if (animalGrid[x, y] == null)
                {
                    animalGrid[x, y] = MakeRandomAnimal(x, y);

                    Vector3 dest = animalGrid[x, y].transform.position;
                    Vector3 start = dest + new Vector3(0.0f, fallDistance * gridSpacing, 0.0f);
                    animalGrid[x, y].transform.position = start;
                    animalGrid[x, y].SetFalling(fallDistance, start, dest);
                }
            }
        }
    }

    // Swap two animals in grid.
    private void SwapAnimalsInGrid(int x1, int y1, int x2, int y2)
    {
        Animal temp = animalGrid[x1, y1];  // Swapping two objects requires a temporary variable
        animalGrid[x1, y1] = animalGrid[x2, y2];
        animalGrid[x2, y2] = temp;

        if (animalGrid[x1, y1] != null)
        {
            animalGrid[x1, y1].X = x1;
            animalGrid[x1, y1].Y = y1;
            animalGrid[x1, y1].name = "Animal_" + x1 + "_" + y1;
        }

        if (animalGrid[x2, y2] != null)
        {
            animalGrid[x2, y2].X = x2;
            animalGrid[x2, y2].Y = y2;
            animalGrid[x2, y2].name = "Animal_" + x2 + "_" + y2;
        }
    }
}

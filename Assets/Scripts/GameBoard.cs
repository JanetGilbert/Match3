using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This struct enables mapping the animal sprites to the animal types.
[System.Serializable] // To set a struct in the editor, it must be marked as Serializable.
public struct AnimalDef
{
    public AnimalType type;
    public Sprite sprite;
}

public enum BoardState {Idle, Matching, Falling}; // The states that the board can be in.

/* This is the master control object for the bubble popping game. 
 * It contains and initializes the game pieces (animals)
 * manages the states,
 * and interprets input from the player.
 */
public class GameBoard : MonoBehaviour
{
    // Set in Editor
    [Header("Prefab of animal game piece")]
    public Animal animalPrefab; 
    [Header("Grid size")]
    public int gridX; // Horizontal 
    public int gridY; // Vertical 
    [Header("Space between the animals in grid")]
    public float gridSpacing;
    [Header("How many animals make a match?")]
    public int matchSize = 3;
    [Header("Map between animal types and animal sprites")]
    public AnimalDef[] animalDefs;

    // The board
    private Animal[,] animalGrid;

    // For checking matches
    private bool[,] scratchGrid; // Grid to ensure we don't check a square twice.
    private List<Animal> matchList; // Animals in current match.

    // State
    private BoardState state;

    void Start()
    {
        InitGrid();
        
        matchList = new List<Animal>();
    }


    void Update()
    {
        // Update states.
        if (state == BoardState.Matching) // Matching State
        {
            bool stillMatching = false;

            for (int x = 0; x < gridX; x++)
            {
                for (int y = 0; y < gridY; y++)
                {
                    if (animalGrid[x, y] != null && animalGrid[x, y].State == AnimalState.Matching)
                    {
                        // Destroy animal when shrinking animation is finished.
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
            // Change to falling state when all matched animals have disappeared.
            if (!stillMatching)
            {
                SetBoardFalling();
                FillEmptySquares();
                state = BoardState.Falling;
            }
        }
        else if (state == BoardState.Falling) // Falling state
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

            // Change to idle state when all animals have fallen into place.
            if (!stillFalling)
            {
                
                state = BoardState.Idle;
            }
        }
    }

    // Interact with board by selecting animal object.
    public void Select(int x, int y)
    {
        if (state != BoardState.Idle)
        {
            return; // Can't interact with board unless in idle state.
        }

        // Check whether the animal that was clicked is part of a group of x of the same type.
        ClearMatchScratch();
        CheckMatch(x, y, animalGrid[x, y].Type);

        // If a matching animal group was found, remove them.
        if (matchList.Count >= matchSize)
        {
            foreach (Animal a in matchList)
            {
                a.StartRemoving();
            }

            state = BoardState.Matching;
        }
    }

    // Find the sprite that matches the animal type.
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

    /*********************/
    /* PRIVATE FUNCTIONS */
    /*********************/

    // Create a grid of animal objects and initialize it randomly.
    private void InitGrid()
    {
        scratchGrid = new bool[gridX, gridY];

        // Create random grid of animals.
        animalGrid = new Animal[gridX, gridY];

        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                animalGrid[x, y] = MakeRandomAnimal(x, y);
            }
        }

        // Fit grid to screen.
        CameraFit cameraFit = Camera.main.GetComponent<CameraFit>();
        cameraFit.Fit(new Rect(transform.position.x - (gridSpacing / 2.0f),
                               transform.position.y - (gridSpacing / 2.0f),
                                gridX * gridSpacing,
                                gridY * gridSpacing));

        state = BoardState.Idle; // Start board in idle state.
    }

    // Generate a new random animal.
    private Animal MakeRandomAnimal(int x, int y)
    {
        Animal newAnimal  = Instantiate(animalPrefab, new Vector3(x, y, 0), Quaternion.identity, this.transform);

        newAnimal.name = "Animal_" + x + "_" + y;
        newAnimal.X = x;
        newAnimal.Y = y;
        newAnimal.Type = (AnimalType)Random.Range(0, AnimalType.GetNames(typeof(AnimalType)).Length);
        newAnimal.transform.position = transform.position + new Vector3(x * gridSpacing, y * gridSpacing, 0.0f);

        return newAnimal;
    }

    // This function *recursively* calls itself to build up a list of animals that match the one at (x,y)
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

        scratchGrid[x, y] = true; // Ensure we don't check this square again.

        // Uses flood-fill algorithm to detect matches.
        // https://en.wikipedia.org/wiki/Flood_fill

        CheckMatch(x - 1, y, typeMatch); // *Recursion* is when a function calls itself.
        CheckMatch(x + 1, y, typeMatch);
        CheckMatch(x, y - 1, typeMatch);
        CheckMatch(x, y + 1, typeMatch);

        matchList.Add(animalGrid[x, y]);
    }


    // Clear scratch variables used in match-detecting algorithm.
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


    // Enter board falling state
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
                    // Set animals to fall to fill up the gaps in the grid left by matches.
                    Vector3 start = animalGrid[x, y].transform.position;
                    Vector3 dest = start - new Vector3(0.0f, fallDistance * gridSpacing, 0.0f);
                    animalGrid[x, y].SetFalling(fallDistance, start, dest);
                    SwapAnimalsInGrid(x, y, x, y - fallDistance);
                }
            }
        }
    }

    // Fill up the empty grid squares left by matches.
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
                    // Create new animal.
                    animalGrid[x, y] = MakeRandomAnimal(x, y);

                    // Set new animal falling into grid.
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

        // Update Animal's knowledge of its position in the grid.
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

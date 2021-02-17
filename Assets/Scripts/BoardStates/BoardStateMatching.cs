using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardStateMatching : BoardStateBase
{
    public override void EnterState(GameBoard board)
    {

    }

    public override void Update(GameBoard board)
    {
        bool stillMatching = false;

        for (int x = 0; x < board.gridX; x++)
        {
            for (int y = 0; y < board.gridY; y++)
            {
                if (board.animalGrid[x, y] != null && board.animalGrid[x, y].currentState == board.animalGrid[x, y].stateMatching)
                {
                    // Destroy animal when shrinking animation is finished.
                    if (board.animalGrid[x, y].IsLerpFinished())
                    {
                        GameBoard.Destroy(board.animalGrid[x, y].gameObject);
                        board.animalGrid[x, y] = null;
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
            board.ChangeState(board.stateFalling);
        }
    }

    /*public override void Update(GameBoard board)
    {
        bool stillMatching = false;

        for (int x = 0; x < board.gridX; x++)
        {
            for (int y = 0; y < board.gridY; y++)
            {
                if (board.animalGrid[x, y] != null && board.animalGrid[x, y].State == AnimalState.Matching)
                {
                    // Destroy animal when shrinking animation is finished.
                    if (board.animalGrid[x, y].IsLerpFinished())
                    {
                        GameBoard.Destroy(board.animalGrid[x, y].gameObject);
                        board.animalGrid[x, y] = null;
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
            board.ChangeState(board.stateFalling);
        }
    }*/

    public override void LeaveState(GameBoard board)
    {
        board.SetBoardFalling();
        board.FillEmptySquares();
    }
}

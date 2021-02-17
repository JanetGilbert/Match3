using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardStateFalling : BoardStateBase
{
    public override void EnterState(GameBoard board)
    {
      
    }

    public override void Update(GameBoard board)
    {
        bool stillFalling = false;

        for (int x = 0; x <board.gridX; x++)
        {
            for (int y = 0; y < board.gridY; y++)
            {
                if (board.animalGrid[x, y] != null)
                {
                    if (board.animalGrid[x, y].currentState == board.animalGrid[x, y].stateFalling)
                    {
                        if (!board.animalGrid[x, y].IsLerpFinished())
                        {
                            stillFalling = true;
                        }
                    }
                }
            }
        }

        // Change to idle state when all animals have fallen into place.
        if (!stillFalling)
        {
            board.ChangeState(board.stateIdle);
        }
    }

    public override void LeaveState(GameBoard board)
    {

    }
}

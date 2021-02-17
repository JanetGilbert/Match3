using UnityEngine;

public abstract class BoardStateBase
{
    public abstract void EnterState(GameBoard board);

    public abstract void Update(GameBoard board);

    public abstract void LeaveState(GameBoard board);
}



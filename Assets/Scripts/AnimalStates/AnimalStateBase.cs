using UnityEngine;

public abstract class AnimalStateBase
{
    public abstract void EnterState(Animal animal);

    public abstract void Update(Animal animal);

    public abstract void LeaveState(Animal animal);
}



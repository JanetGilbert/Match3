using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalStateFalling : AnimalStateBase
{
    public override void EnterState(Animal animal)
    {

    }

    public override void Update(Animal animal)
    {
        animal.lerpTime += Time.deltaTime;
        if (animal.lerpTime > animal.lerpTimeMax)
        {
            animal.lerpTime = animal.lerpTimeMax;
            animal.ChangeState(animal.stateIdle);
        }

        animal.transform.position = Vector3.Lerp(animal.lerpOrigin, animal.lerpTarget, animal.lerpTime / animal.lerpTimeMax);
    }

    public override void LeaveState(Animal animal)
    {

    }
}

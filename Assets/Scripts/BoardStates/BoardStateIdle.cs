using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardStateIdle : BoardStateBase
{
    public override void EnterState(GameBoard board)
    {

    }

    public override void Update(GameBoard board)
    {
        if (Input.GetMouseButtonUp(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Animal"))
                {
                    Animal animal = hit.collider.gameObject.GetComponent<Animal>();
                    board.CheckMatch(animal);
                }
            }
        }

    }

    public override void LeaveState(GameBoard board)
    {

    }
}

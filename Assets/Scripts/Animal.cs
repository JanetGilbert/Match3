using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimalType { Cat, Chick, Fox, Mouse, Pig, Rabbit };
public enum AnimalState { Idle, Moving, Matching};

public class Animal : MonoBehaviour
{
    // Getter/Setter with backing variable for type of animal.
    public AnimalType Type
    {
        get
        {
            return _type;
        }
        set
        {
           _type = value;

            sprRenderer.sprite = board.GetAnimalSprite(_type);
        }
    }

    private AnimalType _type;

    // State getter/setter
    public AnimalState State
    {
        get
        {
            return _state;
        }
        set
        {
            _state = value;
        }
    }

    private AnimalState _state;

    public int X
    {
        get
        {
            return _x;
        }
        set
        {
             _x = value;
        }
    }
    private int _x = 0;

    public int Y
    {
        get
        {
            return _y;
        }
        set
        {
            _y = value;
        }
    }
    private int _y = 0;



    // Cache
    private GameBoard board; // A link to the board.
    private SpriteRenderer sprRenderer; 

    // Lerp move
    private Vector3 lerpOrigin;// Where the animal is lerping from.
    private Vector3 lerpTarget;// Where the animal is lerping to.
    private float lerpTime; // Lerp progress
    private float lerpTimeMax; // Lerp maximum

    // Constants
    private const float FALL_LERP_TIME = 0.2f;
    private const float MATCH_LERP_TIME = 0.5f;

    void Awake()
    {
        board = GetComponentInParent<GameBoard>();
        sprRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {

    }

    void Update()
    {
        if (State == AnimalState.Moving)
        {
            lerpTime += Time.deltaTime;
            if (lerpTime > lerpTimeMax)
            {
                lerpTime = lerpTimeMax;
                State = AnimalState.Idle;
            }

            transform.position = Vector3.Lerp(lerpOrigin, lerpTarget, lerpTime / lerpTimeMax);
        }
        else if (State == AnimalState.Matching)
        {
            lerpTime += Time.deltaTime;
            if (lerpTime > lerpTimeMax)
            {
                lerpTime = lerpTimeMax;
              
            }

            transform.localScale = Vector3.Lerp(lerpOrigin, lerpTarget, lerpTime / lerpTimeMax);
        }
    }


    public bool IsLerpFinished()
    {
        return lerpTime == lerpTimeMax;
    }

    private void OnMouseUp()
    {
        board.Select(X, Y);
    }

    public void StartRemoving()
    {
        State = AnimalState.Matching;
        lerpOrigin = transform.localScale;
        lerpTarget = Vector3.zero;
        lerpTime = 0.0f;
        lerpTimeMax = MATCH_LERP_TIME;

    }

    public void SetFalling(int dist, Vector3 start, Vector3 destination)
    {
        State = AnimalState.Moving;
        lerpOrigin = start;
        lerpTarget = destination;
        lerpTime = 0.0f;
        lerpTimeMax = FALL_LERP_TIME * dist;
    }
}

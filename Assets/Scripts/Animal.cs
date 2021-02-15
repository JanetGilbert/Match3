using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimalType { Cat, Chick, Fox, Mouse, Pig, Rabbit }; 
public enum AnimalState { Idle, Moving, Matching};

/* An Animal is a game piece object that is held in an array in GameBoard.
 * Animals may be idle (waiting for clicks),
 * Moving (falling down into place)
 * or Matching (shrinking into nothing after being clicked in a group of 3+)*/
public class Animal : MonoBehaviour
{
    // Getter/Setter for type of animal.
    public AnimalType Type
    {
        get
        {
            return _type;
        }
        set
        {
           _type = value;

            sprRenderer.sprite = board.GetAnimalSprite(_type); // Set correct sprite.
        }
    }

    private AnimalType _type; // Backing variable for Type.

    // Getters/setters
    public AnimalState State { get; set; }
    public int X {get; set;}
    public int Y { get; set; }

    // Cache
    private GameBoard board; // A link to the board.
    // TODO It would be better style if the Animal object doesn't know the GameBoard object exists - how could this be achieved?
    private SpriteRenderer sprRenderer; // Link to the Sprite Renderer.

    // Lerp move
    private Vector3 lerpOrigin;// Where the animal is lerping from.
    private Vector3 lerpTarget;// Where the animal is lerping to.
    private float lerpTime; // Lerp progress
    private float lerpTimeMax; // Lerp maximum

    // Constants
    private const float FALL_LERP_TIME = 0.2f;
    private const float MATCH_LERP_TIME = 0.5f;

    /* Awake runs as soon as the object is instantiated, unlike Start() which runs just before the first Update() loop is called.
       This is important because we instantiate Animal objects in code and access them right away.
       If board and sprRenderer were cached in Start() they would not be available until the next frame when Start() is called.*/
    void Awake()
    {
        // Cache links to useful components
        // This means we only need to get them once, saving time over getting them whenever used.
        board = GetComponentInParent<GameBoard>();
        sprRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {

    }

    void Update()
    {
        // Different behaviour depending on current state.
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

    // Is the current Lerp finished?
    public bool IsLerpFinished()
    {
        return lerpTime >= lerpTimeMax;
    }

    // Triggers when Animal object is clicked.
    private void OnMouseUp()
    {
        board.Select(X, Y);
    }

    // Move to matching state.
    public void StartRemoving()
    {
        State = AnimalState.Matching;
        lerpOrigin = transform.localScale;
        lerpTarget = Vector3.zero;
        lerpTime = 0.0f;
        lerpTimeMax = MATCH_LERP_TIME;

    }

    // Move to falling state.
    public void SetFalling(int dist, Vector3 start, Vector3 destination)
    {
        State = AnimalState.Moving;
        lerpOrigin = start;
        lerpTarget = destination;
        lerpTime = 0.0f;
        lerpTimeMax = FALL_LERP_TIME * dist;
    }
}

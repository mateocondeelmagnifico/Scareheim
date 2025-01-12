using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyMovement : MonoBehaviour
{
    public Vector2 myPos;
    public Movement playerMove;
    public CardSlot initialSlot;
    public GameObject cardGrid;
    private TextManager textManager;
    private Animator animador;
    private TurnCheck turnCounter;
    private GameManager gameManager;

    [SerializeField] private bool isMoving, isSlow;
    private bool hasTalked, hasMoved;

    private Vector2 destination;
    private Vector2 cardGridPos;
    private Vector2 cardActualPos;

    public int turnsUntilStart;

    private void Start()
    {
        textManager = TextManager.Instance;
        animador = GetComponent<Animator>();
        gameManager = GameManager.Instance;
        turnCounter = gameManager.turnCounter;
        TurnInStasis(0);
    }
    private void Update()
    {
        if(isMoving)
        {
            Move();
        }

        #region Talking
        if ((cardGridPos.x == playerMove.myPos.x && (cardGridPos.y == playerMove.myPos.y + 1 || cardGridPos.y == playerMove.myPos.y - 1)) || (cardGridPos.y == playerMove.myPos.y && (cardGridPos.x == playerMove.myPos.x + 1 || cardGridPos.x == playerMove.myPos.x - 1)))
        {
            if(!hasTalked)
            {
                textManager.Talk(TextManager.EnemyStates.NearPlayer);
                hasTalked = true;
            }
            textManager.closeToEnemy = true;
        }
        else
        {
            if(textManager.closeToEnemy && (Mathf.Abs(cardGridPos.x) - Mathf.Abs(playerMove.myPos.x) > 1 || Mathf.Abs(cardGridPos.y) - Mathf.Abs(playerMove.myPos.y) > 1))
            {
                textManager.closeToEnemy = false;
                textManager.SwapSprite();
                textManager.Talk(TextManager.EnemyStates.Annoyed);
            }
            
            hasTalked = false;
        }
        #endregion

        if (turnsUntilStart == 0)
        {
            if(isSlow)
            {
                animador.SetBool("shaking", true);
            }
            else if(!hasMoved)
            {
                animador.SetBool("shaking", true);
            }

            if(!isSlow && hasMoved) animador.SetBool("shaking", false);
        }
        else animador.SetBool("shaking", false);

    }

    // logica del enemigo
    public void EnemyLogic()
    {
        // primero averigua el cardGridPos del CardSlot al que se quiere mover.
        if(playerMove.myPos.y == myPos.y || playerMove.myPos.x == myPos.x)
        {
            #region Not random calculation
            if (playerMove.myPos.y == myPos.y)
            {
                if(playerMove.myPos.x > myPos.x) cardGridPos = new Vector2(myPos.x + 1, myPos.y);
                else cardGridPos = new Vector2(myPos.x - 1, myPos.y); ;
            }

            if (playerMove.myPos.x == myPos.x)
            {
                if(playerMove.myPos.y > myPos.y) cardGridPos = new Vector2(myPos.x, myPos.y + 1);
                else cardGridPos = new Vector2(myPos.x, myPos.y - 1);
            }
            #endregion
        }
        else
        {
            #region Random calculation
            int random = Random.Range(0, 2);

            if(random == 0)
            {
                if(playerMove.myPos.x > myPos.x)
                {
                    cardGridPos = new Vector2(myPos.x + 1, myPos.y);
                }
                else
                {
                    cardGridPos = new Vector2(myPos.x - 1, myPos.y);
                }
            }
            else
            {
                if (playerMove.myPos.y > myPos.y)
                {
                    cardGridPos = new Vector2(myPos.x, myPos.y + 1);
                }
                else
                {
                    cardGridPos = new Vector2(myPos.x, myPos.y - 1);
                }
            }
            #endregion
        }

        // luego obtiene el cardActualPos de CardSlot al que se quiere mover.
        CardSlot destineCard = FindCardSlot(cardGridPos);
        cardActualPos = new Vector2(destineCard.transform.position.x, destineCard.transform.position.y);
    }
    public CardSlot FindCardSlot(Vector2 location)
    {
        CardSlot[] cardSlots = cardGrid.GetComponentsInChildren<CardSlot>();

        foreach (CardSlot cardSlot in cardSlots)
        {
            if (cardSlot.Location == location)
            {
                return cardSlot; // Return the CardSlot with the matching location
            }
        }

        // If the card slot is not found, you can return null or handle it as needed
        return null;
    }

    public void TryMove()
    {
        // EnemyLogic establece cual es el destination del enemigo
        EnemyLogic();
        if (cardGridPos.x <= myPos.x + 1 && cardGridPos.x >= myPos.x - 1 && cardGridPos.y <= myPos.y + 1 && cardGridPos.y >= myPos.y - 1 && !isMoving)
        {
            destination = new Vector3(cardActualPos.x, cardActualPos.y, -0.13f);
            myPos = cardGridPos;
            if(playerMove.myPos.x == myPos.x && playerMove.myPos.y == myPos.y)
            {
                GetComponent<SpriteRenderer>().sortingOrder = 5;
            }
            isMoving = true;
            turnCounter.MoveLeft();
        }
    }

    public void Move()
    {
        hasMoved = true;

        Vector2 pos = new Vector2(transform.position.x, transform.position.y);

        if(pos != destination)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, 4.5f * Time.deltaTime);
        }
        else
        {
            if(gameManager.currentState == GameManager.turnState.Endturn && isMoving)
            {
                gameManager.ChangeState(GameManager.turnState.CheckMovement);
                isMoving = false;

                if (isSlow) TurnInStasis(1);
            }
        }
    }

    public void TurnInStasis(int amount)
    {
        turnsUntilStart += amount;    }
}

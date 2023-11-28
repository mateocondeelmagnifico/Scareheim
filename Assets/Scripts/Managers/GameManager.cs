using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Movement playerMove;
    public bool moveCardToHand, moveCard, cardInformed;

    public int cardDiscarded;

    public GameState State;
    public bool playerTurnInProgress, trapTriggered, powerUpOn, costumeOn;
    private bool winCondition, loseCondition;

    [HideInInspector]
    public enum turnState
    {
        CheckMovement,
        Moving,
        CheckCardEffect,
        ApplyCardEffect,
        Movecard,
        ReplaceCard,
        Endturn
    }

    public turnState currentState;

    public CardManager cardManager;
    public EnemyMovement enemy;

    public int turnCount;

    private List<GameObject> cardsInHand = new List<GameObject>();

    public GameObject player, selectedCardSlot, handSlotPrefab, selectedCard, newCardSlot;

    public Transform deck, discardPile;
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        currentState = turnState.CheckMovement;

        
    }

    void Start()
    {
        playerTurnInProgress = true;
        turnCount = 1;
        updateGameState(GameState.PlayerTurn);
    }

    private void Update()
    {
        if(winCondition)
            updateGameState(GameState.Victory);
        else if(loseCondition)
            updateGameState(GameState.Defeat);
        else if (playerTurnInProgress)
            updateGameState(GameState.PlayerTurn);
        else
            updateGameState(GameState.EnemyTurn);
    }
    public void updateGameState(GameState newState)
    {
        State = newState;
        switch (newState)
        {
            case GameState.PlayerTurn:
                HandlePlayerTurn(currentState);
                break;
            case GameState.EnemyTurn:
                HandleEnemyTurn();
                break;
            case GameState.Victory:
                break;
            case GameState.Defeat:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    private void HandlePlayerTurn(turnState currentState)
    {
        // turno del jugador

        switch (currentState)
        {
            //En muchos estados no pasa nada, pero estan ah�a para las acciones vayan de estado en estado
            case turnState.CheckMovement:
                //Player movement is the one that changes to the next state
            break;

            case turnState.Moving:
                //Also changed in player movement
                break;

            case turnState.CheckCardEffect:
                //This is changed by the card's script
                if(!trapTriggered)
                {
                    InformCard();
                }
                break;

            case turnState.ApplyCardEffect:
                //Changed by the cardSlot
            break;

            case turnState.Movecard:
                if (moveCardToHand || moveCard)
                {
                    if (moveCard)
                        MoveCard(selectedCard, discardPile.position);

                    if(moveCardToHand)
                    MoveCardToHand(newCardSlot);
                }
                else
                {
                    MoveToReplaceCard();
                }
                break;

            case turnState.ReplaceCard:
                if (cardDiscarded > 0)
                {
                    cardManager.DistributeCard();
                }
                MoveCard(selectedCard, selectedCardSlot.transform.position);
                break;

            case turnState.Endturn:
                EndPlayerTurn();
            break;
        }

       

    }
    private void HandleEnemyTurn()
    {
        // turno del enemigo
        if (enemy != null)
        {
            enemy.TryMove();

            if (enemy.myPos == playerMove.myPos) 
                loseCondition = true;
        }
        turnCount++;
        playerTurnInProgress = true;
        currentState = turnState.CheckMovement;
    }
    
    public void EndPlayerTurn()
    {
        turnCount++;
        playerTurnInProgress = false;

        // win cons and lose cons
        // de momento no tenemos la casilla de salida asi que la condicion de victoria es descartar todas las cartas
        if (cardManager.cardsUntilExit == 0)
            winCondition = true;

        // condicion de derrota por hacer, if(fear >= 10)

    }

    private void InformCard()
    {
        //This script tells the card that it has to activate
        selectedCardSlot.transform.GetChild(0).GetComponent<CardObject>().myCard.Effect(selectedCardSlot.transform.GetChild(0).gameObject, handSlotPrefab);
        cardInformed = true;
    }

    private void MoveCard( GameObject whatCard, Vector3 desiredPos)
    {
        //This cript is used to move cards to the deck and discard pile
        whatCard.transform.position = Vector3.MoveTowards(whatCard.transform.position, desiredPos, 8 * Time.deltaTime);
        if(whatCard.transform.position == desiredPos)
        {
            moveCard = false;

            if(desiredPos == discardPile.position)
            {
                whatCard.transform.parent = discardPile;
                currentState = turnState.ReplaceCard;
            }
            if (desiredPos == selectedCardSlot.transform.position)
            {
                currentState = turnState.Endturn;
            }
        }
    }

    public void MoveCardToHand(GameObject card)
    {
        cardsInHand.Add(card);

        SortCardsInHand(card);
        //Vector3 desiredPos = new Vector3(0, -5f, 0);
        //card.transform.position = Vector3.MoveTowards(card.transform.position,desiredPos, 5 * Time.deltaTime);
        //if(card.transform.position == desiredPos)
        //{
        //    moveCard = false;
        //    if(card.GetComponent<CardSlotHand>() != null)
        //    {
        //        //The component is disabled until it arrives to avoid bugs
        //        card.GetComponent<CardSlotHand>().enabled = true;
        //    }
        //}
    }

    public void SortCardsInHand(GameObject card)
    {
        Vector3 desiredPos = new Vector3(0, -5f, -2);
        card.transform.position = desiredPos;

        if (card.transform.position == desiredPos)
        {
            moveCardToHand = false;
            if (card.GetComponent<CardSlotHand>() != null)
            {
                //The component is disabled until it arrives to avoid bugs
                card.GetComponent<CardSlotHand>().enabled = true;
            }
            currentState = turnState.ReplaceCard;
        }

        /*
        for(int i = 0; i < cardsInHand.Count; i++)
        {
            
            //card.transform.position = Vector3.MoveTowards(card.transform.position, desiredPos, 5 * Time.deltaTime);
            
        }
        */
    }

    private void MoveToReplaceCard()
    {
        //Esta funcion esta porque no funciona cambiar el state dentro del switch
        currentState = turnState.ReplaceCard;
    }

    public enum GameState
    {
        PlayerTurn,
            MonsterCardEffect,
            CardToHandEffect,
            CardToDiscardPileEffect,
        EnemyTurn,
        Victory,
        Defeat
    }
}

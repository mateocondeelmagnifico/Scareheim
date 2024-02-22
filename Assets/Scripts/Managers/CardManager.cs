using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance {get; set;}

    public List<GameObject[]> cards = new List<GameObject[]>();

    public GameObject cardsOnBoard, cardPrefab, exitCard;
    private GameObject newCard;
    [SerializeField] private GameObject[] enviroments, enviroments2, enviroments3, treats, costumes;

    public int cardsUntilExit, treatAmount, costumeAmount;
    private int resetTimes;

    public bool exitCardDealt;
    private bool powerUpDealt, canEnd;
    private GameManager gameManager;
    private CardSlot cardSlot;
    public TMPro.TextMeshProUGUI doorText;

    private Transform playerPos;
    public Transform tricks;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        gameManager = GameManager.Instance;
        playerPos = gameManager.player.transform;
        doorText.text = cardsUntilExit.ToString();

        #region Create Cards List
        cards.Add(enviroments);
        cards.Add(enviroments2);
        cards.Add(enviroments3);
        cards.Add(treats);
        cards.Add(costumes);
        #endregion
    }
    private void Start()
    {
        #region Distribute Tricks
        Vector3[] assignedPositions = new Vector3[tricks.childCount];

        for (int i = 0; i < cardsOnBoard.transform.childCount; i++)
        {
            //cada casilla tiene una probabilidad sobre 3 de tener una trampa
            //reset times esta para evitar bucles infinitos
            if (Random.Range(0, 7) + resetTimes > 6)
            {
                for (int e = 0; e < assignedPositions.Length; e++)
                {
                    if (assignedPositions[e] == Vector3.zero)
                    {
                        //Chekea si el lugar tiene un enemigo, o si ahi esta el jugador
                        Transform chosenCard = cardsOnBoard.transform.GetChild(i);
                        canEnd = false;

                        if (!chosenCard.GetChild(0).CompareTag("Enemy") && (chosenCard.position.x != playerPos.position.x && chosenCard.position.y != playerPos.position.y))
                        {
                            bool canplace = true;
                            if ((e == 1 && chosenCard.position == assignedPositions[e - 1]) || (e == 2 && (chosenCard.position == assignedPositions[e - 2] || chosenCard.position == assignedPositions[e - 1])))
                            {
                                canplace = false;
                            }

                            //If you were to place two traps in the same position
                            if (canplace)
                            {
                                assignedPositions[e] = chosenCard.position;
                                tricks.GetChild(e).position = assignedPositions[e];
                            }
                        }
                    }
                    else if (e == assignedPositions.Length - 1)
                    {
                        canEnd = true;
                    }
                }
            }

            if (resetTimes > 5)
            {
                Debug.Log("infinite");
                canEnd = true;
            }

            if (!canEnd)
            {
                if (i == cardsOnBoard.transform.childCount - 1)
                {
                    i = 0;
                    resetTimes++;
                }
            }
            else
            {
                i = cardsOnBoard.transform.childCount;
            }
        }
        #endregion
    }

    public void CardDiscarded(CardSlot whatSlot)
    {
        //cardSlot = whatSlot;
        gameManager.slotToReplaceNew = whatSlot.gameObject;
        doorText.text = cardsUntilExit.ToString();
    }
    public void DistributeCard()
    {
        //Called by gameManager
        cardSlot = gameManager.slotToReplaceOld.GetComponent<CardSlot>();

        #region Create card
        if (cardsUntilExit == 0 && !exitCardDealt)
        {
            //distance < 4
            List<GameObject> board = new List<GameObject>();
             
            for(int i = 0; i < cardsOnBoard.transform.childCount; i++)
            {
                //Get all the cards that are far away from the player
               if(Vector2.Distance(cardsOnBoard.transform.GetChild(i).position, playerPos.position) >= 4)
               {
                   board.Add(cardsOnBoard.transform.GetChild(i).gameObject);
               }
            }

            #region Replace Card with Exit Card
            int randomNum = Random.Range(0, board.Count);
            Destroy(board[randomNum].transform.GetChild(0).gameObject);
            newCard = Instantiate(exitCard, board[randomNum].transform);
            newCard.transform.parent = board[randomNum].transform;
            board[randomNum].GetComponent<CardSlot>().cardObject = newCard;
            #endregion

            ReplaceCard();
            exitCardDealt = true;
        }
        else
        {
            ReplaceCard();
        }
        #endregion

        #region Assign card
        cardSlot.cardObject = newCard;
        newCard.transform.parent = cardSlot.gameObject.transform;
        gameManager.newCard = newCard;
        #endregion
    }

    private void ReplaceCard()
    {
        //this selects an individual card within card arrays
        //The first array is for enviroments, second for treats, third for costumes
        int randomInt = 0;
        if (powerUpDealt)
        {
            //This is so you don't get two treats or costumes in a row
            randomInt = Random.Range(0, 3);
            powerUpDealt = false;
        }
        else
        {
            randomInt = Random.Range(0, cards.Count);
        }
        GameObject[] chosenArray = cards[randomInt];
        int newRandom = Random.Range(0, chosenArray.Length);

        newCard = Instantiate(chosenArray[newRandom], gameManager.deck);

        #region Check if it has run out of treats or costumes
        if (cards.Count == 4)
        {
            if (randomInt == 3)
            {
                if (costumeAmount > 0)
                {
                    costumeAmount--;
                    if (costumeAmount <= 0)
                    {
                        cards.RemoveAt(3);
                    }

                }
                if (treatAmount > 0)
                {
                    treatAmount--;
                    if (costumeAmount <= 0)
                    {
                        cards.RemoveAt(3);
                    }
                }

                powerUpDealt = true;
            }
        }
        if (cards.Count == 5)
        {
            if (randomInt == 3)
            {
                treatAmount--;
                if (treatAmount <= 0)
                {
                    cards.RemoveAt(3);
                }
                powerUpDealt = true;
            }
            if (randomInt == 4)
            {
                costumeAmount--;
                if (costumeAmount <= 0)
                {
                    cards.RemoveAt(4);
                }
                powerUpDealt = true;
            }
        }
        #endregion
    }

}

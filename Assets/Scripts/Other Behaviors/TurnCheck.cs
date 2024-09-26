using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TurnCheck : MonoBehaviour
{
    [SerializeField] private List<Transform> positions;
    [SerializeField] private Transform startPos;
    [SerializeField] private Sprite enemySprite, playerSprite, costumeSprite;

    [SerializeField] private int moveFrequency;
    public int turnsUntilEnemy;
    private int costumeTurns;

    private bool move;
    private bool[] hasArrived = new bool[5];


    private void Update()
    {
        //Move when turn progresses
        if (move)
        {
            for (int i = 4; i >= 0; i--)
            {
               //Move towards objective and decelerate when near
               Vector3 currentDestination = positions[i].GetComponent<DestinationInfo>().destination;
                if (positions[i].position != currentDestination) positions[i].position = Vector3.MoveTowards(positions[i].position, currentDestination, 1 * Time.deltaTime);
                else hasArrived[i] = true;             
            }

            #region Check if all have arrived
            int count = 0;

            for(int i = 0; i < hasArrived.Length; i++)
            {
                if (hasArrived[i]) count++;
            }

            if (count == 5) move = false;
            #endregion
        }
    }

    public void MoveLeft()
    {
        for(int i = 4; i >= 0; i--)
        {
            #region Inform images where to go
            if (i == 0)
            {
                Transform firstIcon = positions[i];
                firstIcon.position = startPos.position;
                firstIcon.GetComponent<DestinationInfo>().destination = startPos.position;
                positions.RemoveAt(0);
                positions.Add(firstIcon);     
            }
            else positions[i].GetComponent<DestinationInfo>().destination = new Vector3(positions[i].position.x - 0.66f, positions[i].position.y, positions[i].position.z);
            #endregion

            #region Decide if it's either player or enemy sprite
            if (i == 4)
            {
                if (turnsUntilEnemy > 0)
                {
                    if (costumeTurns <= 0) positions[i].GetComponent<SpriteRenderer>().sprite = playerSprite;
                    else
                    {
                        positions[i].GetComponent<SpriteRenderer>().sprite = costumeSprite;
                        costumeTurns--;
                    }
                    turnsUntilEnemy--;
                }
                else
                {
                    positions[i].GetComponent<SpriteRenderer>().sprite = enemySprite;
                    turnsUntilEnemy = moveFrequency;
                }
            }
            #endregion
        }

        #region Command Move
        for (int i = 0; i < hasArrived.Length; i++)
        {
            hasArrived[i] = false;
        }
        move = true;
        #endregion
    }

    public void DisplayCostumeTurns(bool takeOff)
    {
        //Display or hide costume turns

        if(takeOff) costumeTurns = 0;
        else costumeTurns = 3;

        for (int i = 0; i < positions.Count; i++)
        {
            if(i >0 && i < 4 && positions[i].GetComponent<SpriteRenderer>().sprite != enemySprite)
            {
                if (!takeOff)
                {
                    positions[i].GetComponent<SpriteRenderer>().sprite = costumeSprite;
                    costumeTurns--;
                }
                else positions[i].GetComponent<SpriteRenderer>().sprite = playerSprite;
            }
        }
    }


}

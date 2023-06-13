using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardControlled : MonoBehaviour
{
    public void selectCard(){
        gameObject.transform.parent.gameObject.GetComponent<CardController>().selectCard(gameObject);
    }
}

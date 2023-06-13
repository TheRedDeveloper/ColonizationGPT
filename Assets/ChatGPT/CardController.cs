using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Febucci.UI;

public class CardController : MonoBehaviour
{
    public ChatGPT chatgpt;
    public GameObject cardPrefab;
    public string selcted;

    public void readCards(){
        foreach (Transform child in gameObject.transform) {
            GameObject.Destroy(child.gameObject);
        }
        foreach(string card in chatgpt.currentPlayer.cards) {
            GameObject instance = Instantiate(cardPrefab, gameObject.transform);
            //instance.transform.SetParent(gameObject.transform);
            instance.name = card;
            instance.GetComponentInChildren<TextAnimator>().SetText(card, false);
            instance.GetComponent<Toggle>().group = gameObject.GetComponent<ToggleGroup>();
        }
    }

    public void selectCard(GameObject card){
        selcted = card.name;
    }
}

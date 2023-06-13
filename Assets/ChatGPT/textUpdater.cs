using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Febucci.UI;

public class textUpdater : MonoBehaviour
{
    public TextAnimator textAnimator;
    public ChatGPT chatgpt;
    public CardController cardController;
    public GlobeController globeController;
    public string Text;

    void Update()
    {
        string newText = $"<bounce>{chatgpt.currentPlayer.name}</bounce> uses <bounce>{cardController.selcted}</bounce> in <bounce>{globeController.selectedCountry.name}</bounce> to";
        if(newText != Text){
            Debug.Log("CHANGE");
            Text = newText;
            textAnimator.SetText(newText, false);
        }
    }
}

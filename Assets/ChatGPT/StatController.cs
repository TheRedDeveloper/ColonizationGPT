using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Febucci.UI;

public class StatController : MonoBehaviour
{
    public ChatGPT chatgpt;
    public TextAnimator name;
    public Slider Trustworthiness;
    public Slider Military;
    public Slider Prosperity;
    public Slider Stability;
    public Slider Happiness;

    public void readStats()
    {
        Debug.Log("Updating Stats");
        name.SetText(chatgpt.currentPlayer.name, false);
        Trustworthiness.value = chatgpt.currentPlayer.Trustworthiness/100;
        Military.value = chatgpt.currentPlayer.Military/100;
        Prosperity.value = chatgpt.currentPlayer.Prosperity/100;
        Stability.value = chatgpt.currentPlayer.Stability/100;
        Happiness.value = chatgpt.currentPlayer.Happiness/100;
    }
}

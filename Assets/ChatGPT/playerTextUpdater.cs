using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Febucci.UI;

public class playerTextUpdater : MonoBehaviour
{
    public TextAnimator textAnimator;
    public TMPro.TMP_Text inputText;
    public GlobeController globeController;
    public string Text;

    void Update()
    {
        string name = inputText.text;
        string country = globeController.selectedCountry.name;
        if(name.Length == 1) name = "SELECT NAME";
        if(country == "space") country = "SELECT STARTING COUNTRY";
        string newText = $"<bounce>{name}</bounce> starts in <bounce>{country}</bounce>";
        if(newText != Text){
            Debug.Log("CHANGE");
            Text = newText;
            textAnimator.SetText(newText, false);
        }
    }
}

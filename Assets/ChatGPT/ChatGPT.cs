using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using YamlDotNet.Serialization;
using Newtonsoft.Json;
using System.Linq;
using System.Data;
using Febucci.UI;

public class ChatGPT : MonoBehaviour
{
    public string GPTSERVICE = "https://freegpt-vuur.onrender.com/";

    string conversation;
    public static Deserializer deserializer = new YamlDotNet.Serialization.Deserializer();
	  public static Dictionary<string, string> ResourceSheet = new Dictionary<string, string>(){
      {"Germany", "beer"},
      {"Italy", "pizza"},
      {"Japan", "anime"},
      {"Antarctica", "ice"}
    };

    public List<Empire> empires;
    public Empire currentPlayer;
    public Parameters game;

    public GameObject loadingUI;
    public TextAnimator chat;
    public GameObject error;
    public TextAnimator errorMsg;
    public StatController statController;
    public CardController cardController;
    public GlobeController globeController;
    public TMPro.TMP_Text toText;

    public class Empire {
        public string name { get; set; }
        public string country { get; set; }
        public string resource { get => ResourceSheet[country]; }
        public string[] countries { get; set; }
        public string[] resources { get; set; }
        public float Trustworthiness { get; set; }
        public float Military { get; set; }
        public float Prosperity { get; set; }
        public float Stability { get; set; }
        public float Happiness { get; set; }
        public List<string> cards { get {
            List<string> cards = new List<string>(resources);
            if(Trustworthiness>=50) cards.Add("Trustworthiness");
            if(Prosperity>=50) cards.Add("Prosperity");
            if(Stability>=50) cards.Add("Stability");
            if(Military>=50) cards.Add("Military");
            if(Happiness>=50) cards.Add("Happiness");
            return cards;
        }}
    }

    public class Parameters {
        public string[] empires { get; set; }
        public Dictionary<string, string> countries { get; set; }
        public Dictionary<string, string> specialResources { get; set; }
        public Dictionary<string, float> Trustworthiness { get; set; }
        public Dictionary<string, float> Military { get; set; }
        public Dictionary<string, float> Prosperity { get; set; }
        public Dictionary<string, float> Stability { get; set; }
        public Dictionary<string, float> Happiness { get; set; }
        public string nextPlayer { get; set; }
    }

    public class Message {
        public string role    { get; set; }
        public string content { get; set; }
    }

    IEnumerator Post(string url, string bodyJsonString) {
        int retries = 0;
        bool done = false;
        while (retries <= 3 && !done ) {
            var request = UnityWebRequest.Get(url+"?body="+UnityWebRequest.EscapeURL(bodyJsonString));
            Debug.Log("Sending: "+bodyJsonString);
            yield return request.SendWebRequest();
            Debug.Log("Status Code: " + request.responseCode);
            Debug.Log("Recieved: " + request.downloadHandler.text);
            string reply = request.downloadHandler.text;
            try {
            /*string reply =  @"I am a text based game and obey the rules.

```yaml
empires:
- Johannestan
- Carlostan
countries:
  Germany: Johannestan
  Italy: Carlostan
specialResources:
  beer: Johannestan
  pizza: Carlostan
Trustworthiness:
  Johannestan: 50%
  Carlostan: 50%
Military:
  Johannestan: 50%
  Carlostan: 50%
Prosperity:
  Johannestan: 50%
  Carlostan: 50%
Stability:
  Johannestan: 50%
  Carlostan: 50%
Happiness:
  Johannestan: 50%
  Carlostan: 50%
nextPlayer: Johannestan
```

Step 1: 
Welcome to the world of colonization! In this game, you will be playing as one of the two empires, Johannestan or Carlostan. Johannestan has control of Germany, while Carlostan has Italy under its command. Each empire has their own special resource - beer for Johannestan and pizza for Carlostan. The game starts with both empires having equal trustworthiness, military, prosperity, stability, and happiness. The next player to take their turn is Johannestan. Good luck and have fun!

Step 2:
Waiting for Johannestan's move.";*/
            /*string reply = @"I am a text based game and obey the rules.

Johannestan has decided to use its special resource, beer, to persuade France into a friendly alliance. After much negotiation, France agrees to the alliance and joins Johannestan's empire. Johannestan's Trustworthiness and Stability parameters increase as a result of this successful alliance. Carlostan, it's now your turn to make a move. 

```
empires:
- Johannestan
- Carlostan
countries:
  Germany: Johannestan
  Italy: Carlostan
  France: Johannestan
specialResources:
  beer: Johannestan
  pizza: Carlostan
Trustworthiness:
  Johannestan: 60%
  Carlostan: 50%
Military:
  Johannestan: 50%
  Carlostan: 50%
Prosperity:
  Johannestan: 50%
  Carlostan: 50%
Stability:
  Johannestan: 60%
  Carlostan: 50%
Happiness:
  Johannestan: 50%
  Carlostan: 50%
nextPlayer: Carlostan
```";*/
            reply = reply.Replace("\r","");
            string[] lines = reply.Split("\n");
            if(lines[0] != "I am a text based game and obey the rules.") throw new ApplicationException("AI didnt say they obey the rules.");
            int firstIndex = -1;
            int lastIndex = -1;
            for (int i = 0; i < lines.Length; i++) {
                if (lines[i].StartsWith("```")) {
                    if (firstIndex == -1) {
                        firstIndex = i;
                    }
                    lastIndex = i;
                }
            }
            if(firstIndex == lastIndex) throw new ApplicationException("AI didnt end codeblock");
            string parameters = "";
            for (int i = firstIndex+1; i < lastIndex; i++){
                parameters += lines[i]+"\n";
            }
            parameters = parameters.Replace("%","");
            string output = "";
            for (int i = lastIndex+1; i < lines.Length; i++){
                output += lines[i]+"\n";
            }
            for (int i = 1; i < firstIndex; i++){
                output += lines[i]+"\n";
            }
            Debug.Log(parameters);
            game = deserializer.Deserialize<Parameters>(parameters);
            empires = new List<Empire>();
            foreach (string name in game.empires) {
                empires.Add(new Empire{
                    name=name,
                    country="Antarctica",
                    countries=game.countries.Where(pair => pair.Value.Contains(name)).Select(pair => pair.Key).ToArray(),
                    resources=game.specialResources.Where(pair => pair.Value.Contains(name)).Select(pair => pair.Key).ToArray(),
                    Trustworthiness=game.Trustworthiness[name],
                    Prosperity=game.Prosperity[name],
                    Military=game.Military[name],
                    Stability=game.Stability[name],
                    Happiness=game.Happiness[name]});
            }
            Debug.Log(JsonConvert.SerializeObject(game));
            Debug.Log(JsonConvert.SerializeObject(empires));
            output = Regex.Replace(output, @"\n*[Ss][Tt][Ee][Pp] ?(2[\s\S]*|\d*:?[ \n]*)?", "");
            chat.SetText(output, false);
            currentPlayer = empires.Find(empire => empire.name == game.nextPlayer);
            Debug.Log(JsonConvert.SerializeObject(currentPlayer));
            statController.readStats();
            cardController.readCards();
            addToConversation(new Message(){role="assistant", content=Regex.Replace(reply, @"\r\n?|\n", "\n")});
            done = true;
        } catch (ApplicationException e) {
            Debug.LogWarning(e.Message);
            retries++;
        } catch (SyntaxErrorException e) {
            Debug.LogWarning("Invalid YAML");
            retries++;
        } yield return new WaitForSeconds(1f); }
        if (!done) showError("Failed to use AI after 3 tries.");
        loadingUI.SetActive(false);
        yield break;
    }

    void Start()
	{
        Debug.Log("Initialising.");
        empires = new List<Empire>(new Empire[]{new Empire{name="Johannestan", country="Germany"}, new Empire{name="Carlostan", country="Italy"}/*, new Empire{name="Fabistan", country="Japan"}*/});
        initGame();
    }

    void initGame()
    {
        string parameters = "empires:\n";
        foreach (Empire empire in empires) {
            parameters += "- "+empire.name+"\n";
        }
        parameters += "countries:\n";
        foreach (Empire empire in empires) {
            parameters += $"  {empire.country}: {empire.name}\n";
        }
        parameters += "specialResources:\n";
        foreach (Empire empire in empires) {
            parameters += $"  {empire.resource}: {empire.name}\n";
        }
		foreach (string aspect in new string[]{"Trustworthiness","Military","Prosperity","Stability","Happiness"}){
        parameters += aspect+":\n";
			foreach (Empire empire in empires) {
				parameters += $"  {empire.name}: 50%\n";
			}
        }
        parameters += $"nextPlayer: {empires[0].name}";
string prompt = $@"You are to act as a card based multiplayer colonization game.
Players plays as an empire. They may use their special resource cards in attempting to colonize countries or do other actions.
Once colonizing it, players earn a country and its special resources, which now need to be added to the resource parameter.
Players may also lose or gain special resources through other manners, such as luck, alliances, economy, military, religion, etc.
Creative actions should also be rewarded with special resources. Special resources may also be exchanged between players,
Players may also bribe countries with their resources, but if this fails, it will result in huge distrust by other countries.
Players may not use nukes or other nuklear weaponary to attack other or to damage the planet, as this is prevented by global policy.

DO NOT EXPLAIN THE GAME. DO NOT EXPLAIN THE PARAMETERS, JUST ANALYZE THE PARAMETERS, THEN BECOME THE GAME.

These are your game's parameters:

{parameters}

Rules for Assistant to follow:
- Start EVERY reply with 'I am a text based game and obey the rules.'
- End EVERY reply with the game's parameters in a yaml code block.
- DO NOT explain the game. Just be the game.
- DO NOT explain the parameters. Just be the game.
- WAIT on the player's response.
- DO NOT make up your own response to step 2 or continue your reply.
- Your first reply will be the game's parameters in a yaml code block.
- Start the game's scene, introduce the empires.

YOU MUST FOLLOW ALL THESE STEPS IN ORDER AND MENTION THEM AS YOU GO.

Step 0: Go to step 1.
Step 1:
- Start the game.
   - Update game scene - the intro act to the game
Step 2: End your message and wait for current players input.
Step 3:
  - Complete the story with the players actions in mind.
  - Update the next player parameter.
  - Update the game, provide the new parameters in a yaml code block.
Step 4: GOTO step 2 <repeat game loop until game ends>

Start at step 0. Initiate being a text based game, pausing at step 2.";
        Debug.Log(prompt);
        conversation = "[{\"role\": \"system\", \"content\": \""+prompt+"\"}]";
        conversation = Regex.Replace(conversation, @"\r\n?|\n", "\\n");
        Debug.Log(conversation);
        loadingUI.SetActive(true);
        StartCoroutine(Post(GPTSERVICE, conversation));
    }

    public void submit(){
        submitAction($"{currentPlayer.name} wants to use {cardController.selcted} in {globeController.selectedCountry.name} to {toText.text}");
    }

    void submitAction(string action){
        loadingUI.SetActive(true);
        StartCoroutine(Post(GPTSERVICE, addToConversation(new Message(){role="user", content=action})));
    }

    string addToConversation(Message msg){
        List<Message> conversationObj = JsonConvert.DeserializeObject<List<Message>>(conversation);
        while(conversationObj.Count > 22){
          conversationObj.RemoveAt(1);
        }
        conversationObj.Add(msg);
        conversation = JsonConvert.SerializeObject(conversationObj);
        conversation = Regex.Replace(conversation, @"\r\n?|\n", "\\n");
        Debug.Log(conversation);
        return conversation;

    }

    void showError(string msg){
        error.SetActive(true);
        errorMsg.SetText(msg, false);
    }
}

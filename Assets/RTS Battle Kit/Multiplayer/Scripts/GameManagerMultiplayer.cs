using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameManagerMultiplayer : NetworkBehaviour {
	
	//variables visible in the inspector
	public float fadespeed;
	public GameObject cam;
	
	//not visible in the inspector
	private GameObject leftCastleStrengthText;
	private GameObject rightCastleStrengthText;
	private GameObject leftCastleStrengthBar;
	private GameObject rightCastleStrengthBar;
	
	private float player1CastleStrengthStart;
	private float player2CastleStrengthStart;
	
	private float player1CastleStrength;
	private float player2CastleStrength;
	
	public static GameObject connectionPanel;
	
	private bool fading;
	private float alpha;
	public static bool battleEnded;
	
	private GameObject battleResultPanel;
	private GameObject battleResultTitle;
	private Text battleResultDuration;
	private Text battleResultKilledEnemies;
	private Text battleResultKilledAllies;
	
	private float battleDuration;
	
	void Start(){
		
		//rotate the castle according to it's position at the battle ground
		if(transform.position.x < 0){
			transform.localEulerAngles = new Vector3(0, 180, 0);
		}
		else{
			transform.localEulerAngles = new Vector3(0, 0, 0);
		}
		
		//find the flag objects to color them later on
		GameObject[] flags = GameObject.FindGameObjectsWithTag("Flag");
		
		//if this is not the local castle:
		if(!isLocalPlayer){
			//if player one's castle exists in the scene
			if(GameObject.FindWithTag("Player 1 castle")){
				//tag this castle 'player 2 castle' since this is player 2
				foreach(Transform castlePart in transform){
					if(!castlePart.gameObject.GetComponent<Camera>()){
						castlePart.gameObject.tag = "Player 2 castle";
					}
				}
				//color the flags of this player blue (client color)
				foreach(GameObject flag in flags){
					if((flag.transform.position.x < 0 && transform.position.x < 0) || (flag.transform.position.x > 0 && transform.position.x > 0)){
						flag.GetComponent<SkinnedMeshRenderer>().material.color = Color.blue;
					}
				}
			}
			else{
				//if player 1 castle doesn't exist yet, this one must be player 1... so tag it 'player 1 castle'
				foreach(Transform castlePart in transform){
					if(!castlePart.gameObject.GetComponent<Camera>()){
						castlePart.gameObject.tag = "Player 1 castle";
					}
				}
				//color the flags of this player red (player 1/host color)
				foreach(GameObject flag in flags){
					if((flag.transform.position.x < 0 && transform.position.x < 0) || (flag.transform.position.x > 0 && transform.position.x > 0)){
						flag.GetComponent<SkinnedMeshRenderer>().material.color = Color.red;
					}
				}
			}
		}
		
		//don't execute the following code unless this is the local player
		if(!isLocalPlayer)
			return;
		
		//find the castle strength ui objects
		leftCastleStrengthText = GameObject.Find("Left castle strength text");
		rightCastleStrengthText = GameObject.Find("Right castle strength text");
		leftCastleStrengthBar = GameObject.Find("Left castle strength bar");
		rightCastleStrengthBar = GameObject.Find("Right castle strength bar");
		
		//find the panel which appears when this player is not battleing yet
		connectionPanel = GameObject.Find("Connection");
		
		//find some other ui objects
		battleResultPanel = GameObject.Find("Battle result");
		battleResultTitle = GameObject.Find("result title");
		battleResultDuration = GameObject.Find("duration").GetComponent<Text>();
		battleResultKilledEnemies = GameObject.Find("killed enemies").GetComponent<Text>();
		battleResultKilledAllies = GameObject.Find("killed allies").GetComponent<Text>();
		
		//don't show the battle result 
		battleResultPanel.SetActive(false);
		
		//battle has not ended
		alpha = connectionPanel.GetComponent<CanvasGroup>().alpha;
		battleEnded = false;
		
		//get the start strength of the castles
		GetCastleStrength();
		
		//set start castle strengths (both 'player1CastleStrength' for when player 2 has not connected... the castles are the same anyway)
		player1CastleStrengthStart = player1CastleStrength;
		player2CastleStrengthStart = player1CastleStrength;	
	}
	
	void Update(){
		//don't execute the following code unless this is the local player
		if(!isLocalPlayer)
			return;
		
		//if not both castles are present in scene, freeze game
		if(FindObjectsOfType<GameManagerMultiplayer>().Length != 2){
			Time.timeScale = 0;
		}
		else{ //when both castles are present...
		
		//count battle length
		if(!battleEnded){
		battleDuration += Time.deltaTime;
		}
		
		//start fading out the connection panel
		if(connectionPanel.activeSelf && !fading){
			Time.timeScale = 1;
			fading = true;
			//don't show hud while battleing
			NetworkManager.singleton.gameObject.GetComponent<NetworkManagerHUD>().showGUI = false;
		}
		
		//fade out
		if(fading && alpha > 0){
		alpha -= Time.deltaTime * fadespeed;
		
		connectionPanel.GetComponent<CanvasGroup>().alpha = alpha;
		}
		else if(alpha <= 0){
		//remove panel when alpha is 0
		fading = false;
		connectionPanel.SetActive(false);		
		}
		
		//keep updating castle strengths
		GetCastleStrength();
		
		if(GetComponent<CharacterManagerMultiplayer>().host){
			//show the castle strengths
			leftCastleStrengthText.GetComponent<Text>().text = "" + (int)player1CastleStrength;
			rightCastleStrengthText.GetComponent<Text>().text = "" + (int)player2CastleStrength;
		
			//set the fillamount (round bar) to the percentage of castles left
			rightCastleStrengthBar.GetComponent<Image>().fillAmount = player2CastleStrength/player2CastleStrengthStart;
			leftCastleStrengthBar.GetComponent<Image>().fillAmount = player1CastleStrength/player1CastleStrengthStart;
			
			//if this is player 1, show red color on the left
			rightCastleStrengthBar.GetComponent<Image>().color = Color.blue;
			leftCastleStrengthBar.GetComponent<Image>().color = Color.red;
			
			//if one of the players lost the battle, end the battle
			if(player1CastleStrength <= 0 || player2CastleStrength <= 0){
				RpcEndBattle(player1CastleStrength, player2CastleStrength, battleDuration, 
				GetComponent<CharacterManagerMultiplayer>().player1kills, GetComponent<CharacterManagerMultiplayer>().player2kills);
			}
		}
		else{
			//show the castle strengths
			leftCastleStrengthText.GetComponent<Text>().text = "" + (int)player2CastleStrength;
			rightCastleStrengthText.GetComponent<Text>().text = "" + (int)player1CastleStrength;
		
			//set the fillamount (round bar) to the percentage of castles left
			rightCastleStrengthBar.GetComponent<Image>().fillAmount = player1CastleStrength/player1CastleStrengthStart;
			leftCastleStrengthBar.GetComponent<Image>().fillAmount = player2CastleStrength/player2CastleStrengthStart;
			
			//if this is player 2, show its blue color on the left
			rightCastleStrengthBar.GetComponent<Image>().color = Color.red;
			leftCastleStrengthBar.GetComponent<Image>().color = Color.blue;
		}
		
		}
		
		//show panel when battle has ended
		if(battleEnded && battleResultPanel.GetComponent<CanvasGroup>().alpha < 1){
			battleResultPanel.GetComponent<CanvasGroup>().alpha += Time.deltaTime * fadespeed;
		}
		//if battle has ended and the panel is visible, freeze game and disconnect player
		else if(battleEnded && Time.timeScale != 0){
			Time.timeScale = 0;
			if(NetworkServer.active){
				NetworkManager.singleton.StopHost();
			}
		}
	}
	
	void GetCastleStrength(){
	//castle strength is 0
	player1CastleStrength = 0;
	player2CastleStrength = 0;
	
	//add the strength of each client castle
	foreach(GameObject player2Castle in GameObject.FindGameObjectsWithTag("Player 2 castle")){
		if(player2Castle.GetComponent<Castle>().lives > 0){
			player2CastleStrength += player2Castle.GetComponent<Castle>().lives;
		}
	}
	//add the strength of each host castle
	foreach(GameObject player1Castle in GameObject.FindGameObjectsWithTag("Player 1 castle")){
		if(player1Castle.GetComponent<Castle>().lives > 0){
			player1CastleStrength += player1Castle.GetComponent<Castle>().lives;
		}
	}	
	}
	
	//on start of new local player
	public override void OnStartLocalPlayer(){
		
		//find flag objects to color them
		GameObject[] flags = GameObject.FindGameObjectsWithTag("Flag");
		
		//if the other player isn't connected to the game yet
		if(FindObjectsOfType<GameManagerMultiplayer>().Length != 2){
			//this is the host, so tag the castle 'player 1 castle'
			foreach(Transform castlePart in transform){
				if(!castlePart.gameObject.GetComponent<Camera>()){
					castlePart.gameObject.tag = "Player 1 castle";
				}
			}
			//color the flags red
			foreach(GameObject flag in flags){
				if((flag.transform.position.x < 0 && transform.position.x < 0) || (flag.transform.position.x > 0 && transform.position.x > 0)){
					flag.GetComponent<SkinnedMeshRenderer>().material.color = Color.red;
				}
			}
			
			//make sure we know this is the host gamemanager
			GetComponent<CharacterManagerMultiplayer>().host = true;
		}
		else{
			//if this is the client, tag this castle 'player 2 castle'
			foreach(Transform castlePart in transform){
				if(!castlePart.gameObject.GetComponent<Camera>()){
					castlePart.gameObject.tag = "Player 2 castle";
				}
			}
			//color the flags blue
			foreach(GameObject flag in flags){
				if((flag.transform.position.x < 0 && transform.position.x < 0) || (flag.transform.position.x > 0 && transform.position.x > 0)){
					flag.GetComponent<SkinnedMeshRenderer>().material.color = Color.blue;
				}
			}
		}
		
		//this is the local player, so let's enable the camera
		cam.SetActive(true);
	}
	
	//server lets both clients know the must end the battle
	[ClientRpc]
	void RpcEndBattle(float player1Castle, float player2Castle, float duration, int player1kills, int player2kills){
		Camera.main.gameObject.transform.root.gameObject.GetComponent<GameManagerMultiplayer>().endBattle(player1Castle, player2Castle, duration, player1kills, player2kills);
	}
	
	//end battle
	public void endBattle(float player1Castle, float player2Castle, float duration, int player1kills, int player2kills){
		
		bool host = GetComponent<CharacterManagerMultiplayer>().host;
		
		//if this is the local player game manager and the battle didn't already end
		if(!battleEnded && isLocalPlayer){
			//set battle result panel alpha to 0, so it can fade in
			battleResultPanel.GetComponent<CanvasGroup>().alpha = 0;
			
			//if this is player1 and player1's castle has been destroyed, or this is player2 and player2's castle has been destroyed, show the DEFEAT text
			if((player1Castle <= 0 && host) || (player2Castle <= 0 && !host)){
				battleResultPanel.GetComponent<Image>().color = new Color(0.9f, 0.2f, 0.2f, 0.95f);
				battleResultTitle.GetComponent<Text>().text = "DEFEAT";
			}
			//if this is player2 and player1's castle has been destroyed, or this is player1 and player2's castle has been destroyed, show the VICTORY text
			else if((player1Castle <= 0 && !host) || (player2Castle <= 0 && host)){
				battleResultPanel.GetComponent<Image>().color = new Color(0, 0.7f, 0.8f, 0.95f);
				battleResultTitle.GetComponent<Text>().text = "VICTORY";
			}
			
			//show the correct amount of kills/killed allies based on the player1 & player2 kills
			if(host){
				battleResultKilledEnemies.text = "" + player1kills;
				battleResultKilledAllies.text = "" + player2kills;
			}
			else{
				battleResultKilledEnemies.text = "" + player2kills;
				battleResultKilledAllies.text = "" + player1kills;
			}
			
			//show the battle duration
			battleResultDuration.text = "" + duration.ToString("f1") + "s";
			
			//show battle result and end the battle
			battleResultPanel.SetActive(true);
			battleEnded = true;
		}
	}
	
	//go back to the start menu (show the network HUD & reload scene)
	public void backToMenu(){
		NetworkManager.singleton.gameObject.GetComponent<NetworkManagerHUD>().showGUI = true;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
}

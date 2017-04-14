using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Manager : MonoBehaviour {
	
	[Header("Mission")]
	public bool destroyCastle;
	public int killEnemies;
	public int timeSeconds;
	
	[Space(10)] 
	
	//variables visible in the inspector
	public float fadespeed;
	public GUIStyle minimap;
	public GameObject Minimap;
	
	//variables not visible in the inspector
	private GameObject GameOverMenu;
	private GameObject VictoryMenu;
	private GameObject GamePanel;
	private GameObject characterButtons;
	
	private bool fading;
	private float alpha;
	private bool showMinimap;
	
	private GameObject showHideUnitsButton;
	private GameObject showHideMinimapButton;
	private GameObject unitsLabel;
	
	private GameObject playerCastleStrengthText;
	private GameObject enemyCastleStrengthText;
	private GameObject playerCastleStrengthBar;
	private GameObject enemyCastleStrengthBar;
	
	private GameObject missionPanel;
	
	private float playerCastleStrengthStart;
	private float enemyCastleStrengthStart;
	
	private float playerCastleStrength;
	private float enemyCastleStrength;
	
	public static int enemiesKilled;
	private float time;
	
	public static bool gameOver;
	public static bool victory;
	public static GameObject StartMenu;
	
	void Start(){
		//find some objects
		characterButtons = GameObject.Find("Character panel");
		showHideUnitsButton = GameObject.Find("Show/hide units");
		showHideMinimapButton = GameObject.Find("Show/hide minimap");
		unitsLabel = GameObject.Find("Amount of units label");
		
		playerCastleStrengthText = GameObject.Find("Player castle strength text");
		enemyCastleStrengthText = GameObject.Find("Enemy castle strength text");
		playerCastleStrengthBar = GameObject.Find("Player castle strength bar");
		enemyCastleStrengthBar = GameObject.Find("Enemy castle strength bar");
		
		//immediately freeze game
		Time.timeScale = 0;
		
		//find all UI panels
		StartMenu = GameObject.Find("Start panel");
		GameOverMenu = GameObject.Find("Game over panel");
		VictoryMenu = GameObject.Find("Victory panel");
		GamePanel = GameObject.Find("Game panel");
		
		//turn off all panels except the start menu panel
		StartMenu.SetActive(true);
		GameOverMenu.SetActive(false);
		VictoryMenu.SetActive(false);
		GamePanel.SetActive(false);
		
		//game over and victory are false
		gameOver = false;
		victory = false;
		
		//start menu alpha is alpa
		alpha = StartMenu.GetComponent<Image>().color.a;
		
		//get strength of all castles together
		GetCastleStrength();
		
		//set start castle strengths
		playerCastleStrengthStart = playerCastleStrength;
		enemyCastleStrengthStart = enemyCastleStrength;
		
		missionPanel = GameObject.Find("Mission");
		string mission = "";
		
		if(destroyCastle){
		mission = "Destroy castle of the opponent";
		}
		else if(killEnemies > 0 && timeSeconds > 0){
		mission = "- Kill " + killEnemies + " enemies. \n- Battle at least " + timeSeconds + " seconds.";
		}
		else if(killEnemies > 0 && timeSeconds <= 0){
		mission = "Kill " + killEnemies + " enemies.";
		}
		else if(killEnemies <= 0 && timeSeconds > 0){
		mission = "Battle at least " + timeSeconds + " seconds.";
		}
		else{
		mission = "No mission";	
		}
	
		missionPanel.transform.Find("Mission text").gameObject.GetComponent<Text>().text = mission;
		missionPanel.SetActive(false);
		time = 0;
	}
	
	void Update(){
		//keep updating castle strengths
		GetCastleStrength();
		
		//show the castle strengths
		playerCastleStrengthText.GetComponent<Text>().text = "" + (int)playerCastleStrength;
		enemyCastleStrengthText.GetComponent<Text>().text = "" + (int)enemyCastleStrength;
		
		//set the fillamount (round bar) to the percentage of castles left
		enemyCastleStrengthBar.GetComponent<Image>().fillAmount = enemyCastleStrength/enemyCastleStrengthStart;
		playerCastleStrengthBar.GetComponent<Image>().fillAmount = playerCastleStrength/playerCastleStrengthStart;
		
		time += Time.deltaTime;
		
		//set game over true when the castles are destroyed
		if(playerCastleStrength <= 0){
			Time.timeScale = 0;
			gameOver = true;
		}
		//set victory based on mission
		if(enemyCastleStrength <= 0 && destroyCastle){
			Time.timeScale = 0;
			victory = true;
		}
		else if(killEnemies > 0 && enemiesKilled >= killEnemies && timeSeconds > 0 && timeSeconds < time){
			Time.timeScale = 0;
			victory = true;
		}
		else if((timeSeconds > 0 && timeSeconds < time) && !(killEnemies > 0)){
			Time.timeScale = 0;
			victory = true;
		}
		else if((killEnemies > 0 && enemiesKilled == killEnemies) && !(timeSeconds > 0)){
			Time.timeScale = 0;
			victory = true;
		}
		
		//control the visibility of the panels
		if(gameOver && !GameOverMenu.activeSelf){
		GamePanel.SetActive(false);
		GameOverMenu.SetActive(true);
		}
		if(victory && !VictoryMenu.activeSelf){
		GamePanel.SetActive(false);
		VictoryMenu.SetActive(true);
		}
		
		//fade the start panel out when fading is true
		if(fading && alpha > 0){
		alpha -= Time.deltaTime * fadespeed;
		
		StartMenu.GetComponent<Image>().color = new Color(
		StartMenu.GetComponent<Image>().color.r, 
		StartMenu.GetComponent<Image>().color.g, 
		StartMenu.GetComponent<Image>().color.b, 
		alpha);
		}
		else if(alpha <= 0){
		//remove start panel when alpha is 0
		fading = false;
		StartMenu.SetActive(false);
		GamePanel.SetActive(true);		
		}
		
		//show minimap
		if(showMinimap && !StartMenu.activeSelf && !Settings.settingsMenu.activeSelf && !GameOverMenu.activeSelf && !VictoryMenu.activeSelf){
		Minimap.SetActive(true);
		}
		else{
		//hide minimap
		Minimap.SetActive(false);	
		}
		
		//get the amount of units and the amount of selected units
		int playerUnits = GameObject.FindGameObjectsWithTag("Knight").Length;
		int enemyUnits = GameObject.FindGameObjectsWithTag("Enemy").Length;
		int selectedUnits = 0;
		
		//add 1 to selected units for each selected unit
		foreach(GameObject unit in GameObject.FindGameObjectsWithTag("Knight")){
			if(unit.GetComponent<Character>().selected){
			selectedUnits++;	
			}
		}
		//show the amount of units
		unitsLabel.GetComponentInChildren<Text>().text = "Player Units: " + playerUnits + "  |  Enemy Units: " + enemyUnits + "  |  Selected Units: " + selectedUnits;
	}
	
	void OnGUI(){
		//show minimap border when minimap is active
		if(showMinimap && !StartMenu.activeSelf && !Settings.settingsMenu.activeSelf && !GameOverMenu.activeSelf && !VictoryMenu.activeSelf){
		GUI.Box(new Rect(Screen.width * 0.795f, Screen.height * 0.69f, Screen.width * 0.205f, Screen.height * 0.31f), "", minimap);
		}
	}
	
	void GetCastleStrength(){
	//castle strength is 0
	playerCastleStrength = 0;
	enemyCastleStrength = 0;
	
	//add the strength of each enemy castle
	foreach(GameObject enemyCastle in GameObject.FindGameObjectsWithTag("Enemy Castle")){
		if(enemyCastle.GetComponent<Castle>().lives > 0){
			enemyCastleStrength += enemyCastle.GetComponent<Castle>().lives;
		}
	}
	//add the strength of each player castle
	foreach(GameObject playerCastle in GameObject.FindGameObjectsWithTag("Player Castle")){
		if(playerCastle.GetComponent<Castle>().lives > 0){
			playerCastleStrength += playerCastle.GetComponent<Castle>().lives;
		}
	}	
	}
	
	//start the game
	public void startGame(){
		//set buttons false
		foreach (Transform button in StartMenu.transform){
		button.gameObject.SetActive(false);
		}
		//set timescale to normal and start fading out
		Time.timeScale = 1;
		fading = true;
	}
	
	//open mission panel to start game
	public void openMissionPanel(){
		missionPanel.SetActive(true);
	}
	
	public void endGame(){
		//end game
		Application.Quit();
	}
	
	public void surrender(){
		//Freeze game and set the game over panel visible
		Time.timeScale = 0;
		Manager.gameOver = true;
	}
	public void showHideUnits(){
		//show or hide the units panel
		characterButtons.SetActive(!characterButtons.activeSelf);
		//change button text
		if(characterButtons.activeSelf){
		showHideUnitsButton.GetComponentInChildren<Text>().text =	"-";
		}
		else{
		showHideUnitsButton.GetComponentInChildren<Text>().text =	"+";	
		}
	}
	
	public void showHideMinimap(){
		//show or hide minimap
		showMinimap = !showMinimap;
		
		//change button text
		if(showMinimap){
		showHideMinimapButton.GetComponentInChildren<Text>().text =	"-";
		}
		else{
		showHideMinimapButton.GetComponentInChildren<Text>().text =	"+";	
		}
	}
}

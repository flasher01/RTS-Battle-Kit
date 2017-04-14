using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobileMultiplayer : MonoBehaviour {

	//not visible in the inspector
	private bool moveRight;
	private bool moveLeft;
	private bool moveUp;
	private bool moveDown;
	
	private GameObject buttons;
	private GameObject switchSelectionModeButton;
	
	public static bool camEnabled = true;
	public static bool deployMode;
	public static bool selectionModeMove;
	
	//visible in the inspector
	public Sprite moveUnits;
	public Sprite selectUnits;
	
	void Start(){
		//find gameobjects
		switchSelectionModeButton = GameObject.Find("Switch selection mode button");
		buttons = GameObject.Find("Buttons");
	}
	
	void Update(){
		
		//return and don't show the buttons when there aren't two castle in the scene
		if(FindObjectsOfType<GameManagerMultiplayer>().Length != 2){
			buttons.SetActive(false);
			return;
		}
		
		//check if you can move the camera
		if(camEnabled){
			
		//use the move booleans to know when to move the camera
		if(moveLeft){
			Camera.main.transform.Translate(Vector3.right * Time.deltaTime * -CamController.movespeed);
		}
		if(moveRight){
			Camera.main.transform.Translate(Vector3.right * Time.deltaTime * CamController.movespeed);
		}
		if(moveUp){
			Camera.main.transform.Translate(Vector3.up * Time.deltaTime * CamController.movespeed);
		}
		if(moveDown){
			Camera.main.transform.Translate(Vector3.up * Time.deltaTime * -CamController.movespeed);
		}
		}
		
		//turn the buttons of when any of the menu's is active
		if(!GameManagerMultiplayer.battleEnded && !Settings.settingsMenu.activeSelf){
			buttons.SetActive(true);
		}
		else{
			buttons.SetActive(false);
		}
		
		//Set mobile selection mode button active/not active
		if(CharacterManagerMultiplayer.selectionMode){
			switchSelectionModeButton.SetActive(true);
		}
		else{
			switchSelectionModeButton.SetActive(false);
		}
	}
	
	//start moving the camera based on the direction
	public void moveCameraButtonDown(string direction){
		if(direction == "right"){
			moveRight = true;
		}
		if(direction == "left"){
			moveLeft = true;
		}
		if(direction == "up"){
			moveUp = true;
		}
		if(direction == "down"){
			moveDown = true;
		}
	}
	
	//stop moving the camera based on direction
	public void moveCameraButtonUp(string direction){
		if(direction == "right"){
			moveRight = false;
		}
		if(direction == "left"){
			moveLeft = false;
		}
		if(direction == "up"){
			moveUp = false;
		}
		if(direction == "down"){
			moveDown = false;
		}
	}
	
	//toggle deploymode on/off
	public void toggleDeployMode(){
		deployMode = !deployMode;
		if(deployMode){
			//set button color to red
			GameObject.Find("Deploy units button").GetComponent<Image>().color = Color.red;
			if(CharacterManagerMultiplayer.selectionMode){
				//switch selection mode off
				GameObject.Find("GameManagerMultiplayer").GetComponent<CharacterManagerMultiplayer>().selectCharacters();
			}
			//camera off
			camEnabled = false;
		}
		else{
			//set button color to white
			GameObject.Find("Deploy units button").GetComponent<Image>().color = Color.white;
			//camera on
			camEnabled = true;
		}
	}
	
	//switch between selection modes (box select & move units mode)
	public void switchSelectionMode(){
		selectionModeMove = !selectionModeMove;
		
		//change the sprite displayed
		if(selectionModeMove){
			switchSelectionModeButton.GetComponent<Image>().sprite = moveUnits;
		}
		else{
			switchSelectionModeButton.GetComponent<Image>().sprite = selectUnits;
		}
	}
}

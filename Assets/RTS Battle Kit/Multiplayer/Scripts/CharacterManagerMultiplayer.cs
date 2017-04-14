using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Networking;

//troop settings
[System.Serializable]
public class troop{
	public GameObject deployableTroops;
	public int troopCosts;
	public Sprite buttonImage;
	[HideInInspector]
	public Button button;
}

public class CharacterManagerMultiplayer : NetworkBehaviour {

	//variables visible in the inspector	
	public GUIStyle rectangleStyle;
	public ParticleSystem newUnitEffect;
	public Texture2D cursorTexture;
	public Texture2D cursorTexture1;
	public bool highlightSelectedButton;
	public Color buttonHighlight;
	public Button button;
	public float bombLoadingSpeed;
	public float BombRange;
	public GameObject bombExplosion;
	public GameObject killLabel;
	public bool ownHalfOnly;
	public float maxGold;
	public float addGoldTime;
	public int addGold;
	
	[Space(10)]
	
	//not visible in the inspector
	public List<troop> troops;
	
	[HideInInspector]
	public float gold;
	public static GameObject target;
	
	private Vector2 mouseDownPos;
    private Vector2 mouseLastPos;
	private bool visible; 
    private bool isDown;
	private GameObject[] allies;
	private int selectedUnit;
	private GameObject goldWarning;
	private GameObject characterList;
	private GameObject characterParent;
	private GameObject selectButton;
	private Image goldBar;
	private Text goldText;
	
	private GameObject bombLoadingBar;
	private GameObject bombButton;
	private float bombProgress;
	private bool isPlacingBomb;
	private GameObject bombRange;
	
	private GameObject deployWarning1;
	private GameObject deployWarning2;
	
	public static bool selectionMode;
	
	[HideInInspector]
	public bool host;
	
	private GameObject killList;
	
	[HideInInspector]
	public int player1kills;
	[HideInInspector]
	public int player2kills;
	
	void Start(){
		//find the list object
		killList = GameObject.Find("Kills list");
		
		//don't execute the following code unless this is the local player
		if(!isLocalPlayer)
			return;
		
		//find some objects
		selectButton = GameObject.Find("Character selection button");
		target = GameObject.Find("target");
		target.SetActive(false);
		
		//set cursor and add the character buttons
		Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
		characterList = GameObject.Find("Character buttons");
		addCharacterButtons();
		//find warning that appears when you don't have enough gold to deploy troops and set it not active
		goldWarning = GameObject.Find("gold warning");
		goldWarning.SetActive(false);
		//play function addGold every five seconds
		InvokeRepeating("AddGold", 1.0f, addGoldTime);
		
		//find bomb gameobjects
		bombLoadingBar = GameObject.Find("Loading bar");
		bombButton = GameObject.Find("Bomb button");
		bombRange = GameObject.Find("Bomb range");
		bombRange.SetActive(false);
		isPlacingBomb = false;
		
		//find warning and gold indicator
		goldBar = GameObject.Find("Gold bar").GetComponent<Image>();
		goldText = GameObject.Find("Gold text").GetComponent<Text>();
		
		deployWarning1 = GameObject.Find("Unit deploy warning 1");
		deployWarning2 = GameObject.Find("Unit deploy warning 2");
		
		//don't show the warnings on the battle ground
		deployWarning1.SetActive(false);
		deployWarning2.SetActive(false);
		
		//make sure the UI buttons work by assigning functions to them
		bombButton.GetComponent<Button>().onClick.AddListener(() => { placeBomb(); });
		selectButton.GetComponent<Button>().onClick.AddListener(() => { selectCharacters(); });
	}
     
    void Update(){
		//make sure this is the local player and there are 2 castles in the scene
		if(!isLocalPlayer || FindObjectsOfType<GameManagerMultiplayer>().Length != 2)
			return;
		
		if(bombProgress < 1){
			//when bomb is loading, set red color and disable button
			bombProgress += Time.deltaTime * bombLoadingSpeed;
			bombLoadingBar.GetComponent<Image>().color = Color.red;
			bombButton.GetComponent<Button>().enabled = false;
		}
		else{
			//if bomb is done, set a blue color and enable the bomb button
			bombProgress = 1;
			bombLoadingBar.GetComponent<Image>().color = new Color(0, 1, 1, 1);
			bombButton.GetComponent<Button>().enabled = true;
		}
		
		//set fillamount to the bomb progress
		bombLoadingBar.GetComponent<Image>().fillAmount = bombProgress;
		
		//ray from main camera
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out hit))
				
		//check if left mouse button gets pressed
        if(Input.GetMouseButtonDown(0)){
			
			//if you didn't click on UI and you have not enought gold, display a warning
			if(gold < troops[selectedUnit].troopCosts && !EventSystem.current.IsPointerOverGameObject()){
			StartCoroutine(GoldWarning());	
			}
			//check if you hit any collider when clicking (just to prevent errors)
			if(hit.collider != null){
			//if you click battle ground, if click doesn't hit any UI, if space is not down and if you have enough gold:
			if(hit.collider.gameObject.CompareTag("Battle ground") && !selectionMode && !isPlacingBomb && !EventSystem.current.IsPointerOverGameObject() 
			&& gold >= troops[selectedUnit].troopCosts && (!GameObject.Find("Mobile multiplayer") || (GameObject.Find("Mobile multiplayer") && MobileMultiplayer.deployMode)) 
			&& (!ownHalfOnly || (ownHalfOnly && ((transform.position.x < 0 && hit.point.x < -2) || (transform.position.x > 0 && hit.point.x > 2))))){
				//if this is not the host, send a command to the server to spawn a unit
				if(!host){
					CmdSpawnUnit(hit.point, selectedUnit);
				}
				else{
					//if this is the host, directly spawn the unit
					Vector3 position = hit.point;
					GameObject newTroop = Instantiate(troops[selectedUnit].deployableTroops, position, troops[selectedUnit].deployableTroops.transform.rotation) as GameObject;
					NetworkServer.SpawnWithClientAuthority(newTroop, connectionToClient);
					//tell the client to add tags locally
					RpcAddTags(newTroop, "Player 1 unit", "Player 2 unit", "Player 2 castle", Color.red);
					
					//also spawn an effect
					ParticleSystem effect = Instantiate(newUnitEffect, position, Quaternion.identity) as ParticleSystem;
					NetworkServer.Spawn(effect.gameObject);	
				}
				//get the amount of gold needed to spawn this unit
				gold -= troops[selectedUnit].troopCosts;
			}
			//if the other side of the battle ground was clicked, and its not allowed, show a warning
			else if(hit.collider.gameObject.CompareTag("Battle ground") && !selectionMode && !isPlacingBomb && !EventSystem.current.IsPointerOverGameObject() 
			&& gold >= troops[selectedUnit].troopCosts && (!GameObject.Find("Mobile multiplayer") || (GameObject.Find("Mobile multiplayer") && MobileMultiplayer.deployMode)) 
			&& ownHalfOnly && transform.position.x < 0 && hit.point.x > -2){
				StartCoroutine(deployWrongSideWarning(2));
			}
			//if the other side of the battle ground was clicked, and its not allowed, show a warning
			else if(hit.collider.gameObject.CompareTag("Battle ground") && !selectionMode && !isPlacingBomb && !EventSystem.current.IsPointerOverGameObject() 
			&& gold >= troops[selectedUnit].troopCosts && (!GameObject.Find("Mobile multiplayer") || (GameObject.Find("Mobile multiplayer") && MobileMultiplayer.deployMode)) 
			&& ownHalfOnly && transform.position.x > 0 && hit.point.x < 2){
				StartCoroutine(deployWrongSideWarning(1));
			}
			
			//if you are placing a bomb and click...
			if(isPlacingBomb && !EventSystem.current.IsPointerOverGameObject()){
				//instantiate explosion
				
				if(host){
					GameObject explosion = Instantiate(bombExplosion, hit.point, Quaternion.identity)as GameObject;
					NetworkServer.Spawn(explosion);
					
					foreach(GameObject enemy in GameObject.FindGameObjectsWithTag("Player 2 unit")){
					if(enemy != null && Vector3.Distance(enemy.transform.position, hit.point) <= BombRange/2){
					//kill enemy if its within the bombrange
					enemy.GetComponent<CharacterMultiplayer>().lives = 0;	
					}
					}
				}
				else{
					//use a command if this is not the host
					CmdSpawnExplosion(hit.point);
				}
				
				//reset bomb progress
				bombProgress = 0;
				isPlacingBomb = false;
				bombRange.SetActive(false);
			}
			else if(hit.collider.gameObject.CompareTag("Battle ground") && isPlacingBomb && EventSystem.current.IsPointerOverGameObject()){
				//if you place a bomb and click any UI element, continue but don't reset bomb progress
				isPlacingBomb = false;
				bombRange.SetActive(false);
			}
			}
        }
		
		if(hit.collider != null && isPlacingBomb && !EventSystem.current.IsPointerOverGameObject()){
			//show the bomb range at mouse position using a spot light
			bombRange.transform.position = new Vector3(hit.point.x, 75, hit.point.z);
			//adjust spotangle to correspond to bomb range
			bombRange.GetComponent<Light>().spotAngle = BombRange;
			bombRange.SetActive(true);
		}
		
		//if space is down too set the position where you first clicked
		if(Input.GetMouseButtonDown(0) && selectionMode && !isPlacingBomb 
		&& (!GameObject.Find("Mobile multiplayer") || (GameObject.Find("Mobile multiplayer") && !MobileMultiplayer.selectionModeMove))){
		mouseDownPos = Input.mousePosition;
        isDown = true;
        visible = true;
		}
 
        // Continue tracking mouse position until mouse button is up
        if(isDown){
        mouseLastPos = Input.mousePosition;
			//if you release mouse button, remove rectangle and stop tracking
            if(Input.GetMouseButtonUp(0)){
                isDown = false;
                visible = false;
            }
        }
		
		//get all ally units according to the team
		if(host){
			allies = GameObject.FindGameObjectsWithTag("Player 1 unit");
		}
		else{
			allies = GameObject.FindGameObjectsWithTag("Player 2 unit");
		}
		
		//if player presses d, deselect all characters
		if(Input.GetKey("x")){
		foreach(GameObject Ally in allies){
		if(Ally != null){
		Ally.GetComponent<CharacterMultiplayer>().selected = false;	
		}
		}
		}
		
		//start selection mode when player presses spacebar
		if(Input.GetKeyDown("space")){
		selectCharacters();	
		}
		
		//update gold display
		goldBar.fillAmount = (gold/maxGold);
		goldText.text = "" + gold;
		
		//color buttons grey if the unit is not deployable yet
		for(int i = 0; i < troops.Count; i++){
			if(troops[i].troopCosts <= gold){
				troops[i].button.gameObject.GetComponent<Image>().color = Color.white;
			}
			else{
				troops[i].button.gameObject.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f, 1);
			}
		}
    }
 
    void OnGUI(){
		//check if rectangle should be visible
		if(visible){
            // Find the corner of the box
            Vector2 origin;
            origin.x = Mathf.Min(mouseDownPos.x, mouseLastPos.x);
			
            // GUI and mouse coordinates are the opposite way around.
            origin.y = Mathf.Max(mouseDownPos.y, mouseLastPos.y);
            origin.y = Screen.height - origin.y;  
			
            //Compute size of box
            Vector2 size = mouseDownPos - mouseLastPos;
            size.x = Mathf.Abs(size.x);
            size.y = Mathf.Abs(size.y);   
			
            // Draw the GUI box
            Rect rect = new Rect(origin.x, origin.y, size.x, size.y);
            GUI.Box(rect, "", rectangleStyle);
			
			foreach(GameObject Ally in allies){
			if(Ally != null){
			Vector3 pos = Camera.main.WorldToScreenPoint(Ally.transform.position);
			pos.y = Screen.height - pos.y;
			//foreach selectable character check its position and if it is within GUI rectangle, set selected to true
			if(rect.Contains(pos)){
			Ally.GetComponent<CharacterMultiplayer>().selected = true;
			}
			}
			}
		}
    }
	
	//function to select another unit
	public void selectUnit(int unit){
	if(highlightSelectedButton){
	//remove all outlines and set the current button outline visible
	for(int i = 0; i < troops.Count; i++){
	troops[i].button.GetComponent<Outline>().enabled = false;	
	}
	EventSystem.current.currentSelectedGameObject.GetComponent<Outline>().enabled = true;
	}
	//selected unit is the pressed button
	selectedUnit = unit;
	}
	
	public void selectCharacters(){
		//turn selection mode on/off
		selectionMode = !selectionMode;
		if(selectionMode){
		//set cursor and button color to show the player selection mode is active
		selectButton.GetComponent<Image>().color = Color.red;	
		Cursor.SetCursor(cursorTexture1, Vector2.zero, CursorMode.Auto);
		if(GameObject.Find("Mobile multiplayer")){
			if(MobileMultiplayer.deployMode){
				GameObject.Find("Mobile multiplayer").GetComponent<MobileMultiplayer>().toggleDeployMode();
			}
			MobileMultiplayer.camEnabled = false;
		}
		}
		else{
		//show the player selection mode is not active
		selectButton.GetComponent<Image>().color = Color.white;	
		Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
		
		//set target object false and deselect all units
		foreach(GameObject Ally in allies){
		if(Ally != null){
		Ally.GetComponent<CharacterMultiplayer>().selected = false;	
		}
		}
		target.SetActive(false);
		if(GameObject.Find("Mobile multiplayer")){
			MobileMultiplayer.camEnabled = true;
		}
		}
	}
	
	//warning if you need more gold
	IEnumerator GoldWarning(){
	if(!goldWarning.activeSelf){
	goldWarning.SetActive(true);
	
	//wait for 2 seconds
	yield return new WaitForSeconds(2);
	goldWarning.SetActive(false);
	}
	}
	
	void addCharacterButtons(){
		//for all troops...
		for(int i = 0; i < troops.Count; i++){
			//add a button to the list of buttons
			Button newButton = Instantiate(button);
			RectTransform rectTransform = newButton.GetComponent<RectTransform>();
			rectTransform.SetParent(characterList.transform, false);
			
			//set button outline
			newButton.GetComponent<Outline>().effectColor = buttonHighlight;
			
			//set the correct button sprite
			newButton.gameObject.GetComponent<Image>().sprite = troops[i].buttonImage;
			//only enable outline for the first button
			if(i == 0 && highlightSelectedButton){
			newButton.GetComponent<Outline>().enabled = true;
			}
			else{
			newButton.GetComponent<Outline>().enabled = false;	
			}
			
			//set button name to its position in the list(important for the button to work later on)
			newButton.transform.name = "" + i;
			
			//add a onclick function to the button with the name to select the proper unit
			newButton.GetComponent<Button>().onClick.AddListener(
			() => { 
			selectUnit(int.Parse(newButton.transform.name)); 
			}
			);
			
			//set the button stats
			newButton.GetComponentInChildren<Text>().text = "Price: " + troops[i].troopCosts + 
			"\n Damage: " + troops[i].deployableTroops.GetComponentInChildren<CharacterMultiplayer>().damage + 
			"\n Lives: " + troops[i].deployableTroops.GetComponentInChildren<CharacterMultiplayer>().lives;
			
			//this is the new button
			troops[i].button = newButton;
		}
	}
	
	public void placeBomb(){
		if(isLocalPlayer)
		//start placing a bomb
		isPlacingBomb = true;
	}
	
	//functions that add 100 to your gold amount and show text to let player know
	void AddGold(){
		if(isLocalPlayer && FindObjectsOfType<GameManagerMultiplayer>().Length == 2 && gold < maxGold){
			if(gold + addGold > maxGold){
				gold += maxGold - gold;
			}
			else{
				gold += addGold;
			}
		}
	}
	
	//tell server to spawn the correct unit and add an effect
	[Command]
	void CmdSpawnUnit(Vector3 position, int unit){
		GameObject newTroop = Instantiate(troops[unit].deployableTroops, position, troops[unit].deployableTroops.transform.rotation) as GameObject;
		NetworkServer.SpawnWithClientAuthority(newTroop, connectionToClient);
		RpcAddTags(newTroop, "Player 2 unit", "Player 1 unit", "Player 1 castle", Color.blue);
		ParticleSystem effect = Instantiate(newUnitEffect, position, Quaternion.identity) as ParticleSystem;
		NetworkServer.Spawn(effect.gameObject);
	}
	
	//add tags on the client
	[ClientRpc]
    void RpcAddTags(GameObject newTroop, string tag, string attackTag, string attackCastleTag, Color color){
        newTroop.tag = tag;
		newTroop.GetComponent<CharacterMultiplayer>().attackTag = attackTag;
		newTroop.GetComponent<CharacterMultiplayer>().attackCastleTag = attackCastleTag;
		//set a color so the player knows which units are his
		newTroop.GetComponent<CharacterMultiplayer>().healthBarFill.GetComponent<Image>().color = color;
    }
	
	//an enemy unit has been killed, show a label
	public void enemyUnitKilled(string tag, string unitType){
		//check if this is the host. If not, send a command to the server
		if(!host){
			CmdEnemyUnitKilled(tag, unitType);
		}
		else{
			//if this is the host, directly send an rpc
			RpcEnemyUnitKilled(tag, unitType);
		}
	}
	
	//server tells client to show the kill label
	[Command]
	void CmdEnemyUnitKilled(string tag, string unitType){
		RpcEnemyUnitKilled(tag, unitType);
	}
	
	//client checks if it should show a label
	[ClientRpc]
    void RpcEnemyUnitKilled(string tag, string unitType){
			//if this is player 2 and a unit of player 1 has been killed, show the kill label
			if(tag == "Player 1 unit"){
				if(!host){
					enemyKilled(unitType);
				}
				
				//player 2 has one extra kill
				player2kills++;
			}
			//if this is player 1 and a unit of player 2 has been killed, show the kill label
			else if(tag == "Player 2 unit"){
				if(host){
					enemyKilled(unitType);
				} 
				
				//player 1 has one extra kill
				player1kills++;
			}
	}
	
	//actually show the kill label locally (it gets destroyed automatically)
	void enemyKilled(string unitType){
		GameObject newLabel = Instantiate(killLabel);
		RectTransform rectTransform = newLabel.GetComponent<RectTransform>();
		rectTransform.SetParent(killList.transform, false);
		newLabel.GetComponentInChildren<Text>().text = "ENEMY " + unitType.ToUpper() + " KILLED";
	}
	
	//shows a warning when you're trying to deploy a unit on the wrong half of the battle ground
	IEnumerator deployWrongSideWarning(int half){
		if(half == 1){
			//show the warning, wait, hide the warning
			deployWarning1.SetActive(true);
			yield return new WaitForSeconds(1);
			deployWarning1.SetActive(false);
		}
		else if(half == 2){
			//show the warning, wait, hide the warning
			deployWarning2.SetActive(true);
			yield return new WaitForSeconds(1);
			deployWarning2.SetActive(false);
		}
	}
	
	//tell the server to spawn an explosion
	[Command]
	void CmdSpawnExplosion(Vector3 position){
		//add the explosion and spawn it on the server
		GameObject explosion = Instantiate(bombExplosion, position, Quaternion.identity) as GameObject ;
		NetworkServer.Spawn(explosion);
		
		//this is a command, so it came from the client, which means the bomb should find and kill all units of player 1 in range (player1 = host)
		foreach(GameObject enemy in GameObject.FindGameObjectsWithTag("Player 1 unit")){
			if(enemy != null && Vector3.Distance(enemy.transform.position, position) <= BombRange/2){
			//kill enemy if its within the bombrange
			enemy.GetComponent<CharacterMultiplayer>().lives = 0;	
		}
		}
	}
}

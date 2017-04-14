using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour {
	
	//variables visible in the inspector
	public GameObject[] enemies;
    public int startEnemyCount;
    public float spawnWait;
    public int waveWait;
	public int startWait;
	public int extraEnemiesPerWave;
	public bool randomSpawnPositions;
	
	//not visible in the inspector
	private GameObject enemyParent;
	private GameObject[] spawnObjects;
	private int currentSpawnPosition;
	
	void Awake(){
		//get/create some objects
		enemyParent = new GameObject("Enemies");
		spawnObjects = GameObject.FindGameObjectsWithTag("Enemy spawn position");
	}

    void Start(){
		//set current spawn position to the first position
		currentSpawnPosition = 0;
		
		//start spawning waves
        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves(){
		//before spawning the first enemies, wait a moment
		yield return new WaitForSeconds(startWait);
        while(true){
			//if not all characters of this wave are spawned, spawn new enemy and that wait some time before spawning next enemy in this wave
            for(int i = 0; i < startEnemyCount; i++){
				int random = Random.Range(0, enemies.Length);
				GameObject newEnemy = Instantiate(enemies[random], spawnObjects[currentSpawnPosition].transform.position, spawnObjects[currentSpawnPosition].transform.rotation) as GameObject;
				newEnemy.transform.parent = enemyParent.transform;
				//change next spawnposition
				if(randomSpawnPositions){
				currentSpawnPosition = Random.Range(0, spawnObjects.Length - 1);
				}
                yield return new WaitForSeconds(spawnWait);
            }
			if(!randomSpawnPositions){
			//if you don't want to change the positions randomly, change them after each wave of enemies
			if(currentSpawnPosition != spawnObjects.Length - 1){
			currentSpawnPosition++;
			}
			else{
			currentSpawnPosition = 0;
			}	
			}
			//make sure the next wave contains more enemies than this one
			startEnemyCount += extraEnemiesPerWave;
			//wait before starting the next wave
            yield return new WaitForSeconds(waveWait);
        }
    }
}

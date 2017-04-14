using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour {
	
	[HideInInspector]
	public GameObject arrowOwner;
	
	void Start(){
		//destroy arrow after 5 seconds
		Destroy(gameObject, 5);
	}

	void OnTriggerEnter(Collider other){
		//freeze arrow when it hits an enemy and parent it to the enemy to move with it
		if((other.gameObject.tag == "Enemy" || other.gameObject.tag == "Knight") && other.gameObject != arrowOwner){
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		GetComponent<Rigidbody>().isKinematic = true;
		transform.parent = other.gameObject.transform;
		}
		else if(other.gameObject.tag == "Battle ground"){
		//destroy arrow when it hits the ground
		Destroy(gameObject);	
		}
	}
}

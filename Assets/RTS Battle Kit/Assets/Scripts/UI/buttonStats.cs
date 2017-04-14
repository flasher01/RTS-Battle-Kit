using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
 
public class buttonStats : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerExitHandler {
	
	//not visible in the inspector
	private GameObject stats;
	
	void Start(){
		//find button stats and turn them off
		stats = transform.Find("Stats").gameObject;
		stats.SetActive(false);
	}
	
    public void OnPointerEnter (PointerEventData eventData) {
		//set stats active on hover
        stats.SetActive(true);
    }
 
    public void OnPointerExit (PointerEventData eventData) {
		//hide stats on exit
        stats.SetActive(false);
    }
	
	public void OnPointerDown (PointerEventData eventData) {
		//hide stats on click
        stats.SetActive(false);
    }
}

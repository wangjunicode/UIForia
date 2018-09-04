using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class temp : UIBehaviour, IPointerClickHandler {

	public void OnPointerClick(PointerEventData eventData) {
		Debug.Log("Clicked!");
		PointerEventData evt = new PointerEventData(EventSystem.current);
		
		GetComponent<InputField>().OnPointerClick(evt);
	}

}

/*
 * if any mask does not cover point
 * compare depth
 * if mask depth > element depth
 * don't allow event
 *
 * draw caret
 * draw highlight
 * find index
 * handle keyboard movement
 * handle copy paste whatever
 * handle cursor blink
 * handle drag select
 * handle interop w/ prefab
 * 
*/
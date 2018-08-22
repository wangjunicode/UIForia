using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TMP_TextSize : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		TMP_Text text = GetComponent<TMP_Text>();
		text.text = text.GetPreferredValues().ToString();
	}
}

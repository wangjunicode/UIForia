using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clip : MonoBehaviour {

	public Rect clipRect;

	public bool cull;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
//	void Update () {
//		GetComponent<CanvasRenderer>().EnableRectClipping(clipRect);
//		GetComponent<CanvasRenderer>().cull = cull;
//	}

	protected void OnRectTransformDimensionsChange() {	}

}

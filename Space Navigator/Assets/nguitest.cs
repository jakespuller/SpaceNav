using UnityEngine;
using System.Collections;

public class nguitest : MonoBehaviour {
	
	
	public GameObject myspritePrefab;
	public GameObject parent;
	// Use this for initialization
	void Start () {
	//NGUITools.AddChild
		GameObject S= NGUITools.AddChild(parent,myspritePrefab);
		S.transform.position.Scale(new Vector3(96,128,0));
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

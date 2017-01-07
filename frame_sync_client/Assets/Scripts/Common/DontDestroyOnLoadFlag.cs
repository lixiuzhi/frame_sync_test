using UnityEngine;
using System.Collections;

public class DontDestroyOnLoadFlag : MonoBehaviour {

	// Use this for initialization
	void Awake () {
        DontDestroyOnLoad(gameObject);
	} 
}

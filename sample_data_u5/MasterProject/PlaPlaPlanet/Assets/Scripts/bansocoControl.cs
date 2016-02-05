using UnityEngine;
using System.Collections;

public class bansocoControl : MonoBehaviour {

	void Start () {
	}
	
	void Update () {
	}

	public void getoff(){
		gameObject.AddComponent<Rigidbody>(); // 형지정--.
		gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
		this.gameObject.transform.parent = null;
	}


}

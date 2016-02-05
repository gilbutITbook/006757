using UnityEngine;
using System.Collections;

public class bansocoControl : MonoBehaviour {

	void Start () {
	}
	
	void Update () {
	}

	public void getoff(){
		gameObject.AddComponent<Rigidbody>(); // 형 지정--.
		gameObject.rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
		this.gameObject.transform.parent = null;
	}


}

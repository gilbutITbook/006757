using UnityEngine;
using System.Collections;

public class SceneControl : MonoBehaviour {

	private BlockRoot block_root = null;
	void Start() {
		// BlockRoot 스크립트를 가져온다.
		this.block_root = this.gameObject.GetComponent<BlockRoot>();
		// BlockRoot 스크립트의 initialSetUp()을 호출한다.
		this.block_root.initialSetUp();
	}
	void Update() {
	}


}

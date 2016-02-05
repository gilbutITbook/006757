using UnityEngine;
using System.Collections;

public class SceneControl : MonoBehaviour {

	private BlockRoot block_root = null;
	void Start() {
		// BlockRoot 스크립트 획득.
		this.block_root = this.gameObject.GetComponent<BlockRoot>();
		// BlockRoot스크립트의 initialSetUp()를 호출.
		this.block_root.initialSetUp();
	}
	void Update() {
	}


}

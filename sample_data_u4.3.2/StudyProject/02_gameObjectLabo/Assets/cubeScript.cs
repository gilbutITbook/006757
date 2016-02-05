using UnityEngine;
using System.Collections;

public class cubeScript : MonoBehaviour {

	void Start () {
	}
	void Update () {
	}

	public void bigsize() {
		// x, y, z 모든 방향에 대해 크기를 3배로 한다.
		this.transform.localScale = new Vector3(3.0f, 3.0f, 3.0f);
	}
}

using UnityEngine;
using System.Collections;

public class BlockCreator : MonoBehaviour {

	public GameObject[] blockPrefabs; // 블록을 저장할 배열.
	private int block_count = 0;	  // 생성한 블록의 개수.

	void Start() {
	}

	void Update() {
	}

	public void createBlock(Vector3 block_position)
	{
		// 만들어야 할 블록의 종류(흰색인가 빨간색인가)를 구한다.
		int next_block_type = this.block_count % this.blockPrefabs.Length;
		// 블록을 생성하고 go에 보관.
		GameObject go =
			GameObject.Instantiate(this.blockPrefabs[next_block_type])
				as GameObject;
		go.transform.position = block_position; // 블록의 위치를 이동.
		this.block_count++; // 블록의 개수를 증가.
	}

}

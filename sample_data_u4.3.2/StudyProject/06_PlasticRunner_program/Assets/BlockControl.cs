using UnityEngine;
using System.Collections;

public class BlockControl : MonoBehaviour {

	public MapCreator map_creator = null; // MapCreator를 보관하는 변수.

	void Start()
	{ // MapCreator를 가져와서 멤버 변수 map_creator에 보관.
		map_creator = GameObject.Find("GameRoot").GetComponent<MapCreator>();
	}

	void Update()
	{
		if(this.map_creator.isDelete(this.gameObject)) { // 안 보이면.
			GameObject.Destroy(this.gameObject); // 자기 자신을 삭제. 
		}
	}
}

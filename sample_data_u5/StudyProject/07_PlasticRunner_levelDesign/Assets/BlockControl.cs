using UnityEngine;
using System.Collections;

public class BlockControl : MonoBehaviour {

	public MapCreator map_creator = null; // MapCreator를 보관할 변수.

	void Start()
	{ // MapCreator를 취득해서 멤버 변수 map_creator에 보관. 
		map_creator = GameObject.Find("GameRoot").GetComponent<MapCreator>();
	}

	void Update()
	{
		if(this.map_creator.isDelete(this.gameObject)) { // 지워졌다면.
			GameObject.Destroy(this.gameObject); // 자기 자신을 삭제.
		}
	}
}

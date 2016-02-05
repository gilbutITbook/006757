using UnityEngine;
using System.Collections;

public class DroppedCoinControl : MonoBehaviour {

	public	MapCreator		map_creator = null;			// 맵 크리에이터.

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
	}
	
	void	Update()
	{
		// 화면에서 벗어나면 지운다.
		if(this.map_creator.isDelete(this.gameObject)) {

			GameObject.Destroy(this.gameObject);
		}
	}
}

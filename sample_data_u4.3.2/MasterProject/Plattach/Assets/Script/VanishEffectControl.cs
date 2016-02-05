using UnityEngine;
using System.Collections;

// 발화했을 때 효과를 만든다.
public class VanishEffectControl : MonoBehaviour {

	public	GameObject	eff_blue;		// CYAN = 0,
	public	GameObject	eff_yellow;		// YELLOW,
	public	GameObject	eff_orange;		// ORANGE,
	public	GameObject	eff_purple;		// MAGENTA,
	public	GameObject	eff_green;		// GREEN,
	public	GameObject	eff_pink;		// PINK,

	public GameObject eff_star;			// 수리용---.

	// ================================================================ //
	// MonoBehaviour에서의 상속..

	void 	Start()
	{
	
	}
	
	void	Update()
	{
	
	}

	public void	createEffect(BlockControl block)
	{
		GameObject	fx_prefab = null;

		switch(block.color) {

			case Block.COLOR.BLUE:		fx_prefab = eff_blue;	break;
			case Block.COLOR.YELLOW:	fx_prefab = eff_yellow;	break;
			case Block.COLOR.ORANGE:	fx_prefab = eff_orange;	break;
			case Block.COLOR.MAGENTA:	fx_prefab = eff_purple;	break;
			case Block.COLOR.GREEN:		fx_prefab = eff_green;	break;
			case Block.COLOR.PINK:		fx_prefab = eff_pink;	break;
		}

		if(fx_prefab != null) {

			GameObject 	go = Instantiate(fx_prefab) as GameObject;

			go.AddComponent<FruitEffect>();

			go.transform.position = block.transform.position + Vector3.back*3.0f;
		}
	}


	// 우주선 수리용----------.
	public void repairEffect(Vector3 pos)
	{
		GameObject 	go = Instantiate(eff_star) as GameObject;
		go.AddComponent<FruitEffect>();
		go.transform.position = pos + Vector3.back*3.0f;
	}




}

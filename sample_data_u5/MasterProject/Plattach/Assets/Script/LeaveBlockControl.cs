using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LeaveBlockControl : MonoBehaviour {

	public	Block.COLOR		color = (Block.COLOR)0;

	public	LeaveBlockRoot	leave_block_root = null;

	public	Block.iPosition		i_pos;

	private	GameObject		models_root;	// 블록 모델들의 부모.
	private	GameObject[]	models;			// 각색 블록 모델.

	private static float	DISP_AREA_SIDE   =  6.0f;		// 화면에 보이는 범위의 가로 끝.
	private static float	DISP_AREA_BOTTOM = -8.0f;		// 화면에 보이는 범위의 아래 끝.

	public	Block.iPosition[]		connected_block;	// 옆 블록(같은 색일 때만).

	private	Animation	neko_motion = null;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void 	Awake()
	{
		// 각색의 블록의 모델을 찾아 둔다.

		this.models = new GameObject[(int)Block.COLOR.NORMAL_COLOR_NUM];

		this.models_root = this.transform.FindChild("models").gameObject;

		this.models[(int)Block.COLOR.PINK]    = this.models_root.transform.FindChild("block_pink").gameObject;
		this.models[(int)Block.COLOR.BLUE]    = this.models_root.transform.FindChild("block_blue").gameObject;
		this.models[(int)Block.COLOR.GREEN]   = this.models_root.transform.FindChild("block_green").gameObject;
		this.models[(int)Block.COLOR.ORANGE]  = this.models_root.transform.FindChild("block_orange").gameObject;
		this.models[(int)Block.COLOR.YELLOW]  = this.models_root.transform.FindChild("block_yellow").gameObject;
		this.models[(int)Block.COLOR.MAGENTA] = this.models_root.transform.FindChild("block_purple").gameObject;
		this.models[(int)Block.COLOR.NECO]    = this.models_root.transform.FindChild("neco").gameObject;

		// 비표시로 하면 가져올 수 없으므로 주의.
		this.neko_motion = this.models[(int)Block.COLOR.NECO].GetComponentInChildren<Animation>();

		// 일단 전부 비표시.
		for(int i = 0;i < this.models.Length;i++) {

			this.models[i].SetActive(false);
		}

		// 이 색의 블록만 표시한다る.
		this.setColor(this.color);

		this.connected_block = new Block.iPosition[(int)Block.DIR4.NUM];

		for(int i = 0;i < this.connected_block.Length;i++) {

			this.connected_block[i].clear();
		}
	}

	void 	Start()
	{
	}
	void	Update ()
	{
	}

	// ================================================================ //
	
	// 블록의 색을 설정한다.
	public void		setColor(Block.COLOR color)
	{
		this.color = color;

		if(this.models != null) {

			foreach(var model in this.models) {
	
				model.SetActive(false);
			}
	
			switch(this.color) {
	
				case Block.COLOR.PINK:
				case Block.COLOR.BLUE:
				case Block.COLOR.YELLOW:
				case Block.COLOR.GREEN:
				case Block.COLOR.MAGENTA:
				case Block.COLOR.ORANGE:
				case Block.COLOR.NECO:
				{
					this.models[(int)this.color].SetActive(true);
				}
				break;
			}
		}
	}

	// 이웃한 블록을 설정한다(같은 색일 때만).
	public void		setConnectedBlock(Block.DIR4 dir, Block.iPosition connected)
	{
		this.connected_block[(int)dir] = connected;
	}

	// 이웃한 블록을 가져온다(같은 색일 때만).
	public Block.iPosition	getConnectedBlock(Block.DIR4 dir)
	{
		return(this.connected_block[(int)dir]);
	}

	// ModelRoot(각색 블록 모델의 부모)를 얻는다.
	public GameObject	getModelsRoot()
	{
		return(this.models_root);
	}

	public Animation	getNecoMotion()
	{
		return(this.neko_motion);
	}

	public void		setNecoRotation(Vector3 forward)
	{
		if(this.color == Block.COLOR.NECO) {

			Quaternion	rot = this.models_root.transform.rotation;

			rot = Quaternion.Lerp(rot, Quaternion.LookRotation(-forward), 0.05f);

			this.models_root.transform.rotation = rot;
		}
	}

	// ================================================================ //

	// 블록이 표시 범위(화면 내)에 있는가?.
	public	bool	isInDispArea()
	{
		bool	ret = false;

		do {

			if(this.transform.position.x < -DISP_AREA_SIDE || DISP_AREA_SIDE < this.transform.position.x) {

				break;
			}
			if(this.transform.position.y < DISP_AREA_BOTTOM) {

				break;
			}

			ret = true;

		} while(false);

		return(ret);
	}
}

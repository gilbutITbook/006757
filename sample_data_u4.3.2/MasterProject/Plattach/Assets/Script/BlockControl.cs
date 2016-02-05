using UnityEngine;
using System.Collections;

// 블록.
public struct Block {

	public static float		COLLISION_SIZE = 1.0f;

	// 색.
	public enum COLOR {

		NONE = -1,

		PINK = 0,
		BLUE,
		GREEN,
		ORANGE,
		YELLOW,
		MAGENTA,

		NECO,

		GRAY,

		NUM,

		FIRST = PINK,
		//LAST = ORANGE,

		NORMAL_COLOR_NUM = GRAY,
	};

	// 네 방향.
	public enum DIR4 {

		NONE = -1,

		RIGHT,
		LEFT,
		UP,
		DOWN,

		NUM,
	};

	public static int	BLOCK_NUM_X = 9;		// 블록 수　가로.
	public static int	BLOCK_NUM_Y = 9;		// 블록 수  세로.

	// 블록의 모눈 상의 위치.
	public struct iPosition {

		public iPosition(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		// 클리어한다(무효인 값으로 한다).
		public void		clear()
		{
			this.x = -1;
			this.y = -1;
		}

		// 유효?.
		public bool		isValid()
		{
			return(this.x >= 0 && this.y >= 0);
		}

		public int		x;
		public int		y;
	};

	// 블록의 상태.
	public enum STEP {

		NONE = -1,

		IDLE = 0,			// 대기.
		GRABBED,			// 잡는 중.
		RELEASED,			// 잡기가 끝났다(연쇄 체크 대기중).
		SLIDE,				// 슬라이드 중(이웃 블록과 교체한다).

		VACANT,				// 사라진 뒤.
		//RESPAWN,			// 사라진 뒤 부활(내려오는 것을 만드는 것 만들 때까지 테스트용).

		FALL,				// 아래에 있는 블록이 사라진 뒤의 낙하 중.

		LONG_SLIDE,

		NUM,
	};
}

// 블록 컨트롤.
public class BlockControl : MonoBehaviour {

	// ================================================================ //

	public	Block.COLOR	color = (Block.COLOR)0;

	public	BlockRoot			block_root  = null;
	public	LeaveBlockRoot		leave_block_root  = null;

	public	Block.iPosition		i_pos;

	public	Block.DIR4		slide_dir = Block.DIR4.NONE;

	private Vector3		position_offset_initial = Vector3.zero;
	public Vector3		position_offset         = Vector3.zero;

	public	float		vanish_timer = -1.0f;		// 색이 모인 후에 사라지는 타이머.
	
	protected float			vanish_spin = 0.0f;
	public float			grab_timer = 0.0f;
	public bool				slide_forward = true;
	protected float			vanish_facing_timer = 0.0f;		// [sec] 정면을 향하는 타이머.

	protected static float	SLIDE_TIME = 0.2f;				// [sec] 슬라이드에 걸린 시간.
	protected static float	GRAB_EFFECT_TIME = 0.1f;		// [sec] 잡힌 연출 시간.


	public Material		opaque_material;			// 불투명용 재질.
	public Material		transparent_material;		// 반투명용 재질.

	private	Block.iPosition[]		connected_block;	// 이웃 블록(같은 색일 때만).

	protected bool	is_visible = true;		// 표시 중?.

	// ================================================================ //

	public Block.STEP	step      = Block.STEP.NONE;
	public Block.STEP	next_step = Block.STEP.NONE;
	public float		step_timer = 0.0f;

	// STEP.FALL에서 사용한다.
	private struct StepFall {

		public float	velocity;		// 현재의 낙하 혹도.
	}
	private StepFall	fall;

	private	GameObject		models_root = null;		// 블록 모델들의 부모.
	private	GameObject[]	models = null;			// 각 색 블록 모델.

	public Transform	nekoAtama = null;
	private	Animation	neko_motion = null;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void 	Awake()
	{
		// 각 색 블록 모델을 찾아 둔다..

		this.models = new GameObject[(int)Block.COLOR.NORMAL_COLOR_NUM];

		this.models_root = this.transform.FindChild("models").gameObject;

		this.models[(int)Block.COLOR.PINK]    = this.models_root.transform.FindChild("block_pink").gameObject;
		this.models[(int)Block.COLOR.BLUE]    = this.models_root.transform.FindChild("block_blue").gameObject;
		this.models[(int)Block.COLOR.GREEN]   = this.models_root.transform.FindChild("block_green").gameObject;
		this.models[(int)Block.COLOR.ORANGE]  = this.models_root.transform.FindChild("block_orange").gameObject;
		this.models[(int)Block.COLOR.YELLOW]  = this.models_root.transform.FindChild("block_yellow").gameObject;
		this.models[(int)Block.COLOR.MAGENTA] = this.models_root.transform.FindChild("block_purple").gameObject;
		this.models[(int)Block.COLOR.NECO]    = this.models_root.transform.FindChild("neco").gameObject;

		// 비표시로 하면 가져올 수 없게 되니 주의.
		this.neko_motion = this.models[(int)Block.COLOR.NECO].GetComponentInChildren<Animation>();

		// 일단 전부 비표시.
		for(int i = 0;i < this.models.Length;i++) {

			this.models[i].SetActive(false);
		}

		// 이 색의 블록만 표시한다.
		this.setColor(this.color);

		if(this.next_step == Block.STEP.NONE) {

			this.next_step = Block.STEP.IDLE;
		}
		
		this.connected_block = new Block.iPosition[(int)Block.DIR4.NUM];

		for(int i = 0;i < this.connected_block.Length;i++) {

			this.connected_block[i].clear();
		}

		//
	}

	void 	Start()
	{
	}

	void 	Update()
	{
		// ---------------------------------------------------------------- //
		// ３D 공간에서 마우스 위치 좌표를 구해 둔다.

		Vector3		mouse_position;

		this.block_root.unprojectMousePosition(out mouse_position, Input.mousePosition);

		Vector2		mouse_position_xy = new Vector2(mouse_position.x, mouse_position.y);

		// ---------------------------------------------------------------- //

		// 사라지는 연출 타이머.
		if(this.vanish_timer >= 0.0f) {

			this.vanish_timer -= Time.deltaTime;

			if(this.vanish_timer < 0.0f) {

				if(this.step != Block.STEP.SLIDE) {

					this.vanish_timer = -1.0f;

					// 퇴장 연출용 블록을 만든다.	
					this.leave_block_root.createLeaveBlock(this);

					for(int i = 0;i < this.connected_block.Length;i++) {
			
						this.connected_block[i].clear();
					}

					// (임시).
					this.next_step = Block.STEP.VACANT;

				} else {

					this.vanish_timer = 0.0f;
				}
			}

			this.vanish_facing_timer += Time.deltaTime;
		}

		// ---------------------------------------------------------------- //
		// 스텝 내 경과시간을 진행한다.

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 다음 상태로 이행할지 검사한다.


		if(this.next_step == Block.STEP.NONE) {

			switch(this.step) {
	
				case Block.STEP.IDLE:
				{
				}
				break;

				case Block.STEP.SLIDE:
				{
					if(this.step_timer >= SLIDE_TIME) {

						if(this.vanish_timer == 0.0f) {

							this.next_step = Block.STEP.VACANT;

						} else {

							this.next_step = Block.STEP.IDLE;
						}
					}
				}
				break;

				case Block.STEP.FALL:
				{
					if(this.position_offset.y <= 0.0f) {

						this.next_step = Block.STEP.IDLE;
						this.position_offset.y = 0.0f;
					}
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환됐을 때의 초기화.

		while(this.next_step != Block.STEP.NONE) {

			this.step      = this.next_step;
			this.next_step = Block.STEP.NONE;

			switch(this.step) {
	
				case Block.STEP.IDLE:
				{
					this.setVisible(true);
					this.position_offset = Vector3.zero;
					this.vanish_spin     = 0.0f;
					this.vanish_facing_timer = 0.0f;
				}
				break;

				case Block.STEP.GRABBED:
				{
					this.setVisible(true);
				}
				break;

				case Block.STEP.SLIDE:
				{
				}
				break;

				case Block.STEP.RELEASED:
				{
					this.setVisible(true);
					this.position_offset = Vector3.zero;
				}
				break;

				case Block.STEP.VACANT:
				{
					this.position_offset = Vector3.zero;
					this.setVisible(false);
				}
				break;

				case Block.STEP.FALL:
				{
					this.setVisible(true);
					this.vanish_spin = 0.0f;
					this.vanish_facing_timer = 0.0f;
					this.fall.velocity = 0.0f;
				}
				break;
			}

			this.step_timer = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		float	scale = 1.0f;

		// 잡힌 스케일.

		if(this.step == Block.STEP.GRABBED) {

			this.grab_timer += Time.deltaTime;

		} else {

			this.grab_timer -= Time.deltaTime;
		}

		this.grab_timer = Mathf.Clamp(this.grab_timer, 0.0f, GRAB_EFFECT_TIME);

		float	grab_ratio = Mathf.Clamp01(this.grab_timer/GRAB_EFFECT_TIME);

		scale = Mathf.Lerp(1.0f, 1.3f, grab_ratio);

		//

		this.models_root.transform.localPosition = Vector3.zero;

		if(this.vanish_timer > 0.0f) {

			// 다른 블록보다 앞에 표시되도록.
			this.models_root.transform.localPosition = Vector3.back;
		}

		switch(this.step) {

			case Block.STEP.IDLE:
			{
			}
			break;

			case Block.STEP.FALL:
			{
				this.fall.velocity += Physics.gravity.y*Time.deltaTime*0.3f;
				this.position_offset.y += this.fall.velocity*Time.deltaTime;

				if(this.position_offset.y < 0.0f) {

					this.position_offset.y = 0.0f;
				}
			}
			break;

			case Block.STEP.GRABBED:
			{
				// 슬라이드 방향.
				this.slide_dir = this.calcSlideDir(mouse_position_xy);

				// 다른 블록보다 앞쪽에 표시되도록.
				this.models_root.transform.localPosition = Vector3.back;
			}
			break;

			case Block.STEP.SLIDE:
			{
				float	ratio = this.step_timer/SLIDE_TIME;
			
				ratio = Mathf.Min(ratio, 1.0f);
				ratio = Mathf.Sin(ratio*Mathf.PI/2.0f);

				this.position_offset = Vector3.Lerp(this.position_offset_initial, Vector3.zero, ratio);

				//

				ratio = this.step_timer/SLIDE_TIME;
				ratio = Mathf.Min(ratio, 1.0f);
				ratio = Mathf.Sin(ratio*Mathf.PI);

				if(this.slide_forward) {

					scale += Mathf.Lerp(0.0f, 0.5f, ratio);

				} else {

					scale += Mathf.Lerp(0.0f, -0.5f, ratio);
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 포지션.

		Vector3		position = BlockRoot.calcBlockPosition(this.i_pos) + this.position_offset;

		this.transform.position = position;

		// ---------------------------------------------------------------- //
		// 사라지는 연출.

		this.models_root.transform.localRotation = Quaternion.identity;

		if(this.vanish_timer >= 0.0f) {

			// facing ... 블록의 윗면(돌기가 있는 면)을 앞으로 향하게 회전.
			// vanish ... 빙글빙글 회전.

			float	facing_ratio = Mathf.InverseLerp(0.0f, 0.2f, this.vanish_facing_timer);

			facing_ratio = Mathf.Clamp01(facing_ratio);

			float	vanish_ratio = Mathf.InverseLerp(0.0f, this.block_root.level_control.getVanishTime(), this.vanish_timer);
			float	spin_speed   = vanish_ratio;

			this.vanish_spin += spin_speed*10.0f;

			if(this.color != Block.COLOR.NECO) {

				this.models_root.transform.localRotation *= Quaternion.AngleAxis(-90.0f*facing_ratio, Vector3.right);
				this.models_root.transform.localRotation *= Quaternion.AngleAxis(this.vanish_spin, Vector3.up);
			}
		}

		this.nekoAtama.localScale = Vector3.one*1.0f;
		this.models_root.transform.localScale = Vector3.one*scale;

		// ---------------------------------------------------------------- //

		if(this.color == Block.COLOR.NECO) {

			float	anim_speed = 1.0f;

			if(this.vanish_timer >= 0.0f) {

				float	vanish_ratio = this.calc_neko_vanish_ratio();

				anim_speed  = Mathf.Lerp(1.0f, 1.5f, vanish_ratio);
			}

			this.neko_motion["00_Idle"].speed = anim_speed;
		}
	}

	void	LateUpdate()
	{
		// 고양이 목 흔들기 IK.
		// 애니메이션의 결과를 덮어써야만 하므로
		// UPdate()가 아니라 LateUpdate()에서 한다.
		do {

			if(this.color != Block.COLOR.NECO) {

				break;
			}
			if(this.vanish_timer < 0.0f) {

				break;
			}

			float	vanish_ratio = this.calc_neko_vanish_ratio();

			float	size_scale  = Mathf.Lerp(1.0f, 1.5f, vanish_ratio);
			float	angle_scale = Mathf.Lerp(1.0f, 2.5f, vanish_ratio);

			this.nekoAtama.localScale = Vector3.one*size_scale;

			float		angle;
			Vector3		axis;

			this.nekoAtama.localRotation.ToAngleAxis(out angle, out axis);
			this.nekoAtama.localRotation = Quaternion.AngleAxis(angle*angle_scale, axis);

		} while(false);
	}

	// 고양이 소실 연출의 보간율 계산.
	private float	calc_neko_vanish_ratio()
	{
		float	vanish_ratio = Mathf.InverseLerp(this.block_root.level_control.getVanishTime(), 0.0f, this.vanish_timer);

		if(vanish_ratio < 0.1f) {

			vanish_ratio = Mathf.InverseLerp(0.0f, 0.1f, vanish_ratio);
			vanish_ratio = Mathf.Lerp(0.0f, 1.0f, vanish_ratio);

		} else if(vanish_ratio < 0.5f) {

			vanish_ratio = Mathf.InverseLerp(0.1f, 0.5f, vanish_ratio);
			vanish_ratio = Mathf.Lerp(1.0f, 0.9f, vanish_ratio);

		} else {

			vanish_ratio = Mathf.InverseLerp(0.5f, 1.0f, vanish_ratio);
			vanish_ratio = vanish_ratio*vanish_ratio;

			vanish_ratio = Mathf.Lerp(0.9f, 0.0f, vanish_ratio);
		}

		return(vanish_ratio);
	}

	// ================================================================ //

	// 고양이의 Animation 컴포넌트를 얻는다.
	public Animation	getNekoMotion()
	{
		return(this.neko_motion);
	}

	// 이웃 블록을 설정한다(같은 색일 때만).
	public void		setConnectedBlock(Block.DIR4 dir, Block.iPosition connected)
	{
		this.connected_block[(int)dir] = connected;
	}

	// 이웃 블록을 얻는다(같은 색일 때만).
	public Block.iPosition	getConnectedBlock(Block.DIR4 dir)
	{
		return(this.connected_block[(int)dir]);
	}

	// 낙하시작(아래에 있는 블록이 사라졌을 때).
	public void		beginFall(BlockControl start)
	{
		this.next_step = Block.STEP.FALL;

		this.position_offset.y = (float)(start.i_pos.y - this.i_pos.y)*Block.COLLISION_SIZE;
	}

	// 슬라이드 동작 시작.
	public void	beginSlide(Vector3 offset)
	{
		this.position_offset_initial = offset;
		this.position_offset         = this.position_offset_initial;
		this.next_step = Block.STEP.SLIDE;
	}
	// 잡는 동작 시작.
	public void	beginGrab()
	{
		this.next_step = Block.STEP.GRABBED;
	}
	// 잡는 동작 끝.
	public void	endGrab()
	{
		this.block_root.hideArrow();
		this.next_step = Block.STEP.IDLE;
	}

	// 발화연출시작.
	public void		toVanishing()
	{
		float	vanish_time = this.block_root.level_control.getVanishTime();

		this.vanish_timer = vanish_time;

		this.block_root.effect_control.createEffect(this);
	}

	// 사라진 후에 위에서 내려오는 처리 시작.
	public void		beginRespawn(int start_ipos_y)
	{
		this.position_offset.y = (float)(start_ipos_y - this.i_pos.y)*Block.COLLISION_SIZE;

		this.next_step = Block.STEP.FALL;

		Block.COLOR		color = this.block_root.selectBlockColor();

		this.setColor(color);

		for(int i = 0;i < this.connected_block.Length;i++) {

			this.connected_block[i].clear();
		}
	}

	// ================================================================ //

	// dir 방향의 오프셋을 구한다.
	public float		calcDirOffset(Vector2 position, Block.DIR4 dir)
	{
		float	offset = 0.0f;

		Vector2	v = position - new Vector2(this.transform.position.x, this.transform.position.y);

		switch(dir) {

			case Block.DIR4.RIGHT:	offset =  v.x;	break;
			case Block.DIR4.LEFT:	offset = -v.x;	break;
			case Block.DIR4.UP:		offset =  v.y;	break;
			case Block.DIR4.DOWN:	offset = -v.y;	break;
		}

		return(offset);
	}

	// 잡을 수 있는가?.
	public bool		isGrabbable()
	{
		bool	is_grabbable = false;

		switch(this.step) {

			case Block.STEP.IDLE:
			{
				// 발화 중엔 이동할 수 없다.
				if(!this.isVanishing()) {
 
					is_grabbable = true;
				}
			}
			break;
		}

		return(is_grabbable);
	}

	// 슬라이드 중?.
	public bool		isSliding()
	{
		bool	is_sliding = (this.position_offset.x != 0.0f);

		return(is_sliding);
	}

	// 발화 중?.
	public bool		isVanishing()
	{
		bool	is_vanishing = (this.vanish_timer > 0.0f);

		return(is_vanishing);
	}
	
	// 발화 중 타이머를 되돌린다.
	public void		rewindVanishTimer()
	{
		float	vanish_time = this.block_root.level_control.getVanishTime();

		this.vanish_timer = vanish_time;
	}

	// 그쪽으로 슬라이드할 수 있는가?
	public bool		isSlidable(Block.DIR4 dir)
	{
		bool	ret = false;

		switch(dir) {

			case Block.DIR4.RIGHT:	ret = (this.i_pos.x < Block.BLOCK_NUM_X - 1);	break;
			case Block.DIR4.LEFT:	ret = (this.i_pos.x > 0);						break;
			case Block.DIR4.UP:		ret = (this.i_pos.y < Block.BLOCK_NUM_Y - 1);	break;
			case Block.DIR4.DOWN:	ret = (this.i_pos.y > 0);						break;
		}

		return(ret);
	}

	// 슬라이드 입력 방향을 구한다.
	public Block.DIR4	calcSlideDir(Vector2 mouse_position)
	{
		Block.DIR4	dir = Block.DIR4.NONE;

		Vector2		v = mouse_position - new Vector2(this.transform.position.x, this.transform.position.y);

		if(v.magnitude > 0.1f) {

			if(v.y > v.x) {
	
				if(v.y > -v.x) {
	
					dir = Block.DIR4.UP;
	
				} else {
	
					dir = Block.DIR4.LEFT;
				}
	
			} else {
	
				if(v.y > -v.x) {
	
					dir = Block.DIR4.RIGHT;
	
				} else {
	
					dir = Block.DIR4.DOWN;
				}
			}
		}

		return(dir);
	}

	// 반대 방향을 구한다.
	static public Block.DIR4	getOppose(Block.DIR4 dir)
	{
		Block.DIR4	oppose = Block.DIR4.NONE;

		switch(dir) {

			case Block.DIR4.RIGHT:	oppose = Block.DIR4.LEFT;	break;
			case Block.DIR4.LEFT:	oppose = Block.DIR4.RIGHT;	break;
			case Block.DIR4.UP:		oppose = Block.DIR4.DOWN;	break;
			case Block.DIR4.DOWN:	oppose = Block.DIR4.UP;	break;
		}

		return(oppose);
	}

	public static Block.iPosition		getNext_iPosition(Block.iPosition i_pos, Block.DIR4 dir)
	{
		Block.iPosition		next_ipos = i_pos;

		switch(dir) {

			case Block.DIR4.RIGHT:	next_ipos.x += 1;	break;
			case Block.DIR4.LEFT:	next_ipos.x -= 1;	break;
			case Block.DIR4.UP:		next_ipos.y += 1;	break;
			case Block.DIR4.DOWN:	next_ipos.y -= 1;	break;
		}

		return(next_ipos);
	}

	public static bool		isValid_iPosition(Block.iPosition i_pos)
	{
		bool	is_valid;

		do {

			is_valid = false;

			if(i_pos.x < 0 || Block.BLOCK_NUM_X <= i_pos.x) {

				break;
			}
			if(i_pos.y < 0 || Block.BLOCK_NUM_Y <= i_pos.y) {

				break;
			}
		
			is_valid = true;

		} while(false);

		return(is_valid);
	}

	// 블록이 대기 상태?(아무것도 하지 않는 중?).
	public bool		isIdle()
	{
		bool	is_idle = false;

		if(this.step == Block.STEP.IDLE && this.next_step == Block.STEP.NONE) {

			is_idle = true;
		}

		return(is_idle);
	}

	// 블록이 비어있나?(사라진 뒤).
	public bool		isVacant()
	{
		bool	is_vacant = false;

		if(this.step == Block.STEP.VACANT && this.next_step == Block.STEP.NONE) {

			is_vacant = true;
		}

		return(is_vacant);
	}

	// 블록 표시 중?.
	public bool		isVisible()
	{
		return(this.is_visible);
	}
	// 블록 표시/비표시를 설정한다.
	public void		setVisible(bool is_visible)
	{
		if(this.is_visible != is_visible) {

			this.is_visible = is_visible;

			this.models_root.SetActive(this.is_visible);
		}
	}


	// position이 블록 안에 있는가?
	public bool		isContainedPosition(Vector2 position)
	{
		bool		ret = false;
		Vector3		center = this.transform.position;
		float		h = Block.COLLISION_SIZE/2.0f;

		do {

			if(position.x < center.x - h || center.x + h < position.x) {

				break;
			}
			if(position.y < center.y - h || center.y + h < position.y) {

				break;
			}

			ret = true;

		} while(false);

		return(ret);
	}

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

	// ModelRoot(각 색 블록 모델의 부모)를 얻는다.
	public GameObject	getModelsRoot()
	{
		return(this.models_root);
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockRoot : MonoBehaviour {

	private	GameObject		main_camera      = null;
	private ScoreCounter	score_counter    = null;
	public	GameObject		blockPrefab      = null;
	public	GameObject		leaveBlockPrefab = null;

	public	GameObject		arrowPrefab = null;
	public	GameObject		arrow = null;

	public 	BlockControl[,]	blocks;							// 블록.

	private int[,]			block_islands;					// 발화 시에 섬의 수를 세기 위해 사용한다.
	private BlockControl	grabbed_block = null;

	private bool			is_vanishing_prev = false;

	public	TextAsset		levelData = null;				// 레벨 데이터의 텍스트 파일.
	public	LevelControl	level_control;					// 레벨 데이터의 컨트롤.

	public SoundControl		sound_control;

	public VanishEffectControl	effect_control = null;		// 발화했을 때 효과를 만드는 클래스.

	public LeaveBlockRoot	leave_block_root = null;

	private bool	neco_fever = false;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Awake()
	{
		this.main_camera   = GameObject.FindGameObjectWithTag("MainCamera");
		this.score_counter = this.gameObject.GetComponent<ScoreCounter>();

		this.arrow = GameObject.Instantiate(this.arrowPrefab) as GameObject;

		this.hideArrow();

		this.sound_control = GameObject.Find("SoundRoot").GetComponent<SoundControl>();

		this.effect_control = this.gameObject.GetComponent<VanishEffectControl>();
		this.leave_block_root = this.gameObject.GetComponent<LeaveBlockRoot>();
	}

	void	Start()
	{
	}

	void	Update()
	{
		Vector3		mouse_position;

		this.unprojectMousePosition(out mouse_position, Input.mousePosition);

		Vector2		mouse_position_xy = new Vector2(mouse_position.x, mouse_position.y);

		if(this.grabbed_block == null) {

			// 블록을 잡지 않았다.
			do {

				// 화면 어딘가에 낙하 중인 블록이 있을 때는 잡을 수 없다.
				if(this.is_has_falling_block()) {

					//break;
				}

				// 마우스를 클릭한 순간이 아니다.
				if(!Input.GetMouseButtonDown(0)) {

					break;
				}

				// 잡을 수 있는지 모든 블록으로 검사한다.
				foreach(BlockControl block in this.blocks) {

					// 잡을 수 없는 상태의 블록은 패스.
					if(!block.isGrabbable()) {

						continue;
					}

					// 마우스 커서가 겹쳐있지 않은 블록은 패스.
					if(!block.isContainedPosition(mouse_position_xy)) {

						continue;
					}

					this.grabbed_block = block;
					this.grabbed_block.beginGrab();

					this.sound_control.playSound(Sound.SOUND.GRAB);		// sound.

					break;
				}

			} while(false);

		} else {

			// 블록 잡는 중.
			do {

				// (임시) 일단 화살표를 지운다..
				this.hideArrow();

				BlockControl	swap_target = this.getNextBlock(grabbed_block, grabbed_block.slide_dir);

				if(swap_target == null) {

					break;
				}

				// 잡을 수 없는 블록(사라지는 중, 낙하 중 등)은 교체할 수 없다.
				if(!swap_target.isGrabbable()) {

					break;
				}

				// 슬라이드 할 수 있을 때는 화살표 표시.
				this.dispArrow(grabbed_block, grabbed_block.slide_dir);

				// 블록의 중심 위치에서 마우스 커서까지의 거리.
				float	offset = this.grabbed_block.calcDirOffset(mouse_position_xy, this.grabbed_block.slide_dir);

				// 잡은 블록이 절반이상 슬라이드했다면 이웃 블록과 교체.
				if(offset < Block.COLLISION_SIZE/2.0f) {

					break;
				}

				// 교체 시작.
				this.swapBlock(grabbed_block, grabbed_block.slide_dir, swap_target);
				this.hideArrow();
				this.grabbed_block = null;

				this.sound_control.playSound(Sound.SOUND.SLIDE);		// sound.

			} while(false);

			// 버튼에서 손을 떼면 블록을 놓는다 
			if(!Input.GetMouseButton(0)) {
				this.grabbed_block.endGrab();
				this.grabbed_block = null;
			}
		}

		// ---------------------------------------------------------------- //
		// 발화 검사.

		// 낙하 중인 블록이 있다 = 연쇄가 끊겼을 때.
		if(this.is_has_falling_block()){

			// 연쇄 수를 클리어한다.
			this.score_counter.clearIgniteCount();
		}

		if(this.is_has_falling_block() || this.is_has_sliding_block()) {

			// 화면 안 어딘가에서 낙하 중이나 슬라이드 중인 블록이 있을.
			// 때는 발화 검사를 하지 않는다.

		} else {

			// 발화한 블록의 수를 센다.
			int		ignite_count = 0;

			foreach(BlockControl block in this.blocks) {

				if(!block.isIdle()) {

					continue;
				}

				if(this.checkConnection(block)) {

					ignite_count++;
				}
			}

			// 어딘가에서 블록이 발화했다면.
			if(ignite_count > 0) {

				// this.sound_control.isSoundPlay(Sound.SOUND.IGNIT1);		// sound.

				if(!this.is_vanishing_prev) {

					// 발화 시작 시에 발화  카운트를 클리어한다.
					this.score_counter.clearIgniteCount();
				}

				// 발화 횟수를 플러스한다.
				this.score_counter.addIgniteCount(ignite_count);

				this.score_counter.updateTotalScore();		// asuna

				// 발화 중인 블록 전체의 발화 중 타이머를 되돌린다.
				// 하는 김에 발화 블록 수도 센다.

				int		block_count = 0;

				foreach(BlockControl block in this.blocks) {

					if(block.isVanishing()) {

						block.rewindVanishTimer();

						block_count++;
					}
				}

				// 발화한 블록 수를 센다.
				this.score_counter.setIgniteBlockCount(block_count);

				if(this.score_counter.getIgniteCount() >= 5) {

					this.neco_fever = true;
				}

				// 섬의 수를 센다.
				this.count_islands();
			}
		}

		// ---------------------------------------------------------------- //
		// 사라진 블록 위에 있는 블록을 아래로 떨어뜨린다.

		bool	is_vanishing = this.is_has_vanishing_block();

		do {

			// 사라지는 중인 블록(정리할 수 있는)後づけできる）이 있는 동안은.
			// 낙하를 시작하지 않는다.
			if(is_vanishing) {

				break;
			}

			// 슬라이드 중인 블록이 있을 경우도 낙하를 시작하지 않는다.
			if(this.is_has_sliding_block()) {

				break;
			}

			// ------------------------------------------------------ //

			for(int x = 0;x < Block.BLOCK_NUM_X;x++) {
	
				// 이 예의 어딘가에 슬라이드 중인 블록이 있으면 패스.
				if(this.is_has_sliding_block_in_column(x)) {
	
					continue;
				}
	
				for(int y = 0;y < Block.BLOCK_NUM_Y - 1;y++) {
	
					if(!this.blocks[x, y].isVacant()) {
	
						continue;
					}
	
					for(int y1 = y + 1;y1 < Block.BLOCK_NUM_Y;y1++) {
	
						if(this.blocks[x, y1].isVacant()) {
		
							continue;
						}
	
						// [x, y] ~ [x, y1 - 1]의 블록이 사라졌으므로 [x, y1]에 있는.
						// 블록을 아래로 떨어뜨린다.
						this.fallBlock(this.blocks[x, y], Block.DIR4.UP, this.blocks[x, y1]);
						break;
					}
				}
			}

			// 스키마 상의 블록을 아래로 떨어뜨린 다음, 비어있는 곳에 
			// 새로 블록을 만든다(화면 위에서 내려온다).
			//
			for(int x = 0;x < Block.BLOCK_NUM_X;x++) {
	
				// 새로운 블록이 출현하는 위치.
				int		fall_start_y = Block.BLOCK_NUM_Y;
	
				for(int y = 0;y < Block.BLOCK_NUM_Y;y++) {
	
					if(!this.blocks[x, y].isVacant()) {
	
						continue;
					}
					this.blocks[x, y].beginRespawn(fall_start_y);
	
					// 동시에 출현하는 블록이 겹치지 않도록.
					// 출현 위치를 위로 올린다.
					fall_start_y++;
				}
			}

		} while(false);

		// ---------------------------------------------------------------- //

		if(!is_vanishing && this.is_vanishing_prev) {

			// 총 점수를 갱신한다.
			// 발화가 끝나면 이번 발화로 얻은 점수가 합산되어 들어가도록.
			this.score_counter.updateTotalScore();
			this.sound_control.playSound(Sound.SOUND.CLEAR);		// sound.
		}

		this.is_vanishing_prev = is_vanishing;
	}

	// 섬의 수를 센다.
	private void	count_islands()
	{
		for(int y = 0;y < Block.BLOCK_NUM_Y;y++) {

			for(int x = 0;x < Block.BLOCK_NUM_X;x++) {

				this.block_islands[x, y] = -1;
			}
		}

		int				island_index = 0;
		int				max_connect  = 0;
		Block.COLOR		block_color  = Block.COLOR.NONE;

		for(int y = 0;y < Block.BLOCK_NUM_Y;y++) {

			for(int x = 0;x < Block.BLOCK_NUM_X;x++) {

				block_color = this.blocks[x, y].color;

				int	connect_count = this.check_island_sub(x, y, block_color, island_index, 0);

				if(connect_count > 0) {

					island_index++;
					max_connect = Mathf.Max(max_connect, connect_count);
				}
			}
		}

		this.score_counter.setIslandCount(island_index);
		this.score_counter.setMaxIslandSize(max_connect);
	}

	private int		check_island_sub(int x, int y, Block.COLOR color, int island_index, int connect_count)
	{
		do {

			if(this.block_islands[x, y] != -1) {

				break;
			}
			if(!this.blocks[x, y].isVanishing()) {

				continue;
			}
			if(this.blocks[x, y].color != color) {

				break;
			}

			//

			this.block_islands[x, y] = island_index;

			connect_count++;

			if(0 < x) {

				connect_count = this.check_island_sub(x - 1, y, color, island_index, connect_count);
			}
			if(x < Block.BLOCK_NUM_X - 1) {

				connect_count = this.check_island_sub(x + 1, y, color, island_index, connect_count);
			}
			if(0 < y) {

				connect_count = this.check_island_sub(x, y - 1, color, island_index, connect_count);
			}
			if(y < Block.BLOCK_NUM_Y - 1) {

				connect_count = this.check_island_sub(x, y + 1, color, island_index, connect_count);
			}

		} while(false);

		return(connect_count);
	}

	// 화면 어딘가에 사라지는 중인 블록이 있는가?.
	private bool	is_has_vanishing_block()
	{
		bool	ret = false;

		foreach(BlockControl block in this.blocks) {

			if(block.vanish_timer > 0.0f) {

				ret = true;
				break;
			}
		}

		return(ret);
	}

	// 화면 어딘가에 슬라이드 중인 블록이 있는가?.
	private bool	is_has_sliding_block()
	{
		bool	ret = false;

		foreach(BlockControl block in this.blocks) {

			if(block.step == Block.STEP.SLIDE) {

				ret = true;
				break;
			}
		}

		return(ret);
	}
	// 화면 어딘가에 낙하 중인 블록이 있는가?.
	private bool	is_has_falling_block()
	{
		bool	ret = false;

		foreach(BlockControl block in this.blocks) {

			if(block.step == Block.STEP.FALL) {

				ret = true;
				break;
			}
		}

		return(ret);
	}


	// 세로 한 줄 어딘가에 슬라이드 중인 블록이 있는가?.
	private bool	is_has_sliding_block_in_column(int x)
	{
		bool	ret = false;

		for(int y = 0;y < Block.BLOCK_NUM_Y;y++) {

			if(this.blocks[x, y].isSliding()) {

				ret = true;
				break;
			}
		}

		return(ret);
	}

	// ================================================================ //

	// 블록을 떨어뜨린다.
	public void		fallBlock(BlockControl block0, Block.DIR4 dir, BlockControl block1)
	{
		// 낙하시킬 블록이 잡는 중이라면 놓는다.
		if(this.grabbed_block == block0 || this.grabbed_block == block1) {

			this.hideArrow();
			this.grabbed_block = null;
		}

		//

		Block.COLOR		color0 = block0.color;
		Block.COLOR		color1 = block1.color;

		Vector3	scale0 = block0.transform.localScale;
		Vector3	scale1 = block1.transform.localScale;

		float	vanish_timer0 = block0.vanish_timer;
		float	vanish_timer1 = block1.vanish_timer;

		bool	visible0 = block0.isVisible();
		bool	visible1 = block1.isVisible();

		Block.STEP	step0 = block0.step;
		Block.STEP	step1 = block1.step;

		float	frame0 = 0.0f;
		float	frame1 = 0.0f;

		if(color1 == Block.COLOR.NECO) {

			frame0 = block0.getNekoMotion()["00_Idle"].time;
			frame1 = block1.getNekoMotion()["00_Idle"].time;
		}

		//

		block0.setColor(color1);
		block1.setColor(color0);

		block0.transform.localScale = scale1;
		block1.transform.localScale = scale0;

		block0.vanish_timer = vanish_timer1;
		block1.vanish_timer = vanish_timer0;

		block0.setVisible(visible1);
		block1.setVisible(visible0);

		block0.step = step1;
		block1.step = step0;

		if(color1 == Block.COLOR.NECO) {

			block0.getNekoMotion()["00_Idle"].time = frame1;
			block1.getNekoMotion()["00_Idle"].time = frame0;
		}

		block0.beginFall(block1);
	}

	// ================================================================ //

	public void		create()
	{
		this.level_control = new LevelControl();
		this.level_control.initialize();
		this.level_control.loadLevelData(this.levelData);
		this.level_control.selectLevel();
	}

	// 초기 배치.
	public void		initialSetUp()
	{
		// 블록 생성, 배치.

		this.blocks       = new BlockControl[Block.BLOCK_NUM_X, Block.BLOCK_NUM_Y];

		Block.COLOR			color = Block.COLOR.FIRST;

		for(int y = 0;y < Block.BLOCK_NUM_Y;y++) {

			for(int x = 0;x < Block.BLOCK_NUM_X;x++) {

				GameObject game_object = Instantiate(this.blockPrefab) as GameObject;

				BlockControl	block = game_object.GetComponent<BlockControl>();

				this.blocks[x, y] = block;

				block.i_pos.x = x;
				block.i_pos.y = y;
				block.block_root = this;
				block.leave_block_root = this.leave_block_root;

				//

				Vector3	position = BlockRoot.calcBlockPosition(block.i_pos);

				block.transform.position = position;

				color = this.selectBlockColor();
				block.setColor(color);

				// 하이어라키 뷰에서 위치를 확인하기 쉽게 블록의 좌표를 이름에 붙여 둔다.
				block.name = "block(" + block.i_pos.x.ToString() + "," + block.i_pos.y.ToString() + ")";
			}
		}

		//

		this.block_islands = new int[Block.BLOCK_NUM_X, Block.BLOCK_NUM_Y];

		for(int y = 0;y < Block.BLOCK_NUM_Y;y++) {

			for(int x = 0;x < Block.BLOCK_NUM_X;x++) {

				this.block_islands[x, y] = -1;
			}
		}
	}

	// 레벨 데이터의 확률로 랜덤하게 블록의 색을 선택한다.
	public Block.COLOR	selectBlockColor()
	{
		Block.COLOR	color = Block.COLOR.FIRST;

		LevelData	level_data = this.level_control.getCurrentLevelData();

		float	rand = Random.Range(0.0f, 1.0f);
		float	sum  = 0.0f;
		int		i = 0;

		for(i = 0;i < level_data.probability.Length - 1;i++) {

			if(level_data.probability[i] == 0.0f) {

				continue;
			}

			sum += level_data.probability[i];

			if(rand < sum) {

				break;
			}
		}

		color = (Block.COLOR)i;

		if(this.neco_fever) {

			if(color == Block.COLOR.BLUE) {
	
				color = Block.COLOR.NECO;
			}
		}

		return(color);
	}

	// 그리드 번호(iPosition)로 좌표를 구한다.
	public static Vector3	calcBlockPosition(Block.iPosition i_pos)
	{
		Vector3		position = new Vector3(-(Block.BLOCK_NUM_X/2.0f - 0.5f), -(Block.BLOCK_NUM_Y/2.0f - 0.5f), 0.0f);

		position.x += (float)i_pos.x*Block.COLLISION_SIZE;
		position.y += (float)i_pos.y*Block.COLLISION_SIZE;

		return(position);
	}

	// ================================================================ //

	// dir 방향의 이웃 블록을 얻는다.
	public BlockControl		getNextBlock(BlockControl block, Block.DIR4 dir)
	{
		BlockControl	next_block = null;

		switch(dir) {

			case Block.DIR4.RIGHT:
			{
				if(block.i_pos.x < Block.BLOCK_NUM_X - 1) {

					next_block = this.blocks[block.i_pos.x + 1, block.i_pos.y];
				}
			}
			break;

			case Block.DIR4.LEFT:
			{
				if(block.i_pos.x > 0) {

					next_block = this.blocks[block.i_pos.x - 1, block.i_pos.y];
				}
			}
			break;

			case Block.DIR4.UP:
			{
				if(block.i_pos.y < Block.BLOCK_NUM_Y - 1) {

					next_block = this.blocks[block.i_pos.x, block.i_pos.y + 1];
				}
			}
			break;

			case Block.DIR4.DOWN:
			{
				if(block.i_pos.y > 0) {

					next_block = this.blocks[block.i_pos.x, block.i_pos.y - 1];
				}
			}
			break;
		}

		return(next_block);
	}

	// 네 방향(dir4)에서 벡터를 얻는다.
	public static Vector3	getDirVector(Block.DIR4 dir)
	{
		Vector3		v = Vector3.zero;

		switch(dir) {

			case Block.DIR4.RIGHT:	v = Vector3.right;	break;
			case Block.DIR4.LEFT:	v = Vector3.left;	break;
			case Block.DIR4.UP:		v = Vector3.up;		break;
			case Block.DIR4.DOWN:	v = Vector3.down;	break;
		}

		v *= Block.COLLISION_SIZE;

		return(v);
	}

	// 반대 방향을 얻는다.
	public static Block.DIR4	getOppositDir(Block.DIR4 dir)
	{
		Block.DIR4		opposit = dir;

		switch(dir) {

			case Block.DIR4.RIGHT:	opposit = Block.DIR4.LEFT;	break;
			case Block.DIR4.LEFT:	opposit = Block.DIR4.RIGHT;	break;
			case Block.DIR4.UP:		opposit = Block.DIR4.DOWN;	break;
			case Block.DIR4.DOWN:	opposit = Block.DIR4.UP;	break;
		}

		return(opposit);
	}

	// 두 블록을 교체한다.
	public void		swapBlock(BlockControl block0, Block.DIR4 dir, BlockControl block1)
	{
		// 교체 상대는 반대 방향으로 슬라이드.
		block1.slide_dir = BlockRoot.getOppositDir(dir);

		Block.COLOR		color0 = block0.color;
		Block.COLOR		color1 = block1.color;

		Vector3	scale0 = block0.transform.localScale;
		Vector3	scale1 = block1.transform.localScale;

		float	vanish_timer0 = block0.vanish_timer;
		float	vanish_timer1 = block1.vanish_timer;

		Vector3	offset0 = BlockRoot.getDirVector(dir);
		Vector3	offset1 = BlockRoot.getDirVector(block1.slide_dir);

		float	grab_timer0 = block0.grab_timer;	
		float	grab_timer1 = block1.grab_timer;	

		//

		block0.setColor(color1);
		block1.setColor(color0);

		block0.transform.localScale = scale1;
		block1.transform.localScale = scale0;

		block0.vanish_timer = vanish_timer1;
		block1.vanish_timer = vanish_timer0;

		block0.grab_timer = grab_timer1;
		block1.grab_timer = grab_timer0;

		block0.slide_forward = false;
		block1.slide_forward = true;

		block0.beginSlide(offset0);
		block1.beginSlide(offset1);
	}

	// 마우스 위치를 ３D 공간의 월드 좌표로 변환한다.
	//
	// ・마우스 커서와 카메라 위치를 통하는 직선.
	// ・地面の当たり判定となる平面
	//　↑ 두 개가 교차하는 곳을 구한다.
	//
	public bool		unprojectMousePosition(out Vector3 world_position, Vector3 mouse_position)
	{
		bool	ret;

		// 지면 충돌 판정이 될 평면.
		Plane	plane = new Plane(Vector3.back, new Vector3(0.0f, 0.0f, -Block.COLLISION_SIZE/2.0f));

		// 카메라 위치와 마우스 커서 위치를 통하는 직선.
		Ray		ray = this.main_camera.GetComponent<Camera>().ScreenPointToRay(mouse_position);

		// 위 두 개가 교차하는 곳을 구한다.

		float	depth;

		if(plane.Raycast(ray, out depth)) {

			world_position = ray.origin + ray.direction*depth;

			ret = true;

		} else {

			world_position = Vector3.zero;

			ret = false;
		}

		return(ret);
	}

	// 슬라이드 화살표를 표시한다.
	public void		dispArrow(BlockControl block, Block.DIR4 dir)
	{
		this.arrow.gameObject.SetActive(true);
		this.arrow.gameObject.transform.position = block.transform.position + Vector3.back*(Block.COLLISION_SIZE*5.0f + 0.01f);

		float	angle = 0.0f;

		switch(dir) {

			case Block.DIR4.RIGHT:	angle =   0.0f;	break;
			case Block.DIR4.LEFT:	angle = 180.0f;	break;
			case Block.DIR4.UP:		angle =  90.0f;	break;
			case Block.DIR4.DOWN:	angle = -90.0f;	break;
		}

		this.arrow.gameObject.transform.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);
	}

	// 슬라이드 표시 화살표를 감춘다.
	public void		hideArrow()
	{
		this.arrow.gameObject.SetActive(false);
	}

	// ================================================================ //

	// 같은 색이 나란히 있는지 체크.
	public bool		checkConnection(BlockControl start)
	{
		bool	ret = false;

		int		normal_block_num = 0;

		if(!start.isVanishing()) {

			normal_block_num = 1;
		}

		// 가로 방향.
		// 같은 색이 나란히 있는 범위를 조사한다.

		int		rx = start.i_pos.x;
		int		lx = start.i_pos.x;

		// start와 같은 색 블록이면 왼쪽으로 나아간다.
		for(int x = lx - 1;x > 0;x--) {

			BlockControl	next_block = this.blocks[x, start.i_pos.y];

			if(next_block.color != start.color) {

				break;
			}
			if(next_block.step == Block.STEP.FALL || next_block.next_step == Block.STEP.FALL) {
	
				break;
			}
			if(next_block.step == Block.STEP.SLIDE || next_block.next_step == Block.STEP.SLIDE) {
	
				break;
			}
			if(!next_block.isVanishing()) {
	
				normal_block_num++;
			}
			lx = x;
		}
		// start 와 같은 색의 블록이면 오른쪽으로 나아간다.
		for(int x = rx + 1;x < Block.BLOCK_NUM_X;x++) {

			BlockControl	next_block = this.blocks[x, start.i_pos.y];

			if(next_block.color != start.color) {

				break;
			}
			if(next_block.step == Block.STEP.FALL || next_block.next_step == Block.STEP.FALL) {
	
				break;
			}
			if(next_block.step == Block.STEP.SLIDE || next_block.next_step == Block.STEP.SLIDE) {
	
				break;
			}
			if(!next_block.isVanishing()) {
	
				normal_block_num++;
			}
			rx = x;
		}

		// rx ~ lx까지의 블록이 같은 색.
		// 
		do {

			// ３개 이하면 지우지 않는다.
			if(rx - lx + 1 < 3) {

				break;
			}
			//  '사라지는 연출 중이 아닌' 블록이 한개도 없으면 사라지지 않는다.
			if(normal_block_num == 0) {

				break;
			}

			for(int x = lx;x < rx + 1;x++) {

				if(this.blocks[x, start.i_pos.y] == this.grabbed_block) {

					this.hideArrow();
					this.grabbed_block.endGrab();
					this.grabbed_block = null;
				}

				// 발화 연출 시작
				this.blocks[x, start.i_pos.y].toVanishing();

				// 이웃 블록을 연결해 둔다.
				if(x > lx) {

					this.connect_x(x - 1, x, start.i_pos.y);
				}
				if(x < rx) {

					this.connect_x(x, x + 1, start.i_pos.y);
				}
			}

			ret = true;

		} while(false);

		// ---------------------------------------------------------------- //
		// 세로 방향.

		normal_block_num = 0;

		if(!start.isVanishing()) {

			normal_block_num = 1;
		}


		int		uy = start.i_pos.y;
		int		dy = start.i_pos.y;

		for(int y = dy - 1;y > 0;y--) {

			BlockControl	next_block = this.blocks[start.i_pos.x, y];

			if(next_block.color != start.color) {

				break;
			}
			if(next_block.step == Block.STEP.FALL || next_block.next_step == Block.STEP.FALL) {
	
				break;
			}
			if(next_block.step == Block.STEP.SLIDE || next_block.next_step == Block.STEP.SLIDE) {
	
				break;
			}
			if(!next_block.isVanishing()) {
	
				normal_block_num++;
			}
			dy = y;
		}
		for(int y = uy + 1;y < Block.BLOCK_NUM_Y;y++) {

			BlockControl	next_block = this.blocks[start.i_pos.x, y];

			if(next_block.color != start.color) {

				break;
			}
			if(next_block.step == Block.STEP.FALL || next_block.next_step == Block.STEP.FALL) {
	
				break;
			}
			if(next_block.step == Block.STEP.SLIDE || next_block.next_step == Block.STEP.SLIDE) {
	
				break;
			}
			if(!next_block.isVanishing()) {
	
				normal_block_num++;
			}
			uy = y;
		}

		do {

			if(uy - dy + 1 < 3) {

				break;
			}
			if(normal_block_num == 0) {

				break;
			}

			for(int y = dy;y < uy + 1;y++) {
	
				if(this.blocks[start.i_pos.x, y] == this.grabbed_block) {

					this.hideArrow();
					this.grabbed_block.endGrab();
					this.grabbed_block = null;
				}

				this.blocks[start.i_pos.x, y].toVanishing();

				// 이웃한 블록을 연결해 둔다.
				if(y > dy) {

					this.connect_y(y - 1, y, start.i_pos.x);
				}
				if(y < uy) {

					this.connect_y(y, y + 1, start.i_pos.x);
				}
			}

			ret = true;

		} while(false);

		// ---------------------------------------------------------------- //

		return(ret);
	}

	// 가로로 나열된 블록을 연결한다(같은 색).
	private void		connect_x(int lx, int rx, int y)
	{
		this.blocks[rx, y].setConnectedBlock(Block.DIR4.LEFT,  new Block.iPosition(lx, y));
		this.blocks[lx, y].setConnectedBlock(Block.DIR4.RIGHT, new Block.iPosition(rx, y));
	}

	// 세로로 나열된 블록을 연결한다(같은 색).
	private void		connect_y(int uy, int dy, int x)
	{
		this.blocks[x, uy].setConnectedBlock(Block.DIR4.DOWN, new Block.iPosition(x, dy));
		this.blocks[x, dy].setConnectedBlock(Block.DIR4.UP,   new Block.iPosition(x, uy));
	}

}

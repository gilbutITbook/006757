using UnityEngine;
using System.Collections;


// 블록에 관련된 정보를 다룬다.
public class Block {
	public static float COLLISION_SIZE = 1.0f; // 블록의 충돌 크기.
	public static float VANISH_TIME = 3.0f; // 발화하고 사라지는 시간.
	public struct iPosition { // 그리드에서의 좌표를 나타내는 구조체.
		public int x; // X좌표.
		public int y; // Y좌표.
	}

	public enum COLOR { // 블록 색상.
		NONE = -1, // 색 지정 없음.
		PINK = 0, // 분홍색.
		BLUE, // 파란색.
		YELLOW, // 노란색.
		GREEN, // 녹색.
		MAGENTA, // 마젠타.
		ORANGE, // 오렌지.
		GRAY, // 회색.
		NUM, // 색상이 몇 종류인지 나타낸다(=7).
		FIRST = PINK, // 초기 색상(분홍색).
		LAST = ORANGE, // 마지막 색상(오렌지).
		NORMAL_COLOR_NUM = GRAY, // 일반 색상(그레이 이외 색)의 수.
	};

	public enum DIR4 { // 상하좌우 네 방향.
		NONE = -1, // 방향 지정 없음.
		RIGHT, // 오른쪽.
		LEFT, // 왼쪽.
		UP, // 위.
		DOWN, // 아래.
		NUM, // 방향이 몇 종류인지 나타낸다(=4).
	};

	public enum STEP { // 블록의 상태 표시.
		NONE = -1, // 상태 정보 없음.
		IDLE = 0, // 대기 중.
		GRABBED, // 잡혀있다.
		RELEASED, // 놓은 순간.
		SLIDE, // 슬라이드 중.
		VACANT, // 소멸 중.
		RESPAWN, // 재생성 중.
		FALL, // 낙하 중.
		LONG_SLIDE, // 길게 슬라이드 하고 있다.
		NUM, // 상태가 몇 종류인지 나타낸다(=8).
	};


	public static int BLOCK_NUM_X = 9; // 블록을 배치할 수 있는 X 방향 최댓값.
	public static int BLOCK_NUM_Y = 9; // 블록을 배치할 수 있는 Y 방향 최댓값.
}



public class BlockControl : MonoBehaviour {

	public Block.COLOR color = (Block.COLOR)0; // 블록 색.
	public BlockRoot block_root = null; // 블록의 신.
	public Block.iPosition i_pos; // 블록 좌표.

	public Block.STEP step = Block.STEP.NONE; // 지금 상태.
	public Block.STEP next_step = Block.STEP.NONE; // 다음 상태.
	private Vector3 position_offset_initial = Vector3.zero; // 교체 전 위치.
	public Vector3 position_offset = Vector3.zero; // 교체 후 위치.


	public float vanish_timer = -1.0f; // 블록이 사라질 때까지의 시간.
	public Block.DIR4 slide_dir = Block.DIR4.NONE; // 슬라이드된 방향.
	public float step_timer = 0.0f; // 블록이 교체된 때의 이동 시간 등.

	// 10-------.
	public Material opaque_material; // 불투명용 재질.
	public Material transparent_material; // 반투명용 재질.


	private struct StepFall {
		public float velocity; // 낙하속도.
	}
	private StepFall fall;



	void Start() {
		this.setColor(this.color); // 색을 칠한다.

		this.next_step = Block.STEP.IDLE; // 다음 블록을 대기 중으로.
	}

	void Update() {
		Vector3 mouse_position; // 마우스 위치.
		this.block_root.unprojectMousePosition( // 마우스 위치 가져오기.
		                                       out mouse_position, Input.mousePosition);
		// 가져온 마우스 위치를 X와 Y만으로 한다.
		Vector2 mouse_position_xy =	new Vector2(mouse_position.x, mouse_position.y);


		if(this.vanish_timer >= 0.0f) { // 타이머가 0이상이면.
			this.vanish_timer -= Time.deltaTime; // 타이머의 값을 줄인다.
			if(this.vanish_timer < 0.0f) { // 타이머가 0미만이면.
				if(this.step != Block.STEP.SLIDE) { // 슬라이드 중이 아니므로.
					this.vanish_timer = -1.0f;
					this.next_step = Block.STEP.VACANT; // 상태를 '소멸 중'으로.
				} else {
					this.vanish_timer = 0.0f;
				}
			}
		}


		this.step_timer += Time.deltaTime;
		float slide_time = 0.2f;

		if(this.next_step == Block.STEP.NONE) { // 상태 정보가 없는 상태.
			switch(this.step) {
			case Block.STEP.SLIDE:
				if(this.step_timer >= slide_time) {
					// 슬라이드 중에 블록이 소멸이 소멸하면.
					// VACANT(사라지는)상태로 전환.
					if(this.vanish_timer == 0.0f) {
						this.next_step = Block.STEP.VACANT;
						// vanish_timer가 0이 아니면.
						// IDLE(대기)상태로 전환.
					} else {
						this.next_step = Block.STEP.IDLE;
					}
				}
				break;

			case Block.STEP.IDLE:
				this.renderer.enabled = true;
				break;
			case Block.STEP.FALL:
				if(this.position_offset.y <= 0.0f) {
					this.next_step = Block.STEP.IDLE;
					this.position_offset.y = 0.0f;
				}
				break;

			}
		}



		// '다음 블록'의 상태가 '정보 없음' 이외인 동안.
		// = '다음 블록'의 상태가 변경된 경우.
		while(this.next_step != Block.STEP.NONE) {
			this.step = this.next_step;
			this.next_step = Block.STEP.NONE;
			switch(this.step) {
			case Block.STEP.IDLE: // '대기' 상태.
				this.position_offset = Vector3.zero;
				// 블록의 표시 크기를 일반 크기로 한다.
				this.transform.localScale = Vector3.one * 1.0f;
				break;
			case Block.STEP.GRABBED: // '잡힌 상태'.
				// 블록 표시 크기를 크게 한다.
				this.transform.localScale = Vector3.one * 1.2f;
				break;
			case Block.STEP.RELEASED: // '놓은 상태'.
				this.position_offset = Vector3.zero;
				// 블록의 표시 크기를 일반 크기로 한다.
				this.transform.localScale = Vector3.one * 1.0f;
				break;

			case Block.STEP.VACANT:
				this.position_offset = Vector3.zero;
				this.setVisible(false); // 블록을 비표시로.
				break;

			case Block.STEP.RESPAWN:
				// 색을 랜덤하게 선택하여 블록을 그 색으로 설정.
				int color_index = Random.Range(
					0, (int)Block.COLOR.NORMAL_COLOR_NUM);
				this.setColor((Block.COLOR)color_index);
				this.next_step = Block.STEP.IDLE;
				break;
			case Block.STEP.FALL:
				this.setVisible(true); // 블록을 표시.
				this.fall.velocity = 0.0f; // 낙하 속도를 리셋.
				break;
			}
			this.step_timer = 0.0f;
		}


		switch(this.step) {
		case Block.STEP.GRABBED: //  '잡힌 상태'.
			// '잡힌 상태'일 때는 항상 슬라이드 방향을 체크.
			this.slide_dir = this.calcSlideDir(mouse_position_xy);
			break;
		case Block.STEP.SLIDE: // 슬라이드(교체) 중.
			// 블록을 서서히 이동하는 처리.
			// (어려우므로 지금은 몰라도 괜찮다).
			float rate = this.step_timer / slide_time;
			rate = Mathf.Min(rate, 1.0f);
			rate = Mathf.Sin(rate*Mathf.PI / 2.0f);
			this.position_offset = Vector3.Lerp(
				this.position_offset_initial, Vector3.zero, rate);
			break;
		case Block.STEP.FALL:
			// 속도에 중력의 영향을 준다.
			this.fall.velocity += Physics.gravity.y * Time.deltaTime * 0.3f;
			// 세로 방향 위치를 계산.
			this.position_offset.y += this.fall.velocity * Time.deltaTime;
			if(this.position_offset.y < 0.0f) { // 다 내려왔다면.
				this.position_offset.y = 0.0f; // 그 자리에 머문다.
			}
			break;
		}



		// 그리드 좌표를 실제 좌표(씬의 좌표)로 변환하고.
		// position_offset을 더한다.
		Vector3 position =
			BlockRoot.calcBlockPosition(this.i_pos) + this.position_offset;
		// 실제 위치를 새로운 위치로 변경.
		this.transform.position = position;


		this.setColor(this.color);
		if(this.vanish_timer >= 0.0f) {
			// 현재 레벨의 연소시간으로 설정.
			float vanish_time =
				this.block_root.level_control.getVanishTime();


			Color color0 = // 현재 색과 흰색의 중간색.
				Color.Lerp(this.renderer.material.color, Color.white, 0.5f);
			Color color1 = // 현재 색과 검은색의 중간색.
				Color.Lerp(this.renderer.material.color, Color.black, 0.5f);
			// 발화 연출 시간의 절반을 지났다면.
			if(this.vanish_timer < Block.VANISH_TIME / 2.0f) {
				// 투명도(a)를 설정.
				color0.a = this.vanish_timer / (Block.VANISH_TIME / 2.0f);
				color1.a = color0.a;
				//  반투명 머티리얼을 적용. 
				this.renderer.material = this.transparent_material;
			}
			// vanish_timer가 줄어들 수록 1에 가까워진다.
			float rate = 1.0f - this.vanish_timer / Block.VANISH_TIME;
			// 서서히 색을 바꾼다.
			this.renderer.material.color = Color.Lerp(color0, color1, rate);
		}

	}


	// 인수 color의 색으로 블록을 칠한다.
	public void setColor(Block.COLOR color)
	{
		this.color = color; // 현재 지정된 색을 멤버 변수에 보관.
		Color color_value; // Color 클래스는 색을 나타낸다.
		switch(this.color) { // 칠할 색에 따라서 분기한다.
		default:
		case Block.COLOR.PINK:
			color_value = new Color(1.0f, 0.5f, 0.5f);
			break;
		case Block.COLOR.BLUE:
			color_value = Color.blue;
			break;
		case Block.COLOR.YELLOW:
			color_value = Color.yellow;
			break;
		case Block.COLOR.GREEN:
			color_value = Color.green;
			break;
		case Block.COLOR.MAGENTA:
			color_value = Color.magenta;
			break;
		case Block.COLOR.ORANGE:
			color_value = new Color(1.0f, 0.46f, 0.0f);
			break;
		}
		// 이 GameObject의 머티리얼 색상을 변경.
		this.renderer.material.color = color_value;
	}


	public void beginGrab()
	{
		this.next_step = Block.STEP.GRABBED;
	}

	public void endGrab()
	{
		this.next_step = Block.STEP.IDLE;
	}

	public bool isGrabbable()
	{
		bool is_grabbable = false;
		switch(this.step) {
		case Block.STEP.IDLE: // 「대기」상태일 때만.
			is_grabbable = true; // true（잡을 수 있다）를 반환한다.
			break;
		}
		return(is_grabbable);
	}

	public bool isContainedPosition(Vector2 position)
	{
		bool ret = false;
		Vector3 center = this.transform.position;
		float h = Block.COLLISION_SIZE / 2.0f;
		do {
			// X좌표가 자신에게 겹쳐있지 않다면 break로 루프를 빠져나온다.
			if(position.x < center.x - h || center.x + h < position.x) {
				break;
			}
			// Y좌표가 자신에게 겹쳐있지 않다면 break로 루프를 빠져나온다.
			if(position.y < center.y - h || center.y + h < position.y) {
				break;
			}
			// X좌표, Y좌표 양쪽이 겹쳐있다면 true를 반환한다.
			ret = true;
		} while(false);
		return(ret);
	}


	public Block.DIR4 calcSlideDir(Vector2 mouse_position)
	{
		Block.DIR4 dir = Block.DIR4.NONE;
		// 지정된 mouse_position과 현재 위치의 차를 나타내는 벡터.
		Vector2 v = mouse_position -
			new Vector2(this.transform.position.x, this.transform.position.y);
		// 벡터의 크기가 0.1보다 크면.
		// (그보다 작으면 슬라이드하지 않은 걸로 간주한다).
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

	public float calcDirOffset(Vector2 position, Block.DIR4 dir)
	{
		float offset = 0.0f;
		// 지정된 위치와 블록의 현재 위치의 차이를 나타내는 벡터.
		Vector2 v = position - new Vector2(
			this.transform.position.x, this.transform.position.y);
		switch(dir) { // 지정된 방향에 따라 분기.
		case Block.DIR4.RIGHT: offset = v.x;
			break;
		case Block.DIR4.LEFT: offset = -v.x;
			break;
		case Block.DIR4.UP: offset = v.y;
			break;
		case Block.DIR4.DOWN: offset = -v.y;
			break;
		}
		return(offset);
	}

	public void beginSlide(Vector3 offset)
	{
		this.position_offset_initial= offset;
		this.position_offset =
			this.position_offset_initial;
		// 상태를 SLIDE로 변경.
		this.next_step = Block.STEP.SLIDE;
	}


	public void toVanishing()
	{
		// 사라질 때까지 걸리는 시간을 규정치로 리셋.
		// this.vanish_timer = Block.VANISH_TIME;
		// 현재 레벨의 연소시간으로 설정.
		float vanish_time = this.block_root.level_control.getVanishTime();
		this.vanish_timer = vanish_time;
	}

	public bool isVanishing()
	{
		// vanish_timer가 0보다 크면 true.
		bool is_vanishing = (this.vanish_timer > 0.0f);
		return(is_vanishing);
	}

	public void rewindVanishTimer()
	{
		// 사라질 때까지 걸리는 시간을 규정치로 리셋.
		// this.vanish_timer = Block.VANISH_TIME;
		// 현재 레벨의 연소시간으로 설정.
		float vanish_time = this.block_root.level_control.getVanishTime();
		this.vanish_timer = vanish_time;
	}

	public bool isVisible()
	{
		// 그리기 가능(renderer.enabled가 true)이라면.
		// 표시되고 있다. 
		bool is_visible = this.renderer.enabled;
		return(is_visible);
	}

	public void setVisible(bool is_visible)
	{
		// 그리기 가능 설정에 인수를 대입한다.
		this.renderer.enabled = is_visible;
	}

	public bool isIdle()
	{
		bool is_idle = false;
		// 현재 블록 상태가 '대기 중'이고.
		// 다음 블록 상태가 '없음'이면.
		if(this.step == Block.STEP.IDLE &&
		   this.next_step == Block.STEP.NONE) {
			is_idle = true;
		}
		return(is_idle);
	}


	public void beginFall(BlockControl start)
	{
		this.next_step = Block.STEP.FALL;
		// 지정된 블록에서 좌표를 계산해 낸다.
		this.position_offset.y =
			(float)(start.i_pos.y - this.i_pos.y) * Block.COLLISION_SIZE;
	}

	public void beginRespawn(int start_ipos_y)
	{
		// 지정 위치까지 y좌표를 이동.
		this.position_offset.y =
			(float)(start_ipos_y - this.i_pos.y) *
				Block.COLLISION_SIZE;
		this.next_step = Block.STEP.FALL;


		// int color_index = Random.Range(
		// (int)Block.COLOR.FIRST, (int)Block.COLOR.LAST + 1);
		// this.setColor((Block.COLOR)color_index);
		// 현재 레벨의 출현 확률을 바탕으로 블록의 색을 결정한다.
		Block.COLOR color = this.block_root.selectBlockColor();
		this.setColor(color);

	}

	public bool isVacant()
	{
		bool is_vacant = false;
		if(this.step == Block.STEP.VACANT && this.next_step == Block.STEP.NONE) {
			is_vacant = true;
		}
		return(is_vacant);
	}

	public bool isSliding()
	{
		bool is_sliding = (this.position_offset.x != 0.0f);
		return(is_sliding);
	}

}

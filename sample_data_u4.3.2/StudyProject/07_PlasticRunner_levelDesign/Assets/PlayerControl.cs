using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {

	public static float ACCELERATION = 10.0f; // 가속도.
	public static float SPEED_MIN = 4.0f; // 최소 속도.
	public static float SPEED_MAX = 8.0f; // 최대 속도.
	public static float JUMP_HEIGHT_MAX = 3.0f; // 점프 높이.
	public static float JUMP_KEY_RELEASE_REDUCE = 0.5f; // 점프 후의 감속도.

	public enum STEP { // Player의 각종 상태를 나타내는 자료형.
		NONE = -1, // 상태정보 없음.
		RUN = 0, // 달린다.
		JUMP, // 점프.
		MISS, // 실패.
		NUM, // 상태가 몇 종류인지 나타낸다(=3).
	};

	public STEP step = STEP.NONE; // Player의 현재 상태. 
	public STEP next_step = STEP.NONE; // Player의 다음 상태.
	public float step_timer = 0.0f; // 경과 시간.
	private bool is_landed = false; // 착지했는가.
	private bool is_colided = false; // 충돌했는가.
	private bool is_key_released = false; // 버튼을 놓았는가.


	public static float NARAKU_HEIGHT = -5.0f;


	public float current_speed = 0.0f; // 현재 속도.
	public LevelControl level_control = null; // LevelControl을 가짐.

	private float click_timer = -3.0f; // 버튼이 눌린 후의 시간.
	private float CLICK_GRACE_TIME = 0.5f; // 점프하고 싶다는 의지'를 받아들이는 시간.

	void Start() {
		this.next_step = STEP.RUN;
	}


	private void check_landed()
	{
		this.is_landed = false; // 우선 false로 해 둔다.
		do {
			Vector3 s = this.transform.position; // Player 현재 위치.
			Vector3 e = s + Vector3.down * 1.0f; // s에서 아래로 1.0f 이동한 위치.
			RaycastHit hit;
			if(! Physics.Linecast(s, e, out hit)) { // s~e 사이에 아무것도 없을 때.
				break; // 아무것도 하지 않고 do~while 루프를 빠져나간다(탈출구로).
			}
			// s~e 사이에 뭔가 있을 때는 다음 처리를 한다.
			if(this.step == STEP.JUMP) { // 현재 점프 상태라면.
				// 경과 시간이 3.0f미만이면.
				if(this.step_timer < Time.deltaTime * 3.0f) {
					break; // 아무것도 하지 않고 do~while 루프를 빠져나온다(탈출구로).
				}
			}
			// s~e 사이에 뭔가 있고, JUMP 직후가 아닐 때만 다음 처리를 실행한다.
			this.is_landed = true;
		} while(false);
		// 루프 탈출구.
	}


	void Update() {
		Vector3 velocity = this.rigidbody.velocity; // 속도를 설정.

		this.current_speed = this.level_control.getPlayerSpeed();

		this.check_landed(); // 착지 상태인지 검사.


		switch(this.step) {
		case STEP.RUN:
		case STEP.JUMP:
			// 현재 위치가 문턱값보다 아래라면.
			if(this.transform.position.y < NARAKU_HEIGHT) {
				this.next_step = STEP.MISS; // '실패' 상태로 한다.
			}
			break;
		}


		this.step_timer += Time.deltaTime; // 경과 시간을 진행한다.


		if(Input.GetMouseButtonDown(0)) { // 버튼이 눌렸다면.
			this.click_timer = 0.0f; // 타이머를 리셋.
		} else {
			if(this.click_timer >= 0.0f) { // 그렇지 않다면.
				this.click_timer += Time.deltaTime; // 경과 시간을 더한다.
			}
		}


		// '다음 상태'가 정해져 있지 않다면 상태 변화를 조사한다.
		if(this.next_step == STEP.NONE) {
			switch(this.step) { // Player의 현재 상태로 분기.
			case STEP.RUN: // 달리는 중.
				/*
				if(! this.is_landed) {
					// 달리고 있고 착지 하지 않은 경우 아무것도 하지 않는다.
				} else {
					if(Input.GetMouseButtonDown(0)) {
						// 달리고 있고, 착지했고, 왼쪽 버튼이 눌렸다면.
						// 다음 상태를 점프로 변경.
						this.next_step = STEP.JUMP;
					}
				}
				*/
				// click_timer가 0이상, CLICK_GRACE_TIME이하라면.
				if(0.0f <= this.click_timer && this.click_timer <= CLICK_GRACE_TIME){
					if(this.is_landed){ // 착지했다면.
						this.click_timer = -1.0f; // 버튼이 눌려있지 않음을 나타내는 -1.0f로.
						this.next_step = STEP.JUMP; // 점프 상태로 한다.
					}
				}


				break;

			case STEP.JUMP: // 점프 중.
				if(this.is_landed) {
					// 점프 중이고 착지했다면.
					// 다음 상태를 달리는 중으로 변경.
					this.next_step = STEP.RUN;
				}
				break;
			}
		}
		//

		// '다음 상태'가 '상태 정보 없음' 이외일 때.
		while(this.next_step != STEP.NONE) {
			this.step = this.next_step; // '현재 상태'를 '다음 상태'로 갱신.
			this.next_step = STEP.NONE; // '다음 상태'를 '상태 없음'으로 변경.
			switch(this.step) { // 갱신된 '현재 상태'가.
			case STEP.JUMP: // '점프'라면.
				// 점프 높이로 점프의 초속을.
				velocity.y = Mathf.Sqrt(
					2.0f * 9.8f * PlayerControl.JUMP_HEIGHT_MAX);
				// '버튼을 놓았음을 나타내는 플래그'를 클리어 한다.
				this.is_key_released = false;
				break;
			}
			// 상태가 변화했으므로 경과 시간을 제로로 리셋.
			this.step_timer = 0.0f;
		}


		// 상태별로 매 프레임의 갱신처리.
		switch(this.step) {
		case STEP.RUN: // 달리는 중.
			// 속도를 높인다.
			velocity.x += PlayerControl.ACCELERATION * Time.deltaTime;

			// 속도가 최고 속도 제한을 넘으면.
			// if(Mathf.Abs(velocity.x) > PlayerControl.SPEED_MAX) {
				// 최고 속도 제한 아래로 유지한다.
			//	velocity.x *= PlayerControl.SPEED_MAX /
			//		Mathf.Abs(this.rigidbody.velocity.x);
			// }

			// 계산으로 구한 속도가 설정해야 할 속도를 넘으면.
			if(Mathf.Abs(velocity.x) > this.current_speed) {
				// 넘지 않게 조정한다.
				velocity.x *= this.current_speed / Mathf.Abs(velocity.x);
			}
			break;


			break;

		case STEP.JUMP: // 점프 중.
			do {
				// '버튼이 떨어진 순간'이 아니면.
				if(! Input.GetMouseButtonUp(0)) {
					break; // 아무 것도 하지 않고 루프를 빠져나간다.
				}
				// 감속되었다면(두 번이상 감속하지 않도록).
				if(this.is_key_released) {
					break; // 아무 것도 하지 않고 루프를 빠져나간다.
				}

				// 상하방향 속도가 0이하라면(하강 중이라면).
				if(velocity.y <= 0.0f) {
					break; // 아무 것도 하지 않고 루프를 빠져나간다.
				}
				// 버튼이 떨어졌고, 상승 중이면 감속 시작.
				// 점프의 상승 처리는 여기서 끝.
				velocity.y *= JUMP_KEY_RELEASE_REDUCE;
				this.is_key_released = true;
			} while(false);
			break;
		
		
		case STEP.MISS:
			// 가속 값(ACCELERATION)을 빼서 Player의 속도를 느리게 해 간다.
			velocity.x -= PlayerControl.ACCELERATION * Time.deltaTime;
			if(velocity.x < 0.0f) { // Player의 속도가 마이너스라면.
				velocity.x = 0.0f; // 제로로 한다.
			}
			break;
		
		}

		// Rigidbody의 속도를 위에서 구한 속도로 갱신.
		// (이 행은 상태에 무관하게 매번 실행된다).
		this.rigidbody.velocity = velocity;
	}


	public bool isPlayEnd() // 게임이 끝났는지 판정한다.
	{
		bool ret = false;
		switch(this.step) {
		case STEP.MISS: // MISS(실패) 상태라면.
			ret = true; // '죽었어요'(true)라고 알려준다.
			break;
		}
		return(ret);
	}




}

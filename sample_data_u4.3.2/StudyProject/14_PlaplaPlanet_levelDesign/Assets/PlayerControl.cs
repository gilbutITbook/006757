using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {
	public static float MOVE_AREA_RADIUS = 15.0f; // 섬의 반지름.
	public static float MOVE_SPEED = 7.0f; // 이동속도. // 5.0f→7.0f.

	private struct Key { // 키 조작 정보 구조체.
		public bool up; // ↑.
		public bool down; // ↓.
		public bool right; // →.
		public bool left; // ←.
		public bool pick; // 줍는다 / 버린다.
		public bool action; // 버린다・수리한다.
	};
	private Key key; // 키 조작 정보를 가지는 변수 .
	public enum STEP { // 플레이어의 상태를 나타내는 열거체.
		NONE = -1, // 상태정보없음.
		MOVE = 0, // 이동중.
		REPAIRING, // 수리중.
		EATING, // 식사중.
		NUM, // 상태가 몇 종류인가(=3).
	};
	public STEP step = STEP.NONE; // 현재 상태.
	public STEP next_step = STEP.NONE; // 다음 상태.
	public float step_timer = 0.0f; // 타이머.


	private GameObject closest_item = null; // 플레이어 정면에 있는 GameObject.
	private GameObject carried_item = null; // 플레이어가 들고 있는 GameObject.
	private ItemRoot item_root = null; // ItemRoot 스크립트를 가짐.
	public GUIStyle guistyle; // 폰트 스타일.

	private GameObject closest_event = null; // 주목하고 있는 이벤트를 저장.
	private EventRoot event_root = null; // EventRoot 클래스를 사용하기 위한 변수.
	private GameObject rocket_model = null; // 우주선 모델을 사용하기 위한 변수.

	private GameStatus game_status = null;


	void Start() {
		this.step = STEP.NONE; // 현 단계의 상태를 초기화.
		this.next_step = STEP.MOVE; // 다음 단계의 상태를 초기화.

		this.item_root =
			GameObject.Find("GameRoot").GetComponent<ItemRoot>();
		this.guistyle.fontSize = 16;

		this.event_root =
			GameObject.Find("GameRoot").GetComponent<EventRoot>();
		this.rocket_model = GameObject.Find("rocket").transform.FindChild(
			"rocket_model").gameObject;

		this.game_status =
			GameObject.Find("GameRoot").GetComponent<GameStatus>();
	}


	void Update() {
		this.get_input(); // 입력 정보를 가져온다.

		this.step_timer += Time.deltaTime;


		float eat_time = 0.5f; // 사과는 2초 걸려서 먹는다. // 2.0f→0.5f.

		float repair_time = 0.5f; // 수리에 걸리는 시간도 2초.// 2.0f→0.5f.


		// 상태를 변화시킨다 --------------------.
		if(this.next_step == STEP.NONE) { // 다음 예정이 없으면.
			switch(this.step) {
			case STEP.MOVE: // '이동중'상태의 처리.
				do {
					if(! this.key.action) { // 액션키가 눌려있지 않으면.
						break; // 루프 탈출.
					}

					// 주목 중인 이벤트가 있을 때.
					if(this.closest_event != null) {
						if(! this.is_event_ignitable()) { // 이벤트가 시작 불가면.
							break; // 아무것도 하지 않는다.
						}
						// 이벤트 종류 취득.
						Event.TYPE ignitable_event =
							this.event_root.getEventType(this.closest_event);
						switch(ignitable_event) {
						case Event.TYPE.ROCKET:
							// 이벤트 종류가 ROCKET이면.
							// REPAIRING(수리) 상태로 이행.
							this.next_step = STEP.REPAIRING;
							break;
						}
						break;
					}


					if(this.carried_item != null) {
						// 들고 있는 아이템 판별.
						Item.TYPE carried_item_type =
							this.item_root.getItemType(this.carried_item);
						switch(carried_item_type) {
						case Item.TYPE.APPLE: // 사과.
						case Item.TYPE.PLANT: // 식물.
							// '식사중'상태로 이행.
							this.next_step = STEP.EATING;
							break;
						}
					}
				} while(false);
				break;
			case STEP.EATING: // '식사중' 상태의 처리.
				if(this.step_timer > eat_time) { // 2초 대기.
					this.next_step = STEP.MOVE; // '이동' 상태로 이행.
				}
				break;

			case STEP.REPAIRING: // '수리중' 상태의 처리.
				if(this.step_timer > repair_time) { // 2초 대기.
					this.next_step = STEP.MOVE; //  '이동' 상태로 이행.
				}
				break;

			}
		}

		// 상태가 변화한 경우------------.
		while(this.next_step != STEP.NONE) { // 상태가 NONE이외 = 상태가 변화했다.
			this.step = this.next_step;
			this.next_step = STEP.NONE;
			switch(this.step) {
			case STEP.MOVE:
				break;
			case STEP.EATING: // '식사 중' 상태의 처리.
				if(this.carried_item != null) {
					// 들고 있는 아이템의 '체력 회복 정도'를 가져와서 설정.
					this.game_status.addSatiety(
						this.item_root.getRegainSatiety(this.carried_item));

					// 들고 있는 아이템은 폐기..
					GameObject.Destroy(this.carried_item);
					this.carried_item = null;
				}
				break;
			case STEP.REPAIRING: // '수리 중'이 되었다면.
				if(this.carried_item != null) {
					// 들고 있는 아이템의 '수리 진척 상태'를 가져와서 설정.
					this.game_status.addRepairment(
						this.item_root.getGainRepairment(this.carried_item));

					// 들고 있는 아이템을 삭제.
					GameObject.Destroy(this.carried_item);
					this.carried_item = null;
					this.closest_item = null;
				}
				break;
			}
			this.step_timer = 0.0f;
		}


		// 각 상태에서 반복할 것----------.
		switch(this.step) {
		case STEP.MOVE:
			this.move_control();
			this.pick_or_drop_control();

			// 이동 가능한 경우는 항상 배가 고파진다.
			this.game_status.alwaysSatiety();

			break;
		case STEP.REPAIRING:
			// 우주선을 회전시킨다.
			this.rocket_model.transform.localRotation *=
				Quaternion.AngleAxis(
					360.0f / 10.0f * Time.deltaTime, Vector3.up);
			break;
		}
	}


	private void get_input()
	{
		this.key.up = false;
		this.key.down = false;
		this.key.right = false;
		this.key.left = false;
		// ↑키가 눌렸으면 true를 대입.
		this.key.up |= Input.GetKey(KeyCode.UpArrow);
		this.key.up |= Input.GetKey(KeyCode.Keypad8);
		// ↓키가 눌렸으면 true를 대입.
		this.key.down |= Input.GetKey(KeyCode.DownArrow);
		this.key.down |= Input.GetKey(KeyCode.Keypad2);
		// →키가 눌렸으면 true를 대입.
		this.key.right |= Input.GetKey(KeyCode.RightArrow);
		this.key.right |= Input.GetKey(KeyCode.Keypad6);
		// ←키가 눌렸으면 true를 대입.
		this.key.left |= Input.GetKey(KeyCode.LeftArrow);
		this.key.left |= Input.GetKey(KeyCode.Keypad4);
		// Z키가 눌렸으면 true를 대입.
		this.key.pick = Input.GetKeyDown(KeyCode.Z);
		// X키가 눌렸으면 true를 대입.
		this.key.action = Input.GetKeyDown(KeyCode.X);
	}

	private void move_control()
	{
		Vector3 move_vector = Vector3.zero; // 이동용 벡터.
		Vector3 position = this.transform.position; // 현재 위치를 보관.
		bool is_moved = false;
		if(this.key.right) { // →키가 눌렸으면.
			move_vector += Vector3.right; // 이동용 벡터를 오른쪽으로 향하게 한다.
			is_moved = true; // '이동중' 플래그를 든다.
		}
		if(this.key.left) {
			move_vector += Vector3.left;
			is_moved = true;
		}
		if(this.key.up) {
			move_vector += Vector3.forward;
			is_moved = true;
		}
		if(this.key.down) {
			move_vector += Vector3.back;
			is_moved = true;
		}
		move_vector.Normalize(); // 길이를 1로.
		move_vector *= MOVE_SPEED * Time.deltaTime; // 속도×시간＝거리.
		position += move_vector; // 위치를 이동.
		position.y = 0.0f; // 높이를 0으로 한다.
		// 세계의 중앙으로부터 갱신한 위치까지의 거리가 섬의 반지름보다 커진 경우.
		if(position.magnitude > MOVE_AREA_RADIUS) {
			position.Normalize();
			position *= MOVE_AREA_RADIUS; // 위치를 섬의 끝에서 멈춘다.
		}
		// 새로 구한 위치(position)의 높이를 현재 높이로 되돌린다.
		position.y = this.transform.position.y;
		// 실제 위치를 새로 구한 위치로 변경한다. 
		this.transform.position = position;
		// 이동 벡터 길이가 0.01보다 큰 경우.
		// =어느 정도 이상 이동한 경우.
		if(move_vector.magnitude > 0.01f) {
			// 캐릭터의 방향을 서서히 바꾼다.
			Quaternion q = Quaternion.LookRotation(move_vector, Vector3.up);
			this.transform.rotation =
				Quaternion.Lerp(this.transform.rotation, q, 0.2f);
			// Lerp의 비율을 0.1f에서 0.2f으로↑↑↑.
		}

		if(is_moved) {
			// 들고 있는 아이템에 따라 '체력 소모 정도'를 조사한다.
			float consume = this.item_root.getConsumeSatiety(this.carried_item);
			// 가져온 '소모 정도'를 체력에서 뺀다.
			this.game_status.addSatiety(-consume * Time.deltaTime);
		}

	}


	void OnTriggerStay(Collider other)
	{
		GameObject other_go = other.gameObject;


		// 트리거인 GameObject의 플레이어 설정이 Item이라면.
		if(other_go.layer == LayerMask.NameToLayer("Item")) {
			// 아무것도 주목하고 있지 않으면.
			if(this.closest_item == null) {
				if(this.is_other_in_view(other_go)) { // 정면에 있으면.
					this.closest_item = other_go; // 주목한다.
				}
				// 뭔가를 주목하고 있다면.
			} else if(this.closest_item == other_go) {
				if(! this.is_other_in_view(other_go)) { // 정면에 없으면.
					this.closest_item = null; // 주목을 멈춘다.
				}
			}

		// 트리거인 GameObject의 플레이어 설정이 Event라면.
		} else if(other_go.layer == LayerMask.NameToLayer("Event")) {

			// 아무것도 주목하고 있지 않으면.
			if(this.closest_event == null) {
				if(this.is_other_in_view(other_go)) { // 정면에 있으면.
					this.closest_event = other_go; // 주목한다.
				}
				// 뭔가를 주목하고 있다면.
			} else if(this.closest_event == other_go) {
				if(!this.is_other_in_view(other_go)) { // 정면에 없으면.
					this.closest_event = null; // 주목을 멈춘다.
				}
			}
		}
	}

	void OnTriggerExit(Collider other)
	{
		if(this.closest_item == other.gameObject) {
			this.closest_item = null; // 주목을 멈춘다.
		}
	}

	/*
	void OnGUI()
	{
		float x = 20.0f;
		float y = Screen.height - 40.0f;
		// 들고 있는 아이템이 있다면.
		if(this.carried_item != null) {
			GUI.Label(new Rect(x, y, 200.0f, 20.0f), "Z:버린다", guistyle);
			GUI.Label(new Rect(x+100.0f, y, 200.0f, 20.0f),
			          "X:먹는다", guistyle);
		} else {
			// 주목하고 있는 아이템이 있다면.
			if(this.closest_item != null) {
				GUI.Label(new Rect(x, y, 200.0f, 20.0f), "Z:줍는다", guistyle);
			}
		}

		switch(this.step) {
		case STEP.EATING:
			GUI.Label(new Rect(x, y, 200.0f, 20.0f),
			          "우걱우걱우물우물……", guistyle);
			break;
		}
	}
	*/

	void OnGUI() {
		float x = 20.0f;
		float y = Screen.height - 40.0f;
		if(this.carried_item != null) {
			GUI.Label(new Rect(x, y, 200.0f, 20.0f), "Z:버린다", guistyle);
			do {
				if(this.is_event_ignitable()) {
					break;
				}
				if(item_root.getItemType(this.carried_item) == Item.TYPE.IRON) {
					break;
				}
				GUI.Label(new Rect(x+100.0f, y, 200.0f, 20.0f),
				          "x:먹는다", guistyle);
			}while(false);
		} else {
			if(this.closest_item != null) {
				GUI.Label(new Rect(x, y, 200.0f, 20.0f), "Z:줍는다", guistyle);
			}
		}
		switch(this.step) {
		case STEP.EATING:
			GUI.Label(new Rect(x, y, 200.0f, 20.0f),
			          "우걱우걱우물우물……", guistyle);
			break;
		case STEP.REPAIRING:
			GUI.Label(new Rect(x+200.0f, y, 200.0f, 20.0f),
			          "수리 중", guistyle);
			break;
		}
		if(this.is_event_ignitable()) { // 이벤트가 시작 가능하면.
			// 이벤트용 메시지를 획득.
			string message =
				this.event_root.getIgnitableMessage(this.closest_event);
			GUI.Label(new Rect(x+200.0f, y, 200.0f, 20.0f),
			          "X:" + message, guistyle);
		}
	}



	private void pick_or_drop_control()
	{
		do {
			if(! this.key.pick) { // '줍는다／버린다'키가 눌려있지 않다면.
				break; // 아무것도 하지 않고 메소드를 종료한다.
			}
			if(this.carried_item == null) { // 들고 있는 아이템이 없고.
				if(this.closest_item == null) { // 주목 중인 아이템이 없다면.
					break; // 아무것도 하지 않고 메소드를 종료.
				}
				// 주목 중인 아이템을 집어든다.
				this.carried_item = this.closest_item;
				// 들고 있는 아이템을 자신의 자식으로 설정.
				this.carried_item.transform.parent = this.transform;
				// 2.0f 위에 배치(머리 위로 이동).
				this.carried_item.transform.localPosition = Vector3.up * 2.0f;
				// 주목 중 아이템을 없앤다 .
				this.closest_item = null;
			} else { // 들고 있는 아이템이 있을 경우.
				// 들고 있는 아이템을 조금(1.0f) 앞으로 이동시키고.
				this.carried_item.transform.localPosition =
					Vector3.forward * 1.0f;
				this.carried_item.transform.parent = null; // 자식 설정을 해제.
				this.carried_item = null; // 들고 있는 아이템을 없앤다.
			}
		} while(false);
	}

	private bool is_other_in_view(GameObject other)
	{
		bool ret = false;
		do {
			Vector3 heading = // 자신이 현재 향한 방향을 보관.
				this.transform.TransformDirection(Vector3.forward);
			Vector3 to_other = // 자신이 본 아이템의 방향을 보관.
				other.transform.position - this.transform.position;
			heading.y = 0.0f;
			to_other.y = 0.0f;
			heading.Normalize(); // 길이를 1로 하고 방향만 있는 벡터로.
			to_other.Normalize(); // 길이를 1로 하고 방향만 있는 벡터로.
			float dp = Vector3.Dot(heading, to_other); // 두 벡터의 내적을 구한다.
			if(dp < Mathf.Cos(45.0f)) { // 내적이 45도인 코사인값 미만이면.
				break; // 루프를 빠져나간다.
			}
			ret = true; // 내적이 45도인 코사인이상이면 정면에 있다.
		} while(false);
		return(ret);
	}

	private bool is_event_ignitable()
	{
		bool ret = false;
		do {
			if(this.closest_event == null) { // 주목 이벤트가 없으면.
				break; // false 반환한다.
			}
			// 들고 있는 아이템의 종류를 가져온다.
			Item.TYPE carried_item_type =
				this.item_root.getItemType(this.carried_item);
			// 들고 있는 아이템의 종류와 주목하는 이벤트의 종류에서.
			// 이벤트 가능한지 판정하고 이벤트 불가능이면 false를 반환한다.
			if(! this.event_root.isEventIgnitable(
				carried_item_type, this.closest_event)) {
				break;
			}
			ret = true; // 여기까지 왔다면 이벤트를 시작할 수 있는 걸로 판정!.
		} while(false);
		return(ret);
	}






}

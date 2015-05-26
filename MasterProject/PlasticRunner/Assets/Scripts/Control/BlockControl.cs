using UnityEngine;
using System.Collections;

// 바닥 블록.
public class BlockControl : MonoBehaviour {

	public MapCreator		map_creator = null;			// 맵 크리에이터.

	private	GameObject		model = null;				// 표시용 모델들.
	private bool			trigger_stepped = false;	// 밟힌 순간?.

	// 밟혔을 때의 효과.
	private struct Spring {

		public	float	velocity;
		public	float	position;
	};
	private	Spring	spring;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
		this.model = this.transform.FindChild("model").gameObject;

		this.spring.velocity = 0.0f;
		this.spring.position = 0.0f;
	}
	
	void	Update()
	{
		// ---------------------------------------------------------------- //
		// 밟혔을 때의 효과.

		// 밟힌 순간에 내려간다(속도를 내린다).
		if(this.trigger_stepped) {

			this.spring.velocity -= 2.0f;

			this.trigger_stepped = false;
		}

		// 속도를 서서히 올린다.
		if(this.spring.velocity < 1.0f) {

			this.spring.velocity += 6.0f*Time.deltaTime;
		}

		this.spring.position += this.spring.velocity*Time.deltaTime;

		// 최초의 위치까지 되돌아왔다.
		if(this.spring.position >= 0.0f) {

			this.spring.position = 0.0f;
			this.spring.velocity = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 화면 왼쪽 끝에서 벗어나면 지운다.

		if(this.map_creator.isDelete(this.gameObject)) {

			GameObject.Destroy(this.gameObject);
		}
	}

	void	LateUpdate()
	{
		// 애니메이션에 덮어쓰기 되지 않도록 Update()가 아니라
		// LateUpdate()로 설정한다 
		//
		this.model.transform.localPosition += Vector3.up*this.spring.position;
	}

	// ================================================================ //

	// 밟혔을 때 호출되는 메소드.
	public void		onStepped()
	{
		this.trigger_stepped = true;
	}
}

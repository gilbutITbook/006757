using UnityEngine;
using System.Collections;

public class blockEffect : MonoBehaviour {

	private Vector3 base_position;
	private Vector3 effect_position;
	private float step_timer;

	void Start () {
		this.base_position = this.transform.position;
		effect_position = this.base_position;
		effect_position.y += Random.Range(1.0f, 3.0f);
		this.transform.position = effect_position;
	}
	
	void Update () {
		this.step_timer += Time.deltaTime;
		float pos = Mathf.Lerp(this.effect_position.y, this.base_position.y, this.step_timer *2.0f);
		Vector3 vec = this.transform.position;
		vec.y = pos;
		this.transform.position = vec;
	}
}

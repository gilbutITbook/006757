using UnityEngine;
using System.Collections;

public class studyScript : MonoBehaviour {


	void Start(){

		// 변수-----------------.
		int hako = 10;


		// 조건분기---------------.
		if(hako ==10){
			Debug.Log( hako);
		}

		if(hako != 9){
			Debug.Log( hako);
		}

		if(hako > 10){
			Debug.Log( "10보다 크다");
		}else if(hako > 5){
			Debug.Log( "5보다 크다");
		}else{
			Debug.Log( "5이하");
		}

		switch(hako){
		case 10:
			Debug.Log( "10이네요");
			break;
		case 5:
			Debug.Log( "5네요");
			break;
		default:
			Debug.Log( "10과 5가 아니네요");
			break;
		}


		if(hako ==10){
			int memo1 = 30;		// 로컬 변수-------.
			Debug.Log( memo1);
		}
		// Debug.Log(memo1);	// 이 주석처리를 해제하면 오류가 난다.



		// 배열----------.
		int[] tana = {123, 234, 345, 456, 567};


		// 반복문-----------.
		for(int i=0; i<tana.Length; i++){
			Debug.Log( tana[i]);
		}

		// 반복문-----------.
		foreach(int i in tana){
			Debug.Log(i);
		}

		hissatuwaza();			// 메소드 호출--------.

		int dmg = damage();		// 메소드에서 돌아온 값을 받는다---------.
		Debug.Log(dmg);

		kaifuku (200);			// 메소드에 값을 전달한다-----------.
	}


	void Update(){
		if(Input.GetKey(KeyCode.Space)){
			Debug.Log( "space");
		}
		if(Input.GetKeyDown(KeyCode.A)){
			Debug.Log( "A");
		}
		if(Input.GetKeyUp(KeyCode.Z)){
			Debug.Log( "Z");
		}

		if(Input.GetMouseButtonDown(0)){
			Debug.Log(Input.mousePosition);
		}
	}


	// 실행할 메소드-----------.
	void hissatuwaza(){
		Debug.Log("몸통박치기");
	}


	// 값을 반환하는 메소드-------------.
	int damage(){
		Debug.Log("대미지");
		int ret = 100;
		return(ret);
	}

	// 값을 받아들이는 메소드-------------.
	void kaifuku(int power){
		Debug.Log("회복" +power);
	}
}

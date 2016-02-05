using UnityEngine;
using System.Collections;

// 점수 표시.
// 게임 중, 결과 표시에서 공통으로 사용하고 싶어서
// 스크립트도 프리팹도 GameRoot에서 독립했다.
public class ScoreDisp : MonoBehaviour {

	public	Texture[]	number_textures;

	// ================================================================ //

//	private static float	TEXTURE_WIDTH 	= 64;		// 텍스처 폭.
//	private static float	TEXTURE_HEIGHT	= 64;		// 텍스처 높이.

//	private static int		MAX_DIGITS		= 3;		// 최대로 표시할 자리수.


	// ================================================================ //


	// MonoBehaviour으로부터 상속.
	void	Start()
	{
	}
	
	void	Update()
	{
	}
	
	// ================================================================ //

	// 숫자를 표시한다.
	// 점수용 폰트를 사용해서 숫자를 표시한다.
	public void	dispNumber(Vector2 pos, int number, float font_size, int figre)
	{
		int		i;
		// 현재 점수가 몇 자린지 조사한다.
		int		digits;
		if(number <= 0) {
			// ０이하일 때는 Log10을 바르게 계산할 수 없으므로.
			number = 0;
			digits = 1;
		} else {
			digits = (int)Mathf.Log10(number) + 1;
		}

		// 표시 위치를 구한다.
		Vector2		p = pos;
		float 	space = font_size *0.7f;


		// p.x += (MAX_DIGITS - 1)* space;
		 p.x += (figre - 1)* space;

		int		n = number;
		// n을10으로 나누어감으로써 n의１의 자리가
		//
		// 점수의１의 자리
		// 점수의 10의 자리
		// 점수의100의 자리
		//		:
		//
		// 로 변해간다.

		for(i = 0;i < digits;i++) {
			// 이번에 표시할 자리의 숫자(0~9).
			int		digit = n%10;
			Texture		texture = this.number_textures[digit];
			GUI.DrawTexture(new Rect(p.x, p.y, font_size, font_size), texture);
			p.x -= space;
			n /= 10;
		}
	}





}

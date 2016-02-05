using UnityEngine;
using System.Collections;

// 점수 표시.
// 게임 중 결과에서 공통으로 사용하고 싶으므로 
// 스크립트도 프리팹도 GameRoot로부터 독립했다.
public class ScoreDisp : MonoBehaviour {

	public	Texture[]	number_textures;
	// ================================================================ //
	private static float	TEXTURE_WIDTH 	= 64;		// 텍스처의 폭.
	private static float	TEXTURE_HEIGHT	= 64;		// 텍스처의 높이.
	private static int		MAX_DIGITS		= 3;		// 최대로 표시할 자리수.
	// ================================================================ //

	void	Start()
	{
	}
	
	void	Update()
	{
	}
	
	// ================================================================ //
	// 숫자를 표시한다.
	// 점수용 폰트를 사용해 숫자를 표시한다.
	// dispNumber(위치, 값,폰트크기,자리수）;
	public void	dispNumber(Vector2 pos, int number, float font_size, int figre)
	{
		int		i;
		// 현재 스코어가 몇 자리인지 조사한다.
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

		 p.x += (figre - 1)* space;

		int		n = number;
		// n을 10으로 나누어가면 n의 1 자리가
		// 스코어의 1자리
		// 스코어의 10자리
		// 스코어의 100자리
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

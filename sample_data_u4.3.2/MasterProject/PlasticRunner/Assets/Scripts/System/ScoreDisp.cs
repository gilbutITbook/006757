using UnityEngine;
using System.Collections;

// 점수 표시.
// 게임 중 결과로 공통으로 사용하고 싶으므로
// 스크립트도 프리팹도 GameRoot로부터 독립했다.
public class ScoreDisp : MonoBehaviour {

	public	Texture[]	number_textures;

	// ================================================================ //
	private static float	TEXTURE_WIDTH 	= 48;		// 텍스처의 폭.
	private static float	TEXTURE_HEIGHT	= 48;		// 텍스처의 높이.
	private static int		MAX_DIGITS		= 4;		// 최대로 표시할 자리숫.
	private static float	CHARACTER_SPACE	= 32;		// 문자 간격.


	// ================================================================ //
	// MonoBehaviour로부터 상속.

	void	Start()
	{
	}
	
	void	Update()
	{
	}
	
	// ================================================================ //

	// 숫자를 표시한다.
	// 점수용 폰트를 사용해서 숫자를 표시한다.
	public void	dispNumber(Vector2 pos, int number)
	{
		int		i;

		// 현재 점수가 몇 자리인지 조사한다.
		int			digits;

		if(number <= 0) {
			// ０이하일 때는 Log10을 바르게 계산할 수 없으므로.
			number = 0;
			digits = 1;
		} else {
			digits = (int)Mathf.Log10(number) + 1;
		}

		// 표시 위치를 구한다.
		Vector2		p = pos;
		// p.x += (MAX_DIGITS - 1)*TEXTURE_WIDTH;
		p.x += (MAX_DIGITS - 1)*CHARACTER_SPACE;
		int		n = number;

		// n을 10으로 나누어가면 n의 1의 자리가
		//
		// 점수의 1의 자리
		// 점수의 10의 자리
		// 점수의 100의 자리
		//		:
		//
		// 로 변해간다.
		for(i = 0;i < digits;i++) {
			// 이번에 표시할 자리의 숫자(0~9).
			int		digit = n%10;
			Texture		texture = this.number_textures[digit];
			GUI.DrawTexture(new Rect(p.x, p.y, TEXTURE_WIDTH, TEXTURE_HEIGHT), texture);
			p.x -= CHARACTER_SPACE;;
			n /= 10;
		}
	}
}

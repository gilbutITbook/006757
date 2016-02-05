using UnityEngine;
using System.Collections;

// 블록 크리에이터.
public class BlockCreator : MonoBehaviour {

	public	MapCreator		map_creator = null;			// 맵 크리에이터.

	public GameObject[]	blockPrefabs;					// 바닥 블록의 프리팹. Inspector에서 설정.

	private int			block_count = 0;				// 만든 블록 개수.

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{	
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	// 블록을 만든다.
	public void		createBlock(LevelControl.CreationInfo current_block, Vector3 block_position)
	{
		if(current_block.block_type == Block.TYPE.FLOOR) {

			// 다음에 만들 블록의 종류를 결정한다.
			// blockPrefabs에 설정된 블록이 차례로 나온다.
			//
			int		next_block_type = this.block_count%this.blockPrefabs.Length;

			GameObject		go        = GameObject.Instantiate(this.blockPrefabs[next_block_type]) as GameObject;
			BlockControl	new_block = go.GetComponent<BlockControl>();

			new_block.transform.position = block_position;

			// BlockControl 클래스에 MapCreator를 기록해 둔다.
			// (BlockControl 클래스에서 MapCreator 클래스의 메소드를 호출하기 위해).
			//
			new_block.map_creator = this.map_creator;

			this.block_count++;
		}
	}
}

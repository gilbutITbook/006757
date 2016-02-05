using UnityEngine;
using System.Collections;

public class LevelControl : MonoBehaviour {

	// 만들어야 할 블록에 관한 정보를 모은 구조체. 
	public struct CreationInfo {
		public Block.TYPE block_type; // 블록의 종류.
		public int max_count; // 블록의 최대 개수.
		public int height; // 블록을 배치할 높이.
		public int current_count; // 작성한 블록의 개수.
	};

	public CreationInfo previous_block; // 이전에 어떤 블록을 만들었는가.
	public CreationInfo current_block; // 이번에 어떤 블록을 만들어야 하는가.
	public CreationInfo next_block; // 다음에 어떤 블록을 만들어야 하는가.
	public int block_count = 0; // 생성한 블록의 총 수.
	public int level = 0; // 난이도.


	private void clear_next_block(ref CreationInfo block)
	{
		// 전달받은 블록(block)을 초기화.
		block.block_type = Block.TYPE.FLOOR;
		block.max_count = 15;
		block.height = 0;
		block.current_count = 0;
	}


	public void initialize()
	{
		this.block_count = 0; // 블록의 총 수를 초기화.
		// 이전, 현재, 다음 블록을 각각.
		// clear_next_block()에 넘겨서 초기화한다.
		this.clear_next_block(ref this.previous_block);
		this.clear_next_block(ref this.current_block);
		this.clear_next_block(ref this.next_block);
	}

	private void update_level(ref CreationInfo current, CreationInfo previous)
	{
		switch(previous.block_type) {
		case Block.TYPE.FLOOR: // 이번 블록이 바닥일 경우.
			current.block_type = Block.TYPE.HOLE; // 다음은 구멍을 만든다.
			current.max_count = 5; // 구멍은 5개 만든다.
			current.height = previous.height; // 높이를 이전과 같게 한다.
			break;
		case Block.TYPE.HOLE: // 이번 블록이 구멍일 경우.
			current.block_type = Block.TYPE.FLOOR; // 다음은 바닥을 만든다.
			current.max_count = 10; // 바닥은 10개 만든다.
			break;
		}
	}

	public void update()
	{
		// 이번에 만든 블록 수를 증가.
		this.current_block.current_count++;
		// 이번에 만든 블록 수가 max_count이상이면.
		if(this.current_block.current_count >= this.current_block.max_count) {
			this.previous_block = this.current_block;
			this.current_block = this.next_block;
			// 다음에 만들 블록의 내용을 초기화.
			this.clear_next_block(ref this.next_block);
			// 다음에 만들 블록을 설정.
			this.update_level(ref this.next_block, this.current_block);
		}
		this.block_count++; // 블록의 총 수를 증가.
	}

}

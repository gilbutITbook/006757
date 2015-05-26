using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LeaveBlockRoot : MonoBehaviour {

	public	GameObject		blockPrefab      = null;

	public	BlockRoot		block_root = null;

	// ================================================================ //
	// MonoBehaviour에서 상속 

	void	Start()
	{
	}
	
	void	Update()
	{
		// 퇴장 대기 블록 목록 갱신.
		this.process_waiting_queue();
	}

	//  퇴장 대기 블록 목록 갱신
	public void		process_waiting_queue()
	{
		List<LeaveBlockControl>		blocks = new List<LeaveBlockControl>();

		bool		is_found = false;

		for(int i = 0;i < this.waiting_blocks.Count;i++) {

			foreach(LeaveBlockControl waiting_block in this.waiting_blocks) {
	
				blocks.Clear();
	
				if(!this.check_ready_to_leave(waiting_block, blocks)) {
	
					continue;
				}

				// 섬의 블록이 전부 일치하면 퇴장 연출을 시작한다.
	
				GameObject	go = new GameObject("island");

				LeaveBlockIsland	island = go.AddComponent<LeaveBlockIsland>();

				foreach(LeaveBlockControl block in blocks) {

					block.transform.parent = island.transform;

					if(block.color == Block.COLOR.NECO) {

						block.getNecoMotion().CrossFade("10_Move", 0.1f);
					}

					this.waiting_blocks.Remove(block);
				}

				is_found = true;
				break;
			}

			if(!is_found) {

				break;
			}
		}
	}

	// ================================================================ //

	public List<LeaveBlockControl>	waiting_blocks = new List<LeaveBlockControl>();

	// 퇴장 연출용 블록을 만든다.
	public LeaveBlockControl		createLeaveBlock(BlockControl block)
	{
		GameObject 		game_object = Instantiate(this.blockPrefab) as GameObject;

		LeaveBlockControl	leave_block = game_object.GetComponent<LeaveBlockControl>();

		//
		leave_block.leave_block_root = this;
		leave_block.i_pos = block.i_pos;
		leave_block.transform.position = block.transform.position;
		leave_block.getModelsRoot().transform.localScale    = block.getModelsRoot().transform.localScale;
		leave_block.getModelsRoot().transform.localPosition = block.getModelsRoot().transform.localPosition;
		leave_block.getModelsRoot().transform.localRotation = block.getModelsRoot().transform.localRotation;

		leave_block.setColor(block.color);

		if(block.color == Block.COLOR.NECO) {

			leave_block.getNecoMotion()["00_Idle"].time = block.getNekoMotion()["00_Idle"].time;
		}

		for(int i = 0;i < (int)Block.DIR4.NUM;i++) {

			Block.DIR4	dir = (Block.DIR4)i;

			leave_block.setConnectedBlock(dir, block.getConnectedBlock(dir));
		}

		// 하이어라키 뷰에서 위치를 확인하기 쉽게 블록 좌표를 이름에 붙여 둔다.
		leave_block.name = "leave_block(" + block.i_pos.x.ToString() + "," + block.i_pos.y.ToString() + ")";

		// 섬(같은 색 블록이 서로 이웃한 덩어리) 블록이 
		// 전부 갖춰질 때까지 기다린다 .
		this.waiting_blocks.Add(leave_block);

		return(leave_block);
	}

	// 섬 블록이 모두 모였는지 조사한다.
	private bool	check_ready_to_leave(LeaveBlockControl block, List<LeaveBlockControl> blocks)
	{
		bool	ready = true;

		do {

			if(blocks.Contains(block)) {
	
				break;
			}
	
			blocks.Add(block);
	
			foreach(Block.iPosition ipos in block.connected_block) {
	
				if(!ipos.isValid()) {
	
					continue;
				}
	
				LeaveBlockControl	next_block = this.waiting_blocks.Find(x => x.i_pos.Equals(ipos));
	
				if(next_block == null) {
	
					ready = false;
					break;
				}
	
				if(!this.check_ready_to_leave(next_block, blocks)) {
		
					ready = false;
					break;
				}
			}

		} while(false);

		return(ready);
	}

}

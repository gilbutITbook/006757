using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LeaveBlockIsland : MonoBehaviour {

	private List<LeaveBlockControl>		blocks = null;

	private class Chain {

		public Vector3	move_direction;
		public Vector3	velocity;
		public float	omega;
	};
	private Chain		chain;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void 	Awake()
	{
		this.blocks = new List<LeaveBlockControl>();
	}

	void	Start()
	{
		this.chain_initialize();
	}

	void	Update()
	{
		do {

			if(!this.is_in_range()) {

				GameObject.Destroy(this.gameObject);
				break;
			}

			this.chain_execute();

		} while(false);
	}

	//화면 내에 들어가는가?.
	private bool	is_in_range()
	{
		bool	is_in_range = false;

		foreach(LeaveBlockControl block in this.blocks) {

			if(block.isInDispArea()) {

				is_in_range = true;
				break;
			}
		}

		return(is_in_range);
	}
	
	// ================================================================ //

	// 초기화.
	private void	chain_initialize()
	{
		do {

			LeaveBlockControl[]	children = this.GetComponentsInChildren<LeaveBlockControl>();

			if(children.Length == 0) {

				break;
			}

			Block.DIR4		dir = this.select_move_dir(children);

			this.chain = new Chain();

			// 선두 블록을 정한다.
			for(int j = 0;j < children.Length;j++) {
	
				int		sel = -1;
	
				for(int i = 0;i < children.Length;i++) {
		
					if(children[i] == null) {
		
						continue;
					}

					bool	sw = false;

					if(sel == -1) {

						sw = true;

					} else {

						switch(dir) {

							case Block.DIR4.RIGHT:	sw = (children[i].i_pos.x > children[sel].i_pos.x);	break;
							case Block.DIR4.LEFT:	sw = (children[i].i_pos.x < children[sel].i_pos.x);	break;
							case Block.DIR4.UP:		sw = (children[i].i_pos.y > children[sel].i_pos.y);	break;
							case Block.DIR4.DOWN:	sw = (children[i].i_pos.y < children[sel].i_pos.y);	break;
						}
					}

					if(sw) {

						sel = i;
					}
				}
	
				if(sel == -1) {
	
					break;
				}
	
				this.blocks.Add(children[sel]);
				children[sel] = null;
			}

			switch(dir) {

				case Block.DIR4.RIGHT:	this.chain.move_direction = Vector3.right;	break;
				case Block.DIR4.LEFT:	this.chain.move_direction = Vector3.left;	break;
				case Block.DIR4.UP:		this.chain.move_direction = Vector3.up;		break;
				case Block.DIR4.DOWN:	this.chain.move_direction = Vector3.down;	break;
			}

			this.chain.velocity = this.chain.move_direction*2.0f;
			this.chain.omega    = 0.0f;

		} while(false);
	}

	// 매 프레임 실행.
	private void	chain_execute()
	{
		// 선두 블록.

		this.chain.velocity += this.chain.move_direction*4.0f*Time.deltaTime;
		this.chain.omega    += 360.0f*Time.deltaTime;

		LeaveBlockControl	trailer = this.blocks[0];

		trailer.transform.Translate(this.chain.velocity*Time.deltaTime);

		if(trailer.color == Block.COLOR.NECO) {

			trailer.setNecoRotation(this.chain.velocity);

		} else {

			trailer.getModelsRoot().transform.Rotate(Vector3.up, this.chain.omega*Time.deltaTime);
		}

		// 2회째 이후의 블록.
		// 이전 블록에 끌어당겨지듯 이동한다.

		float	omega = this.chain.omega*0.75f;

		float	prev_velocity = this.chain.velocity.magnitude;

		for(int i = 1;i < this.blocks.Count;i++) {

			LeaveBlockControl	prev = this.blocks[i - 1];
			LeaveBlockControl	crnt = this.blocks[i];

			// 위치 이동.

			Vector3		distance_vector = crnt.transform.position - prev.transform.position;

			float		distance_limit = 1.0f;
			float		distance       = distance_vector.magnitude;

			if(distance > distance_limit) {

				float		velocity = prev_velocity + (distance - distance_limit)*0.1f;
	
				if(distance - velocity*Time.deltaTime < distance_limit) {
	
					velocity = (distance - distance_limit)/Time.deltaTime;
				}
	
				distance -= velocity*Time.deltaTime;
	
				distance_vector.Normalize();
				distance_vector *= distance;
	
				crnt.transform.position = prev.transform.position + distance_vector;

				prev_velocity = velocity;
			}

			// 회전.
			if(trailer.color == Block.COLOR.NECO) {

				crnt.setNecoRotation(-distance_vector);

			} else {

				crnt.getModelsRoot().transform.Rotate(Vector3.up, omega*Time.deltaTime);
			}

			omega *= 0.75f;
		}
	}

	// 블록이 퇴장하는 방향을 정한다.
	private Block.DIR4	select_move_dir(LeaveBlockControl[]	children)
	{
		Block.DIR4	dir = Block.DIR4.RIGHT;

		float		cx = 0.0f;

		foreach(LeaveBlockControl block in children) {

			cx += (float)block.i_pos.x;
		}

		cx /= (float)children.Length;

		if(cx < (float)Block.BLOCK_NUM_X/2) {

			dir = Block.DIR4.LEFT;

		} else {

			dir = Block.DIR4.RIGHT;
		}

		return(dir);
	}

	// ================================================================ //
}

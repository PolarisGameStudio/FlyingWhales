﻿using UnityEngine;
using System.Collections;

public class Ghoul : Monster {

	public Ghoul (MONSTER type, HexTile originHextile) : base (type, originHextile){
		Initialize();
	}


	#region Overrides
//	internal override void Attack (){
//		if(this.avatar != null){
//			this.avatar.GetComponent<GhoulAvatar> ().HasAttacked();
//			if(this.avatar.GetComponent<GhoulAvatar> ().direction == DIRECTION.LEFT){
//				this.avatar.GetComponent<GhoulAvatar> ().animator.Play ("Attack_Left");
//			}else if(this.avatar.GetComponent<GhoulAvatar> ().direction == DIRECTION.RIGHT){
//				this.avatar.GetComponent<GhoulAvatar> ().animator.Play ("Attack_Right");
//			}else if(this.avatar.GetComponent<GhoulAvatar> ().direction == DIRECTION.UP){
//				this.avatar.GetComponent<GhoulAvatar> ().animator.Play ("Attack_Up");
//			}else{
//				this.avatar.GetComponent<GhoulAvatar> ().animator.Play ("Attack_Down");
//			}
//		}
//	}
	internal override void Initialize(){
        CreateAvatarGO();
		//this.avatar = GameObject.Instantiate (Resources.Load ("GameObjects/Ghoul"), this.originHextile.transform) as GameObject;
		//this.avatar.transform.localPosition = Vector3.zero;
		this.avatar.GetComponent<GhoulAvatar>().Init(this);
	}
	internal override void Death (){
		base.Death();
		if(this.avatar != null){
			ObjectPoolManager.Instance.DestroyObject(this.avatar);
			this.avatar = null;
		}
	}
	internal override void DoneAction (){
		base.DoneAction ();
		this.Death();
	}
	#endregion
}
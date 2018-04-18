﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panda;
using EZObjectPools;

public class MonsterAvatar : PooledObject {
	public delegate void onBehavior ();
	public static event onBehavior onBehaviorAction;

	public Monster monster;
	public PandaBehaviour pandaBehaviour;
	public Animator animator;
	public TextMesh txtDamage;
	public bool collidedWithHostile;
	public Citizen hostile;
	private SmoothMovement smoothMovement;

	private bool hasArrived = false;
	//	private bool isMoving = false;
	//	private Vector3 targetPosition = Vector3.zero;
	private List<HexTile> pathToUnhighlight = new List<HexTile> ();

	//	public float speed;
	internal DIRECTION direction;

	void Awake(){
		this.smoothMovement = this.animator.GetComponent<SmoothMovement> ();
		this.smoothMovement.avatarGO = this.gameObject;
	}

	internal void Init(Monster monster){
		this.monster = monster;
		this.txtDamage.text = monster.hp.ToString ();
		this.direction = DIRECTION.LEFT;
		ResetValues ();
		this.AddBehaviourTree ();
	}
	void OnTriggerEnter2D(Collider2D other){
		if(other.tag == "Avatar"){
			if(this.gameObject != null && other.gameObject != null){
				Citizen otherAgent = other.gameObject.GetComponent<CitizenAvatar>().citizenRole.citizen;
				if(!otherAgent.isDead){
					this.hostile = otherAgent;
//					CombatManager.Instance.HasCollidedWithMonster (this.monster, otherAgent.assignedRole);
				}
			}
		}
	}


	#region Behaviour Tree
	[Task]
	public void HasArrivedAtTargetHextile(){
//		if(onBehaviorAction != null){
//			onBehaviorAction ();
//		}
		if (this.monster.location == this.monster.targetLocation) {
			if (this.monster.lair.lairSpawn.behavior == BEHAVIOR.HOMING) {
				if (!this.hasArrived) {
					this.hasArrived = true;
					this.GetComponent<BoxCollider2D> ().enabled = false;
					this.monster.Attack ();
				}
				Task.current.Succeed ();
			} else if (this.monster.lair.lairSpawn.behavior == BEHAVIOR.ROAMING) {
				if (this.monster.location.isOccupied && this.monster.location.isHabitable && (this.monster.location.city != null && this.monster.location.city.id != 0)) {
					if (!this.hasArrived) {
						this.hasArrived = true;
						this.GetComponent<BoxCollider2D> ().enabled = false;
						this.monster.Attack ();
					}
					Task.current.Succeed ();
				} else {
					this.monster.AcquireTarget ();
					Task.current.Fail ();
				}
			}
		}else {
			Task.current.Fail ();
		}
	}

	[Task]
	public void MoveToNextTile(){
		Move ();
		Task.current.Succeed ();
	}	
	#endregion

	internal void MakeCitizenMove(HexTile startTile, HexTile targetTile){
		if(startTile.transform.position.x <= targetTile.transform.position.x){
			if(this.animator.gameObject.transform.localScale.x > 0){
				this.animator.gameObject.transform.localScale = new Vector3(this.animator.gameObject.transform.localScale.x * -1, this.animator.gameObject.transform.localScale.y, this.animator.gameObject.transform.localScale.z);
			}
		}else{
			if(this.animator.gameObject.transform.localScale.x < 0){
				this.animator.gameObject.transform.localScale = new Vector3(this.animator.gameObject.transform.localScale.x * -1, this.animator.gameObject.transform.localScale.y, this.animator.gameObject.transform.localScale.z);
			}
		}
		if(startTile.transform.position.y < targetTile.transform.position.y){
			this.direction = DIRECTION.UP;
			this.animator.Play("Walk_Up");
		}else if(startTile.transform.position.y > targetTile.transform.position.y){
			this.direction = DIRECTION.DOWN;
			this.animator.Play("Walk_Down");
		}else{
			if(startTile.transform.position.x < targetTile.transform.position.x){
				this.direction = DIRECTION.RIGHT;
				this.animator.Play("Walk_Right");
			}else{
				this.direction = DIRECTION.LEFT;
				this.animator.Play("Walk_Left");
			}
		}
		this.smoothMovement.Move(targetTile.transform.position, this.direction);
		this.UpdateUI ();
	}
	private void StopMoving(){
		this.animator.Play("Idle");
		//		this.isMoving = false;
		//		this.targetPosition = Vector3.zero;
	}
	internal void UpdateUI(){
		if(this.monster != null){
			this.txtDamage.text = this.monster.hp.ToString ();
		}
	}

	private void ResetValues(){
		this.collidedWithHostile = false;
		this.hostile = null;
	}

	private void Move(){
		if(this.monster.targetLocation != null){
			if(this.monster.path != null){
				if (this.monster.path.Count > 0) {
					this.MakeCitizenMove(this.monster.location, this.monster.path[0]);
					this.monster.prevLocation = this.monster.location;
					this.monster.location = this.monster.path[0];
					this.monster.path.RemoveAt(0);
//					this.monster.AcquireTarget ();
				}
			}
		}
	}

	private void HomingBehavior(){
		if(this.monster.location == this.monster.targetLocation){
			if(!this.hasArrived){
				this.hasArrived = true;
				this.GetComponent<BoxCollider2D> ().enabled = false;
				this.monster.Attack ();
			}
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}
	}

	private void RoamingBehavior(){
		if(this.monster.location == this.monster.targetLocation){
			if(this.monster.location.isOccupied && this.monster.location.isHabitable && (this.monster.location.city != null && this.monster.location.city.id != 0)){
				if(!this.hasArrived){
					this.hasArrived = true;
					this.GetComponent<BoxCollider2D> ().enabled = false;
					this.monster.Attack ();
				}
				Task.current.Succeed ();
			}else{
				this.monster.AcquireTarget ();
				Task.current.Fail ();
			}

		}else{
			Task.current.Fail ();
		}
	}

	internal void AddBehaviourTree(){
        //BehaviourTreeManager.Instance.allTrees.Add (this.pandaBehaviour);
        Messenger.AddListener(Signals.DAY_END, this.pandaBehaviour.Tick);
    }

	internal void RemoveBehaviourTree(){
        //BehaviourTreeManager.Instance.allTrees.Remove (this.pandaBehaviour);
        Messenger.RemoveListener(Signals.DAY_END, this.pandaBehaviour.Tick);
    }


	void OnMouseEnter(){
		if (!UIManager.Instance.IsMouseOnUI()) {
			UIManager.Instance.ShowSmallInfo (this.monster.type.ToString());
			this.HighlightPath ();
		}
	}

	void OnMouseExit(){
		UIManager.Instance.HideSmallInfo ();
		this.UnHighlightPath ();
	}

	private void Update() {
		if (KingdomManager.Instance.useFogOfWar) {
			if (monster.location.currFogOfWarState == FOG_OF_WAR_STATE.VISIBLE) {
				gameObject.GetComponent<SpriteRenderer>().enabled = true;
			} else {
				gameObject.GetComponent<SpriteRenderer>().enabled = false;
			}
		}
	}

	void HighlightPath(){
		this.pathToUnhighlight.Clear ();
		if(this.monster.path != null){
			for (int i = 0; i < this.monster.path.Count; i++) {
				this.monster.path [i].highlightGO.SetActive (true);
				this.pathToUnhighlight.Add (this.monster.path [i]);
			}
		}
	}

	void UnHighlightPath(){
		for (int i = 0; i < this.pathToUnhighlight.Count; i++) {
			this.pathToUnhighlight[i].highlightGO.SetActive(false);
		}
	}

	void OnDestroy(){
		RemoveBehaviourTree();
		UnHighlightPath ();
	}

	public void OnEndAttack(){
		this.monster.DoneAction();
	}

	internal void HasAttacked(){
		this.smoothMovement.hasAttacked = true;
	}

    #region overrides
    public override void Reset() {
        base.Reset();
        pandaBehaviour.Reset();
        RemoveBehaviourTree();
        UnHighlightPath();
        ResetValues();
        hasArrived = false;
        this.direction = DIRECTION.LEFT;
		this.smoothMovement.Reset();
        this.GetComponent<BoxCollider2D>().enabled = true;
    }
    #endregion
}
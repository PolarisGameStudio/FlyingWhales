﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panda;

public class InvestigatorAvatar : MonoBehaviour {
	public Investigator investigator;
	public PandaBehaviour pandaBehaviour;
	public Animator animator;
	public bool collidedWithHostile;
	public General otherGeneral;

	private bool hasArrived = false;
//	private bool isMoving = false;
//	private Vector3 targetPosition = Vector3.zero;
	private List<HexTile> pathToUnhighlight = new List<HexTile> ();

//	public float speed;
	internal DIRECTION direction;
//	void Update(){
//		if(this.isMoving){
//			if(this.targetPosition != null){
//				float step = speed * Time.deltaTime;
//				this.transform.position = Vector3.MoveTowards (this.transform.position, this.targetPosition, step);
//				if(Vector3.Distance(this.transform.position, this.targetPosition) < 0.1f){
//					StopMoving ();
//				}
//			}
//		}
//	}
	internal void Init(Investigator investigator){
		this.investigator = investigator;
		this.direction = DIRECTION.LEFT;
		this.GetComponent<Avatar> ().kingdom = this.investigator.citizen.city.kingdom;
		this.GetComponent<Avatar> ().gameEvent = this.investigator.firstAndKeystone;
		this.GetComponent<Avatar> ().citizen = this.investigator.citizen;
        visibleTiles = new List<HexTile>();

        ResetValues ();
		this.AddBehaviourTree ();
	}
//	void OnTriggerEnter2D(Collider2D other){
//		if(other.tag == "General"){
//			this.collidedWithHostile = false;
//			if(this.gameObject != null && other.gameObject != null){
//				if(other.gameObject.GetComponent<GeneralAvatar>().general.citizen.city.kingdom.id != this.raider.citizen.city.kingdom.id){
//					if(!other.gameObject.GetComponent<GeneralAvatar> ().general.citizen.isDead){
//						this.collidedWithHostile = true;
//						this.otherGeneral = other.gameObject.GetComponent<GeneralAvatar> ().general;
//					}
//				}
//			}
//		}
//	}
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
//		this.transform.position = Vector3.MoveTowards (startTile.transform.position, targetTile.transform.position, 0.5f);
//		if(startTile.transform.position.x <= targetTile.transform.position.x){
//			if(this.generalAnimator.gameObject.transform.localScale.x > 0){
//				this.generalAnimator.gameObject.transform.localScale = new Vector3(this.generalAnimator.gameObject.transform.localScale.x * -1, this.generalAnimator.gameObject.transform.localScale.y, this.generalAnimator.gameObject.transform.localScale.z);
//			}
//		}else{
//			if(this.generalAnimator.gameObject.transform.localScale.x < 0){
//				this.generalAnimator.gameObject.transform.localScale = new Vector3(this.generalAnimator.gameObject.transform.localScale.x * -1, this.generalAnimator.gameObject.transform.localScale.y, this.generalAnimator.gameObject.transform.localScale.z);
//			}
//		}
//		if(startTile.transform.position.y < targetTile.transform.position.y){
//			this.generalAnimator.Play("Walk_Up");
//		}else{
//			this.generalAnimator.Play("Walk");
//		}
		this.GetComponent<SmoothMovement>().direction = this.direction;
		this.GetComponent<SmoothMovement>().Move(targetTile.transform.position);
//		this.targetPosition = targetTile.transform.position;
//		this.UpdateUI ();
//		this.isMoving = true;
	}
	private void StopMoving(){
		this.animator.Play("Idle");
//		this.isMoving = false;
//		this.targetPosition = Vector3.zero;
	}
//	internal void UpdateUI(){
//		if(this.general != null){
//			this.textMesh.text = this.general.army.hp.ToString ();
//		}
//	}
	[Task]
	public void IsThereCitizen(){
		if(this.investigator.citizen != null){
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}
	}
	[Task]
	public void IsThereEvent(){
		if(this.investigator.firstAndKeystone != null){
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}
	}

	[Task]
	public void HasArrivedAtTargetHextile(){
		if(this.investigator.location == this.investigator.targetLocation){
			if(!this.hasArrived){
				this.hasArrived = true;
				this.investigator.Attack ();
			}
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}

	}
		
//	[Task]
//	public void HasCollidedWithHostileGeneral(){
//		if(this.collidedWithHostile){
//			this.collidedWithHostile = false;
//			if(!this.otherGeneral.citizen.isDead){
//				//Death by general
//				this.raider.raid.DeathByGeneral (this.otherGeneral);
//				Task.current.Succeed ();
//			}else{
//				Task.current.Fail ();
//			}
//
//		}else{
//			Task.current.Fail ();
//		}
//	}
	[Task]
	public void HasDiedOfOtherReasons(){
		if (this.investigator.citizen.isDead) {
			//Citizen has died
			this.investigator.firstAndKeystone.DeathByOtherReasons ();
			Task.current.Succeed();
		}else {
			Task.current.Fail ();
		}
	}
	[Task]
	public void MoveToNextTile(){
		Move ();
		Task.current.Succeed ();
	}

	private void ResetValues(){
		this.collidedWithHostile = false;
		this.otherGeneral = null;
	}

	private void Move(){
		if(this.investigator.targetLocation != null){
			if(this.investigator.path != null){
				if(this.investigator.path.Count > 0){
					this.MakeCitizenMove (this.investigator.location, this.investigator.path [0]);
//					this.raider.daysBeforeMoving = this.raider.path [0].movementDays;
					this.investigator.location = this.investigator.path[0];
					this.investigator.citizen.currentLocation = this.investigator.path [0];
                    this.UpdateFogOfWar();
					this.investigator.path.RemoveAt (0);
                    this.investigator.location.CollectEventOnTile(this.investigator.citizen.city.kingdom, this.investigator.citizen);
                    this.CheckForKingdomDiscovery();
//					if(this.raider.daysBeforeMoving <= 0){
//						this.MakeCitizenMove (this.raider.location, this.raider.path [0]);
//						this.raider.daysBeforeMoving = this.raider.path [0].movementDays;
//						this.raider.location = this.raider.path[0];
//						this.raider.citizen.currentLocation = this.raider.path [0];
//						this.raider.path.RemoveAt (0);
//					}
//					this.raider.daysBeforeMoving -= 1;
				}
			}
		}
	}

    private void CheckForKingdomDiscovery() {
		HexTile currentLocation = this.investigator.location;
        if (currentLocation.isOccupied && currentLocation.ownedByCity != null &&
			currentLocation.ownedByCity.kingdom.id != this.investigator.citizen.city.kingdom.id) {
			Kingdom thisKingdom = this.investigator.citizen.city.kingdom;
            Kingdom otherKingdom = currentLocation.ownedByCity.kingdom;
            thisKingdom.DiscoverKingdom(otherKingdom);
            otherKingdom.DiscoverKingdom(thisKingdom);
        } else if (currentLocation.isBorder) {
			Kingdom thisKingdom = this.investigator.citizen.city.kingdom;
            Kingdom otherKingdom = CityGenerator.Instance.GetCityByID(currentLocation.isBorderOfCityID).kingdom;
			if (otherKingdom.id != this.investigator.citizen.city.kingdom.id) {
                thisKingdom.DiscoverKingdom(otherKingdom);
                otherKingdom.DiscoverKingdom(thisKingdom);
            }
        }
    }

    private List<HexTile> visibleTiles;
    private void UpdateFogOfWar(bool forDeath = false) {
        for (int i = 0; i < visibleTiles.Count; i++) {
            HexTile currTile = visibleTiles[i];
            this.investigator.citizen.city.kingdom.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.SEEN);
        }
        visibleTiles.Clear();
        if (!forDeath) {
            visibleTiles.Add(this.investigator.location);
            visibleTiles.AddRange(this.investigator.location.AllNeighbours);
            for (int i = 0; i < visibleTiles.Count; i++) {
                HexTile currTile = visibleTiles[i];
                this.investigator.citizen.city.kingdom.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.VISIBLE);
            }
        }

    }

    internal void AddBehaviourTree(){
		BehaviourTreeManager.Instance.allTrees.Add (this.pandaBehaviour);
	}

	internal void RemoveBehaviourTree(){
		BehaviourTreeManager.Instance.allTrees.Remove (this.pandaBehaviour);
	}


	void OnMouseEnter(){
		if (!UIManager.Instance.IsMouseOnUI()) {
			UIManager.Instance.ShowSmallInfo (this.investigator.firstAndKeystone.name);
			this.HighlightPath ();
		}
	}

	void OnMouseExit(){
		UIManager.Instance.HideSmallInfo ();
		this.UnHighlightPath ();
	}

    private void FixedUpdate() {
        if (this.investigator.location.currFogOfWarState == FOG_OF_WAR_STATE.VISIBLE) {
            gameObject.GetComponent<SpriteRenderer>().enabled = true;
        } else {
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    void HighlightPath(){
		this.pathToUnhighlight.Clear ();
		for (int i = 0; i < this.investigator.path.Count; i++) {
			this.investigator.path [i].highlightGO.SetActive (true);
			this.pathToUnhighlight.Add (this.investigator.path [i]);
		}
	}

	void UnHighlightPath(){
		for (int i = 0; i < this.pathToUnhighlight.Count; i++) {
			this.pathToUnhighlight[i].highlightGO.SetActive(false);
		}
	}

	void OnDestroy(){
		BehaviourTreeManager.Instance.allTrees.Remove (this.pandaBehaviour);
		UnHighlightPath ();
        UpdateFogOfWar(true);
    }

	public void OnEndAttack(){
		this.investigator.firstAndKeystone.DoneCitizenAction(this.investigator.citizen);
		this.investigator.DestroyGO ();
	}

	internal void HasAttacked(){
		this.GetComponent<SmoothMovement> ().hasAttacked = true;
	}
//	private string CampaignInfo(Campaign campaign){
//		string info = string.Empty;
//		info += "id: " + campaign.id;
//		info += "\n";
//
//		info += "campaign type: " + campaign.campaignType.ToString ();
//		info += "\n";
//
//		info += "general: " + this.general.citizen.name;
//		info += "\n";
//
//		info += "target city: " + campaign.targetCity.name;
//		info += "\n";
//		if (campaign.rallyPoint == null) {
//			info += "rally point: N/A";
//		} else {
//			info += "rally point: " + campaign.rallyPoint.name; 
//		}
//		info += "\n";
//
//		info += "leader: " + campaign.leader.name;
//		info += "\n";
//
//		info += "war type: " + campaign.warType.ToString ();
//		info += "\n";
//
//		info += "needed army: " + campaign.neededArmyStrength.ToString ();
//		info += "\n";
//
//		info += "army: " + campaign.GetArmyStrength ().ToString ();
//		info += "\n";
//
//		if (campaign.campaignType == CAMPAIGN.DEFENSE) {
//			if (campaign.expiration == -1) {
//				info += "expiration: none";
//			} else {
//				info += "will expire in " + campaign.expiration + " days";
//			}
//		} else {
//			info += "expiration: none";
//
//		}
//
//		return info;
//	}


}

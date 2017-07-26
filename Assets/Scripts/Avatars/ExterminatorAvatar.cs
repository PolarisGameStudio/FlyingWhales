﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panda;

public class ExterminatorAvatar : MonoBehaviour {
    public Exterminator exterminator;
    public PandaBehaviour pandaBehaviour;
    public Animator animator;
    public bool collidedWithHostile;
    public General otherGeneral;

    private bool hasArrived = false;
    //	private bool isMoving = false;
    //	private Vector3 targetPosition = Vector3.zero;
    private List<HexTile> pathToUnhighlight = new List<HexTile>();

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
    internal void Init(Exterminator exterminator) {
        this.exterminator = exterminator;
        this.direction = DIRECTION.LEFT;
        this.GetComponent<Avatar>().kingdom = this.exterminator.citizen.city.kingdom;
        this.GetComponent<Avatar>().gameEvent = this.exterminator.plagueEvent;
        this.GetComponent<Avatar>().citizen = this.exterminator.citizen;
        visibleTiles = new List<HexTile>();

        ResetValues();
        this.AddBehaviourTree();
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
	void OnTriggerEnter2D(Collider2D other){
		if(other.tag == "Avatar"){
			if(this.gameObject != null && other.gameObject != null){
				if(other.gameObject.GetComponent<Avatar>().kingdom.id != this.exterminator.citizen.city.kingdom.id){
					if(!other.gameObject.GetComponent<Avatar> ().citizen.isDead){
						CombatManager.Instance.HasCollidedWithHostile (this.GetComponent<Avatar> (), other.gameObject.GetComponent<Avatar>());
					}
				}
			}
		}else if(other.tag == "Trader"){
			if(this.gameObject != null && other.gameObject != null){
				Kingdom kingdomOfGeneral = this.exterminator.citizen.city.kingdom;
				Kingdom kingdomOfTrader = other.gameObject.GetComponent<Avatar>().kingdom;
				if (kingdomOfGeneral.id != kingdomOfTrader.id) {
					RelationshipKings relOfGeneralWithTrader = kingdomOfGeneral.king.GetRelationshipWithCitizen(kingdomOfTrader.king);
					RelationshipKings relOfTraderWithGeneral = kingdomOfTrader.king.GetRelationshipWithCitizen(kingdomOfGeneral.king);
					if (relOfGeneralWithTrader.lordRelationship == RELATIONSHIP_STATUS.ENEMY || relOfGeneralWithTrader.lordRelationship == RELATIONSHIP_STATUS.RIVAL ||
						relOfTraderWithGeneral.lordRelationship == RELATIONSHIP_STATUS.ENEMY || relOfTraderWithGeneral.lordRelationship == RELATIONSHIP_STATUS.RIVAL) {
						if (!other.gameObject.GetComponent<Avatar>().citizen.isDead) {
							CombatManager.Instance.HasCollidedWithHostile (this.GetComponent<Avatar> (), other.gameObject.GetComponent<Avatar>());
						}
					}  
				}
			}
		}
	}
    internal void MakeCitizenMove(HexTile startTile, HexTile targetTile) {
        if (startTile.transform.position.x <= targetTile.transform.position.x) {
            if (this.animator.gameObject.transform.localScale.x > 0) {
                this.animator.gameObject.transform.localScale = new Vector3(this.animator.gameObject.transform.localScale.x * -1, this.animator.gameObject.transform.localScale.y, this.animator.gameObject.transform.localScale.z);
            }
        } else {
            if (this.animator.gameObject.transform.localScale.x < 0) {
                this.animator.gameObject.transform.localScale = new Vector3(this.animator.gameObject.transform.localScale.x * -1, this.animator.gameObject.transform.localScale.y, this.animator.gameObject.transform.localScale.z);
            }
        }
        if (startTile.transform.position.y < targetTile.transform.position.y) {
            this.direction = DIRECTION.UP;
            this.animator.Play("Walk_Up");
        } else if (startTile.transform.position.y > targetTile.transform.position.y) {
            this.direction = DIRECTION.DOWN;
            this.animator.Play("Walk_Down");
        } else {
            if (startTile.transform.position.x < targetTile.transform.position.x) {
                this.direction = DIRECTION.RIGHT;
                this.animator.Play("Walk_Right");
            } else {
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
    private void StopMoving() {
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
    public void IsThereCitizen() {
        if (this.exterminator.citizen != null) {
            Task.current.Succeed();
        } else {
            Task.current.Fail();
        }
    }
    [Task]
    public void IsThereEvent() {
        if (this.exterminator.plagueEvent != null) {
            Task.current.Succeed();
        } else {
            Task.current.Fail();
        }
    }

    [Task]
    public void HasArrivedAtTargetHextile() {
        if (this.exterminator.location == this.exterminator.targetLocation) {
            if (!this.hasArrived) {
                this.hasArrived = true;
                this.exterminator.Attack();
            }
            Task.current.Succeed();
        } else {
            Task.current.Fail();
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
    public void HasDiedOfOtherReasons() {
        if (this.exterminator.citizen.isDead) {
            //Citizen has died
            this.exterminator.plagueEvent.DeathByOtherReasons();
            Task.current.Succeed();
        } else {
            Task.current.Fail();
        }
    }
    [Task]
    public void MoveToNextTile() {
        Move();
        Task.current.Succeed();
    }

    private void ResetValues() {
        this.collidedWithHostile = false;
        this.otherGeneral = null;
    }

    private void Move() {
        if (this.exterminator.targetLocation != null) {
            if (this.exterminator.path != null) {
                if (this.exterminator.path.Count > 0) {
                    this.MakeCitizenMove(this.exterminator.location, this.exterminator.path[0]);
                    this.exterminator.location = this.exterminator.path[0];
                    this.exterminator.citizen.currentLocation = this.exterminator.path[0];
                    this.UpdateFogOfWar();
                    this.exterminator.path.RemoveAt(0);
                    this.exterminator.location.CollectEventOnTile(this.exterminator.citizen.city.kingdom, this.exterminator.citizen);
                    this.CheckForKingdomDiscovery();
                }
            }
        }
    }

    private void CheckForKingdomDiscovery() {
        HexTile currentLocation = this.exterminator.location;
        if (currentLocation.isOccupied && currentLocation.ownedByCity != null &&
            currentLocation.ownedByCity.kingdom.id != this.exterminator.citizen.city.kingdom.id) {
            Kingdom thisKingdom = this.exterminator.citizen.city.kingdom;
            Kingdom otherKingdom = currentLocation.ownedByCity.kingdom;
            thisKingdom.DiscoverKingdom(otherKingdom);
            otherKingdom.DiscoverKingdom(thisKingdom);
        } else if (currentLocation.isBorder) {
            Kingdom thisKingdom = this.exterminator.citizen.city.kingdom;
            Kingdom otherKingdom = CityGenerator.Instance.GetCityByID(currentLocation.isBorderOfCityID).kingdom;
            if (otherKingdom.id != this.exterminator.citizen.city.kingdom.id) {
                thisKingdom.DiscoverKingdom(otherKingdom);
                otherKingdom.DiscoverKingdom(thisKingdom);
            }
        }
    }

    private List<HexTile> visibleTiles;
    private void UpdateFogOfWar(bool forDeath = false) {
        for (int i = 0; i < visibleTiles.Count; i++) {
            HexTile currTile = visibleTiles[i];
            this.exterminator.citizen.city.kingdom.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.SEEN);
        }
        visibleTiles.Clear();
        if (!forDeath) {
            visibleTiles.Add(this.exterminator.location);
            visibleTiles.AddRange(this.exterminator.location.AllNeighbours);
            for (int i = 0; i < visibleTiles.Count; i++) {
                HexTile currTile = visibleTiles[i];
                this.exterminator.citizen.city.kingdom.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.VISIBLE);
            }
        }

    }

    internal void AddBehaviourTree() {
        BehaviourTreeManager.Instance.allTrees.Add(this.pandaBehaviour);
    }

    internal void RemoveBehaviourTree() {
        BehaviourTreeManager.Instance.allTrees.Remove(this.pandaBehaviour);
    }


    void OnMouseEnter() {
        if (!UIManager.Instance.IsMouseOnUI()) {
            UIManager.Instance.ShowSmallInfo(this.exterminator.plagueEvent.name);
            this.HighlightPath();
        }
    }

    void OnMouseExit() {
        UIManager.Instance.HideSmallInfo();
        this.UnHighlightPath();
    }

    private void FixedUpdate() {
        if (KingdomManager.Instance.useFogOfWar) {
            if (this.exterminator.location.currFogOfWarState == FOG_OF_WAR_STATE.VISIBLE) {
                gameObject.GetComponent<SpriteRenderer>().enabled = true;
            } else {
                gameObject.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }

    void HighlightPath() {
        this.pathToUnhighlight.Clear();
        for (int i = 0; i < this.exterminator.path.Count; i++) {
            this.exterminator.path[i].highlightGO.SetActive(true);
            this.pathToUnhighlight.Add(this.exterminator.path[i]);
        }
    }

    void UnHighlightPath() {
        for (int i = 0; i < this.pathToUnhighlight.Count; i++) {
            this.pathToUnhighlight[i].highlightGO.SetActive(false);
        }
    }

    void OnDestroy() {
		RemoveBehaviourTree();
        UnHighlightPath();
        UpdateFogOfWar(true);
    }

    public void OnEndAttack() {
        this.exterminator.plagueEvent.DoneCitizenAction(this.exterminator.citizen);
        this.exterminator.DestroyGO();
    }

    internal void HasAttacked() {
        this.GetComponent<SmoothMovement>().hasAttacked = true;
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

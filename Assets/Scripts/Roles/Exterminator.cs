﻿using UnityEngine;
using System.Collections;

public class Exterminator : Role {
    private Plague _plagueEvent;

    #region getters/setters
    public Plague plagueEvent {
        get { return this._plagueEvent; }
    }
    #endregion
    public Exterminator(Citizen citizen): base(citizen){
        
    }

    internal override void Initialize(GameEvent gameEvent) {
        if (gameEvent is Plague) {
            base.Initialize(gameEvent);
            this._plagueEvent = (Plague)gameEvent;
            this._plagueEvent.AddAgentToList(this.citizen);
            this.avatar.GetComponent<ExterminatorAvatar>().Init(this);
        }
    }

//    internal override void Attack() {
//        //		base.Attack ();
//        if (this.avatar != null) {
//			this.avatar.GetComponent<ExterminatorAvatar>().HasAttacked();
//			if (this.avatar.GetComponent<ExterminatorAvatar>().direction == DIRECTION.LEFT) {
//				this.avatar.GetComponent<ExterminatorAvatar>().animator.Play("Attack_Left");
//			} else if (this.avatar.GetComponent<ExterminatorAvatar>().direction == DIRECTION.RIGHT) {
//				this.avatar.GetComponent<ExterminatorAvatar>().animator.Play("Attack_Right");
//			} else if (this.avatar.GetComponent<ExterminatorAvatar>().direction == DIRECTION.UP) {
//				this.avatar.GetComponent<ExterminatorAvatar>().animator.Play("Attack_Up");
//            } else {
//				this.avatar.GetComponent<ExterminatorAvatar>().animator.Play("Attack_Down");
//            }
//        }
//    }
}

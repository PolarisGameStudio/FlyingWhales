﻿using UnityEngine;
using System.Collections;

public class RelationshipItem : MonoBehaviour {

	[SerializeField] private UI2DSprite relationshipSprite;
    [SerializeField] private UILabel likenessLbl;
    [SerializeField] private GameObject forTestingGO;
    [SerializeField] private UILabel newLikenessModifierLbl;
	private KingdomRelationship rk;

	private bool isHovering = false;

	public void SetRelationship(KingdomRelationship rk, bool forTesting = false){
		this.rk = rk;
		this.relationshipSprite.color = Utilities.GetColorForRelationship (rk.relationshipStatus);
        likenessLbl.text = Mathf.Clamp(this.rk.totalLike, -100, 100).ToString();

        if (forTesting) {
            forTestingGO.SetActive(true);
            newLikenessModifierLbl.text = rk.forTestingLikeModifier.ToString();
        } else {
            forTestingGO.SetActive(false);
        }
	}

    void OnHover(bool isOver) {
        if (isOver) {
            string summary = string.Empty;
            summary += rk.relationshipSummary + "\n";
            if(rk.eventLikenessModifier != 0) {
                if (rk.eventLikenessModifier > 0) {
                    summary += "+ ";
                }
                summary += rk.eventLikenessModifier.ToString() + " Opinions";
            }

            if (rk.forTestingLikeModifier != 0) {
                if (rk.forTestingLikeModifier > 0) {
                    summary += "+ ";
                }
                summary += rk.forTestingLikeModifier.ToString() + " Admin Modifier";
            }
			summary += "\n SEP: " + rk._usedSourceEffectivePower.ToString() + ", SED: " + rk._usedSourceEffectiveDef.ToString();
			summary += "\n TEP: " + rk._usedTargetEffectivePower.ToString() + ", TED: " + rk._usedTargetEffectiveDef.ToString();

            UIManager.Instance.ShowRelationshipSummary(this.rk.targetKingdom.king, summary);
        } else {
            UIManager.Instance.HideRelationshipSummary();
        }
    }

    #region For Testing
    public void ChangeLikenessModifier() {
        rk.forTestingLikeModifier = System.Int32.Parse(newLikenessModifierLbl.text);
        rk.UpdateKingRelationshipStatus();
        relationshipSprite.color = Utilities.GetColorForRelationship(rk.relationshipStatus);
        likenessLbl.text = Mathf.Clamp(this.rk.totalLike, -100, 100).ToString();
    }
    #endregion

}

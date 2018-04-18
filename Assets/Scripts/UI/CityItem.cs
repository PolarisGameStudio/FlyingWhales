﻿using UnityEngine;
using System.Collections;

public class CityItem : MonoBehaviour {

    private City _city;

    [SerializeField] private GameObject governorParentGO;
 //   [SerializeField] private GameObject powerGO;
	//[SerializeField] private GameObject defenseGO;
	//[SerializeField] private GameObject hpParentGO;
    [SerializeField] private GameObject cityNameParentGO;
    [SerializeField] private GameObject structuresParentGO;
    [SerializeField] private GameObject growthMeterParentGO;

    [SerializeField] private CharacterPortrait _governor;
 //   [SerializeField] private UILabel _hpLbl;
	//[SerializeField] private UILabel _powerLbl;
	//[SerializeField] private UILabel _defenseLbl;
    [SerializeField] private UILabel _structuresLbl;
    [SerializeField] private UILabel _cityLbl;
    [SerializeField] private UIProgressBar _hpProgBar;

    [SerializeField] private GameObject _loyaltyGO;
    [SerializeField] private UILabel _loyaltyLbl;
    [SerializeField] private UIEventTrigger _loyaltyEventTrigger;
	[SerializeField] private GameObject _rebelIcon;
    [SerializeField] private UIProgressBar _growthProgBar;

	[SerializeField] private GameObject _noFoodGO;
	[SerializeField] private GameObject _noMaterialGO;
	[SerializeField] private GameObject _noOreGO;

    [SerializeField] private GameObject _emblemGO;
    [SerializeField] private UI2DSprite _emblemBG;
    [SerializeField] private UI2DSprite _emblemSprite;
    [SerializeField] private UI2DSprite _emblemOutline;

    [Header("For Testing")]
    [SerializeField] private GameObject forTestingGO;
    //[SerializeField] private UILabel newPowerLbl;
    //[SerializeField] private UILabel newDefLabel;
    [SerializeField] private UILabel loyaltyAdjustmentLbl;

    #region getters/setters
    public City city {
        get { return this._city; }
    }
	public GameObject rebelIcon {
		get { return this._rebelIcon; }
	}
    #endregion

    public void SetCity(City _city, bool showLoyalty = false, bool showNameOnly = false, bool showForTesting = false, bool forNamePlate = false) {
        this._city = _city;
		_structuresLbl.text = city.ownedTiles.Count.ToString();
		_cityLbl.text = city.name;

        //if (forNamePlate) {
        //    _emblemGO.SetActive(true);
        //    Color emblemBGColor = _city.kingdom.kingdomColor;
        //    emblemBGColor.a = 255f / 255f;
        //    _emblemBG.sprite2D = _city.kingdom.emblemBG;
        //    _emblemBG.color = emblemBGColor;
        //    _emblemBG.MakePixelPerfect();
        //    _emblemBG.width += Mathf.FloorToInt(_emblemBG.width * 0.25f);

        //    _emblemOutline.sprite2D = _city.kingdom.emblemBG;
        //    _emblemOutline.MakePixelPerfect();
        //    _emblemOutline.width = (_emblemBG.width + 8);
        //    Color outlineColor;
        //    ColorUtility.TryParseHtmlString("#2d2e2e", out outlineColor);
        //    _emblemOutline.color = outlineColor;

        //    _emblemSprite.sprite2D = _city.kingdom.emblem;
        //    _emblemSprite.MakePixelPerfect();
        //    _emblemSprite.width += Mathf.FloorToInt(_emblemSprite.width * 0.25f);
        //} else {
        //    _emblemGO.SetActive(false);
        //    _governor.SetCitizen(city.governor);
        //    _structuresLbl.text = city.ownedTiles.Count.ToString();
        //    _cityLbl.text = city.name;
        //    _growthProgBar.value = (float)city.currentGrowth / (float)city.maxGrowth;

        //    if (showLoyalty) {
        //        _loyaltyGO.SetActive(true);
        //        _loyaltyLbl.text = _governor.citizen.loyaltyToKing.ToString();
        //        EventDelegate.Set(_loyaltyEventTrigger.onHoverOver, delegate () {
        //            ShowLoyaltySummary();
        //        });
        //        EventDelegate.Set(_loyaltyEventTrigger.onHoverOut, delegate () { UIManager.Instance.HideRelationshipSummary(); });
        //    }

        //    if (showNameOnly) {
        //        governorParentGO.SetActive(false);
        //        cityNameParentGO.SetActive(true);
        //        structuresParentGO.SetActive(false);
        //        growthMeterParentGO.SetActive(false);
        //    } else {
        //        governorParentGO.SetActive(true);
        //        cityNameParentGO.SetActive(true);
        //        structuresParentGO.SetActive(true);
        //        growthMeterParentGO.SetActive(true);
        //    }

        //    if (showForTesting) {
        //        forTestingGO.SetActive(true);
        //        loyaltyAdjustmentLbl.text = ((Governor)_city.governor.assignedRole).forTestingLoyaltyModifier.ToString();
        //    } else {
        //        forTestingGO.SetActive(false);
        //    }
        //}

        

    }
	public void UpdateFoodMaterialOreUI(){
		if(_city.foodCount <= 0){
			_noFoodGO.SetActive (true);
		}else{
			_noFoodGO.SetActive (false);
		}
		if(_city.materialCount <= 0){
			_noMaterialGO.SetActive (true);
		}else{
			_noMaterialGO.SetActive (false);
		}
		if(_city.oreCount <= 0){
			_noOreGO.SetActive (true);
		}else{
			_noOreGO.SetActive (false);
		}
	}
    public void CenterOnCity() {
        CameraMove.Instance.CenterCameraOn(_city.hexTile.gameObject);
    }

    private void ShowLoyaltySummary() {
        Citizen thisCitizen = _governor.citizen;
        string loyaltySummary = string.Empty;
        if(thisCitizen.loyaltyDeductionFromWar != 0) {
            loyaltySummary += thisCitizen.loyaltyDeductionFromWar + "   Active Wars\n";
        }
        loyaltySummary += thisCitizen.loyaltySummary;

        int loyaltyFromStability = thisCitizen.GetLoyaltyFromStability();
        if(loyaltyFromStability != 0) {
            if (loyaltyFromStability > 0) {
                loyaltySummary += "+";
            }
            loyaltySummary += loyaltyFromStability + "   Stability\n";
        }

        if (thisCitizen.loyaltyModifierForTesting != 0) {
            if(thisCitizen.loyaltyModifierForTesting > 0) {
                loyaltySummary += "+";
            }
            loyaltySummary += thisCitizen.loyaltyModifierForTesting + "   Admin Modifier\n";
        }
    }

    #region For Testing
    //public void SetPower() {
    //    _city.SetWeapons(System.Int32.Parse(newPowerLbl.text));
    //    _city.hexTile.UpdateCityNamePlate();
    //    this._powerLbl.text = city.weapons.ToString();
    //}
    //public void SetDefense() {
    //    _city.SetArmor(System.Int32.Parse(newDefLabel.text));
    //    _city.hexTile.UpdateCityNamePlate();
    //    this._defenseLbl.text = city.armor.ToString();
    //}
    public void SetGovernorLoyaltyAdjustment() {
        _city.governor.loyaltyModifierForTesting = System.Int32.Parse(loyaltyAdjustmentLbl.text);
        SetCity(_city, true, false, true);
    }
    #endregion

}
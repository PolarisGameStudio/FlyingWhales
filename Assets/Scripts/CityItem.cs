﻿using UnityEngine;
using System.Collections;

public class CityItem : MonoBehaviour {

    private City _city;

    [SerializeField] private GameObject governorParentGO;
    [SerializeField] private GameObject hpParentGO;
    [SerializeField] private GameObject cityNameParentGO;
    [SerializeField] private GameObject structuresParentGO;
    [SerializeField] private GameObject growthMeterParentGO;

    [SerializeField] private CharacterPortrait _governor;
    [SerializeField] private UILabel _hpLbl;
    [SerializeField] private UILabel _structuresLbl;
    [SerializeField] private UILabel _cityLbl;
    [SerializeField] private UIProgressBar _hpProgBar;
    [SerializeField] private GameObject _loyaltyGO;
    [SerializeField] private UILabel _loyaltyLbl;
    [SerializeField] private UIEventTrigger _loyaltyEventTrigger;
	[SerializeField] private GameObject _rebelIcon;
    [SerializeField] private UIProgressBar _growthProgBar;

    [Header("For Testing")]
    [SerializeField] private GameObject forTestingGO;
    [SerializeField] private UILabel newPowerLbl;
    [SerializeField] private UILabel newDefLabel;

    #region getters/setters
    public City city {
        get { return this._city; }
    }
	public GameObject rebelIcon {
		get { return this._rebelIcon; }
	}
    #endregion

    public void SetCity(City _city, bool showLoyalty = false, bool showNameOnly = false, bool showForTesting = false) {
        this._city = _city;
        _governor.SetCitizen(city.governor);
        _hpLbl.text = city.hp.ToString();
        _structuresLbl.text = city.ownedTiles.Count.ToString();
        _cityLbl.text = city.name;
		float hpValue = (float)city.hp / (float)city.maxHP;
		if(hpValue > 1f){
			hpValue = 1f;
		}
        _hpProgBar.value = hpValue;
        _growthProgBar.value = (float)city.currentGrowth / (float)city.maxGrowth;

        if (showLoyalty) {
            _loyaltyGO.SetActive(true);
            Governor thisGovernor = (Governor)_governor.citizen.assignedRole;
            _loyaltyLbl.text = thisGovernor.loyalty.ToString();
			thisGovernor._eventLoyaltySummary = string.Empty;
			for (int i = 0; i < thisGovernor.eventModifiers.Count; i++) {
				thisGovernor._eventLoyaltySummary += "\n" + thisGovernor.eventModifiers [i].summary;
			}
            string loyaltySummary = thisGovernor.loyaltySummary;
            if(thisGovernor.ownedCity.kingdom.disloyaltyFromPrestige > 0) {
                loyaltySummary += "\n-" + thisGovernor.ownedCity.kingdom.disloyaltyFromPrestige.ToString() + " Lack of Prestige";
            }
            EventDelegate.Set(_loyaltyEventTrigger.onHoverOver, delegate () {
                UIManager.Instance.ShowRelationshipSummary(thisGovernor.citizen, loyaltySummary);
            });
            EventDelegate.Set(_loyaltyEventTrigger.onHoverOut, delegate () { UIManager.Instance.HideRelationshipSummary(); });
        }

        if (showNameOnly) {
            governorParentGO.SetActive(false);
            hpParentGO.SetActive(false);
            cityNameParentGO.SetActive(true);
            structuresParentGO.SetActive(false);
            growthMeterParentGO.SetActive(false);
        } else {
            governorParentGO.SetActive(true);
            hpParentGO.SetActive(true);
            cityNameParentGO.SetActive(true);
            structuresParentGO.SetActive(true);
            growthMeterParentGO.SetActive(true);
        }

        if (showForTesting) {
            forTestingGO.SetActive(true);
            newPowerLbl.text = _city.power.ToString();
            newDefLabel.text = _city.defense.ToString();
        } else {
            forTestingGO.SetActive(false);
            
        }
    }

    public void CenterOnCity() {
        CameraMove.Instance.CenterCameraOn(_city.hexTile.gameObject);
    }

    public void SetKingdomAsSelected() {
        //UIManager.Instance.SetKingdomAsSelected(_city.kingdom);
    }

    #region For Testing
    public void SetPower() {
        _city.SetPower(System.Int32.Parse(newPowerLbl.text));
    }
    public void SetDefense() {
        _city.SetDefense(System.Int32.Parse(newDefLabel.text));
    }
    #endregion

}

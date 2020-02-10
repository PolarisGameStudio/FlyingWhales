﻿
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Traits;
using UnityEngine;
using UtilityScripts;

public class TokenManager : MonoBehaviour {
    public static TokenManager Instance;

    public List<SpecialTokenSettings> specialTokenSettings;

    [SerializeField] private ItemSpriteDictionary itemSpritesDictionary;
    public List<SpecialObject> specialObjects { get; private set; }
    public List<SpecialToken> specialTokens { get; private set; }

    public Dictionary<SPECIAL_TOKEN, ItemData> itemData { get; private set; }

    void Awake() {
        Instance = this;

        //TODO: move this somewhere safer
        specialObjects = new List<SpecialObject>();
        specialTokens = new List<SpecialToken>();
    }

    public void Initialize() {
        ConstructItemData();
        //LoadSpecialTokens();
    }

    public void LoadSpecialTokens(Settlement settlement) {
        ////Reference: https://trello.com/c/Kuqt3ZSP/2610-put-2-healing-potions-in-the-warehouse-at-start-of-the-game
        LocationStructure mainStorage = settlement.mainStorage;
        for (int i = 0; i < 4; i++) {
            mainStorage.AddItem(CreateSpecialToken(SPECIAL_TOKEN.HEALING_POTION));
        }
        for (int i = 0; i < 2; i++) {
            mainStorage.AddItem(CreateSpecialToken(SPECIAL_TOKEN.TOOL));
        }

        // for (int i = 0; i < specialTokenSettings.Count; i++) {
        //     SpecialTokenSettings currSetting = specialTokenSettings[i];
        //     List<Settlement> areas = LandmarkManager.Instance.allNonPlayerAreas;
        //     for (int j = 0; j < currSetting.quantity; j++) {
        //         if (UnityEngine.Random.Range(0, 100) < currSetting.appearanceWeight) {
        //             Settlement chosenArea = areas[UnityEngine.Random.Range(0, areas.Count)];
        //             SpecialToken createdToken = CreateSpecialToken(currSetting.tokenType, currSetting.appearanceWeight);
        //             if (createdToken != null) {
        //                 chosenArea.AddSpecialTokenToLocation(createdToken);
        //                 //createdToken.SetOwner(chosenArea.owner); //Removed this because of redundancy, SetOwner is already being called inside AddSpecialTokenToLocation
        //                 //Messenger.Broadcast<SpecialToken>(Signals.SPECIAL_TOKEN_CREATED, createdToken);
        //             }
        //         }
        //     }
        // }
    }
    public SpecialToken CreateRandomDroppableSpecialToken() {
        SPECIAL_TOKEN[] choices = CollectionUtilities.GetEnumValues<SPECIAL_TOKEN>().Where(x => x.CreatesObjectWhenDropped()).ToArray();
        SPECIAL_TOKEN random = choices[UnityEngine.Random.Range(0, choices.Length)];
        return CreateSpecialToken(random);
    }
    public SpecialToken CreateSpecialToken(SPECIAL_TOKEN tokenType, int appearanceWeight = 0) {
        SpecialToken createdToken;
        switch (tokenType) {
            case SPECIAL_TOKEN.BLIGHTED_POTION:
                createdToken = new BlightedPotion();
                break;
            case SPECIAL_TOKEN.BOOK_OF_THE_DEAD:
                createdToken = new BookOfTheDead();
                break;
            case SPECIAL_TOKEN.CHARM_SPELL:
                createdToken = new CharmSpell();
                break;
            case SPECIAL_TOKEN.FEAR_SPELL:
                createdToken = new FearSpell();
                break;
            case SPECIAL_TOKEN.MARK_OF_THE_WITCH:
                createdToken = new MarkOfTheWitch();
                break;
            case SPECIAL_TOKEN.BRAND_OF_THE_BEASTMASTER:
                createdToken = new BrandOfTheBeastmaster();
                break;
            case SPECIAL_TOKEN.BOOK_OF_WIZARDRY:
                createdToken = new BookOfWizardry();
                break;
            case SPECIAL_TOKEN.SECRET_SCROLL:
                createdToken = new SecretScroll();
                break;
            case SPECIAL_TOKEN.MUTAGENIC_GOO:
                createdToken = new MutagenicGoo();
                break;
            case SPECIAL_TOKEN.DISPEL_SCROLL:
                createdToken = new DispelScroll();
                break;
            case SPECIAL_TOKEN.PANACEA:
                createdToken = new Panacea();
                break;
            case SPECIAL_TOKEN.ENCHANTED_AMULET:
                createdToken = new EnchantedAmulet();
                break;
            case SPECIAL_TOKEN.GOLDEN_NECTAR:
                createdToken = new GoldenNectar();
                break;
            case SPECIAL_TOKEN.SCROLL_OF_POWER:
                createdToken = new ScrollOfPower();
                break;
            case SPECIAL_TOKEN.ACID_FLASK:
                createdToken = new AcidFlask();
                break;
            case SPECIAL_TOKEN.HEALING_POTION:
                createdToken = new HealingPotion();
                break;
            case SPECIAL_TOKEN.WATER_BUCKET:
                createdToken = new WaterBucket();
                break;
            default:
                createdToken = new SpecialToken(tokenType, appearanceWeight);
                break;
        }
        return createdToken;
    }
    public SpecialTokenSettings GetTokenSettings(SPECIAL_TOKEN tokenType) {
        for (int i = 0; i < specialTokenSettings.Count; i++) {
            if(specialTokenSettings[i].tokenType == tokenType) {
                return specialTokenSettings[i];
            }
        }
        return null;
    }
    public List<Settlement> GetPossibleAreaSpawns(SpecialTokenSettings setting) {
        List<Settlement> areas = new List<Settlement>();
        for (int i = 0; i < setting.areaLocations.Count; i++) {
            string areaName = setting.areaLocations[i];
            Settlement settlement = LandmarkManager.Instance.GetAreaByName(areaName);
            if (settlement == null) {
                //throw new System.Exception("There is no settlement named " + areaName);
            } else {
                areas.Add(settlement);
            }
        }
        return areas;
    }
    public Sprite GetItemSprite(SPECIAL_TOKEN tokenType) {
        if (itemSpritesDictionary.ContainsKey(tokenType)) {
            return itemSpritesDictionary[tokenType];
        }
        return null;
    }
    public void AddSpecialObject(SpecialObject obj) {
        specialObjects.Add(obj);
    }
    public SpecialObject GetSpecialObjectByID(int id) {
        for (int i = 0; i < specialObjects.Count; i++) {
            if(specialObjects[i].id == id) {
                return specialObjects[i];
            }
        }
        return null;
    }
    public void AddSpecialToken(SpecialToken token) {
        specialTokens.Add(token);
    }
    public SpecialToken GetSpecialTokenByID(int id) {
        for (int i = 0; i < specialTokens.Count; i++) {
            if (specialTokens[i].id == id) {
                return specialTokens[i];
            }
        }
        return null;
    }
    private void ConstructItemData() {
        itemData = new Dictionary<SPECIAL_TOKEN, ItemData>() {
            {SPECIAL_TOKEN.TOOL, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new string[] { "Builder" } } },
            {SPECIAL_TOKEN.BLIGHTED_POTION, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new string[] { "Builder" } } },
            {SPECIAL_TOKEN.BOOK_OF_THE_DEAD, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new string[] { "Builder" } } },
            {SPECIAL_TOKEN.CHARM_SPELL, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new string[] { "Builder" } } },
            {SPECIAL_TOKEN.FEAR_SPELL, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new string[] { "Builder" } } },
            {SPECIAL_TOKEN.MARK_OF_THE_WITCH, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new string[] { "Builder" } } },
            {SPECIAL_TOKEN.BRAND_OF_THE_BEASTMASTER, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new string[] { "Builder" } } },
            {SPECIAL_TOKEN.BOOK_OF_WIZARDRY, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new string[] { "Builder" } } },
            {SPECIAL_TOKEN.SECRET_SCROLL, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new string[] { "Builder" } } },
            {SPECIAL_TOKEN.MUTAGENIC_GOO, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new string[] { "Builder" } } },
            {SPECIAL_TOKEN.DISPEL_SCROLL, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new string[] { "Builder" } } },
            {SPECIAL_TOKEN.PANACEA, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new string[] { "Builder" } } },
            {SPECIAL_TOKEN.JUNK, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new string[] { "Builder" } } },
            {SPECIAL_TOKEN.HEALING_POTION, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new string[] { "Healer", "Herbalist" } } },
            {SPECIAL_TOKEN.ENCHANTED_AMULET, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new string[] { "Builder" } } },
            {SPECIAL_TOKEN.GOLDEN_NECTAR, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new string[] { "Builder" } } },
            {SPECIAL_TOKEN.SCROLL_OF_POWER, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new string[] { "Builder" } } },
            {SPECIAL_TOKEN.ACID_FLASK, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new string[] { "Builder" } } },
            {SPECIAL_TOKEN.SCROLL_OF_FRENZY, new ItemData(){
                supplyValue = 15,
                craftCost = 25,
                purchaseCost = 35,
                canBeCraftedBy = new string[] { "Builder" } } },
        };
    }
}

[System.Serializable]
public class SpecialTokenSettings {
    public string tokenName;
    public SPECIAL_TOKEN tokenType;
    public int quantity;
    public int appearanceWeight;
    public List<string> areaLocations;
}

public struct ItemData {
    public int supplyValue;
    public int craftCost;
    public int purchaseCost;
    public string[] canBeCraftedBy;
}
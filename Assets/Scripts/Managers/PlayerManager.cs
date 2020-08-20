﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Serialization;
using Traits;
using Archetype;
using Locations.Settlements;
using UnityEngine.Assertions;

public class PlayerManager : MonoBehaviour {
    public static PlayerManager Instance;
    public Player player;

    public COMBAT_ABILITY[] allCombatAbilities;

    [Header("Job Action Icons")]
    [FormerlySerializedAs("jobActionIcons")] [SerializeField] private StringSpriteDictionary spellIcons;

    [Header("Combat Ability Icons")]
    [SerializeField] private StringSpriteDictionary combatAbilityIcons;
    
    [Header("Intervention Ability Tiers")]
    [FormerlySerializedAs("interventionAbilityTiers")] [SerializeField] private InterventionAbilityTierDictionary spellTiers;

    [Header("Chaos Orbs")] 
    [SerializeField] private GameObject chaosOrbPrefab;

    private bool _hasWinCheckTimer;

    private void Awake() {
        Instance = this;
    }
    public void Initialize() {
        availableChaosOrbs = new List<ChaosOrb>();
        Messenger.AddListener<InfoUIBase>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<InfoUIBase>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
        Messenger.AddListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
        Messenger.AddListener<Vector3, int, InnerTileMap>(Signals.CREATE_CHAOS_ORBS, CreateChaosOrbsAt);
        Messenger.AddListener<Character, ActualGoapNode>(Signals.CHARACTER_DID_ACTION_SUCCESSFULLY, OnCharacterDidActionSuccess);
        Messenger.AddListener(Signals.CHECK_IF_PLAYER_WINS, CheckWinCondition);
    }
    public void InitializePlayer(HexTile portal) {
        player = new Player();
        player.CreatePlayerFaction();
        player.SetPortalTile(portal);
        PlayerSettlement existingPlayerNpcSettlement = portal.settlementOnTile as PlayerSettlement;
        Assert.IsNotNull(existingPlayerNpcSettlement, $"Portal does not have a player settlement on its tile");
        player.SetPlayerArea(existingPlayerNpcSettlement);
        
        LandmarkManager.Instance.OwnSettlement(player.playerFaction, existingPlayerNpcSettlement);

        PlayerUI.Instance.UpdateUI();
    }
    public void InitializePlayer(SaveDataCurrentProgress data) {
        player = data.LoadPlayer();
        player.CreatePlayerFaction(data.playerSave);
        PlayerUI.Instance.UpdateUI();
        // if (WorldConfigManager.Instance.isDemoWorld) {
        //     player.LoadPlayerData(SaveManager.Instance.currentSaveDataPlayer);    
        // }

    }
    //public void InitializePlayer(SaveDataPlayer data) {
    //    player = new Player(data);
    //    player.CreatePlayerFaction(data);
    //    // NPCSettlement existingPlayerNpcSettlement = LandmarkManager.Instance.GetAreaByID(data.playerAreaID);
    //    // player.SetPlayerArea(existingPlayerNpcSettlement);
    //    //PlayerUI.Instance.UpdateUI();
    //    //PlayerUI.Instance.InitializeThreatMeter();
    //    //PlayerUI.Instance.UpdateThreatMeter();

    //    for (int i = 0; i < data.minions.Count; i++) {
    //        data.minions[i].Load(player);
    //    }
    //    //for (int i = 0; i < data.summonSlots.Count; i++) {
    //    //    Summon summon = CharacterManager.Instance.GetCharacterByID(data.summonIDs[i]) as Summon;
    //    //    player.GainSummon(summon);
    //    //}
    //    //for (int i = 0; i < data.artifacts.Count; i++) {
    //    //    data.artifacts[i].Load(player);
    //    //}
    //    //for (int i = 0; i < data.interventionAbilities.Count; i++) {
    //    //    data.interventionAbilities[i].Load(player);
    //    //}
    //    for (int i = 0; i < player.minions.Count; i++) {
    //        if(player.minions[i].character.id == data.currentMinionLeaderID) {
    //            player.SetMinionLeader(player.minions[i]);
    //        }
    //    }
    //    //player.SetPlayerTargetFaction(LandmarkManager.Instance.enemyOfPlayerArea.owner);
    //}
    public int GetManaCostForSpell(int tier) {
        if (tier == 1) {
            return 150;
        } else if (tier == 2) {
            return 100;
        } else {
            return 50;
        }
    }

    #region Utilities
    public Sprite GetJobActionSprite(string actionName) {
        if (spellIcons.ContainsKey(actionName)) {
            return spellIcons[actionName];
        }
        return null;
    }
    public Sprite GetCombatAbilitySprite(string abilityName) {
        if (combatAbilityIcons.ContainsKey(abilityName)) {
            return combatAbilityIcons[abilityName];
        }
        return null;
    }
    #endregion

    #region Intervention Ability
    public PlayerSpell CreateNewInterventionAbility(SPELL_TYPE abilityType) {
        switch (abilityType) {
            //case SPELL_TYPE.ABDUCT:
            //    return new Abduct();
            //case SPELL_TYPE.ACCESS_MEMORIES:
            //    return new AccessMemories();
            //case SPELL_TYPE.DESTROY:
            //    return new Destroy();
            //case SPELL_TYPE.DISABLE:
            //    return new Disable();
            //case SPELL_TYPE.ENRAGE:
            //    return new Enrage();
            case SPELL_TYPE.KLEPTOMANIA:
                return new Kleptomania();
            case SPELL_TYPE.LYCANTHROPY:
                return new Lycanthropy();
            case SPELL_TYPE.UNFAITHFULNESS:
                return new Unfaithfulness();
            case SPELL_TYPE.VAMPIRISM:
                return new Vampirism();
            //case SPELL_TYPE.JOLT:
            //    return new Jolt();
            //case SPELL_TYPE.PROVOKE:
            //    return new Provoke();
            //case SPELL_TYPE.RAISE_DEAD:
            //    return new RaiseDead();
            //case INTERVENTION_ABILITY.SHARE_INTEL:
            //    return new ShareIntel();
            //case SPELL_TYPE.SPOOK:
            //    return new Spook();
            //case SPELL_TYPE.ZAP:
            //    return new Zap();
            case SPELL_TYPE.CANNIBALISM:
                return new Cannibalism();
            //case SPELL_TYPE.CLOAK_OF_INVISIBILITY:
            //    return new CloakOfInvisibility();
            //case SPELL_TYPE.LURE:
            //    return new Lure();
            case SPELL_TYPE.METEOR:
                return new Meteor();
            //case SPELL_TYPE.IGNITE:
            //    return new Ignite();
            case SPELL_TYPE.CURSED_OBJECT:
                return new CursedObject();
            //case SPELL_TYPE.SPOIL:
            //    return new Spoil();
            case SPELL_TYPE.ALCOHOLIC:
                return new Alcoholic();
            //case SPELL_TYPE.LULLABY:
            //    return new Lullaby();
            case SPELL_TYPE.PLAGUE:
                return new Plagued();
            case SPELL_TYPE.AGORAPHOBIA:
                return new Agoraphobia();
            case SPELL_TYPE.PARALYSIS:
                return new Paralysis();
            //case SPELL_TYPE.RELEASE:
            //    return new Release();
            case SPELL_TYPE.ZOMBIE_VIRUS:
                return new ZombieVirus();
            case SPELL_TYPE.PSYCHOPATHY:
                return new Psychopathy();
            case SPELL_TYPE.TORNADO:
                return new Tornado();
        }
        return null;
    }
    public int GetSpellTier(SPELL_TYPE abilityType) {
        if (spellTiers.ContainsKey(abilityType)) {
            return spellTiers[abilityType];
        }
        return 3;
    }
    #endregion

    #region Combat Ability
    public CombatAbility CreateNewCombatAbility(COMBAT_ABILITY abilityType) {
        switch (abilityType) {
            case COMBAT_ABILITY.SINGLE_HEAL:
                return new SingleHeal();
            case COMBAT_ABILITY.FLAMESTRIKE:
                return new Flamestrike();
            case COMBAT_ABILITY.FEAR_SPELL:
                return new FearSpellAbility();
            case COMBAT_ABILITY.SACRIFICE:
                return new Sacrifice();
            case COMBAT_ABILITY.TAUNT:
                return new Taunt();
        }
        return null;
    }
    #endregion

    #region Unit Selection
    private List<Character> selectedUnits = new List<Character>();
    public void SelectUnit(Character character) {
        if (!selectedUnits.Contains(character)) {
            selectedUnits.Add(character);
        }
    }
    public void DeselectUnit(Character character) {
        if (selectedUnits.Remove(character)) {

        }
    }
    public void DeselectAllUnits() {
        Character[] units = selectedUnits.ToArray();
        for (int i = 0; i < units.Length; i++) {
            DeselectUnit(units[i]);
        }
    }
    private void OnMenuOpened(InfoUIBase @base) {
        if (@base is CharacterInfoUI) {
            DeselectAllUnits();
            CharacterInfoUI infoUi = @base as CharacterInfoUI;
            SelectUnit(infoUi.activeCharacter);
            //if (infoUI.activeCharacter.CanBeInstructedByPlayer()) {
            //    SelectUnit(infoUI.activeCharacter);
            //}
        }
    }
    private void OnMenuClosed(InfoUIBase @base) {
        if (@base is CharacterInfoUI) {
            DeselectAllUnits();
        }
    }
    // private void OnKeyPressedDown(KeyCode keyCode) {
    //     if (selectedUnits.Count > 0) {
    //         if (keyCode == KeyCode.Mouse1) {
    //             //right click
    //             for (int i = 0; i < selectedUnits.Count; i++) {
    //                 Character character = selectedUnits[i];
    //                 if (!character.CanBeInstructedByPlayer()) {
    //                     continue;
    //                 }
    //                 IPointOfInterest hoveredPOI = InnerMapManager.Instance.currentlyHoveredPoi;
    //                 character.StopCurrentActionNode(false, "Stopped by the player");
    //                 if (character.stateComponent.currentState != null) {
    //                     character.stateComponent.ExitCurrentState();
    //                 }
    //                 character.combatComponent.ClearHostilesInRange();
    //                 character.combatComponent.ClearAvoidInRange();
    //                 character.SetIsFollowingPlayerInstruction(false); //need to reset before giving commands
    //                 if (hoveredPOI is Character) {
    //                     Character target = hoveredPOI as Character;
    //                     if (character.IsHostileWith(target) && character.IsCombatReady()) {
    //                         character.combatComponent.Fight(target);
    //                         character.combatComponent.AddOnProcessCombatAction((combatState) => combatState.SetForcedTarget(target));
    //                         //CombatState cs = character.stateComponent.currentState as CombatState;
    //                         //if (cs != null) {
    //                         //    cs.SetForcedTarget(target);
    //                         //} else {
    //                         //    throw new System.Exception(character.name + " was instructed to attack " + target.name + " but did not enter combat state!");
    //                         //}
    //                     } else {
    //                         Debug.Log(character.name + " is not combat ready or is not hostile with " + target.name + ". Ignoring command.");
    //                     }
    //                 } else {
    //                     character.marker.GoTo(InnerMapManager.Instance.currentlyShowingMap.worldUiCanvas.worldCamera.ScreenToWorldPoint(Input.mousePosition), () => OnFinishInstructionFromPlayer(character));
    //                 }
    //                 character.SetIsFollowingPlayerInstruction(true);
    //             }
    //         } else if (keyCode == KeyCode.Mouse0) {
    //             DeselectAllUnits();
    //         }
    //     }
    // }
    private void OnFinishInstructionFromPlayer(Character character) {
        character.SetIsFollowingPlayerInstruction(false);
    }
    #endregion

    #region Chaos Orbs
    public List<ChaosOrb> availableChaosOrbs;
    private void CreateChaosOrbsAt(Vector3 worldPos, int amount, InnerTileMap mapLocation) {
        StartCoroutine(ChaosOrbCreationCoroutine(worldPos, amount, mapLocation));
    }
    private IEnumerator ChaosOrbCreationCoroutine(Vector3 worldPos, int amount, InnerTileMap mapLocation) {
        for (int i = 0; i < amount; i++) {
            GameObject chaosOrbGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(chaosOrbPrefab.name, Vector3.zero, 
                Quaternion.identity, mapLocation.objectsParent);
            chaosOrbGO.transform.position = worldPos;
            ChaosOrb chaosOrb = chaosOrbGO.GetComponent<ChaosOrb>();
            chaosOrb.Initialize(mapLocation.region);
            AddAvailableChaosOrb(chaosOrb);
            yield return null;
        }
        Debug.Log($"Created {amount.ToString()} chaos orbs at {mapLocation.region.name}. Position {worldPos.ToString()}");
    }
    private void OnCharacterDidActionSuccess(Character character, ActualGoapNode actionNode) {
        if (character.isNormalCharacter) {
            CRIME_SEVERITY crimeType = CrimeManager.Instance.GetCrimeTypeConsideringAction(actionNode);
            if (crimeType != CRIME_SEVERITY.None) {
                int orbsToCreate;
                switch (crimeType) {
                    case CRIME_SEVERITY.Infraction:
                        orbsToCreate = 2;
                        break;
                    case CRIME_SEVERITY.Misdemeanor:
                        orbsToCreate = 2;
                        break;
                    case CRIME_SEVERITY.Serious:
                        orbsToCreate = 3;
                        break;
                    case CRIME_SEVERITY.Heinous:
                        orbsToCreate = 3;
                        break;
                    default:
                        orbsToCreate = 0;
                        break;
                }
                if(orbsToCreate != 0) {
                    character.logComponent.PrintLogIfActive($"{character.name} performed a crime of type {crimeType.ToString()}. Expelling {orbsToCreate.ToString()} Chaos Orbs.");
                    Messenger.Broadcast(Signals.CREATE_CHAOS_ORBS, character.marker.transform.position, orbsToCreate, character.currentRegion.innerMap);
                }
            }    
        }
    }
    private void AddAvailableChaosOrb(ChaosOrb chaosOrb) {
        availableChaosOrbs.Add(chaosOrb);
        Messenger.Broadcast(Signals.CHAOS_ORB_SPAWNED);
    }
    public void RemoveChaosOrbFromAvailability(ChaosOrb chaosOrb) {
        availableChaosOrbs.Remove(chaosOrb);
        Messenger.Broadcast(Signals.CHAOS_ORB_DESPAWNED);
    }
    #endregion

    #region Archetypes
    public static PlayerArchetype CreateNewArchetype(PLAYER_ARCHETYPE archetype) {
        string typeName = $"Archetype.{ UtilityScripts.Utilities.NotNormalizedConversionEnumToStringNoSpaces(archetype.ToString()) }, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        System.Type type = System.Type.GetType(typeName);
        if (type != null) {
            PlayerArchetype obj = System.Activator.CreateInstance(type) as PlayerArchetype;
            return obj;
        }
        throw new System.Exception($"Could not create new archetype {archetype} because there is no data for it!");
    }
    #endregion

    #region End Game Mechanics
    private void OnCharacterDied(Character character) {
        CheckWinCondition();
    }
    private void OnCharacterCanNoLongerPerform(Character character) {
        //CheckWinCondition();
    }
    private void OnCharacterCanNoLongerMove(Character character) {
        //CheckWinCondition();
    }
    private void CheckWinCondition() {
        if (DoesPlayerWin()) {
            if (!_hasWinCheckTimer) {
                CreateWinCheckTimer();
            }
        }
    }
    private void FinalCheckWinCondition() {
        if (DoesPlayerWin()) {
            //player.SaveSummons();
            player.SaveTileObjects();
            PlayerUI.Instance.WinGameOver();
        }
        _hasWinCheckTimer = false;
    }
    private bool DoesPlayerWin() {
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character character = CharacterManager.Instance.allCharacters[i];
            if(character.faction.isMajorNonPlayerFriendlyNeutral && character.isNormalCharacter && !character.isAlliedWithPlayer) {
                if(!character.isDead) {
                    return false;
                }
            }
        }
        //check limbo characters
        for (int i = 0; i < CharacterManager.Instance.limboCharacters.Count; i++) {
            Character character = CharacterManager.Instance.limboCharacters[i];
            if(character.faction.isMajorNonPlayerFriendlyNeutral && character.isNormalCharacter && !character.isAlliedWithPlayer) {
                if(!character.isDead) {
                    return false;
                }
            }
        }
        return true;
    }
    private void CreateWinCheckTimer() {
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnMinutes(15));
        SchedulingManager.Instance.AddEntry(dueDate, FinalCheckWinCondition, this);
        _hasWinCheckTimer = true;
    }
    #endregion
}

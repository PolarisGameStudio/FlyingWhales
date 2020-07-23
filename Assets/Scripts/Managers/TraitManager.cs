﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Linq;
using Traits;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UtilityScripts;

public class TraitManager : MonoBehaviour {
    public static TraitManager Instance;

    private Dictionary<string, Trait> _allTraits;

    //Trait Override Function Identifiers
    public const string Collision_Trait = "Collision_Trait";
    public const string Enter_Grid_Tile_Trait = "Enter_Grid_Tile_Trait";
    public const string Initiate_Map_Visual_Trait = "Initiate_Map_Visual_Trait";
    public const string Destroy_Map_Visual_Trait = "Destroy_Map_Visual_Trait";
    public const string Execute_Pre_Effect_Trait = "Execute_Pre_Effect_Trait";
    public const string Execute_Per_Tick_Effect_Trait = "Execute_Pre_Effect_Trait";
    public const string Execute_After_Effect_Trait = "Execute_Pre_Effect_Trait";
    public const string Execute_Expected_Effect_Trait = "Execute_Expected_Effect_Trait";
    public const string Start_Perform_Trait = "Start_Perform_Trait";
    public const string Death_Trait = "Death_Trait";
    public const string Tick_Ended_Trait = "Tick_Ended_Trait";
    public const string Tick_Started_Trait = "Tick_Started_Trait";
    public const string Hour_Started_Trait = "Hour_Started_Trait";
    public const string See_Poi_Trait = "See_Poi_Trait";
    public const string See_Poi_Cannot_Witness_Trait = "See_Poi_Cannot_Witness_Trait";

    public static string[] instancedTraits = new string[] {
        "Restrained", "Cursed", "Injured", "Kleptomaniac", "Lycanthrope", "Vampiric",
        "Poisoned", "Resting", "Sick", "Unconscious", "Zapped", "Spooked", "Cannibal", "Lethargic",
        "Dead", "Unfaithful", "Drunk", "Burning", "Burnt", "Agoraphobic", "Infected", "Music Lover", "Music Hater", 
        "Psychopath", "Plagued", "Vigilant", "Diplomatic", "Wet", "Character Trait", "Nocturnal", "Glutton", 
        "Suspicious", "Narcoleptic", "Hothead", "Inspiring", "Pyrophobic", "Angry", "Alcoholic", "Pessimist", "Lazy", 
        "Coward", "Berserked", "Catatonic", "Griefstricken", "Heartbroken", "Chaste", "Lustful", "Edible", "Paralyzed", 
        "Malnourished", "Withdrawal", "Suicidal", "Criminal", "Dazed", "Hiding", "Bored", "Overheating",
        "Freezing", "Frozen", "Ravenous", "Feeble", "Forlorn", "Accident Prone", "Disoriented", "Consumable",
        "Fire Prone", "Electric", "Venomous", "Booby Trapped", "Betrayed", "Abomination Germ", "Ensnared", "Melting",
        "Fervor", "Tended", "Tending", "Cleansing", "Dousing", "Drying", "Patrolling", "Necromancer", "Mining", 
        "Webbed", "Cultist", "Stealthy", "Invisible", "Noxious Wanderer", "DeMooder", "Defender", "Invader", "Disabler", "Infestor",
        "Abductor", "Arsonist", "Hibernating", "Baby Infestor", "Tower", "Mighty"
    };
    [FormerlySerializedAs("traitIconDictionary")] [SerializeField] private StringSpriteDictionary traitPortraitDictionary;
    [SerializeField] private StringSpriteDictionary traitIconDictionary;
    public GameObject traitIconPrefab;
    
    //Trait Processors
    public static TraitProcessor characterTraitProcessor;
    public static TraitProcessor tileObjectTraitProcessor;
    public static TraitProcessor defaultTraitProcessor;
    
    //list of traits that a character can gain on their own
    private readonly string[] traitPool = new string[] { "Vigilant", "Diplomatic",
        "Fireproof", "Accident Prone", "Unfaithful", "Alcoholic", "Music Lover", "Music Hater", "Unattractive", "Nocturnal",
        "Optimist", "Pessimist", "Fast", "Chaste", "Lustful", "Coward", "Lazy", "Glutton", "Robust", "Suspicious" , "Inspiring", "Pyrophobic",
        "Narcoleptic", "Hothead", "Evil", "Treacherous", "Ambitious", "Authoritative", "Fire Prone", "Blessed", "Persuasive", //, "Electric", "Venomous"
    };
    public List<string> buffTraitPool { get; private set; }
    public List<string> flawTraitPool { get; private set; }
    public List<string> neutralTraitPool { get; private set; }

    public List<string> removeStatusTraits = new List<string> {
        "Unconscious", "Injured", "Poisoned", "Plagued",
        "Infected", "Cursed", "Freezing", "Frozen", "Burning",
    };
    public List<string> specialIllnessTraits = new List<string> {
        "Poisoned", "Plagued", "Infected"
    };

    //This is for instanced traits that do not have unique data
    //In order to save some memory all instanced traits that do not have unique data must be stored here, they are identified by isSingleton
    //If isSingleton returns true, they are stored here, and will share only 1 instance of the trait class
    private Dictionary<string, Trait> instancedSingletonTraits;

    #region getters/setters
    public Dictionary<string, Trait> allTraits {
        get { return _allTraits; }
    }
    #endregion

    void Awake() {
        Instance = this;
        CreateTraitProcessors();
    }

    public void Initialize() {
        instancedSingletonTraits = new Dictionary<string, Trait>();
        _allTraits = new Dictionary<string, Trait>();
        string path = $"{UtilityScripts.Utilities.dataPath}Traits/";
        string[] files = Directory.GetFiles(path, "*.json");
        for (int i = 0; i < files.Length; i++) {
            Trait trait = JsonUtility.FromJson<Trait>(System.IO.File.ReadAllText(files[i]));
            if (trait.type == TRAIT_TYPE.STATUS) {
                trait = JsonUtility.FromJson<Status>(System.IO.File.ReadAllText(files[i]));
            }
            _allTraits.Add(trait.name, trait);
        }
        
        AddInstancedTraits(); //Traits with their own classes
        
        buffTraitPool = new List<string>();
        flawTraitPool = new List<string>();
        neutralTraitPool = new List<string>();
        
        //Categorize traits from trait pool
        for (int i = 0; i < traitPool.Length; i++) {
            string currTraitName = traitPool[i];
            if (allTraits.ContainsKey(currTraitName)) {
                Trait trait = allTraits[currTraitName];
                if (trait.type == TRAIT_TYPE.BUFF) {
                    buffTraitPool.Add(currTraitName);
                } else if (trait.type == TRAIT_TYPE.FLAW) {
                    flawTraitPool.Add(currTraitName);
                } else {
                    neutralTraitPool.Add(currTraitName);
                }
            } else {
                throw new Exception($"There is no trait named: {currTraitName}");
            }
        }
    }

    #region Utilities
    private void AddInstancedTraits() {
        //TODO: REDO INSTANCED TRAITS, USE SCRIPTABLE OBJECTS for FIXED DATA
        for (int i = 0; i < instancedTraits.Length; i++) {
            string traitName = instancedTraits[i];
            Trait trait = CreateNewInstancedTraitClass(traitName);
            _allTraits.Add(traitName, trait);
        }
    }
    public Sprite GetTraitPortrait(string traitName) {
        return traitPortraitDictionary.ContainsKey(traitName) ? traitPortraitDictionary[traitName] 
            : traitPortraitDictionary.Values.First();
    }
    public Sprite GetTraitIcon(string traitName) {
        return traitIconDictionary.ContainsKey(traitName) ? traitIconDictionary[traitName] 
            : traitIconDictionary.Values.First();
    }
    public bool HasTraitIcon(string traitName) {
        return traitPortraitDictionary.ContainsKey(traitName);
    }
    public bool IsInstancedTrait(string traitName) {
        for (int i = 0; i < instancedTraits.Length; i++) {
            if (string.Equals(instancedTraits[i], traitName, StringComparison.OrdinalIgnoreCase)) { //|| string.Equals(currTrait.GetType().ToString(), traitName, StringComparison.OrdinalIgnoreCase)
                return true;
            }
        }
        return false;
    }
    public bool IsTraitElemental(string traitName) {
        return traitName == "Burning" || traitName == "Freezing" || traitName == "Poisoned" || traitName == "Wet" || traitName == "Zapped" || traitName == "Overheating";
    }
    public Trait CreateNewInstancedTraitClass(string traitName) {
        if (instancedSingletonTraits.ContainsKey(traitName)) {
            return instancedSingletonTraits[traitName];
        } else {
            string noSpacesTraitName = UtilityScripts.Utilities.RemoveAllWhiteSpace(traitName);
            string typeName = $"Traits.{ noSpacesTraitName }, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            Type type = System.Type.GetType(typeName);
            Assert.IsNotNull(type, $"No instanced trait with type, {typeName}");
            Trait trait = System.Activator.CreateInstance(type) as Trait;
            if (trait.isSingleton) {
                instancedSingletonTraits.Add(traitName, trait);
            }
            return trait;
        }
    }
    public List<Trait> GetAllTraitsOfType(TRAIT_TYPE type) {
        List<Trait> traits = new List<Trait>();
        foreach (Trait trait in _allTraits.Values) {
            if(trait.type == type) {
                traits.Add(trait);
            }
        }
        return traits;
    }
    public List<string> GetAllBuffTraitsThatCharacterCanHave(Character character) {
        List<string> allBuffs = new List<string>(buffTraitPool);
        for (int i = 0; i < character.traitContainer.traits.Count; i++) {
            Trait trait = character.traitContainer.traits[i];
            if (trait.mutuallyExclusive != null) {
                allBuffs = CollectionUtilities.RemoveElements(ref allBuffs, trait.mutuallyExclusive);
            }
        }
        return allBuffs;
    }
    /// <summary>
    /// Utility function to determine if this character's flaws can still be activated
    /// </summary>
    /// <returns></returns>
    public bool CanStillTriggerFlaws(Character character) {
        if (character.isDead || character.faction.isPlayerFaction || UtilityScripts.GameUtilities.IsRaceBeast(character.race) || character is Summon 
            || character.returnedToLife) {
            return false;
        }
        //if(doNotDisturb > 0) {
        //    return false;
        //}
        return true;
    }
    public void CopyTraitOrStatus(Trait trait, ITraitable from, ITraitable to) {
        if (from.traitContainer.HasTrait(trait.name)) {
            int numOfStacks = 1;
            if (from.traitContainer.stacks.ContainsKey(trait.name)) {
                numOfStacks = from.traitContainer.stacks[trait.name];    
            }
            //In the loop, override duration to zero so that it will not reset the trait's timer
            Trait duplicateTrait = null;
            for (int i = 0; i < numOfStacks; i++) {
                to.traitContainer.AddTrait(to, trait.name, out duplicateTrait, overrideDuration: 0, bypassElementalChance: true);
            }
            if(duplicateTrait != null) {
                //Copy the trait's responsible characters and gainedFromDoing
                if(trait.responsibleCharacters != null && trait.responsibleCharacters.Count > 0) {
                    for (int i = 0; i < trait.responsibleCharacters.Count; i++) {
                        duplicateTrait.AddCharacterResponsibleForTrait(trait.responsibleCharacters[i]);
                    }
                }
                duplicateTrait.SetGainedFromDoing(trait.gainedFromDoing);

                //Copy the trait's timer
                if (from.traitContainer.scheduleTickets.ContainsKey(trait.name)) {
                    List<TraitRemoveSchedule> traitRemoveSchedules = from.traitContainer.scheduleTickets[trait.name];
                    for (int i = 0; i < traitRemoveSchedules.Count; i++) {
                        TraitRemoveSchedule removeSchedule = traitRemoveSchedules[i];
                        string ticket = SchedulingManager.Instance.AddEntry(removeSchedule.removeDate, () => to.traitContainer.RemoveTraitOnSchedule(to, duplicateTrait), to.traitProcessor);
                        to.traitContainer.AddScheduleTicket(trait.name, ticket, removeSchedule.removeDate);
                    }
                }
            }

        } else {
            throw new Exception("Trying to copy trait " + trait.name + " of " + from.name + " to " + to.name + " but " + from.name + " does not have the trait!");
        }
    }
    public void CopyStatuses(ITraitable from, ITraitable to) {
        List<Status> statuses = new List<Status>(from.traitContainer.statuses);
        for (int i = 0; i < statuses.Count; i++) {
            Status status = statuses[i];
            if (!to.traitContainer.HasTrait(status.name)) {
                CopyTraitOrStatus(status, from, to);
                if (status is AbominationGerm) {
                    //if copied status is abomination germ, then remove that trait from the object it came from,
                    //since it was transferred to the other object
                    from.traitContainer.RemoveTrait(from, status);
                }
            }
        }
    }
    public string GetNeutralizingTraitFor(TileObject tileObject) {
        if(tileObject is TornadoTileObject) {
            return "Wind Master";
        } else if (tileObject is LocustSwarmTileObject) {
            return "Beastmaster";
        } else if (tileObject is PoisonCloudTileObject) {
            return "Poison Expert";
        } else if (tileObject is BallLightningTileObject) {
            return "Thunder Master";
        } else if (tileObject is FireBallTileObject) {
            return "Fire Master";
        } else if (tileObject is Quicksand) {
            return "Earth Master";
        }
        return string.Empty;
    }
    #endregion

    #region Trait Processors
    private void CreateTraitProcessors() {
        characterTraitProcessor = new CharacterTraitProcessor();
        tileObjectTraitProcessor = new TileObjectTraitProcessor();
        defaultTraitProcessor = new DefaultTraitProcessor();
    }
    public void ProcessBurningTrait(ITraitable traitable, Trait trait, ref BurningSource burningSource) {
        if (trait is Burning burning && traitable.gridTileLocation != null) {
            if (burningSource == null) {
                burningSource = new BurningSource(traitable.gridTileLocation.parentMap.region);
            }
            burning.SetSourceOfBurning(burningSource, traitable);
        }
    }
    #endregion
}

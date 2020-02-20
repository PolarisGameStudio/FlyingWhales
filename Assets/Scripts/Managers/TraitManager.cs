﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Linq;
using Traits;
using UtilityScripts;

public class TraitManager : MonoBehaviour {
    public static TraitManager Instance;

    private Dictionary<string, Trait> _allTraits;
    private string[] instancedTraits { get; set; }
    [SerializeField] private StringSpriteDictionary traitIconDictionary;

    //Trait Processors
    public static TraitProcessor characterTraitProcessor;
    public static TraitProcessor tileObjectTraitProcessor;
    public static TraitProcessor specialTokenTraitProcessor;
    public static TraitProcessor defaultTraitProcessor;
    
    //list of traits that a character can gain on their own
    public readonly string[] traitPool = new string[] { "Vigilant", "Diplomatic",
        "Fireproof", "Accident Prone", "Unfaithful", "Drunkard", "Music Lover", "Music Hater", "Ugly", "Blessed", "Nocturnal",
        "Herbalist", "Optimist", "Pessimist", "Fast", "Chaste", "Lustful", "Coward", "Lazy", "Hardworking", "Glutton", "Robust", "Suspicious" , "Inspiring", "Pyrophobic",
        "Narcoleptic", "Hothead", "Evil", "Treacherous", "Ambitious", "Authoritative", "Healer"
    };
    //"Kleptomaniac","Curious", "Craftsman", "Disillusioned",
    public List<string> buffTraitPool { get; private set; }
    public List<string> flawTraitPool { get; private set; }
    public List<string> neutralTraitPool { get; private set; }

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
        _allTraits = new Dictionary<string, Trait>();
        string path = $"{UtilityScripts.Utilities.dataPath}Traits/";
        string[] files = Directory.GetFiles(path, "*.json");
        for (int i = 0; i < files.Length; i++) {
            Trait attribute = JsonUtility.FromJson<Trait>(System.IO.File.ReadAllText(files[i]));
            _allTraits.Add(attribute.name, attribute);
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
        instancedTraits = new string[] {
            //"Builder",
            "Grudge",
            "Patrolling Character",
            "Reanimated",
            "Restrained",
            //"Assaulter",
            //"AttemptedMurderer",
            "Cursed",
            "Injured",
            "Kleptomaniac",
            "Lycanthrope",
            "Vampiric",
            //"Murderer",
            "Poisoned",
            "Resting",
            "Sick",
            //"Thief",
            "Unconscious",
            "Zapped",
            "Spooked",
            "Jolted",
            "Taunted",
            "Cannibal",
            "Lethargic",
            "Berserk Buff",
            //"Aberration",
            "Dead",
            "Disabled",
            "Invisible",
            "Unfaithful",
            "Drunk",
            "Burning",
            "Burnt",
            "Agoraphobic",
            "Infected",
            "Music Lover",
            "Music Hater",
            "Psychopath",
            "Plagued",
            "Vigilant",
            "Curious",
            //"Healer",
            "Diplomatic",
            // "AccidentProne",
            "Wet",
            "Character Trait",
            "Nocturnal",
            "Herbalist",
            "Hardworking",
            "Glutton",
            "Suspicious",
            "Narcoleptic",
            "Hothead",
            "Inspiring",
            "Pyrophobic",
            "Angry",
            "Drunkard",
            "Pessimist",
            "Lazy",
            "Coward",
            "Berserked",
            "Catatonic",
            "Griefstricken",
            "Heartbroken",
            "Cultist",
            //"Disillusioned",
            "Chaste",
            "Lustful",
            "Edible",
            "Elemental Master",
            "Paralyzed",
            "Malnourished",
            "Withdrawal",
            "Suicidal",
            "Criminal",
            "Dazed", 
            "Hiding", 
            "Bored", 
            "Overheating",
            "Freezing",
            "Frozen",
            "Ravenous",
            "Feeble",
            "Forlorn",
            "Accident Prone",
        };
        //TODO: REDO INSTANCED TRAITS, USE SCRIPTABLE OBJECTS for FIXED DATA
        for (int i = 0; i < instancedTraits.Length; i++) {
            Trait trait = CreateNewInstancedTraitClass(instancedTraits[i]);
            _allTraits.Add(trait.name, trait);
        }
    }
    public Sprite GetTraitIcon(string traitName) {
        if (traitIconDictionary.ContainsKey(traitName)) {
            return traitIconDictionary[traitName];
        }
        return traitIconDictionary.Values.First();
    }
    public bool HasTraitIcon(string traitName) {
        return traitIconDictionary.ContainsKey(traitName);
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
        string noSpacesTraitName = UtilityScripts.Utilities.RemoveAllWhiteSpace(traitName);
        string typeName = $"Traits.{ noSpacesTraitName }";
        Type type = System.Type.GetType(typeName);
        return System.Activator.CreateInstance(type) as Trait;
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
    public List<string> GetAllBuffTraits() {
        List<string> buffTraits = new List<string>();
        foreach (KeyValuePair<string, Trait> kvp in allTraits) {
            if (kvp.Value.type == TRAIT_TYPE.BUFF) {
                buffTraits.Add(kvp.Key);
            }
        }
        return buffTraits;
    }
    public List<string> GetAllFlawTraits() {
        List<string> flawTraits = new List<string>();
        foreach (KeyValuePair<string, Trait> kvp in allTraits) {
            if (kvp.Value.type == TRAIT_TYPE.FLAW) {
                flawTraits.Add(kvp.Key);
            }
        }
        return flawTraits;
    }
    public List<string> GetAllBuffTraitsThatCharacterCanHave(Character character) {
        List<string> allBuffs = new List<string>(buffTraitPool);
        for (int i = 0; i < character.traitContainer.allTraits.Count; i++) {
            Trait trait = character.traitContainer.allTraits[i];
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
        if (!PlayerManager.Instance.player.archetype.canTriggerFlaw || character.isDead || character.faction.isPlayerFaction || UtilityScripts.GameUtilities.IsRaceBeast(character.race) || character is Summon 
            || character.returnedToLife) {
            return false;
        }
        //if(doNotDisturb > 0) {
        //    return false;
        //}
        return true;
    }
    #endregion

    #region Trait Processors
    private void CreateTraitProcessors() {
        characterTraitProcessor = new CharacterTraitProcessor();
        tileObjectTraitProcessor = new TileObjectTraitProcessor();
        specialTokenTraitProcessor = new SpecialTokenTraitProcessor();
        defaultTraitProcessor = new DefaultTraitProcessor();
    }
    #endregion
}

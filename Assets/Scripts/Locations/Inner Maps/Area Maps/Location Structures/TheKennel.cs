﻿using System;
using System.Collections.Generic;
using System.Linq;
using Actionables;
using UnityEngine;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class TheKennel : LocationStructure{
        public override Vector2 selectableSize { get; }

        private const int MaxCapacity = 10;
        private int _remainingCapacity;
        private bool _isCurrentlyBreeding;
        private string _breedingScheduleKey;

        private HashSet<Summon> _ownedSummons;
        
        public TheKennel(ILocation location) : base(STRUCTURE_TYPE.THE_KENNEL, location){
            selectableSize = new Vector2(10f, 10f);
            _ownedSummons = new HashSet<Summon>();
        }

        #region Overrides
        public override void Initialize() {
            base.Initialize();
            _remainingCapacity = MaxCapacity;
            AddBreedMonsterAction();
        }
        protected override void DestroyStructure() {
            base.DestroyStructure();
            RemoveBreedMonsterAction();
            if (string.IsNullOrEmpty(_breedingScheduleKey) == false) {
                SchedulingManager.Instance.RemoveSpecificEntry(_breedingScheduleKey);
            }
            Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        }
        #endregion
        
        #region Breed Monster
        private SUMMON_TYPE GetMonsterType(RaceClass raceClass) {
            if (raceClass.race == RACE.SKELETON) {
                return SUMMON_TYPE.Skeleton;
            } else if (raceClass.race == RACE.WOLF) {
                return SUMMON_TYPE.Wolf;
            } else if (raceClass.race == RACE.GOLEM) {
                return SUMMON_TYPE.Golem;
            } else if (raceClass.race == RACE.DEMON) {
                if (raceClass.className == Incubus.ClassName) {
                    return SUMMON_TYPE.Incubus;
                } else if (raceClass.className == Succubus.ClassName) {
                    return SUMMON_TYPE.Succubus;
                }
            } else if (raceClass.race == RACE.ELEMENTAL) {
                if (raceClass.className == FireElemental.ClassName) {
                    return SUMMON_TYPE.FireElemental;
                }
            }
            throw new Exception($"No summon type for monster {raceClass.ToString()}");
        }
        private int GetMonsterCapacityCost(SUMMON_TYPE summon) {
            switch (summon) {
                case SUMMON_TYPE.Skeleton:
                    return 1;
                case SUMMON_TYPE.Wolf:
                    return 2;
                case SUMMON_TYPE.FireElemental:
                    return 3;
                case SUMMON_TYPE.Golem:
                    return 3;
                case SUMMON_TYPE.Incubus:
                    return 2;
                case SUMMON_TYPE.Succubus:
                    return 2;
                default:
                    throw new Exception($"No capacity for monster {summon.ToString()}");
            }
        }
        private void AddBreedMonsterAction() {
            PlayerAction action = new PlayerAction(PlayerDB.Breed_Monster_Action, CanDoBreedMonster, OnClickBreedMonster);
            AddPlayerAction(action);
        }
        private void RemoveBreedMonsterAction() {
            RemovePlayerAction(GetPlayerAction(PlayerDB.Breed_Monster_Action));
        }
        private bool CanDoBreedMonster() {
            return _remainingCapacity > 0 && _isCurrentlyBreeding == false;
        }
        private void OnClickBreedMonster() {
            List<RaceClass> monsters = PlayerManager.Instance.player.archetype.monsters;
            UIManager.Instance.ShowClickableObjectPicker(monsters, OnChooseBreedMonster, null, CanBreedMonster, "Choose Monster to Breed." );
        }
        private void OnChooseBreedMonster(object obj) {
            RaceClass raceClass = (RaceClass)obj;
            StartBreedingMonster(raceClass);
            UIManager.Instance.HideObjectPicker();
            Messenger.Broadcast(Signals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
        }
        private bool CanBreedMonster(RaceClass raceClass) {
            return _remainingCapacity >= GetMonsterCapacityCost(GetMonsterType(raceClass));
        }
        private void StartBreedingMonster(RaceClass raceClass) {
            _isCurrentlyBreeding = true;
            GameDate dueDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(1));
            _breedingScheduleKey = SchedulingManager.Instance.AddEntry(dueDate, () => SpawnMonster(raceClass), this);
        }
        private void SpawnMonster(RaceClass raceClass) {
            _breedingScheduleKey = string.Empty;
            _isCurrentlyBreeding = false;
            Summon summon = CharacterManager.Instance.CreateNewSummon(GetMonsterType(raceClass),
                PlayerManager.Instance.player.playerFaction, settlementLocation);
            CharacterManager.Instance.PlaceSummon(summon, CollectionUtilities.GetRandomElement(unoccupiedTiles));
            summon.AddTerritory(occupiedBuildSpot.spot.hexTileOwner);
            AddOwnedSummon(summon);
            PlayerManager.Instance.player.AddSummon(summon);
        }
        private void AddOwnedSummon(Summon summon) {
            if (_ownedSummons.Contains(summon) == false) {
                _ownedSummons.Add(summon);
                DecreaseCapacityBasedOn(summon.summonType);
                if (_ownedSummons.Count == 1) {
                    Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
                }
            }
        }
        private void RemoveOwnedSummon(Summon summon) {
            if (_ownedSummons.Remove(summon)) {
                IncreaseCapacityBasedOn(summon.summonType);
                if (_ownedSummons.Count == 0) {
                    Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
                }
            }
        }
        private void DecreaseCapacityBasedOn(SUMMON_TYPE summonType) {
            _remainingCapacity -= GetMonsterCapacityCost(summonType);
            _remainingCapacity = Mathf.Max(_remainingCapacity, 0);
        }
        private void IncreaseCapacityBasedOn(SUMMON_TYPE summonType) {
            _remainingCapacity += GetMonsterCapacityCost(summonType);
            _remainingCapacity = Mathf.Min(_remainingCapacity, MaxCapacity);
        }
        private void OnCharacterDied(Character character) {
            if (character is Summon summon && _ownedSummons.Contains(character)) {
                RemoveOwnedSummon(summon);
                PlayerManager.Instance.player.RemoveSummon(summon);
            }
        }
        #endregion
    }
}
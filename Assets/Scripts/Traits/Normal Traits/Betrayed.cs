﻿using UnityEngine;
namespace Traits {
    public class Betrayed : Status {
        public override bool isSingleton => true;

        public Betrayed() {
            name = "Betrayed";
            description = "Someone backstabbed it.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(12);
            moodEffect = -10;
            isStacking = true;
            stackLimit = 5;
            stackModifier = 0.25f;
            hindersSocials = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Death_Trait);
        }
        public override bool OnDeath(Character character) {
            //spawn a ghost after 30 minutes
            SchedulingManager.Instance.AddEntry(
                GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnMinutes(30)),
                () => SpawnGhostOf(character), this);
            return base.OnDeath(character);
        }
        private void SpawnGhostOf(Character character) {
            if (character.gridTileLocation == null) {
                Debug.Log($"{character.name} was not placed because {character.name} no longer has a gridTileLocation.");
                return;
            }
            Summon ghost = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Ghost,
                FactionManager.Instance.neutralFaction, null, character.homeRegion);
            ghost.SetName(character.name);
            (ghost as Ghost).SetBetrayedBy(responsibleCharacter);
            CharacterManager.Instance.PlaceSummon(ghost, character.gridTileLocation);
            Log log = new Log(GameManager.Instance.Today(), "Trait", this.name, "spawn_ghost");
            log.AddToFillers(ghost, ghost.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            ghost.logComponent.AddHistory(log);
        }
    }
}
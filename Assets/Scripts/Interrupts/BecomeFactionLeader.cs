﻿using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;
using UtilityScripts;

namespace Interrupts {
    public class BecomeFactionLeader : Interrupt {
        public BecomeFactionLeader() : base(INTERRUPT.Become_Faction_Leader) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            interruptHolder.actor.faction.SetLeader(interruptHolder.actor);

            Faction faction = interruptHolder.actor.faction;
            if (faction.isMajorNonPlayer) {
                faction.factionType.ClearIdeologies();

                //Set Peace-Type Ideology:
                if (interruptHolder.actor.traitContainer.HasTrait("Hothead", "Treacherous", "Evil")) {
                    Warmonger warmonger = FactionManager.Instance.CreateIdeology<Warmonger>(FACTION_IDEOLOGY.Warmonger);
                    faction.factionType.AddIdeology(warmonger);
                } else {
                    Peaceful peaceful = FactionManager.Instance.CreateIdeology<Peaceful>(FACTION_IDEOLOGY.Peaceful);
                    faction.factionType.AddIdeology(peaceful);
                }

                //Set Inclusivity-Type Ideology:
                if (GameUtilities.RollChance(60)) {
                    Inclusive inclusive = FactionManager.Instance.CreateIdeology<Inclusive>(FACTION_IDEOLOGY.Inclusive);
                    faction.factionType.AddIdeology(inclusive);
                } else {
                    Exclusive exclusive = FactionManager.Instance.CreateIdeology<Exclusive>(FACTION_IDEOLOGY.Exclusive);
                    if (GameUtilities.RollChance(60)) {
                        exclusive.SetRequirement(interruptHolder.actor.race);
                    } else {
                        exclusive.SetRequirement(interruptHolder.actor.gender);
                    }
                    faction.factionType.AddIdeology(exclusive);
                }


                //Set Religion-Type Ideology:

                //If Demon Worshipper, friendly with player faction
                FactionRelationship relationshipToPlayerFaction = faction.GetRelationshipWith(PlayerManager.Instance.player.playerFaction);
                relationshipToPlayerFaction.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Hostile);
                if (interruptHolder.actor.traitContainer.HasTrait("Cultist")) {
                    DemonWorship inclusive = FactionManager.Instance.CreateIdeology<DemonWorship>(FACTION_IDEOLOGY.Demon_Worship);
                    relationshipToPlayerFaction.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Friendly);
                    faction.factionType.AddIdeology(inclusive);
                } else if (interruptHolder.actor.race == RACE.ELVES) {
                    NatureWorship natureWorship = FactionManager.Instance.CreateIdeology<NatureWorship>(FACTION_IDEOLOGY.Nature_Worship);
                    faction.factionType.AddIdeology(natureWorship);
                } else if (interruptHolder.actor.race == RACE.HUMANS) {
                    DivineWorship divineWorship = FactionManager.Instance.CreateIdeology<DivineWorship>(FACTION_IDEOLOGY.Divine_Worship);
                    faction.factionType.AddIdeology(divineWorship);
                }

                //create relationships
                for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
                    Faction otherFaction = FactionManager.Instance.allFactions[i];
                    if (otherFaction.id != faction.id) {
                        FactionRelationship factionRelationship = faction.GetRelationshipWith(otherFaction);
                        if (otherFaction.isPlayerFaction) {
                            continue;
                        } else if (otherFaction.leader != null && otherFaction.leader is Character otherFactionLeader) {
                            //Check each Faction Leader of other existing factions if available:
                            if (interruptHolder.actor.relationshipContainer.IsEnemiesWith(otherFactionLeader)) {
                                //If this one's Faction Leader considers that an Enemy or Rival, war with that faction
                                if (factionRelationship.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Hostile)) {
                                    Log dislikeLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Faction", "Generic", "dislike_leader", null, LOG_TAG.Life_Changes);
                                    dislikeLog.AddToFillers(faction.leader as Character, faction.leader.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                                    dislikeLog.AddToFillers(otherFaction.leader as Character, otherFaction.leader.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                                    
                                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Faction", "Generic", "declare_war", null, LOG_TAG.Life_Changes);
                                    log.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
                                    log.AddToFillers(otherFaction, otherFaction.name, LOG_IDENTIFIER.FACTION_2);
                                    log.AddToFillers(dislikeLog.fillers);
                                    log.AddToFillers(null, dislikeLog.unReplacedText, LOG_IDENTIFIER.APPEND);
                                    log.AddLogToDatabase();    
                                    PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
                                }
                            } else if (interruptHolder.actor.relationshipContainer.IsFriendsWith(otherFactionLeader)) {
                                //If this one's Faction Leader considers that a Friend or Close Friend, friendly with that faction
                                if (factionRelationship.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Friendly)) {
                                    Log likeLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Faction", "Generic", "like_leader", null, LOG_TAG.Life_Changes);
                                    likeLog.AddToFillers(faction.leader as Character, faction.leader.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                                    likeLog.AddToFillers(otherFaction.leader as Character, otherFaction.leader.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                                    
                                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Faction", "Generic", "declare_peace", null, LOG_TAG.Life_Changes);
                                    log.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
                                    log.AddToFillers(otherFaction, otherFaction.name, LOG_IDENTIFIER.FACTION_2);
                                    log.AddToFillers(likeLog.fillers);
                                    log.AddToFillers(null, likeLog.unReplacedText, LOG_IDENTIFIER.APPEND);
                                    log.AddLogToDatabase();
                                    PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
                                }
                            } 
                            // else {
                            //     //The rest should be set as neutral
                            //     factionRelationship.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Neutral);
                            // }
                        }
                    }
                }

                Log changeIdeologyLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Faction", "Generic", "ideology_change", null, LOG_TAG.Life_Changes);
                changeIdeologyLog.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
                changeIdeologyLog.AddLogToDatabase();

                // Log changeRelationsLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Faction", "Generic", "relation_change");
                // changeRelationsLog.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
                // changeRelationsLog.AddLogToInvolvedObjects();
                
                //check if faction characters still meets ideology requirements
                List<Character> charactersToCheck = new List<Character>(faction.characters);
                charactersToCheck.Remove(interruptHolder.actor);
                for (int i = 0; i < charactersToCheck.Count; i++) {
                    Character factionMember = charactersToCheck[i];
                    faction.CheckIfCharacterStillFitsIdeology(factionMember);
                }
            }

            overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Become Faction Leader", "became_leader", null, LOG_TAG.Life_Changes);
            overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideEffectLog.AddToFillers(interruptHolder.actor.faction, interruptHolder.actor.faction.name, LOG_IDENTIFIER.FACTION_1);
            return true;
        }
        #endregion
    }
}

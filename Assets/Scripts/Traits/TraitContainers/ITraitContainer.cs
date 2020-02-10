﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    /// <summary>
    /// Interface for anything that should hold traits.
    /// Responsible for adding/removing traits.
    /// </summary>
    public interface ITraitContainer {

        List<Trait> allTraits { get; }
        Dictionary<string, int> stacks { get; }
        //Dictionary<Trait, int> currentDurations { get; }
        //List<RelationshipTrait> relationshipTraits { get; }

        #region Adding
        bool AddTrait(ITraitable addTo, string traitName, Character characterResponsible = null, ActualGoapNode gainedFromDoing = null);
        bool AddTrait(ITraitable addTo, Trait trait, Character characterResponsible = null, ActualGoapNode gainedFromDoing = null);
        #endregion

        #region Removing
        bool RemoveTrait(ITraitable removeFrom, Trait trait, Character removedBy = null, bool bySchedule = false);
        void RemoveTraitAndStacks(ITraitable removeFrom, Trait trait, Character removedBy = null, bool bySchedule = false);
        void RemoveTraitAndStacks(ITraitable removeFrom, string name, Character removedBy = null, bool bySchedule = false);

        bool RemoveTrait(ITraitable removeFrom, string traitName, Character removedBy = null, bool bySchedule = false);
        bool RemoveTrait(ITraitable removeFrom, int index, Character removedBy = null);
        void RemoveTrait(ITraitable removeFrom, List<Trait> traits);
        List<Trait> RemoveAllTraitsByType(ITraitable removeFrom, TRAIT_TYPE traitType);
        void RemoveAllTraitsByName(ITraitable removeFrom, string name);
        bool RemoveTraitOnSchedule(ITraitable removeFrom, Trait trait);
        void RemoveAllNonPersistentTraits(ITraitable traitable);
        void RemoveAllTraits(ITraitable traitable);
        #endregion

        #region Getting
        T GetNormalTrait<T>(params string[] traitNames) where T : Trait;
        List<T> GetNormalTraits<T>(params string[] traitNames) where T : Trait;
        bool HasTrait(params string[] traitNames);
        bool HasTraitOf(TRAIT_TYPE traitType);
        bool HasTraitOf(TRAIT_TYPE type, TRAIT_EFFECT effect);
        bool HasTraitOf(TRAIT_EFFECT traitEffect);
        List<Trait> GetAllTraitsOf(TRAIT_TYPE type);
        List<Trait> GetAllTraitsOf(TRAIT_TYPE type, TRAIT_EFFECT effect);
        #endregion

        #region Processes
        void ProcessOnTickStarted(ITraitable owner);
        void ProcessOnTickEnded(ITraitable owner);
        void ProcessOnHourStarted(ITraitable owner);
        #endregion
        
        #region Schedule Ticket
        void AddScheduleTicket(string traitName, string ticket);
        void RemoveScheduleTicket(string traitName, bool bySchedule);
        #endregion
        
        #region Switches
        void SwitchOnTrait(string name);
        void SwitchOffTrait(string name);
        #endregion
    }
}
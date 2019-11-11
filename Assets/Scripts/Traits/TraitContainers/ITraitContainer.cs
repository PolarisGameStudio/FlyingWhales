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
        List<RelationshipTrait> relationshipTraits { get; }

        #region Adding
        bool AddTrait(ITraitable addTo, string traitName, Character characterResponsible = null, GoapAction gainedFromDoing = null);
        bool AddTrait(ITraitable addTo, Trait trait, Character characterResponsible = null, GoapAction gainedFromDoing = null);
        #endregion

        #region Removing
        bool RemoveTrait(ITraitable removeFrom, Trait trait, Character removedBy = null);
        bool RemoveTrait(ITraitable removeFrom, string traitName, Character removedBy = null);
        void RemoveTrait(ITraitable removeFrom, List<Trait> traits);
        List<Trait> RemoveAllTraitsByType(ITraitable removeFrom, TRAIT_TYPE traitType);
        bool RemoveTraitOnSchedule(ITraitable removeFrom, Trait trait);
        void RemoveAllNonPersistentTraits(ITraitable traitable);
        void RemoveAllTraits(ITraitable traitable);
        #endregion

        #region Getting
        Trait GetNormalTrait(params string[] traitNames);
        bool HasTraitOf(TRAIT_TYPE traitType);
        bool HasTraitOf(TRAIT_TYPE type, TRAIT_EFFECT effect);
        List<Trait> GetAllTraitsOf(TRAIT_TYPE type);
        List<Trait> GetAllTraitsOf(TRAIT_TYPE type, TRAIT_EFFECT effect);
        #endregion
    }
}
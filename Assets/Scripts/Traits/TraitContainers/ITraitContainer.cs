﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    /// <summary>
    /// Interface for anything that should hold traits.
    /// Responsible for adding/removing traits.
    /// </summary>
    public interface ITraitContainer {

        Dictionary<string, Trait> allTraitsAndStatuses { get; }
        List<Status> statuses { get; }
        List<Trait> traits { get; }
        Dictionary<string, List<Trait>> traitOverrideFunctions { get; }
        Dictionary<string, List<TraitRemoveSchedule>> scheduleTickets { get; }
        Dictionary<string, int> stacks { get; }

        #region Adding
        bool AddTrait(ITraitable addTo, string traitName, Character characterResponsible = null, ActualGoapNode gainedFromDoing = null, bool bypassElementalChance = false, int overrideDuration = -1);
        bool AddTrait(ITraitable addTo, Trait trait, Character characterResponsible = null, ActualGoapNode gainedFromDoing = null, bool bypassElementalChance = false, int overrideDuration = -1);
        bool AddTrait(ITraitable addTo, string traitName, out Trait trait, Character characterResponsible = null, ActualGoapNode gainedFromDoing = null, bool bypassElementalChance = false, int overrideDuration = -1);
        void AddTraitOverrideFunction(string identifier, Trait trait);
        bool RestrainAndImprison(ITraitable addTo, Character characterResponsible = null, Faction factionThatImprisoned = null, Character characterThatImprisoned = null);
        #endregion

        #region Removing
        bool RemoveTrait(ITraitable removeFrom, Trait trait, Character removedBy = null, bool bySchedule = false);
        void RemoveStatusAndStacks(ITraitable removeFrom, string name, Character removedBy = null, bool bySchedule = false);
        bool RemoveTrait(ITraitable removeFrom, string traitName, Character removedBy = null, bool bySchedule = false);
        bool RemoveTrait(ITraitable removeFrom, int index, Character removedBy = null);
        void RemoveTrait(ITraitable removeFrom, List<Trait> traits);
        List<Trait> RemoveAllTraitsAndStatusesByType(ITraitable removeFrom, TRAIT_TYPE traitType);
        void RemoveAllTraitsAndStatusesByName(ITraitable removeFrom, string name);
        bool RemoveTraitOnSchedule(ITraitable removeFrom, Trait trait);
        void RemoveAllNonPersistentTraitAndStatuses(ITraitable traitable);
        void RemoveAllTraitsAndStatuses(ITraitable traitable);
        void RemoveAllTraits(ITraitable traitable);
        void RemoveAllTraitsByType(ITraitable traitable, TRAIT_TYPE traitType);
        void RemoveTraitOverrideFunction(string identifier, Trait trait);
        bool RemoveRestrainAndImprison(ITraitable removedFrom, Character removedBy = null);
        #endregion

        #region Getting
        T GetTraitOrStatus<T>(params string[] traitNames) where T : Trait;
        T GetTraitOrStatus<T>(string traitName) where T : Trait;
        List<T> GetTraitsOrStatuses<T>(params string[] traitNames) where T : Trait;
        bool HasTrait(params string[] traitNames);
        bool HasTrait(string traitName);
        bool HasTraitOf(TRAIT_TYPE traitType);
        bool HasTraitOrStatusOf(TRAIT_EFFECT traitEffect);
        List<Trait> GetAllTraitsOf(TRAIT_TYPE type);
        List<Trait> GetTraitOverrideFunctions(string identifier);
        int GetStacks(string traitName);
        bool IsBlessed();
        #endregion

        #region Processes
        void ProcessOnTickStarted(ITraitable owner);
        void ProcessOnTickEnded(ITraitable owner);
        void ProcessOnHourStarted(ITraitable owner);
        #endregion
        
        #region Schedule Ticket
        void AddScheduleTicket(string traitName, string ticket, GameDate removeDate);
        void RemoveScheduleTicket(string traitName, bool bySchedule);
        void RescheduleLatestTraitRemoval(ITraitable p_traitable, Trait p_trait, GameDate p_newRemoveDate);
        GameDate GetLatestExpiryDate(string p_traitName);
        #endregion

        #region Inquiry
        bool HasTangibleTrait();
        #endregion

        #region Loading
        void Load(ITraitable owner, SaveDataTraitContainer saveDataTraitContainer);
        #endregion
        
        #region Clean Up
        void CleanUp();
        #endregion
    }
}
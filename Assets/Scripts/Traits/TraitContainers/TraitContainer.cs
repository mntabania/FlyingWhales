using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps;
using Traits;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UtilityScripts;
using Debug = System.Diagnostics.Debug;

namespace Traits {
    public class TraitContainer : ITraitContainer {

        //public List<Trait> allTraitsAndStatuses { get; private set; }
        public Dictionary<string, Trait> allTraitsAndStatuses { get; private set; }
        public List<Trait> traits { get; private set; }
        public List<Status> statuses { get; private set; }
        public Dictionary<string, List<Trait>> traitOverrideFunctions { get; private set; }
        public Dictionary<string, int> stacks { get; private set; }
        public Dictionary<string, List<TraitRemoveSchedule>> scheduleTickets { get; private set; }
        //public Dictionary<string, bool> traitSwitches { get; private set; }

        #region getters/setters
        #endregion

        public TraitContainer() {
            allTraitsAndStatuses = new Dictionary<string, Trait>();
            statuses = new List<Status>();
            traits = new List<Trait>();
            traitOverrideFunctions = new Dictionary<string, List<Trait>>();
            stacks = new Dictionary<string, int>();
            scheduleTickets = new Dictionary<string, List<TraitRemoveSchedule>>();
            //traitSwitches = new Dictionary<string, bool>();
        }

        #region Adding
        /// <summary>
        /// The main AddTrait function. All other AddTrait functions will eventually call this.
        /// </summary>
        /// <returns>If the trait was added or not.</returns>
        public bool AddTrait(ITraitable addTo, Trait trait, Character characterResponsible = null, bool bypassElementalChance = false, int overrideDuration = -1) {
            if (TraitValidator.CanAddTraitGeneric(addTo, trait.name, this) == false) {
                return false;
            }
            if (TraitManager.Instance.IsTraitElemental(trait.name)) {
                return TryAddElementalStatus(addTo, trait, characterResponsible, bypassElementalChance, overrideDuration);
            }
            return TraitAddition(addTo, trait, characterResponsible, overrideDuration);
        }
        public bool AddTrait(ITraitable addTo, string traitName, out Trait trait, Character characterResponsible = null, bool bypassElementalChance = false, int overrideDuration = -1) {
            if (TraitValidator.CanAddTraitGeneric(addTo, traitName, this) == false) {
                trait = null;
                return false;
            }
            if (TraitManager.Instance.IsTraitElemental(traitName)) {
                return TryAddElementalStatus(addTo, traitName, out trait, characterResponsible, bypassElementalChance, overrideDuration);
            }
            return TraitAddition(addTo, traitName, out trait, characterResponsible, overrideDuration);
        }
        public bool AddTrait(ITraitable addTo, string traitName, Character characterResponsible = null, bool bypassElementalChance = false, int overrideDuration = -1) {
            if (TraitValidator.CanAddTraitGeneric(addTo, traitName, this) == false) {
                return false;
            }
            if (TraitManager.Instance.IsTraitElemental(traitName)) {
                return TryAddElementalStatus(addTo, traitName, characterResponsible, bypassElementalChance, overrideDuration);
            }
            return TraitAddition(addTo, traitName, characterResponsible, overrideDuration);
        }
        private bool TryAddElementalStatus(ITraitable addTo, string traitName, Character characterResponsible, bool bypassElementalChance, int overrideDuration) {
            if (addTo is MovingTileObject || addTo is Quicksand) {
                return false;
            }
            bool shouldAddTrait = ProcessBeforeAddingElementalStatus(addTo, traitName, bypassElementalChance, characterResponsible);
            if (shouldAddTrait) {
                shouldAddTrait = ProcessBeforeSuccessfullyAddingElementalStatus(addTo, traitName, ref overrideDuration);
                if (shouldAddTrait) {
                    Trait trait = null;
                    shouldAddTrait = TraitAddition(addTo, traitName, out trait, characterResponsible, overrideDuration);
                    if (shouldAddTrait) {
                        ProcessAfterSuccessfulAddingElementalTrait(addTo, trait as Status);
                    }
                }
            }
            return shouldAddTrait;
        }
        private bool TryAddElementalStatus(ITraitable addTo, string traitName, out Trait trait, Character characterResponsible, bool bypassElementalChance, int overrideDuration) {
            trait = null;
            if(addTo is MovingTileObject || addTo is Quicksand) {
                return false;
            }
            bool shouldAddTrait = ProcessBeforeAddingElementalStatus(addTo, traitName, bypassElementalChance, characterResponsible);
            if (shouldAddTrait) {
                shouldAddTrait = ProcessBeforeSuccessfullyAddingElementalStatus(addTo, traitName, ref overrideDuration);
                if (shouldAddTrait) {
                    shouldAddTrait = TraitAddition(addTo, traitName, out trait, characterResponsible, overrideDuration);
                    if (shouldAddTrait) {
                        ProcessAfterSuccessfulAddingElementalTrait(addTo, trait as Status);
                    }
                }
            }
            return shouldAddTrait;
        }
        private bool TryAddElementalStatus(ITraitable addTo, Trait trait, Character characterResponsible, bool bypassElementalChance, int overrideDuration) {
            bool shouldAddTrait = ProcessBeforeAddingElementalStatus(addTo, trait.name, bypassElementalChance, characterResponsible);
            if (shouldAddTrait) {
                shouldAddTrait = ProcessBeforeSuccessfullyAddingElementalStatus(addTo, trait.name, ref overrideDuration);
                if (shouldAddTrait) {
                    shouldAddTrait = TraitAddition(addTo, trait, characterResponsible, overrideDuration);
                    if (shouldAddTrait) {
                        ProcessAfterSuccessfulAddingElementalTrait(addTo, trait as Status);
                    }
                }
            }
            return shouldAddTrait;
        }
        //Returns true or false, if trait should be added or not
        private bool ProcessBeforeAddingElementalStatus(ITraitable addTo, string traitName, bool bypassElementalChance,
            Character characterResponsible) {
            bool shouldAddTrait = true;
            if (addTo is TileObject tileObject && tileObject.CanBeAffectedByElementalStatus(traitName) == false) {
                //Hidden Well Spots in Water Tiles should not receive elemental damage and status effects
                //Thick walls should not receive elemental status effects
                return false;
            }
            if (traitName == "Burning") {
                if (HasTrait("Freezing")) {
                    RemoveTrait(addTo, "Freezing");
                    shouldAddTrait = false;
                }
                if (HasTrait("Frozen")) {
                    RemoveTrait(addTo, "Frozen");
                    shouldAddTrait = false;
                }
                if (HasTrait("Poisoned")) {
                    int poisonStacks = stacks["Poisoned"];
                    Poisoned poisoned = GetTraitOrStatus<Poisoned>("Poisoned");
                    RemoveStatusAndStacks(addTo, "Poisoned");
                    if (addTo is IPointOfInterest to && addTo.gridTileLocation != null) {
                        CombatManager.Instance.PoisonExplosion(to, addTo.gridTileLocation, poisonStacks, characterResponsible, 1, poisoned.isPlayerSource);
                    }
                    shouldAddTrait = false;
                }
            } else if (traitName == "Poisoned") {
                if (HasTrait("Wet")) {
                    shouldAddTrait = false;
                } else if (HasTrait("Burning")) {
                    RemoveTrait(addTo, "Burning");
                }
            } else if (traitName == "Overheating") {
                if (HasTrait("Wet")) {
                    RemoveStatusAndStacks(addTo, "Wet");
                    shouldAddTrait = false;
                }
                if (HasTrait("Freezing")) {
                    RemoveTrait(addTo, "Freezing");
                    shouldAddTrait = false;
                }
                if (HasTrait("Frozen")) {
                    RemoveTrait(addTo, "Frozen");
                    shouldAddTrait = false;
                }
            } else if (traitName == "Freezing") {
                if (addTo is Character && HasTrait("Overheating")) {
                    RemoveTrait(addTo, "Overheating");
                    //shouldAddTrait = false;
                }
                if (HasTrait("Poisoned")) {
                    RemoveTrait(addTo, "Poisoned");
                    shouldAddTrait = false;
                }
                //else if (HasTrait("Frozen")) {
                //    RemoveTrait(addTo, "Frozen");
                //    shouldAddTrait = false;
                //}
            } else if (traitName == "Zapped") {
                if (HasTrait("Electric")) {
                    shouldAddTrait = false;
                } else if(addTo is GenericTileObject || addTo is ThinWall) {
                    if (!HasTrait("Wet")) {
                        //Ground floor tiles and walls do not get Zapped by electric damage unless they are Wet.
                        shouldAddTrait = false;
                    }
                }
                if (HasTrait("Frozen")) {
                    Frozen frozen = GetTraitOrStatus<Frozen>("Frozen");
                    RemoveTrait(addTo, "Frozen");
                    //NOTE: Do not trigger frozen explosion if frozen object is the floor, this is to prevent frozen explosion from getting wild in snow biomes where every tile is frozen
                    if (addTo is IPointOfInterest && addTo is GenericTileObject == false) { 
                        CombatManager.Instance.FrozenExplosion(addTo as IPointOfInterest, addTo.gridTileLocation, 1, frozen.isPlayerSource);
                    }
                    shouldAddTrait = false;
                }
            }
            if (shouldAddTrait) {
                // if (bypassElementalChance) {
                //     return true;
                // }
                int roll = UnityEngine.Random.Range(0, 100);
                int chance = GetElementalTraitChanceToBeAdded(traitName, addTo, bypassElementalChance);
                if (roll < chance) {
                    return true;
                }
            }
            return false;
        }
        private bool ProcessBeforeSuccessfullyAddingElementalStatus(ITraitable addTo, string traitName, ref int overrideDuration) {
            bool shouldAddTrait = true;
            if (traitName == "Freezing") {
                if (HasTrait("Frozen")) {
                    //Add Frozen trait again to reset the duration
                    AddTrait(addTo, "Frozen");
                    shouldAddTrait = false;
                }
            }
            return shouldAddTrait;
        }
        private void ProcessAfterSuccessfulAddingElementalTrait(ITraitable traitable, Status status) {
            if (status.name == "Freezing") {
                if (stacks[status.name] >= status.stackLimit) {
                    bool isPlayerSource = false;
                    if (status is IElementalTrait et) {
                        isPlayerSource = et.isPlayerSource;
                    }
                    RemoveStatusAndStacks(traitable, status.name);
                    AddTrait(traitable, "Frozen");
                    Frozen frozen = GetTraitOrStatus<Frozen>("Frozen");
                    frozen.SetIsPlayerSource(isPlayerSource);
                }
            } else if (status.name == "Frozen") {
                //Remove all stacks of Wet when a character gains Frozen.
                //Reference: https://trello.com/c/0IT5tWi5/4291-remove-all-stacks-of-wet-when-a-character-gains-frozen
                RemoveStatusAndStacks(traitable, "Wet");
            }
        }
        private bool TraitAddition(ITraitable addTo, string traitName, Character characterResponsible, int overrideDuration) {
            if (TraitManager.Instance.IsInstancedTrait(traitName)) {
                return AddTraitRoot(addTo, TraitManager.Instance.CreateNewInstancedTraitClass<Trait>(traitName), characterResponsible, overrideDuration);
            } else {
                Assert.IsTrue(TraitManager.Instance.allTraits.ContainsKey(traitName), $"No trait named {traitName} in all traits");
                return AddTraitRoot(addTo, TraitManager.Instance.allTraits[traitName], characterResponsible, overrideDuration);
            }
        }
        private bool TraitAddition(ITraitable addTo, string traitName, out Trait trait, Character characterResponsible, int overrideDuration) {
            if (TraitManager.Instance.IsInstancedTrait(traitName)) {
                //Highly non performant, because it always creates new instance even though the trait is just being stacked
                trait = TraitManager.Instance.CreateNewInstancedTraitClass<Trait>(traitName);
                return AddTraitRoot(addTo, trait, characterResponsible, overrideDuration);
            } else {
                Assert.IsTrue(TraitManager.Instance.allTraits.ContainsKey(traitName), $"No trait named {traitName} in all traits");
                trait = TraitManager.Instance.allTraits[traitName];
                return AddTraitRoot(addTo, trait, characterResponsible, overrideDuration);
            }
        }
        private bool TraitAddition(ITraitable addTo, Trait trait, Character characterResponsible, int overrideDuration) {
            return AddTraitRoot(addTo, trait, characterResponsible, overrideDuration);
        }
        private bool AddTraitRoot(ITraitable addTo, Trait trait, Character characterResponsible, int overrideDuration) {
            if (TraitValidator.CanAddTrait(addTo, trait, this) == false) {
                return false;
            }
            if(trait is Status status) {
                string statusName = status.name;
                if (status.isStacking) {
                    if (stacks.ContainsKey(statusName)) {
                        stacks[statusName]++;
                        if (TraitManager.Instance.IsInstancedTrait(statusName)) {
                            Status existingStatus = GetTraitOrStatus<Status>(statusName);
                            addTo.traitProcessor.OnStatusStacked(addTo, existingStatus, characterResponsible, overrideDuration);
                        } else {
                            addTo.traitProcessor.OnStatusStacked(addTo, status, characterResponsible, overrideDuration);
                        }
                    } else {
                        stacks.Add(statusName, 1);
                        statuses.Add(status);
                        allTraitsAndStatuses.Add(statusName, status);    
                        addTo.traitProcessor.OnTraitAdded(addTo, status, characterResponsible, overrideDuration);
                    }
                } else {
                    statuses.Add(status);
                    allTraitsAndStatuses.Add(statusName, status);
                    addTo.traitProcessor.OnTraitAdded(addTo, status, characterResponsible, overrideDuration);
                }
            } else {
                traits.Add(trait);
                allTraitsAndStatuses.Add(trait.name, trait);
                addTo.traitProcessor.OnTraitAdded(addTo, trait, characterResponsible, overrideDuration);
            }
            return true;
        }
        public int GetElementalTraitChanceToBeAdded(string traitName, ITraitable addTo, bool bypassElementalChance) {
            int chance = 100;
            if (traitName == "Burning") {
                chance = bypassElementalChance ? 100 : 15;
                if(HasTrait("Fire Resistant", "Wet", "Burnt") || !HasTrait("Flammable")) {
                    chance = 0;
                } else if (HasTrait("Poisoned")) {
                    chance = 100;
                }
            } else if (traitName == "Freezing") {
                chance = bypassElementalChance ? 100 : 20;
                if (HasTrait("Cold Blooded", "Burning", "Frozen Immune", "Iceproof")) {
                    chance = 0;
                } else if (HasTrait("Wet")) {
                    chance = 100;
                }
            } else if (traitName == "Zapped") {
                chance = bypassElementalChance ? 100 : 15;
                if (HasTrait("Electric")) {
                    chance = 0;
                } else if (HasTrait("Wet")) {
                    chance = 100;
                }
            } else if (traitName == "Poisoned") {
                chance = 100;
                if (!bypassElementalChance) {
                    if (addTo is Character) {
                        chance = 25;
                        if (HasTrait("Poisoned")) {
                            chance = 15;
                        }
                    }
                }
            } else if (traitName == "Frozen") {
                chance = 100;
                if (HasTrait("Cold Blooded", "Burning", "Frozen Immune", "Iceproof")) {
                    chance = 0;
                }
            }
            return chance;
        }
        public bool RestrainAndImprison(ITraitable addTo, Character characterResponsible = null, Faction factionThatImprisoned = null, Character characterThatImprisoned = null) {
            AddTrait(addTo, "Restrained", characterResponsible);
            AddTrait(addTo, "Prisoner", characterResponsible);
            Prisoner prisoner = GetTraitOrStatus<Prisoner>("Prisoner");
            Restrained restrained = GetTraitOrStatus<Restrained>("Restrained");

            if (prisoner != null) {
                prisoner.ClearResponsibleCharacters();
                if (characterResponsible != null) {
                    prisoner.AddCharacterResponsibleForTrait(characterResponsible);
                }
                prisoner.SetPrisonerOfFaction(factionThatImprisoned);
                prisoner.SetPrisonerOfCharacter(characterThatImprisoned);
            }
            if (restrained != null) {
                restrained.ClearResponsibleCharacters();
                if (characterResponsible != null) {
                    restrained.AddCharacterResponsibleForTrait(characterResponsible);
                }
            }
            return true; //Always return true because once this is called, even if character is already restrained, it will be overridden by the new restrain
        }
        public bool RemoveRestrainAndImprison(ITraitable removedFrom, Character removedBy = null) {
            bool hasRemovedRestrained = RemoveTrait(removedFrom, "Restrained", removedBy);
            bool hasRemovedPrisoner = RemoveTrait(removedFrom, "Prisoner", removedBy);
            return hasRemovedRestrained && hasRemovedPrisoner;
        }

        #endregion

        #region Removing
        /// <summary>
        /// The main RemoveTrait function. All other RemoveTrait functions eventually call this.
        /// </summary>
        /// <returns>If the trait was removed or not.</returns>
        public bool RemoveTrait(ITraitable removeFrom, Trait trait, Character removedBy = null, bool bySchedule = false) {
            bool removedOrUnstacked = false;
            if(trait is Status status) {
                removedOrUnstacked = RemoveStatus(removeFrom, status, removedBy, bySchedule);
            } else {
                removedOrUnstacked = traits.Remove(trait);
                if (removedOrUnstacked) {
                    allTraitsAndStatuses.Remove(trait.name);
                    removeFrom.traitProcessor.OnTraitRemoved(removeFrom, trait, removedBy);
                    RemoveScheduleTicket(trait.name, bySchedule);
                }
            }
            return removedOrUnstacked;
        }
        public bool RemoveTrait(ITraitable removeFrom, string traitName, Character removedBy = null, bool bySchedule = false) {
            if (HasTrait(traitName)) {
                if (removeFrom is Character character) {
                    PLAYER_SKILL_TYPE afflictionType = PlayerSkillManager.Instance.GetAfflictionTypeByTraitName(traitName);
                    if (afflictionType != PLAYER_SKILL_TYPE.NONE) {
                        character.afflictionsSkillsInflictedByPlayer.Remove(afflictionType);
                    }
                }
                Trait trait = GetTraitOrStatus<Trait>(traitName);
                return RemoveTrait(removeFrom, trait, removedBy, bySchedule);
            }
            return false;
        }
        private bool RemoveStatusAndStacks(ITraitable removeFrom, Status status, Character removedBy = null, bool bySchedule = false) {
            int loopNum = 1;
            if (stacks.ContainsKey(status.name)) {
                loopNum = stacks[status.name];
            }
            int removedCount = 0;
            for (int i = 0; i < loopNum; i++) {
                if(RemoveStatus(removeFrom, status, removedBy, bySchedule)) {
                    removedCount++;
                }
            }
            return removedCount == loopNum;
        }
        public void RemoveStatusAndStacks(ITraitable removeFrom, string name, Character removedBy = null, bool bySchedule = false) {
            Status trait = GetTraitOrStatus<Status>(name);
            if (trait != null) {
                RemoveStatusAndStacks(removeFrom, trait, removedBy, bySchedule);
            }
        }
        public bool RemoveStatus(ITraitable removeFrom, Status status, Character removedBy = null, bool bySchedule = false) {
            bool removedOrUnstacked = true;
            if (!status.isStacking) {
                removedOrUnstacked = statuses.Remove(status);
                if (removedOrUnstacked) {
                    allTraitsAndStatuses.Remove(status.name);
                    removeFrom.traitProcessor.OnTraitRemoved(removeFrom, status, removedBy);
                    RemoveScheduleTicket(status.name, bySchedule);
                }
            } else {
                //status is stacking
                if (stacks.ContainsKey(status.name)) {
                    if (stacks[status.name] > 1) {
                        stacks[status.name]--;
                        removeFrom.traitProcessor.OnStatusUnstack(removeFrom, status, removedBy);
                        RemoveScheduleTicket(status.name, bySchedule);
                        removedOrUnstacked = true;
                    } else {
                        removedOrUnstacked = statuses.Remove(status);
                        if (removedOrUnstacked) {
                            allTraitsAndStatuses.Remove(status.name);
                            stacks.Remove(status.name);
                            removeFrom.traitProcessor.OnTraitRemoved(removeFrom, status, removedBy);
                            RemoveScheduleTicket(status.name, bySchedule);
                        }
                    }
                }
            }
            return removedOrUnstacked;
        }
        public bool RemoveTrait(ITraitable removeFrom, int index, Character removedBy = null) {
            bool removedOrUnstacked = true;
            if(index < 0 || index >= allTraitsAndStatuses.Count) {
                removedOrUnstacked = false;
            } else {
                Trait trait = traits[index];
                traits.RemoveAt(index);
                allTraitsAndStatuses.Remove(trait.name);
                removeFrom.traitProcessor.OnTraitRemoved(removeFrom, trait, removedBy);
                RemoveScheduleTicket(trait.name);
            }
            return removedOrUnstacked;
        }
        public void RemoveTrait(ITraitable removeFrom, List<Trait> traits) {
            for (int i = 0; i < traits.Count; i++) {
                RemoveTrait(removeFrom, traits[i]);
            }
        }
        public List<Trait> RemoveAllTraitsAndStatusesByType(ITraitable removeFrom, TRAIT_TYPE traitType) {
            List<Trait> removedTraits = new List<Trait>();
            //List<Trait> all = new List<Trait>(allTraits);
            for (int i = 0; i < statuses.Count; i++) {
                Status trait = statuses[i];
                if (trait.type == traitType) {
                    if (RemoveStatusAndStacks(removeFrom, trait)) {
                        removedTraits.Add(trait);
                        i--;
                    }
                }
            }
            for (int i = 0; i < traits.Count; i++) {
                Trait trait = traits[i];
                if (trait.type == traitType) {
                    if(RemoveTrait(removeFrom, i)) {
                        removedTraits.Add(trait);
                        i--;
                    }
                }
            }
            return removedTraits;
        }
        public void RemoveAllTraitsByType(ITraitable removeFrom, TRAIT_TYPE traitType) {
            for (int i = 0; i < traits.Count; i++) {
                Trait trait = traits[i];
                if (trait.type == traitType) {
                    if (RemoveTrait(removeFrom, i)) {
                        i--;
                    }
                }
            }
        }
        public void RemoveAllTraitsAndStatusesByName(ITraitable removeFrom, string name) {
            //List<Trait> removedTraits = new List<Trait>();
            //List<Trait> all = new List<Trait>(allTraits);
            for (int i = 0; i < statuses.Count; i++) {
                Status trait = statuses[i];
                if (trait.name == name) {
                    //removedTraits.Add(trait);
                    if (RemoveStatusAndStacks(removeFrom, trait)) {
                        i--;
                    }
                }
            }
            for (int i = 0; i < traits.Count; i++) {
                Trait trait = traits[i];
                if (trait.name == name) {
                    //removedTraits.Add(trait);
                    if (RemoveTrait(removeFrom, i)) {
                        i--;
                    }
                }
            }
            //return removedTraits;
        }
        public bool RemoveTraitOnSchedule(ITraitable removeFrom, Trait trait) {
            if(RemoveTrait(removeFrom, trait, bySchedule: true)) {
                trait.OnRemoveStatusBySchedule(removeFrom);
                return true;
            }
            return false;
        }
        public bool RemoveTraitOnSchedule(ITraitable removeFrom, string traitName) {
            if (HasTrait(traitName)) {
                Trait trait = GetTraitOrStatus<Trait>(traitName);
                return RemoveTraitOnSchedule(removeFrom, trait);
            }
            return false;
        }
        /// <summary>
        /// Remove all traits that are not persistent.
        /// </summary>
        public void RemoveAllNonPersistentTraitAndStatuses(ITraitable traitable) {
            //List<Trait> allTraits = new List<Trait>(this.allTraits);
            for (int i = 0; i < statuses.Count; i++) {
                Status currTrait = statuses[i];
                if (!currTrait.isPersistent) {
                    if (RemoveStatusAndStacks(traitable, currTrait)) {
                        i--;
                    }
                }
            }
            for (int i = 0; i < traits.Count; i++) {
                Trait currTrait = traits[i];
                if (!currTrait.isPersistent) {
                    if(RemoveTrait(traitable, i)) {
                        i--;
                    }
                }
            }
        }
        public void RemoveAllTraitsAndStatuses(ITraitable traitable) {
            //List<Trait> allTraits = new List<Trait>(this.allTraits);
            for (int i = 0; i < statuses.Count; i++) {
                if (RemoveStatusAndStacks(traitable, statuses[i])) { //remove all traits
                    i--;
                }
            }
            for (int i = 0; i < traits.Count; i++) {
                if (RemoveTrait(traitable, i)) { //remove all traits
                    i--;
                }
            }
        }
        public void RemoveAllTraits(ITraitable traitable) {
            for (int i = 0; i < traits.Count; i++) {
                if (RemoveTrait(traitable, i)) { //remove all traits
                    i--;
                }
            }
        }
        #endregion

        #region Getting
        public T GetTraitOrStatus<T>(string traitName) where T : Trait {
            if (HasTrait(traitName)) {
                return allTraitsAndStatuses[traitName] as T;
            }
            return null;
        }
        public T GetTraitOrStatus<T>(string traitName1, string traitName2) where T : Trait {
            if (HasTrait(traitName1)) {
                return allTraitsAndStatuses[traitName1] as T;
            } else if (HasTrait(traitName2)) {
                return allTraitsAndStatuses[traitName2] as T;
            }
            return null;
        }
        //public List<T> GetTraitsOrStatuses<T>(params string[] traitNames) where T : Trait {
        //    List<T> traits = new List<T>();
        //    for (int i = 0; i < traitNames.Length; i++) {
        //        string name = traitNames[i];
        //        if (HasTrait(name)) {
        //            traits.Add(allTraitsAndStatuses[name] as T);
        //        }
        //    }
        //    //for (int i = 0; i < allTraitsAndStatuses.Count; i++) {
        //    //    Trait trait = allTraitsAndStatuses[i];
        //    //    if (traitNames.Contains(trait.name)) {
        //    //        traits.Add(trait as T);
        //    //    }
        //    //}
        //    return traits;
        //}
        public bool HasTraitOf(TRAIT_TYPE traitType) {
            for (int i = 0; i < traits.Count; i++) {
                if (traits[i].type == traitType) {
                    return true;
                }
            }
            return false;
        }
        public bool HasTraitOrStatusOf(TRAIT_EFFECT traitEffect) {
            for (int i = 0; i < traits.Count; i++) {
                if (traits[i].effect == traitEffect) {
                    return true;
                }
            }
            for (int i = 0; i < statuses.Count; i++) {
                if (statuses[i].effect == traitEffect) {
                    return true;
                }
            }
            return false;
        }
        public List<Trait> GetAllTraitsOf(TRAIT_TYPE type) {
            List<Trait> traits = new List<Trait>();
            for (int i = 0; i < traits.Count; i++) {
                Trait currTrait = traits[i];
                if (currTrait.type == type) {
                    traits.Add(currTrait);
                }
            }
            //for (int i = 0; i < allTraitsAndStatuses.Count; i++) {
            //    Trait currTrait = allTraitsAndStatuses[i];
            //    if (currTrait.type == type) {
            //        traits.Add(currTrait);
            //    }
            //}
            return traits;
        }
        public int GetStacks(string traitName) {
            if (stacks.ContainsKey(traitName)) {
                return stacks[traitName];
            }
            return 0;
        }
        public bool IsBlessed() {
            return HasTrait("Blessed") || HasTrait("Dark Blessing");
        }
        #endregion

        #region Processes
        public void ProcessOnTickStarted(ITraitable owner) {
            List<Trait> traitOverrideFunctions = GetTraitOverrideFunctions(TraitManager.Tick_Started_Trait);
            if (traitOverrideFunctions != null) {
                for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                    Trait trait = traitOverrideFunctions[i];
#if DEBUG_PROFILER
                    Profiler.BeginSample($"{owner.name} - {trait.name} - Tick Started Process");
#endif
                    trait.OnTickStarted(owner);
#if DEBUG_PROFILER
                    Profiler.EndSample();
#endif
                }
            }
            //if (allTraitsAndStatuses != null) {
            //    for (int i = 0; i < allTraitsAndStatuses.Count; i++) {
            //        allTraitsAndStatuses[i].OnTickStarted();
            //    }
            //}
        }
        public void ProcessOnTickEnded(ITraitable owner) {
            List<Trait> traitOverrideFunctions = GetTraitOverrideFunctions(TraitManager.Tick_Ended_Trait);
            if (traitOverrideFunctions != null) {
                for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                    Trait trait = traitOverrideFunctions[i];
                    trait.OnTickEnded(owner);
                }
            }
            //if (allTraitsAndStatuses != null) {
            //    for (int i = 0; i < allTraitsAndStatuses.Count; i++) {
            //        Trait trait = allTraitsAndStatuses[i];
            //        trait.OnTickEnded();
            //    }
            //}
        }
        public void ProcessOnHourStarted(ITraitable owner) {
            List<Trait> traitOverrideFunctions = GetTraitOverrideFunctions(TraitManager.Hour_Started_Trait);
            if (traitOverrideFunctions != null) {
                for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                    Trait trait = traitOverrideFunctions[i];
                    trait.OnHourStarted(owner);
                }
            }
            //if (allTraitsAndStatuses != null) {
            //    for (int i = 0; i < allTraitsAndStatuses.Count; i++) {
            //        allTraitsAndStatuses[i].OnHourStarted();
            //    }
            //}
        }
#endregion
        
#region Schedule Tickets
        public void AddScheduleTicket(string traitName, string ticket, GameDate removeDate) {
            TraitRemoveSchedule traitRemoveSchedule = ObjectPoolManager.Instance.CreateNewTraitRemoveSchedule();
            traitRemoveSchedule.removeDate = removeDate;
            traitRemoveSchedule.ticket = ticket;

            if (scheduleTickets.ContainsKey(traitName)) {
                scheduleTickets[traitName].Add(traitRemoveSchedule);
            } else {
                scheduleTickets.Add(traitName, new List<TraitRemoveSchedule>() { traitRemoveSchedule });
            }
        }
        public void RemoveScheduleTicket(string traitName, bool bySchedule = false) {
            if (scheduleTickets.ContainsKey(traitName)) {
                TraitRemoveSchedule traitRemoveSchedule = null;
                if(scheduleTickets[traitName].Count > 0) {
                    traitRemoveSchedule = scheduleTickets[traitName][0];
                }
                if (!bySchedule && traitRemoveSchedule != null) {
                    SchedulingManager.Instance.RemoveSpecificEntry(traitRemoveSchedule.ticket);
                }
                if (scheduleTickets[traitName].Count <= 0) {
                    scheduleTickets.Remove(traitName);
                } else {
                    scheduleTickets[traitName].RemoveAt(0);
                    ObjectPoolManager.Instance.ReturnTraitRemoveScheduleToPool(traitRemoveSchedule);
                }
            } 
        }
        public void RescheduleLatestTraitRemoval(ITraitable p_traitable, Trait p_trait, GameDate p_newRemoveDate) {
            string traitName = p_trait.name;
            if (scheduleTickets.ContainsKey(traitName)) {
                TraitRemoveSchedule traitRemoveSchedule = null;
                if(scheduleTickets[traitName].Count > 0) {
                    traitRemoveSchedule = scheduleTickets[traitName].Last();
                }
                if (traitRemoveSchedule != null) {
                    SchedulingManager.Instance.RemoveSpecificEntry(traitRemoveSchedule.ticket);
                    scheduleTickets[traitName].RemoveAt(scheduleTickets[traitName].IndexOf(traitRemoveSchedule));
                    ObjectPoolManager.Instance.ReturnTraitRemoveScheduleToPool(traitRemoveSchedule);
                }
                string ticket = SchedulingManager.Instance.AddEntry(p_newRemoveDate, () => p_traitable.traitContainer.RemoveTraitOnSchedule(p_traitable, p_trait), this);
                p_traitable.traitContainer.AddScheduleTicket(p_trait.name, ticket, p_newRemoveDate);    
                
                if (p_traitable is Character character) {
                    //TODO: Make this more abstracted
                    character.moodComponent.RescheduleMoodEffect(p_trait, p_newRemoveDate);
                }
            }
        }
        public GameDate GetLatestExpiryDate(string p_traitName) {
            if (scheduleTickets.ContainsKey(p_traitName)) {
                TraitRemoveSchedule traitRemoveSchedule = null;
                if(scheduleTickets[p_traitName].Count > 0) {
                    traitRemoveSchedule = scheduleTickets[p_traitName].Last();
                }
                if (traitRemoveSchedule != null) {
                    return traitRemoveSchedule.removeDate;
                }
            }
            return default;
        }
#endregion
        
#region Switches
        //public void SwitchOnTrait(string name) {
        //    if (traitSwitches.ContainsKey(name)) {
        //        traitSwitches[name] = true;
        //    } else {
        //        traitSwitches.Add(name, true);
        //    }
        //}
        //public void SwitchOffTrait(string name) {
        //    if (traitSwitches.ContainsKey(name)) {
        //        traitSwitches[name] = false;
        //    } else {
        //        traitSwitches.Add(name, false);
        //    }
        //}
        //private bool HasTraitSwitch(string name) {
        //    if (traitSwitches.ContainsKey(name)) {
        //        return traitSwitches[name];
        //    }
        //    return false;
        //}
        public bool HasTrait(string traitName) {
            return allTraitsAndStatuses.ContainsKey(traitName);
        }
        public bool HasTrait(string traitName1, string traitName2) {
            if (allTraitsAndStatuses.ContainsKey(traitName1) || allTraitsAndStatuses.ContainsKey(traitName2)) {
                return true;
            }
            return false;
        }
        public bool HasTrait(string traitName1, string traitName2, string traitName3) {
            if (allTraitsAndStatuses.ContainsKey(traitName1) || allTraitsAndStatuses.ContainsKey(traitName2) || allTraitsAndStatuses.ContainsKey(traitName3)) {
                return true;
            }
            return false;
        }
        public bool HasTrait(string traitName1, string traitName2, string traitName3, string traitName4) {
            if (allTraitsAndStatuses.ContainsKey(traitName1) || allTraitsAndStatuses.ContainsKey(traitName2) 
                || allTraitsAndStatuses.ContainsKey(traitName3) || allTraitsAndStatuses.ContainsKey(traitName4)) {
                return true;
            }
            return false;
        }
        public bool HasTrait(string traitName1, string traitName2, string traitName3, string traitName4, string traitName5) {
            if (allTraitsAndStatuses.ContainsKey(traitName1) || allTraitsAndStatuses.ContainsKey(traitName2)
                || allTraitsAndStatuses.ContainsKey(traitName3) || allTraitsAndStatuses.ContainsKey(traitName4)
                || allTraitsAndStatuses.ContainsKey(traitName5)) {
                return true;
            }
            return false;
        }
        public bool HasTrait(string[] traitNames) {
            for (int i = 0; i < traitNames.Length; i++) {
                if (allTraitsAndStatuses.ContainsKey(traitNames[i])) {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Trait Override Functions
        public void AddTraitOverrideFunction(string identifier, Trait trait) {
            if (traitOverrideFunctions.ContainsKey(identifier)) {
                traitOverrideFunctions[identifier].Add(trait);
            } else {
                traitOverrideFunctions.Add(identifier, new List<Trait>() { trait });
            }
        }
        public void RemoveTraitOverrideFunction(string identifier, Trait trait) {
            if (traitOverrideFunctions.ContainsKey(identifier)) {
                traitOverrideFunctions[identifier].Remove(trait);
            }
        }
        public List<Trait> GetTraitOverrideFunctions(string identifier) {
            if (traitOverrideFunctions.ContainsKey(identifier)) {
                return traitOverrideFunctions[identifier];
            }
            return null;
        }
        //public void AddOnCollideWithTrait(Trait trait) {
        //    onCollideWithTraits.Add(trait);
        //}
        //public bool RemoveOnCollideWithTrait(Trait trait) {
        //    return onCollideWithTraits.Remove(trait);
        //}
        //public void AddOnEnterGridTileTrait(Trait trait) {
        //    onEnterGridTileTraits.Add(trait);
        //}
        //public bool RemoveOnEnterGridTileTrait(Trait trait) {
        //    return onEnterGridTileTraits.Remove(trait);
        //}
#endregion

#region Inquiry
        public bool HasTangibleTrait() {
            for (int i = 0; i < statuses.Count; i++) {
                Status currTrait = statuses[i];
                if (currTrait.isTangible) {
                    return true;
                }
            }
            return false;
        }
#endregion

#region Loading
        private bool LoadUnInstancedTrait(ITraitable addTo, string traitName) {
            Assert.IsTrue(TraitManager.Instance.allTraits.ContainsKey(traitName), $"No trait named {traitName} in all traits");
            Trait trait = TraitManager.Instance.allTraits[traitName];
            //if (trait.IsUnique()) {
                //if uninstanced trait is unique then do not add it if object already has it.
                if (addTo.traitContainer.HasTrait(trait.name)) {
                    return false;
                }
            //}
            return LoadTraitRoot(addTo, trait);
        }
        private bool LoadInstancedTrait(ITraitable addTo, Trait trait) {
            return LoadTraitRoot(addTo, trait);
        }
        private bool LoadTraitRoot(ITraitable addTo, Trait trait) {
            if(trait is Status status) {
                statuses.Add(status);
            } else {
                traits.Add(trait);
            }
            trait.LoadTraitOnLoadTraitContainer(addTo);
            if (allTraitsAndStatuses.ContainsKey(trait.name)) {
                UnityEngine.Debug.LogError($"Trait {trait.name} already exists in {addTo}'s traits!");
            } else {
                allTraitsAndStatuses.Add(trait.name, trait);    
            }

            if(trait.traitOverrideFunctionIdentifiers != null && trait.traitOverrideFunctionIdentifiers.Count > 0) {
                for (int i = 0; i < trait.traitOverrideFunctionIdentifiers.Count; i++) {
                    string identifier = trait.traitOverrideFunctionIdentifiers[i];
                    AddTraitOverrideFunction(identifier, trait);
                }
            }
            
            return true;
        }
        /// <summary>
        /// Load all the traits of this trait container.
        /// IMPORTANT NOTE: This assumes that the ITraitable has already been placed in the world.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="saveDataTraitContainer"></param>
        public void Load(ITraitable owner, SaveDataTraitContainer saveDataTraitContainer) {
            Debug.Assert(owner.gridTileLocation != null, $"{owner} gridTileLocation != null");
            for (int i = 0; i < saveDataTraitContainer.nonInstancedTraits.Count; i++) {
                string nonInstancedTraitName = saveDataTraitContainer.nonInstancedTraits[i];
                //add trait to container, bue set duration as 0, since the traits removal will be handled when loading the schedule tickets
                LoadUnInstancedTrait(owner, nonInstancedTraitName);
            }
            for (int i = 0; i < saveDataTraitContainer.instancedTraitsIDs.Count; i++) {
                string persistentID = saveDataTraitContainer.instancedTraitsIDs[i];
                Trait trait = DatabaseManager.Instance.traitDatabase.GetTraitByPersistentID(persistentID);
                LoadInstancedTrait(owner, trait);
            }
            stacks.Clear();
            foreach (var stack in saveDataTraitContainer.stacks) {
                stacks.Add(stack.Key, stack.Value);
            }
            foreach (var ticket in saveDataTraitContainer.scheduleTickets) {
                for (int i = 0; i < ticket.Value.Count; i++) {
                    GameDate removeDate = ticket.Value[i];
                    string ticketID = SchedulingManager.Instance.AddEntry(removeDate, () => RemoveTraitOnSchedule(owner, ticket.Key), this);
                    AddScheduleTicket(ticket.Key, ticketID, removeDate);
                }
            }
            //traitSwitches.Clear();
            //foreach (var pair in saveDataTraitContainer.traitSwitches) {
            //    traitSwitches.Add(pair.Key, pair.Value);
            //}
        }
#endregion

#region Clean Up
        public void CleanUp() {
            allTraitsAndStatuses?.Clear();
            traits?.Clear();
            statuses?.Clear();
            traitOverrideFunctions?.Clear();
            stacks?.Clear();
            scheduleTickets?.Clear();
            //traitSwitches?.Clear();
        }
#endregion
    }
}

public class TraitRemoveSchedule {
    public GameDate removeDate;
    public string ticket;

    public void Initialize() {
        //ticket = string.Empty;
    }
    public void Reset() {
        removeDate = new GameDate();
        ticket = string.Empty;
    }
}

#region Save Data
public class SaveDataTraitContainer : SaveData<ITraitContainer> {
    public List<string> nonInstancedTraits; //list of trait names
    public List<string> instancedTraitsIDs; //list of persistent ids per instanced trait
    public Dictionary<string, int> stacks;
    public Dictionary<string, List<GameDate>> scheduleTickets;
    //public Dictionary<string, bool> traitSwitches;
    
    public override void Save(ITraitContainer data) {
        base.Save(data);
        nonInstancedTraits = new List<string>();
        instancedTraitsIDs = new List<string>();
        for (int i = 0; i < data.traits.Count; i++) {
            Trait trait = data.traits[i];
            if (TraitManager.Instance.IsInstancedTrait(trait.name)) {
                instancedTraitsIDs.Add(trait.persistentID);
                Assert.IsFalse(string.IsNullOrEmpty(trait.persistentID), $"Persistent id of instanced trait {trait.name} is empty!");
                SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(trait);
            } else {
                nonInstancedTraits.Add(trait.name);
            }
        }
        for (int i = 0; i < data.statuses.Count; i++) {
            Trait trait = data.statuses[i];
            if (TraitManager.Instance.IsInstancedTrait(trait.name)) {
                instancedTraitsIDs.Add(trait.persistentID);
                Assert.IsFalse(string.IsNullOrEmpty(trait.persistentID), $"Persistent id of instanced status {trait.name} is empty!");
                SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(trait);
            } else {
                nonInstancedTraits.Add(trait.name);
            }
        }
        //for (int i = 0; i < data.allTraitsAndStatuses.Count; i++) {
        //    Trait trait = data.allTraitsAndStatuses[i];
        //    if (TraitManager.Instance.IsInstancedTrait(trait.name)) {
        //        instancedTraitsIDs.Add(trait.persistentID);
        //        Assert.IsFalse(string.IsNullOrEmpty(trait.persistentID), $"Persistent id of instanced trait {trait.name} is empty!");
        //        SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(trait);
        //    } else {
        //        nonInstancedTraits.Add(trait.name);
        //    }
        //}
        stacks = new Dictionary<string, int>();
        foreach (var kvp in data.stacks) {
            stacks.Add(kvp.Key, kvp.Value);
        }
        scheduleTickets = new Dictionary<string, List<GameDate>>();
        foreach (var schedule in data.scheduleTickets) {
            scheduleTickets.Add(schedule.Key, new List<GameDate>());
            for (int i = 0; i < schedule.Value.Count; i++) {
                TraitRemoveSchedule removeSchedule = schedule.Value[i];
                scheduleTickets[schedule.Key].Add(removeSchedule.removeDate);
            }
        }
        //traitSwitches = data.traitSwitches;
    }
}
#endregion
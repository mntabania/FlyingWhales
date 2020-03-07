using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps;

namespace Traits {
    public class TraitContainer : ITraitContainer {

        public List<Trait> allTraitsAndStatuses { get; private set; }
        public List<Trait> traits { get; private set; }
        public List<Status> statuses { get; private set; }
        public List<Trait> onCollideWithTraits { get; private set; }
        public List<Trait> onEnterGridTileTraits { get; private set; }

        public Dictionary<string, int> stacks { get; private set; }
        public Dictionary<string, List<string>> scheduleTickets { get; private set; }
        public Dictionary<string, bool> traitSwitches { get; private set; }
        //public Dictionary<Trait, int> currentDurations { get; private set; } //Temporary only, fix this by making all traits instanced based and just object pool them

        #region getters/setters
        #endregion

        public TraitContainer() {
            allTraitsAndStatuses = new List<Trait>();
            statuses = new List<Status>();
            traits = new List<Trait>();
            onCollideWithTraits = new List<Trait>();
            onEnterGridTileTraits = new List<Trait>();
            stacks = new Dictionary<string, int>();
            scheduleTickets = new Dictionary<string, List<string>>();
            traitSwitches = new Dictionary<string, bool>();
            //currentDurations = new Dictionary<Trait, int>();
        }

        #region Adding
        /// <summary>
        /// The main AddTrait function. All other AddTrait functions will eventually call this.
        /// </summary>
        /// <returns>If the trait was added or not.</returns>
        public bool AddTrait(ITraitable addTo, Trait trait, Character characterResponsible = null, 
            ActualGoapNode gainedFromDoing = null, bool bypassElementalChance = false, int overrideDuration = -1) {
            if (TraitManager.Instance.IsTraitElemental(trait.name)) {
                return TryAddElementalStatus(addTo, trait, characterResponsible, gainedFromDoing, bypassElementalChance, overrideDuration);
            }
            return TraitAddition(addTo, trait, characterResponsible, gainedFromDoing, overrideDuration);
        }
        public bool AddTrait(ITraitable addTo, string traitName, out Trait trait, Character characterResponsible = null, 
            ActualGoapNode gainedFromDoing = null, bool bypassElementalChance = false, int overrideDuration = -1) {
            if (TraitManager.Instance.IsTraitElemental(traitName)) {
                return TryAddElementalStatus(addTo, traitName, out trait, characterResponsible, 
                    gainedFromDoing, bypassElementalChance, overrideDuration);
            }
            return TraitAddition(addTo, traitName, out trait, characterResponsible, gainedFromDoing, overrideDuration);
        }
        public bool AddTrait(ITraitable addTo, string traitName, Character characterResponsible = null, 
            ActualGoapNode gainedFromDoing = null, bool bypassElementalChance = false, int overrideDuration = -1) {
            if (TraitManager.Instance.IsTraitElemental(traitName)) {
                return TryAddElementalStatus(addTo, traitName, characterResponsible, gainedFromDoing, bypassElementalChance, overrideDuration);
            }
            return TraitAddition(addTo, traitName, characterResponsible, gainedFromDoing, overrideDuration);
        }
        private bool TryAddElementalStatus(ITraitable addTo, string traitName, Character characterResponsible, 
            ActualGoapNode gainedFromDoing, bool bypassElementalChance, int overrideDuration) {
            bool shouldAddTrait = ProcessBeforeAddingElementalStatus(addTo, traitName, bypassElementalChance);
            if (shouldAddTrait) {
                shouldAddTrait = ProcessBeforeSuccessfullyAddingElementalStatus(addTo, traitName, ref overrideDuration);
                if (shouldAddTrait) {
                    Trait trait = null;
                    shouldAddTrait = TraitAddition(addTo, traitName, out trait, characterResponsible, gainedFromDoing, overrideDuration);
                    if (shouldAddTrait) {
                        ProcessAfterSuccessfulAddingElementalTrait(addTo, trait as Status);
                    }
                }
            }
            return shouldAddTrait;
        }
        private bool TryAddElementalStatus(ITraitable addTo, string traitName, out Trait trait, Character characterResponsible, 
            ActualGoapNode gainedFromDoing, bool bypassElementalChance, int overrideDuration) {
            trait = null;
            bool shouldAddTrait = ProcessBeforeAddingElementalStatus(addTo, traitName, bypassElementalChance);
            if (shouldAddTrait) {
                shouldAddTrait = ProcessBeforeSuccessfullyAddingElementalStatus(addTo, traitName, ref overrideDuration);
                if (shouldAddTrait) {
                    shouldAddTrait = TraitAddition(addTo, traitName, out trait, characterResponsible, gainedFromDoing, overrideDuration);
                    if (shouldAddTrait) {
                        ProcessAfterSuccessfulAddingElementalTrait(addTo, trait as Status);
                    }
                }
            }
            return shouldAddTrait;
        }
        private bool TryAddElementalStatus(ITraitable addTo, Trait trait, Character characterResponsible, 
            ActualGoapNode gainedFromDoing, bool bypassElementalChance, int overrideDuration) {
            bool shouldAddTrait = ProcessBeforeAddingElementalStatus(addTo, trait.name, bypassElementalChance);
            if (shouldAddTrait) {
                shouldAddTrait = ProcessBeforeSuccessfullyAddingElementalStatus(addTo, trait.name, ref overrideDuration);
                if (shouldAddTrait) {
                    shouldAddTrait = TraitAddition(addTo, trait, characterResponsible, gainedFromDoing, overrideDuration);
                    if (shouldAddTrait) {
                        ProcessAfterSuccessfulAddingElementalTrait(addTo, trait as Status);
                    }
                }
            }
            return shouldAddTrait;
        }
        //Returns true or false, if trait should be added or not
        private bool ProcessBeforeAddingElementalStatus(ITraitable addTo, string traitName, bool bypassElementalChance) {
            bool shouldAddTrait = true;
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
                    RemoveStatusAndStacks(addTo, "Poisoned");
                    if (addTo is IPointOfInterest) {
                        CombatManager.Instance.PoisonExplosion(addTo as IPointOfInterest, addTo.gridTileLocation, poisonStacks);
                    }
                    shouldAddTrait = false;
                }
            } else if (traitName == "Poisoned") {
                if (HasTrait("Burning")) {
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
                } else if(addTo is GenericTileObject || addTo is StructureWallObject || addTo is BlockWall) {
                    if (!HasTrait("Wet")) {
                        //Ground floor tiles and walls do not get Zapped by electric damage unless they are Wet.
                        shouldAddTrait = false;
                    }
                }
            }
            if (shouldAddTrait) {
                if (bypassElementalChance) {
                    return true;
                }
                int roll = UnityEngine.Random.Range(0, 100);
                int chance = GetElementalTraitChanceToBeAdded(traitName);
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
                    AddTrait(addTo, "Frozen");
                    shouldAddTrait = false;
                }
            } else if (traitName == "Zapped") {
                if (HasTrait("Frozen")) {
                    RemoveTrait(addTo, "Frozen");
                    if(addTo is IPointOfInterest) {
                        CombatManager.Instance.FrozenExplosion(addTo as IPointOfInterest, addTo.gridTileLocation, 1);
                    }
                    shouldAddTrait = false;
                }
            } else if (traitName == "Poisoned") {
                if(addTo is TileObject) {
                    overrideDuration = GameManager.Instance.GetTicksBasedOnHour(24);
                }
            }
            return shouldAddTrait;
        }
        private void ProcessAfterSuccessfulAddingElementalTrait(ITraitable traitable, Status status) {
            if (status.name == "Freezing") {
                if (stacks[status.name] >= status.stackLimit) {
                    RemoveStatusAndStacks(traitable, status.name);
                    AddTrait(traitable, "Frozen");
                }
            }
        }
        private bool TraitAddition(ITraitable addTo, string traitName, Character characterResponsible, ActualGoapNode gainedFromDoing, int overrideDuration) {
            if (TraitManager.Instance.IsInstancedTrait(traitName)) {
                return AddTraitRoot(addTo, TraitManager.Instance.CreateNewInstancedTraitClass(traitName), characterResponsible, gainedFromDoing, overrideDuration);
            } else {
                return AddTraitRoot(addTo, TraitManager.Instance.allTraits[traitName], characterResponsible, gainedFromDoing, overrideDuration);
            }
        }
        private bool TraitAddition(ITraitable addTo, string traitName, out Trait trait, Character characterResponsible, ActualGoapNode gainedFromDoing, int overrideDuration) {
            if (TraitManager.Instance.IsInstancedTrait(traitName)) {
                trait = TraitManager.Instance.CreateNewInstancedTraitClass(traitName);
                return AddTraitRoot(addTo, trait, characterResponsible, gainedFromDoing, overrideDuration);
            } else {
                trait = TraitManager.Instance.allTraits[traitName];
                return AddTraitRoot(addTo, trait, characterResponsible, gainedFromDoing, overrideDuration);
            }
        }
        private bool TraitAddition(ITraitable addTo, Trait trait, Character characterResponsible, ActualGoapNode gainedFromDoing, int overrideDuration) {
            return AddTraitRoot(addTo, trait, characterResponsible, gainedFromDoing, overrideDuration);
        }
        private bool AddTraitRoot(ITraitable addTo, Trait trait, Character characterResponsible, ActualGoapNode gainedFromDoing, int overrideDuration) {
            //TODO: Either move or totally remove validation from inside this container
            if (TraitValidator.CanAddTrait(addTo, trait, this) == false) {
                return false;
            }
            if(trait is Status) {
                Status status = trait as Status;
                if (status.isStacking) {
                    if (stacks.ContainsKey(status.name)) {
                        stacks[status.name]++;
                        if (TraitManager.Instance.IsInstancedTrait(status.name)) {
                            Status existingStatus = GetNormalTrait<Status>(status.name);
                            addTo.traitProcessor.OnStatusStacked(addTo, existingStatus, characterResponsible, gainedFromDoing, overrideDuration);
                        } else {
                            addTo.traitProcessor.OnStatusStacked(addTo, status, characterResponsible, gainedFromDoing, overrideDuration);
                        }
                    } else {
                        stacks.Add(status.name, 1);
                        statuses.Add(status);
                        allTraitsAndStatuses.Add(status);
                        addTo.traitProcessor.OnTraitAdded(addTo, status, characterResponsible, gainedFromDoing, overrideDuration);
                    }
                } else {
                    statuses.Add(status);
                    allTraitsAndStatuses.Add(status);
                    addTo.traitProcessor.OnTraitAdded(addTo, status, characterResponsible, gainedFromDoing, overrideDuration);
                }
            } else {
                traits.Add(trait);
                allTraitsAndStatuses.Add(trait);
                addTo.traitProcessor.OnTraitAdded(addTo, trait, characterResponsible, gainedFromDoing, overrideDuration);
            }
            return true;
        }
        private int GetElementalTraitChanceToBeAdded(string traitName) {
            int chance = 100;
            if (traitName == "Burning") {
                chance = 15;
                if(HasTrait("Fireproof", "Wet", "Burnt") || !HasTrait("Flammable")) {
                    chance = 0;
                } else if (HasTrait("Poisoned")) {
                    chance = 100;
                }
            } else if (traitName == "Freezing") {
                chance = 20;
                if (HasTrait("Cold Blooded", "Burning")) {
                    chance = 0;
                } else if (HasTrait("Wet")) {
                    chance = 100;
                }
            }
            return chance;
        }
        #endregion

        #region Removing
        /// <summary>
        /// The main RemoveTrait function. All other RemoveTrait functions eventually call this.
        /// </summary>
        /// <returns>If the trait was removed or not.</returns>
        public bool RemoveTrait(ITraitable removeFrom, Trait trait, Character removedBy = null, bool bySchedule = false) {
            bool removedOrUnstacked = false;
            if(trait is Status) {
                Status status = trait as Status;
                if (!status.isStacking) {
                    removedOrUnstacked = statuses.Remove(status);
                    if (removedOrUnstacked) {
                        allTraitsAndStatuses.Remove(status);
                        removeFrom.traitProcessor.OnTraitRemoved(removeFrom, status, removedBy);
                        RemoveScheduleTicket(status.name, bySchedule);
                    }
                } else {
                    if (stacks.ContainsKey(status.name)) {
                        if (stacks[status.name] > 1) {
                            stacks[status.name]--;
                            removeFrom.traitProcessor.OnStatusUnstack(removeFrom, status, removedBy);
                            RemoveScheduleTicket(status.name, bySchedule);
                            removedOrUnstacked = true;
                        } else {
                            removedOrUnstacked = statuses.Remove(status);
                            if (removedOrUnstacked) {
                                allTraitsAndStatuses.Remove(status);
                                stacks.Remove(status.name);
                                removeFrom.traitProcessor.OnTraitRemoved(removeFrom, status, removedBy);
                                RemoveScheduleTicket(status.name, bySchedule);
                            }
                        }
                    }
                }
            } else {
                removedOrUnstacked = traits.Remove(trait);
                if (removedOrUnstacked) {
                    allTraitsAndStatuses.Remove(trait);
                    removeFrom.traitProcessor.OnTraitRemoved(removeFrom, trait, removedBy);
                    RemoveScheduleTicket(trait.name, bySchedule);
                }
            }
            return removedOrUnstacked;
        }
        public bool RemoveTrait(ITraitable removeFrom, string traitName, Character removedBy = null, bool bySchedule = false) {
            Trait trait = GetNormalTrait<Trait>(traitName);
            if (trait != null) {
                return RemoveTrait(removeFrom, trait, removedBy, bySchedule);
            }
            return false;
        }
        private void RemoveStatusAndStacks(ITraitable removeFrom, Status status, Character removedBy = null, bool bySchedule = false) {
            int loopNum = 1;
            if (stacks.ContainsKey(status.name)) {
                loopNum = stacks[status.name];
            }
            for (int i = 0; i < loopNum; i++) {
                RemoveTrait(removeFrom, status, removedBy, bySchedule);
            }
        }
        public void RemoveStatusAndStacks(ITraitable removeFrom, string name, Character removedBy = null, bool bySchedule = false) {
            Status trait = GetNormalTrait<Status>(name);
            if (trait != null) {
                RemoveStatusAndStacks(removeFrom, trait, removedBy, bySchedule);
            }
        }
        public bool RemoveStatus(ITraitable removeFrom, int index, Character removedBy = null) {
            bool removedOrUnstacked = true;
            if (index < 0 || index >= statuses.Count) {
                removedOrUnstacked = false;
            } else {
                Status status = statuses[index];
                if (!status.isStacking) {
                    statuses.RemoveAt(index);
                    allTraitsAndStatuses.Remove(status);
                    removeFrom.traitProcessor.OnTraitRemoved(removeFrom, status, removedBy);
                    RemoveScheduleTicket(status.name);
                } else {
                    if (stacks.ContainsKey(status.name)) {
                        if (stacks[status.name] > 1) {
                            stacks[status.name]--;
                            removeFrom.traitProcessor.OnStatusUnstack(removeFrom, status, removedBy);
                            RemoveScheduleTicket(status.name);
                        } else {
                            stacks.Remove(status.name);
                            statuses.RemoveAt(index);
                            allTraitsAndStatuses.Remove(status);
                            removeFrom.traitProcessor.OnTraitRemoved(removeFrom, status, removedBy);
                            RemoveScheduleTicket(status.name);
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
                allTraitsAndStatuses.Remove(trait);
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
                Trait trait = statuses[i];
                if (trait.type == traitType) {
                    if (RemoveStatus(removeFrom, i)) {
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
        public void RemoveAllTraitsAndStatusesByName(ITraitable removeFrom, string name) {
            //List<Trait> removedTraits = new List<Trait>();
            //List<Trait> all = new List<Trait>(allTraits);
            for (int i = 0; i < statuses.Count; i++) {
                Trait trait = statuses[i];
                if (trait.name == name) {
                    //removedTraits.Add(trait);
                    if (RemoveStatus(removeFrom, i)) {
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
        /// <summary>
        /// Remove all traits that are not persistent.
        /// </summary>
        public void RemoveAllNonPersistentTraitAndStatuses(ITraitable traitable) {
            //List<Trait> allTraits = new List<Trait>(this.allTraits);
            for (int i = 0; i < statuses.Count; i++) {
                Trait currTrait = statuses[i];
                if (!currTrait.isPersistent) {
                    if (RemoveStatus(traitable, i)) {
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
                if (RemoveStatus(traitable, i)) { //remove all traits
                    i--;
                }
            }
            for (int i = 0; i < traits.Count; i++) {
                if (RemoveTrait(traitable, i)) { //remove all traits
                    i--;
                }
            }
        }
        #endregion

        #region Getting
        public T GetNormalTrait<T>(params string[] traitNames) where T : Trait {
            for (int i = 0; i < allTraitsAndStatuses.Count; i++) {
                Trait trait = allTraitsAndStatuses[i];
                for (int j = 0; j < traitNames.Length; j++) {
                    if (trait.name == traitNames[j]) { // || trait.GetType().ToString() == traitNames[j]
                        return trait as T;
                    }
                }
            }
            return null;
        }
        public List<T> GetNormalTraits<T>(params string[] traitNames) where T : Trait {
            List<T> traits = new List<T>();
            for (int i = 0; i < allTraitsAndStatuses.Count; i++) {
                Trait trait = allTraitsAndStatuses[i];
                if (traitNames.Contains(trait.name)) {
                    traits.Add(trait as T);
                }
            }
            return traits;
        }
        public bool HasTraitOf(TRAIT_TYPE traitType) {
            for (int i = 0; i < allTraitsAndStatuses.Count; i++) {
                if (allTraitsAndStatuses[i].type == traitType) {
                    return true;
                }
            }
            return false;
        }
        public bool HasTraitOf(TRAIT_TYPE type, TRAIT_EFFECT effect) {
            for (int i = 0; i < allTraitsAndStatuses.Count; i++) {
                Trait currTrait = allTraitsAndStatuses[i];
                if (currTrait.effect == effect && currTrait.type == type) {
                    return true;
                }
            }
            return false;
        }
        public bool HasTraitOf(TRAIT_EFFECT traitEffect) {
            for (int i = 0; i < allTraitsAndStatuses.Count; i++) {
                Trait currTrait = allTraitsAndStatuses[i];
                if (currTrait.effect == traitEffect) {
                    return true;
                }
            }
            return false;
        }
        public List<Trait> GetAllTraitsOf(TRAIT_TYPE type) {
            List<Trait> traits = new List<Trait>();
            for (int i = 0; i < allTraitsAndStatuses.Count; i++) {
                Trait currTrait = allTraitsAndStatuses[i];
                if (currTrait.type == type) {
                    traits.Add(currTrait);
                }
            }
            return traits;
        }
        public List<Trait> GetAllTraitsOf(TRAIT_TYPE type, TRAIT_EFFECT effect) {
            List<Trait> traits = new List<Trait>();
            for (int i = 0; i < allTraitsAndStatuses.Count; i++) {
                Trait currTrait = allTraitsAndStatuses[i];
                if (currTrait.effect == effect && currTrait.type == type) {
                    traits.Add(currTrait);
                }
            }
            return traits;
        }
        #endregion

        #region Processes
        public void ProcessOnTickStarted(ITraitable owner) {
            if(allTraitsAndStatuses != null) {
                for (int i = 0; i < allTraitsAndStatuses.Count; i++) {
                    allTraitsAndStatuses[i].OnTickStarted();
                }
            }
        }
        public void ProcessOnTickEnded(ITraitable owner) {
            if (allTraitsAndStatuses != null) {
                for (int i = 0; i < allTraitsAndStatuses.Count; i++) {
                    Trait trait = allTraitsAndStatuses[i];
                    trait.OnTickEnded();
                    //if (currentDurations.ContainsKey(trait)) {
                    //    currentDurations[trait]++;
                    //    if(currentDurations[trait] >= trait.ticksDuration) {
                    //        int prevCount = allTraits.Count;
                    //        bool removed = RemoveTrait(owner, i);
                    //        if (removed) {
                    //            if(allTraits.Count != prevCount) {
                    //                i--;
                    //            }
                    //        }
                    //    }
                    //}
                }
            }
        }
        public void ProcessOnHourStarted(ITraitable owner) {
            if (allTraitsAndStatuses != null) {
                for (int i = 0; i < allTraitsAndStatuses.Count; i++) {
                    allTraitsAndStatuses[i].OnHourStarted();
                }
            }
        }
        #endregion
        
        #region Schedule Tickets
        public void AddScheduleTicket(string traitName, string ticket) {
            if (scheduleTickets.ContainsKey(traitName)) {
                scheduleTickets[traitName].Add(ticket);
            } else {
                scheduleTickets.Add(traitName, new List<string>() { ticket });
            }
        }
        public void RemoveScheduleTicket(string traitName, bool bySchedule = false) {
            if (scheduleTickets.ContainsKey(traitName)) {
                if (!bySchedule) {
                    SchedulingManager.Instance.RemoveSpecificEntry(scheduleTickets[traitName][0]);
                }
                if (scheduleTickets[traitName].Count <= 0) {
                    scheduleTickets.Remove(traitName);
                } else {
                    scheduleTickets[traitName].RemoveAt(0);
                }
            } 
        }
        #endregion
        
        #region Switches
        public void SwitchOnTrait(string name) {
            if (traitSwitches.ContainsKey(name)) {
                traitSwitches[name] = true;
            } else {
                traitSwitches.Add(name, true);
            }
        }
        public void SwitchOffTrait(string name) {
            if (traitSwitches.ContainsKey(name)) {
                traitSwitches[name] = false;
            } else {
                traitSwitches.Add(name, false);
            }
        }
        private bool HasTraitSwitch(string name) {
            if (traitSwitches.ContainsKey(name)) {
                return traitSwitches[name];
            }
            return false;
        }
        public bool HasTrait(params string[] traitNames) {
            for (int i = 0; i < traitNames.Length; i++) {
                if (HasTraitSwitch(traitNames[i])) {
                    return true;
                }
            }
            return false;
        }
        #endregion
        
        #region Trait Override Functions
        public void AddOnCollideWithTrait(Trait trait) {
            onCollideWithTraits.Add(trait);
        }
        public bool RemoveOnCollideWithTrait(Trait trait) {
            return onCollideWithTraits.Remove(trait);
        }
        public void AddOnEnterGridTileTrait(Trait trait) {
            onEnterGridTileTraits.Add(trait);
        }
        public bool RemoveOnEnterGridTileTrait(Trait trait) {
            return onEnterGridTileTraits.Remove(trait);
        }
        #endregion
    }
}

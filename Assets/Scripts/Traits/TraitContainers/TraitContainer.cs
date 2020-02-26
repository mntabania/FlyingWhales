using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps;

namespace Traits {
    public class TraitContainer : ITraitContainer {

        private List<Trait> _allTraits;
        public List<Trait> onCollideWithTraits { get; private set; }
        
        public Dictionary<string, int> stacks { get; private set; }
        public Dictionary<string, List<string>> scheduleTickets { get; private set; }
        public Dictionary<string, bool> traitSwitches { get; private set; }
        //public Dictionary<Trait, int> currentDurations { get; private set; } //Temporary only, fix this by making all traits instanced based and just object pool them

        #region getters/setters
        public List<Trait> allTraits { get { return _allTraits; } }
        #endregion

        public TraitContainer() {
            _allTraits = new List<Trait>();
            onCollideWithTraits = new List<Trait>();
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
            ActualGoapNode gainedFromDoing = null, bool bypassElementalChance = false) {
            if (TraitManager.Instance.IsTraitElemental(trait.name)) {
                return TryAddElementalTrait(addTo, trait, characterResponsible, gainedFromDoing, bypassElementalChance);
            }
            return TraitAddition(addTo, trait, characterResponsible, gainedFromDoing);
        }
        public bool AddTrait(ITraitable addTo, string traitName, out Trait trait, Character characterResponsible = null, 
            ActualGoapNode gainedFromDoing = null, bool bypassElementalChance = false) {
            if (TraitManager.Instance.IsTraitElemental(traitName)) {
                return TryAddElementalTrait(addTo, traitName, out trait, characterResponsible, 
                    gainedFromDoing, bypassElementalChance);
            }
            return TraitAddition(addTo, traitName, out trait, characterResponsible, gainedFromDoing);
        }
        public bool AddTrait(ITraitable addTo, string traitName, Character characterResponsible = null, 
            ActualGoapNode gainedFromDoing = null, bool bypassElementalChance = false) {
            if (TraitManager.Instance.IsTraitElemental(traitName)) {
                return TryAddElementalTrait(addTo, traitName, characterResponsible, gainedFromDoing, bypassElementalChance);
            }
            return TraitAddition(addTo, traitName, characterResponsible, gainedFromDoing);
        }
        private bool TryAddElementalTrait(ITraitable addTo, string traitName, Character characterResponsible, 
            ActualGoapNode gainedFromDoing, bool bypassElementalChance) {
            bool shouldAddTrait = ProcessBeforeAddingElementalTrait(addTo, traitName, bypassElementalChance);
            if (shouldAddTrait) {
                shouldAddTrait = ProcessBeforeSuccessfullyAddingElementalTrait(addTo, traitName);
                if (shouldAddTrait) {
                    Trait trait = null;
                    shouldAddTrait = TraitAddition(addTo, traitName, out trait, characterResponsible, gainedFromDoing);
                    if (shouldAddTrait) {
                        ProcessAfterSuccessfulAddingElementalTrait(addTo, trait);
                    }
                }
            }
            return shouldAddTrait;
        }
        private bool TryAddElementalTrait(ITraitable addTo, string traitName, out Trait trait, Character characterResponsible, 
            ActualGoapNode gainedFromDoing, bool bypassElementalChance) {
            trait = null;
            bool shouldAddTrait = ProcessBeforeAddingElementalTrait(addTo, traitName, bypassElementalChance);
            if (shouldAddTrait) {
                shouldAddTrait = ProcessBeforeSuccessfullyAddingElementalTrait(addTo, traitName);
                if (shouldAddTrait) {
                    shouldAddTrait = TraitAddition(addTo, traitName, out trait, characterResponsible, gainedFromDoing);
                    if (shouldAddTrait) {
                        ProcessAfterSuccessfulAddingElementalTrait(addTo, trait);
                    }
                }
            }
            return shouldAddTrait;
        }
        private bool TryAddElementalTrait(ITraitable addTo, Trait trait, Character characterResponsible, 
            ActualGoapNode gainedFromDoing, bool bypassElementalChance) {
            bool shouldAddTrait = ProcessBeforeAddingElementalTrait(addTo, trait.name, bypassElementalChance);
            if (shouldAddTrait) {
                shouldAddTrait = ProcessBeforeSuccessfullyAddingElementalTrait(addTo, trait.name);
                if (shouldAddTrait) {
                    shouldAddTrait = TraitAddition(addTo, trait, characterResponsible, gainedFromDoing);
                    if (shouldAddTrait) {
                        ProcessAfterSuccessfulAddingElementalTrait(addTo, trait);
                    }
                }
            }
            return shouldAddTrait;
        }
        //Returns true or false, if trait should be added or not
        private bool ProcessBeforeAddingElementalTrait(ITraitable addTo, string traitName, bool bypassElementalChance) {
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
                    RemoveTraitAndStacks(addTo, "Poisoned");
                    if (addTo is IPointOfInterest) {
                        CombatManager.Instance.PoisonExplosion(addTo as IPointOfInterest, addTo.gridTileLocation, poisonStacks);
                    }
                    shouldAddTrait = false;
                }
            } else if (traitName == "Overheating") {
                if (HasTrait("Wet")) {
                    RemoveTraitAndStacks(addTo, "Wet");
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
                if(addTo is GenericTileObject || addTo is StructureWallObject || addTo is BlockWall) {
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
        private bool ProcessBeforeSuccessfullyAddingElementalTrait(ITraitable addTo, string traitName) {
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
            }
            return shouldAddTrait;
        }
        private void ProcessAfterSuccessfulAddingElementalTrait(ITraitable traitable, Trait trait) {
            if (trait.name == "Freezing") {
                if (stacks[trait.name] >= trait.stackLimit) {
                    RemoveTraitAndStacks(traitable, trait.name);
                    AddTrait(traitable, "Frozen");
                }
            }
        }
        private bool TraitAddition(ITraitable addTo, string traitName, Character characterResponsible, ActualGoapNode gainedFromDoing) {
            if (TraitManager.Instance.IsInstancedTrait(traitName)) {
                return AddTraitRoot(addTo, TraitManager.Instance.CreateNewInstancedTraitClass(traitName), characterResponsible, gainedFromDoing);
            } else {
                return AddTraitRoot(addTo, TraitManager.Instance.allTraits[traitName], characterResponsible, gainedFromDoing);
            }
        }
        private bool TraitAddition(ITraitable addTo, string traitName, out Trait trait, Character characterResponsible, ActualGoapNode gainedFromDoing) {
            if (TraitManager.Instance.IsInstancedTrait(traitName)) {
                trait = TraitManager.Instance.CreateNewInstancedTraitClass(traitName);
                return AddTraitRoot(addTo, trait, characterResponsible, gainedFromDoing);
            } else {
                trait = TraitManager.Instance.allTraits[traitName];
                return AddTraitRoot(addTo, trait, characterResponsible, gainedFromDoing);
            }
        }
        private bool TraitAddition(ITraitable addTo, Trait trait, Character characterResponsible, ActualGoapNode gainedFromDoing) {
            return AddTraitRoot(addTo, trait, characterResponsible, gainedFromDoing);
        }
        private bool AddTraitRoot(ITraitable addTo, Trait trait, Character characterResponsible = null, ActualGoapNode gainedFromDoing = null) {
            //TODO: Either move or totally remove validation from inside this container
            if (TraitValidator.CanAddTrait(addTo, trait, this) == false) {
                //if (trait.IsUnique()) {
                //    Trait oldTrait = GetNormalTrait<Trait>(trait.name);
                //    if (oldTrait != null) {
                //        oldTrait.AddCharacterResponsibleForTrait(characterResponsible);
                //        oldTrait.AddCharacterResponsibleForTrait(characterResponsible);
                //        //if (oldTrait.broadcastDuplicates) {
                //        //    Messenger.Broadcast(Signals.TRAIT_ADDED, this, oldTrait);
                //        //}
                //    }
                //}
                return false;
            }

            if (trait.isStacking) {
                if (stacks.ContainsKey(trait.name)) {
                    stacks[trait.name]++;
                    if (TraitManager.Instance.IsInstancedTrait(trait.name)) {
                        Trait existingTrait = GetNormalTrait<Trait>(trait.name);
                        addTo.traitProcessor.OnTraitStacked(addTo, existingTrait, characterResponsible, gainedFromDoing);
                    } else {
                        addTo.traitProcessor.OnTraitStacked(addTo, trait, characterResponsible, gainedFromDoing);
                    }
                } else {
                    stacks.Add(trait.name, 1);
                    _allTraits.Add(trait);
                    addTo.traitProcessor.OnTraitAdded(addTo, trait, characterResponsible, gainedFromDoing);
                }
            } else {
                _allTraits.Add(trait);
                addTo.traitProcessor.OnTraitAdded(addTo, trait, characterResponsible, gainedFromDoing);
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
            if (!trait.isStacking) {
                removedOrUnstacked = _allTraits.Remove(trait);
                if (removedOrUnstacked) {
                    removeFrom.traitProcessor.OnTraitRemoved(removeFrom, trait, removedBy);
                    RemoveScheduleTicket(trait.name, bySchedule);
                }
            } else {
                if (stacks.ContainsKey(trait.name)) {
                    if(stacks[trait.name] > 1) {
                        stacks[trait.name]--;
                        removeFrom.traitProcessor.OnTraitUnstack(removeFrom, trait, removedBy);
                        RemoveScheduleTicket(trait.name, bySchedule);
                        removedOrUnstacked = true;
                    } else {
                        removedOrUnstacked = _allTraits.Remove(trait);
                        if (removedOrUnstacked) {
                            stacks.Remove(trait.name);
                            removeFrom.traitProcessor.OnTraitRemoved(removeFrom, trait, removedBy);
                            RemoveScheduleTicket(trait.name, bySchedule);
                        }
                    }
                }
                //else {
                //    throw new Exception("Removing stack of " + trait.name + " trait from " + removeFrom.name + " but there is not stack, this should not happen...");
                //}
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
        private void RemoveTraitAndStacks(ITraitable removeFrom, Trait trait, Character removedBy = null, bool bySchedule = false) {
            int loopNum = 1;
            if (stacks.ContainsKey(trait.name)) {
                loopNum = stacks[trait.name];
            }
            for (int i = 0; i < loopNum; i++) {
                RemoveTrait(removeFrom, trait, removedBy, bySchedule);
            }
        }
        public void RemoveTraitAndStacks(ITraitable removeFrom, string name, Character removedBy = null, bool bySchedule = false) {
            Trait trait = GetNormalTrait<Trait>(name);
            if (trait != null) {
                RemoveTraitAndStacks(removeFrom, trait, removedBy, bySchedule);
            }
        }
        public bool RemoveTrait(ITraitable removeFrom, int index, Character removedBy = null) {
            bool removedOrUnstacked = true;
            if(index < 0 || index >= _allTraits.Count) {
                removedOrUnstacked = false;
            } else {
                Trait trait = _allTraits[index];
                if (!trait.isStacking) {
                    _allTraits.RemoveAt(index);
                    removeFrom.traitProcessor.OnTraitRemoved(removeFrom, trait, removedBy);
                    RemoveScheduleTicket(trait.name);
                } else {
                    if (stacks.ContainsKey(trait.name)) {
                        if (stacks[trait.name] > 1) {
                            stacks[trait.name]--;
                            removeFrom.traitProcessor.OnTraitUnstack(removeFrom, trait, removedBy);
                            RemoveScheduleTicket(trait.name);
                        } else {
                            stacks.Remove(trait.name);
                            _allTraits.RemoveAt(index);
                            removeFrom.traitProcessor.OnTraitRemoved(removeFrom, trait, removedBy);
                            RemoveScheduleTicket(trait.name);
                        }
                    } 
                    //else {
                    //    throw new Exception("Removing stack of " + trait.name + " trait from " + removeFrom.name + " but there is not stack, this should not happen...");
                    //}
                }
            }
            return removedOrUnstacked;
        }
        public void RemoveTrait(ITraitable removeFrom, List<Trait> traits) {
            for (int i = 0; i < traits.Count; i++) {
                RemoveTrait(removeFrom, traits[i]);
            }
        }
        public List<Trait> RemoveAllTraitsByType(ITraitable removeFrom, TRAIT_TYPE traitType) {
            List<Trait> removedTraits = new List<Trait>();
            //List<Trait> all = new List<Trait>(allTraits);
            for (int i = 0; i < allTraits.Count; i++) {
                Trait trait = allTraits[i];
                if (trait.type == traitType) {
                    removedTraits.Add(trait);
                    if(RemoveTrait(removeFrom, i)) {
                        i--;
                    }
                }
            }
            return removedTraits;
        }
        public void RemoveAllTraitsByName(ITraitable removeFrom, string name) {
            //List<Trait> removedTraits = new List<Trait>();
            //List<Trait> all = new List<Trait>(allTraits);
            for (int i = 0; i < allTraits.Count; i++) {
                Trait trait = allTraits[i];
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
                trait.OnRemoveTraitBySchedule(removeFrom);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Remove all traits that are not persistent.
        /// </summary>
        public void RemoveAllNonPersistentTraits(ITraitable traitable) {
            //List<Trait> allTraits = new List<Trait>(this.allTraits);
            for (int i = 0; i < allTraits.Count; i++) {
                Trait currTrait = allTraits[i];
                if (!currTrait.isPersistent) {
                    if(RemoveTrait(traitable, i)) {
                        i--;
                    }
                }
            }
        }
        public void RemoveAllTraits(ITraitable traitable) {
            //List<Trait> allTraits = new List<Trait>(this.allTraits);
            for (int i = 0; i < allTraits.Count; i++) {
                if (RemoveTrait(traitable, i)) { //remove all traits
                    i--;
                }
            }
        }
        #endregion

        #region Getting
        public T GetNormalTrait<T>(params string[] traitNames) where T : Trait {
            for (int i = 0; i < allTraits.Count; i++) {
                Trait trait = allTraits[i];
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
            for (int i = 0; i < allTraits.Count; i++) {
                Trait trait = allTraits[i];
                if (traitNames.Contains(trait.name)) {
                    traits.Add(trait as T);
                }
            }
            return traits;
        }
        public bool HasTraitOf(TRAIT_TYPE traitType) {
            for (int i = 0; i < allTraits.Count; i++) {
                if (allTraits[i].type == traitType) {
                    return true;
                }
            }
            return false;
        }
        public bool HasTraitOf(TRAIT_TYPE type, TRAIT_EFFECT effect) {
            for (int i = 0; i < allTraits.Count; i++) {
                Trait currTrait = allTraits[i];
                if (currTrait.effect == effect && currTrait.type == type) {
                    return true;
                }
            }
            return false;
        }
        public bool HasTraitOf(TRAIT_EFFECT traitEffect) {
            for (int i = 0; i < allTraits.Count; i++) {
                Trait currTrait = allTraits[i];
                if (currTrait.effect == traitEffect) {
                    return true;
                }
            }
            return false;
        }
        public List<Trait> GetAllTraitsOf(TRAIT_TYPE type) {
            List<Trait> traits = new List<Trait>();
            for (int i = 0; i < allTraits.Count; i++) {
                Trait currTrait = allTraits[i];
                if (currTrait.type == type) {
                    traits.Add(currTrait);
                }
            }
            return traits;
        }
        public List<Trait> GetAllTraitsOf(TRAIT_TYPE type, TRAIT_EFFECT effect) {
            List<Trait> traits = new List<Trait>();
            for (int i = 0; i < allTraits.Count; i++) {
                Trait currTrait = allTraits[i];
                if (currTrait.effect == effect && currTrait.type == type) {
                    traits.Add(currTrait);
                }
            }
            return traits;
        }
        #endregion

        #region Processes
        public void ProcessOnTickStarted(ITraitable owner) {
            if(allTraits != null) {
                for (int i = 0; i < allTraits.Count; i++) {
                    allTraits[i].OnTickStarted();
                }
            }
        }
        public void ProcessOnTickEnded(ITraitable owner) {
            if (allTraits != null) {
                for (int i = 0; i < allTraits.Count; i++) {
                    Trait trait = allTraits[i];
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
            if (allTraits != null) {
                for (int i = 0; i < allTraits.Count; i++) {
                    allTraits[i].OnHourStarted();
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
        #endregion
    }
}

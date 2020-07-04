using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

namespace Traits {
    public class Overheating : Status {
        //public override bool isSingleton => true;
        public ITraitable traitable { get; private set; }
        public List<LocationStructure> excludedStructuresInSeekingShelter { get; private set; }
        public LocationStructure currentShelterStructure { get; private set; }

        public Overheating() {
            name = "Overheating";
            description = "This is overheating.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(2);
            isStacking = true;
            moodEffect = -6;
            stackLimit = 3;
            stackModifier = 1.5f;
            excludedStructuresInSeekingShelter = new List<LocationStructure>();
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            traitable = addedTo;
            if (addedTo is Character character) {
                Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
            }
        }
        public override void OnStackStatus(ITraitable addedTo) {
            base.OnStackStatus(addedTo);
            if (addedTo is Character character) {
                int stacks = character.traitContainer.stacks[name];
                if (stacks >= 1 && stacks < stackLimit && !character.combatComponent.isInCombat) {
                    character.jobComponent.TriggerSeekShelterJob();
                }
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                if (character.trapStructure.forcedStructure == currentShelterStructure) {
                    character.trapStructure.SetForcedStructure(null);
                }
                Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
            }
        }
        #endregion

        #region General
        public void AddExcludedStructureInSeekingShelter(LocationStructure structure) {
            excludedStructuresInSeekingShelter.Add(structure);
        }
        public bool IsStructureExludedInSeekingShelter(LocationStructure structure) {
            return excludedStructuresInSeekingShelter.Contains(structure);
        }
        public void SetCurrentShelterStructure(LocationStructure structure) {
            currentShelterStructure = structure;
        }
        private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
            if (traitable is Character owner && owner == character) {
                if (currentShelterStructure != null && currentShelterStructure != structure) {
                    AddExcludedStructureInSeekingShelter(currentShelterStructure);
                    SetCurrentShelterStructure(null);
                    owner.trapStructure.SetForcedStructure(null);
                }
            }
        }
        #endregion
    }
}

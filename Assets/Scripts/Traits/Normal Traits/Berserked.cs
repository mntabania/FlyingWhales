using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Traits {
    public class Berserked : Status {
        private Character _owner;
        
        public Berserked() {
            name = "Berserked";
            description = "Mentally not there and is just rampaging like crazy.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(6);
            hindersWitness = true;
            hindersSocials = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Initiate_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Destroy_Map_Visual_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                _owner = character;
                if (character.marker) {
                    character.marker.BerserkedMarker();
                    character.marker.visionCollider.VoteToUnFilterVision();
                }
                character.jobQueue.CancelAllJobs();
                character.behaviourComponent.AddBehaviourComponent(typeof(BerserkBehaviour));
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                if (character.marker) {
                    if (!character.traitContainer.HasTrait("Agitated")) {
                        character.marker.UnberserkedMarker();
                    }
                    character.marker.visionCollider.VoteToFilterVision();
                }
                //check hostiles in range, remove any poi's that are not hostile with the character 
                List<IPointOfInterest> hostilesToRemove = new List<IPointOfInterest>();
                for (int i = 0; i < character.combatComponent.hostilesInRange.Count; i++) {
                    IPointOfInterest poi = character.combatComponent.hostilesInRange[i];
                    if (poi is Character) {
                        //poi is a character, check for hostilities
                        Character otherCharacter = poi as Character;
                        if (character.IsHostileWith(otherCharacter) == false || otherCharacter.isDead) {
                            hostilesToRemove.Add(otherCharacter);
                        }    
                    } else {
                        //poi is not a character, remove
                        hostilesToRemove.Add(poi);
                    }
                }

                //remove all non hostiles from hostile in range
                for (int i = 0; i < hostilesToRemove.Count; i++) {
                    IPointOfInterest hostile = hostilesToRemove[i];
                    character.combatComponent.RemoveHostileInRange(hostile);
                }
                character.behaviourComponent.RemoveBehaviourComponent(typeof(BerserkBehaviour));
                character.needsComponent.CheckExtremeNeeds();
            }
        }
        public override void OnInitiateMapObjectVisual(ITraitable traitable) {
            if (traitable is Character character) {
                if (character.marker) {
                    character.marker.BerserkedMarker();
                }
            }
        }
        public override void OnDestroyMapObjectVisual(ITraitable traitable) {
            if (_owner != null) {
                if (_owner.marker) {
                    if (!_owner.traitContainer.HasTrait("Agitated")) {
                        _owner.marker.UnberserkedMarker();
                    }
                    _owner.marker.visionCollider.VoteToFilterVision();
                }
            }
        }
        //public override void OnSeePOIEvenCannotWitness(IPointOfInterest targetPOI, Character character) {
        //    base.OnSeePOIEvenCannotWitness(targetPOI, character);
        //    BerserkCombat(targetPOI, character);
        //}
        public void BerserkCombat(IPointOfInterest targetPOI, Character character) {
            if (targetPOI is Character) {
                Character targetCharacter = targetPOI as Character;
                if (!targetCharacter.isDead) {
                    if (character.faction.isPlayerFaction) {
                        character.combatComponent.Fight(targetCharacter, CombatManager.Berserked, isLethal: true); //check hostility if from player faction, so as not to attack other characters that are also from the same faction.
                    } else {
                        if (!targetCharacter.traitContainer.HasTrait("Unconscious")) {
                            character.combatComponent.Fight(targetCharacter, CombatManager.Berserked, isLethal: false);
                        }
                    }
                }
            } else if (targetPOI is TileObject) { // || targetPOI is SpecialToken
                if (Random.Range(0, 100) < 35) {
                    //character.jobComponent.TriggerDestroy(targetPOI);
                    character.combatComponent.Fight(targetPOI, CombatManager.Berserked, isLethal: false);
                }
            }
        }
        //public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
        //    if (targetPOI is Character) {
        //        Character targetCharacter = targetPOI as Character;
        //        if (!targetCharacter.isDead) {
        //            if (characterThatWillDoJob.faction.isPlayerFaction) {
        //                return characterThatWillDoJob.combatComponent.AddHostileInRange(targetCharacter, isLethal: true); //check hostility if from player faction, so as not to attack other characters that are also from the same faction.
        //            } else {
        //                return characterThatWillDoJob.combatComponent.AddHostileInRange(targetCharacter, checkHostility: false, isLethal: false);
        //            }
        //        }
        //    }
        //    else if (targetPOI is TileObject || targetPOI is SpecialToken) {
        //        return characterThatWillDoJob.combatComponent.AddHostileInRange(targetPOI, checkHostility: false, isLethal: false);
        //    } 
        //    return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
        //}
        //public override void OnTickStarted() {
        //    base.OnTickStarted();
        //    if (_owner.stateComponent.currentState is CombatState) {
        //        CheckForChaosOrb();
        //    }
        //}
        #endregion
    }
}


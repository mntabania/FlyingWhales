using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Cannibal : Trait {
        public override bool isSingleton => true;

        public Cannibal() {
            name = "Cannibal";
            description = "Not a very picky eater.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            canBeTriggered = true;
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            base.OnAddTrait(sourcePOI);
            if (sourcePOI is Character) {
                Character owner = sourcePOI as Character;
                GoapPlanJob job = owner.jobQueue.GetJob(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT) as GoapPlanJob;
                if (job != null) {
                    job.CancelJob(false);
                }
            }
        }
        //public override void OnRemoveTrait(IPointOfInterest sourcePOI) {
        //    base.OnRemoveTrait(sourcePOI);
        //}
        protected override void OnChangeLevel() {
            base.OnChangeLevel();
            //if (level == 1) {
            //    daysDuration = GameManager.Instance.GetTicksBasedOnHour(3);
            //} else if (level == 2) {
            //    daysDuration = GameManager.Instance.GetTicksBasedOnHour(6);
            //} else if (level == 3) {
            //    daysDuration = GameManager.Instance.GetTicksBasedOnHour(9);
            //}
        }
        public override string TriggerFlaw(Character character) {
            string successLogKey = base.TriggerFlaw(character);
            IPointOfInterest poi = GetPOIToTransformToFood(character);
            if (poi != null) {
                if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.BUTCHER, poi, character);
                    character.jobQueue.AddJobInQueue(job);
                    return successLogKey;
                } else {
                    return "has_trigger_flaw";
                }
            } else {
                return "no_target";
            }
        }
        #endregion

        private IPointOfInterest GetPOIToTransformToFood(Character characterThatWillDoJob) {
            IPointOfInterest chosenPOI = null;
            for (int i = 0; i < characterThatWillDoJob.currentRegion.charactersAtLocation.Count; i++) {
                Character otherCharacter = characterThatWillDoJob.currentRegion.charactersAtLocation[i];
                if (characterThatWillDoJob != otherCharacter && otherCharacter.isDead && otherCharacter.isNormalCharacter &&
                    otherCharacter.gridTileLocation != null && characterThatWillDoJob.movementComponent.HasPathTo(otherCharacter.gridTileLocation)) {
                    if (otherCharacter.grave != null) {
                        chosenPOI = otherCharacter.grave;
                    } else {
                        chosenPOI = otherCharacter;
                    }
                    break;
                }
            }

            //if no dead characters were found then target enemies
            if (chosenPOI == null) {
                for (int i = 0; i < characterThatWillDoJob.currentRegion.charactersAtLocation.Count; i++) {
                    Character otherCharacter = characterThatWillDoJob.currentRegion.charactersAtLocation[i];
                    if (characterThatWillDoJob != otherCharacter && otherCharacter.isNormalCharacter && characterThatWillDoJob.relationshipContainer.IsEnemiesWith(otherCharacter) &&
                        otherCharacter.gridTileLocation != null && characterThatWillDoJob.movementComponent.HasPathTo(otherCharacter.gridTileLocation)) {
                        chosenPOI = otherCharacter;
                        break;
                    }
                }
            }

            if (chosenPOI == null) {
                for (int i = 0; i < characterThatWillDoJob.currentRegion.charactersAtLocation.Count; i++) {
                    Character otherCharacter = characterThatWillDoJob.currentRegion.charactersAtLocation[i];
                    string opinionLabel = characterThatWillDoJob.relationshipContainer.GetOpinionLabel(otherCharacter);
                    if (characterThatWillDoJob != otherCharacter && otherCharacter.isNormalCharacter && (opinionLabel == RelationshipManager.Acquaintance || string.IsNullOrEmpty(opinionLabel)) &&
                        otherCharacter.gridTileLocation != null && characterThatWillDoJob.movementComponent.HasPathTo(otherCharacter.gridTileLocation)) {
                        chosenPOI = otherCharacter;
                        break;
                    }
                }
            }
            return chosenPOI;
        }
    }
}


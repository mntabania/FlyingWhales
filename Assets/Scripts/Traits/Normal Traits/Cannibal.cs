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
                    job.CancelJob();
                }
            }
        }
        public override string TriggerFlaw(Character character) {
            string successLogKey = base.TriggerFlaw(character);
            if (character.traitContainer.HasTrait("Vampire")) {
                Character targetCharacter = GetDrinkBloodTarget(character);
                if (targetCharacter != null) {
                    if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
                        character.jobComponent.CreateDrinkBloodJob(JOB_TYPE.TRIGGER_FLAW, targetCharacter);
                        return successLogKey;
                    } else {
                        return "has_trigger_flaw";
                    }
                } else {
                    return "no_target_vampire";
                }
            } else {
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
        }
        #endregion

        private IPointOfInterest GetPOIToTransformToFood(Character characterThatWillDoJob) {
            IPointOfInterest chosenPOI = null;
            for (int i = 0; i < characterThatWillDoJob.currentRegion.charactersAtLocation.Count; i++) {
                Character otherCharacter = characterThatWillDoJob.currentRegion.charactersAtLocation[i];
                if (characterThatWillDoJob != otherCharacter && otherCharacter.isDead && otherCharacter.isNormalCharacter &&
                    otherCharacter.gridTileLocation != null && characterThatWillDoJob.movementComponent.HasPathTo(otherCharacter.gridTileLocation)) {
                    // if (otherCharacter.grave != null) {
                    //     chosenPOI = otherCharacter.grave;
                    // } else {
                        chosenPOI = otherCharacter;
                    // }
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

        private Character GetDrinkBloodTarget(Character vampire) {
            Character chosenTarget = null;
            List<Character> targets = ObjectPoolManager.Instance.CreateNewCharactersList();
            if (vampire.currentRegion != null) {
                for (int i = 0; i < vampire.currentRegion.charactersAtLocation.Count; i++) {
                    Character character = vampire.currentRegion.charactersAtLocation[i];
                    if (vampire != character) {
                        if (character.traitContainer.HasTrait("Vampire") && character.carryComponent.IsNotBeingCarried() && character.Advertises(INTERACTION_TYPE.DRINK_BLOOD) && !character.isDead
                            && vampire.movementComponent.HasPathToEvenIfDiffRegion(character.gridTileLocation)) {
                            targets.Add(character);
                        }
                    }
                }
            }
            if (targets != null && targets.Count > 0) {
                chosenTarget = UtilityScripts.CollectionUtilities.GetRandomElement(targets);
            }
            ObjectPoolManager.Instance.ReturnCharactersListToPool(targets);
            return chosenTarget;
        }
    }
}


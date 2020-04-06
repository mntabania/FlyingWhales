using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

namespace Traits {
    public class Kleptomaniac : Trait {
        public List<Character> noItemCharacters { get; private set; }
        private Character traitOwner;

        //private int _happinessDecreaseRate;
        public Kleptomaniac() {
            name = "Kleptomaniac";
            description = "Kleptomaniacs enjoy stealing.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            //effects = new List<TraitEffect>();
            noItemCharacters = new List<Character>();
            canBeTriggered = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Execute_Expected_Effect_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Death_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            //(sourceCharacter as Character).RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "afflicted", null, "Kleptomania");
            traitOwner = sourceCharacter as Character;
            //traitOwner.needsComponent.AdjustHappinessDecreaseRate(_happinessDecreaseRate);
            base.OnAddTrait(sourceCharacter);
            //owner.AddInteractionType(INTERACTION_TYPE.STEAL);
            Messenger.AddListener(Signals.DAY_STARTED, CheckForClearNoItemsList);
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            base.OnRemoveTrait(sourceCharacter, removedBy);
            //owner.RemoveInteractionType(INTERACTION_TYPE.STEAL);
            //traitOwner.needsComponent.AdjustHappinessDecreaseRate(-_happinessDecreaseRate);
            Messenger.RemoveListener(Signals.DAY_STARTED, CheckForClearNoItemsList);
        }
        public override bool OnDeath(Character character) {
            Messenger.RemoveListener(Signals.DAY_STARTED, CheckForClearNoItemsList);
            return base.OnDeath(character);
        }
        public override void OnReturnToLife(Character character) {
            base.OnReturnToLife(character);
            Messenger.AddListener(Signals.DAY_STARTED, CheckForClearNoItemsList);
        }
        public override string GetTestingData(ITraitable traitable) {
            string testingData = string.Empty;
            testingData += "Known character'S with no items: \n";
            for (int i = 0; i < noItemCharacters.Count; i++) {
                testingData += $"{noItemCharacters[i].name}, ";
            }
            return testingData;
        }
        //protected override void OnChangeLevel() {
        //    base.OnChangeLevel();
        //    if (level == 1) {
        //        _happinessDecreaseRate = 10;
        //    } else if (level == 2) {
        //        _happinessDecreaseRate = 15;
        //    } else if (level == 3) {
        //        _happinessDecreaseRate = 20;
        //    }
        //}
        // public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
        //     if (targetPOI is SpecialToken) {
        //         SpecialToken token = targetPOI as SpecialToken;
        //         if (token.characterOwner != null && token.characterOwner != characterThatWillDoJob && characterThatWillDoJob.marker.CanDoStealthActionToTarget(targetPOI) && token.mapObjectState == MAP_OBJECT_STATE.BUILT) {
        //             GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.STEAL, INTERACTION_TYPE.STEAL, targetPOI, characterThatWillDoJob);
        //             job.SetIsStealth(true);
        //             characterThatWillDoJob.jobQueue.AddJobInQueue(job);
        //             return true;
        //         }
        //     } else if (targetPOI is Character) {
        //         Character targetCharacter = targetPOI as Character;
        //         if (characterThatWillDoJob.opinionComponent.GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.POSITIVE && characterThatWillDoJob.marker.CanDoStealthActionToTarget(targetCharacter) && targetCharacter.items.Count > 0) {
        //             SpecialToken item = targetCharacter.items[Random.Range(0, targetCharacter.items.Count)];
        //             GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.STEAL, INTERACTION_TYPE.STEAL, item, characterThatWillDoJob);
        //             job.SetIsStealth(true);
        //             characterThatWillDoJob.jobQueue.AddJobInQueue(job);
        //             return true;
        //         }
        //     }
        //     return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
        // }
        public override string TriggerFlaw(Character character) {
            //The character will begin Hunt for Blood.
            if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
                bool triggerBrokenhearted = false;
                Heartbroken heartbroken = character.traitContainer.GetNormalTrait<Heartbroken>("Heartbroken");
                if (heartbroken != null) {
                    triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < (25 * character.traitContainer.stacks[heartbroken.name]);
                }
                if (!triggerBrokenhearted) {
                    if (character.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY)) {
                        character.jobQueue.CancelAllJobs(JOB_TYPE.HAPPINESS_RECOVERY);
                    }

                    List<TileObject> choices = new List<TileObject>();
                    for (int i = 0; i < character.currentRegion.charactersAtLocation.Count; i++) {
                        Character otherCharacter = character.currentRegion.charactersAtLocation[i];
                        for (int j = 0; j < otherCharacter.items.Count; j++) {
                            TileObject currItem = otherCharacter.items[j];
                            if (CanBeStolen(currItem)) {
                                choices.Add(currItem);    
                            }
                        }
                    }
                    
                    //NOTE: Might be heavy on performance, optimize this!
                    foreach (KeyValuePair<STRUCTURE_TYPE,List<LocationStructure>> pair in character.currentRegion.structures) {
                        for (int i = 0; i < pair.Value.Count; i++) {
                            LocationStructure structure = pair.Value[i];
                            for (int j = 0; j < structure.pointsOfInterest.Count; j++) {
                                IPointOfInterest poi = structure.pointsOfInterest.ElementAt(j);
                                if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                                    TileObject item = poi as TileObject;
                                    if (CanBeStolen(item)) {
                                        choices.Add(item);
                                    }
                                }
                            }
                        }
                    }
                    if (choices.Count > 0) {
                        IPointOfInterest target = CollectionUtilities.GetRandomElement(choices);
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.STEAL, target, character);
                        character.jobQueue.AddJobInQueue(job);
                    } else {
                        return "no_target";
                    }
                } else {
                    heartbroken.TriggerBrokenhearted();
                }
            }
            return base.TriggerFlaw(character);
        }
        public override void ExecuteCostModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, object[] otherData, ref int cost) {
            if (action == INTERACTION_TYPE.STEAL) {
                cost = 0;//Utilities.rng.Next(5, 10);//5,46
            } else if (action == INTERACTION_TYPE.PICK_UP) {
                cost = 10000;//Utilities.rng.Next(5, 10);//5,46
            }
        }
        //public override void ExecuteActionAfterEffects(INTERACTION_TYPE action, ActualGoapNode goapNode, ref bool isRemoved) {
        //    base.ExecuteActionAfterEffects(action, goapNode, ref isRemoved);
        //    if (action == INTERACTION_TYPE.STEAL) {
        //        traitOwner.needsComponent.AdjustHappiness(6000);
        //    }
        //}
        public override void ExecuteExpectedEffectModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, object[] otherData, ref List<GoapEffect> effects) {
            if (action == INTERACTION_TYPE.STEAL) {
                effects.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
            }
        }
        #endregion

        public void AddNoItemCharacter(Character character) {
            noItemCharacters.Add(character);
        }
        public void RemoveNoItemCharacter(Character character) {
            noItemCharacters.Remove(character);
        }

        private void ClearNoItemsList() {
            noItemCharacters.Clear();
            //Debug.Log(GameManager.Instance.TodayLogString() + "Cleared " + owner.name + "'s Kleptomaniac list of character's with no items.");
        }

        private void CheckForClearNoItemsList() {
            //Store the character into the Kleptomaniac trait if it does not have any items. 
            //Exclude all characters listed in Kleptomaniac trait from Steal actions. Clear out the list at the start of every even day.
            if (UtilityScripts.Utilities.IsEven(GameManager.Instance.Today().day)) {
                ClearNoItemsList();
            }
        }

        private bool CanBeStolen(TileObject item) {
            if (item.isBeingCarriedBy != null) {
                if (item.isBeingCarriedBy == this.traitOwner || item.isBeingCarriedBy.relationshipContainer.IsFriendsWith(this.traitOwner)) {
                    return false;
                }
                return true;
            } else {
                return item.characterOwner != null && item.characterOwner != traitOwner && item.characterOwner.relationshipContainer.IsFriendsWith(this.traitOwner);
            }
        }
    }

    public class SaveDataKleptomaniac : SaveDataTrait {
        public List<int> noItemCharacterIDs;

        public override void Save(Trait trait) {
            base.Save(trait);
            Kleptomaniac derivedTrait = trait as Kleptomaniac;
            for (int i = 0; i < derivedTrait.noItemCharacters.Count; i++) {
                noItemCharacterIDs.Add(derivedTrait.noItemCharacters[i].id);
            }
        }

        public override Trait Load(ref Character responsibleCharacter) {
            Trait trait = base.Load(ref responsibleCharacter);
            Kleptomaniac derivedTrait = trait as Kleptomaniac;
            for (int i = 0; i < noItemCharacterIDs.Count; i++) {
                derivedTrait.AddNoItemCharacter(CharacterManager.Instance.GetCharacterByID(noItemCharacterIDs[i]));
            }
            return trait;
        }
    }
}


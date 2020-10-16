using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;
using Traits;
using UnityEngine.Assertions;

namespace Traits {
    public class Vampire : Trait {
        //public override bool isSingleton => true;
        public bool dislikedBeingVampire { get; private set; }
        public int numOfConvertedVillagers { get; private set; }
        public List<Character> awareCharacters { get; private set; }
        public override Type serializedData => typeof(SaveDataVampire);

        private Character _owner;
        
        public Vampire() {
            name = "Vampire";
            description = "Sustains itself by drinking other's blood.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            canBeTriggered = true;
            awareCharacters = new List<Character>();
            AddTraitOverrideFunctionIdentifier(TraitManager.Execute_Expected_Effect_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.See_Poi_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Before_Start_Flee);
            AddTraitOverrideFunctionIdentifier(TraitManager.After_Exiting_Combat);
            //AddTraitOverrideFunctionIdentifier(TraitManager.Tick_Ended_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            if (sourceCharacter is Character character) {
                _owner = character;
                character.jobQueue.CancelAllJobs(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT, JOB_TYPE.ENERGY_RECOVERY_NORMAL, JOB_TYPE.ENERGY_RECOVERY_URGENT);
                character.needsComponent.SetTirednessForcedTick(0);
                //character.needsComponent.SetForcedFullnessRecoveryTimeInWords(TIME_IN_WORDS.AFTER_MIDNIGHT);
                character.needsComponent.SetFullnessForcedTick(0);
                character.needsComponent.AdjustDoNotGetTired(1);
                character.needsComponent.ResetTirednessMeter();
                DetermineIfDesireOrDislike(character);
                character.behaviourComponent.AddBehaviourComponent(typeof(VampireBehaviour));
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            if (sourceCharacter is Character) {
                Character character = sourceCharacter as Character;
                character.jobQueue.CancelAllJobs(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT);
                character.needsComponent.SetTirednessForcedTick();
                //character.needsComponent.SetForcedFullnessRecoveryTimeInWords(TIME_IN_WORDS.LUNCH_TIME);
                character.needsComponent.SetFullnessForcedTick();
                character.needsComponent.AdjustDoNotGetTired(-1);
                character.behaviourComponent.RemoveBehaviourComponent(typeof(VampireBehaviour));
            }
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        //public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
        //    if (targetPOI is Character) {
        //        //In Vampiric, the parameter traitOwner is the target character, that's why you must pass the target character in this parameter not the actual owner of the trait, the actual owner of the trait is the characterThatWillDoJob
        //        //Character targetCharacter = targetPOI as Character;
        //        //if (characterThatWillDoJob.currentActionNode.action != null && characterThatWillDoJob.currentActionNode.action.goapType == INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD && !characterThatWillDoJob.currentActionNode.isDone) {
        //        //    if (characterThatWillDoJob.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) != RELATIONSHIP_EFFECT.POSITIVE && targetCharacter.traitContainer.GetNormalTrait<Trait>("Vampire") == null && characterThatWillDoJob.marker.CanDoStealthActionToTarget(targetCharacter)) {
        //        //        //TODO: GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(characterThatWillDoJob.currentJobNode.jobType, INTERACTION_TYPE.DRINK_BLOOD, targetCharacter);
        //        //        //job.SetIsStealth(true);
        //        //        //characterThatWillDoJob.currentActionNode.action.parentPlan.job.jobQueueParent.CancelJob(characterThatWillDoJob.currentActionNode.action.parentPlan.job);
        //        //        //characterThatWillDoJob.jobQueue.AddJobInQueue(job, false);
        //        //        //characterThatWillDoJob.jobQueue.AssignCharacterToJobAndCancelCurrentAction(job, characterThatWillDoJob);
        //        //        return true;
        //        //    }
        //        //}
        //    }
        //    return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
        //}
        public override string TriggerFlaw(Character character) {
            //The character will begin Hunt for Blood.
            if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
                // Character targetCharacter = GetDrinkBloodTarget(character);
                // if(targetCharacter != null) {
                //     bool triggerGrieving = false;
                //     Griefstricken griefstricken = character.traitContainer.GetTraitOrStatus<Griefstricken>("Griefstricken");
                //     if (griefstricken != null) {
                //         triggerGrieving = UnityEngine.Random.Range(0, 100) < (25 * character.traitContainer.stacks[griefstricken.name]);
                //     }
                //     if (!triggerGrieving) {
                //         character.jobComponent.CreateDrinkBloodJob(JOB_TYPE.TRIGGER_FLAW, targetCharacter);
                //     } else {
                //         griefstricken.TriggerGrieving();
                //     }
                // } else {
                //     return "no_victim";
                // }
                WeightedDictionary<Character> embraceChoices = VampireBehaviour.GetVampiricEmbraceTargetWeights(character);
                if (embraceChoices.GetTotalOfWeights() > 0) {
                    string log = embraceChoices.GetWeightsSummary($"{character.name} embrace choices:");
                    Character target = embraceChoices.PickRandomElementGivenWeights();
                    log = $"{log}\n- Chosen target is {target.name}";
                    Debug.Log(log);
                    character.jobComponent.CreateVampiricEmbraceJob(JOB_TYPE.TRIGGER_FLAW, target);
                } else {
                    return "no_victim";
                }
            } else {
                return "has_trigger_flaw";
            }
            return base.TriggerFlaw(character);
        }
        public override void ExecuteExpectedEffectModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, ref List<GoapEffect> effects) {
            if (action == INTERACTION_TYPE.DRINK_BLOOD) {
                effects.Add(new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
            }
        }
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is Character targetCharacter && targetCharacter.advertisedActions.Contains(INTERACTION_TYPE.DRINK_BLOOD) && characterThatWillDoJob.needsComponent.isStarving) {
                if (!characterThatWillDoJob.relationshipContainer.IsFriendsWith(targetCharacter) &&
                    !characterThatWillDoJob.relationshipContainer.IsFamilyMember(targetCharacter) && 
                    !characterThatWillDoJob.relationshipContainer.HasSpecialPositiveRelationshipWith(targetCharacter)) {
                    characterThatWillDoJob.jobComponent.CreateDrinkBloodJob(JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT, targetCharacter);
                }
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        public override void OnBeforeStartFlee(ITraitable traitable) {
            base.OnBeforeStartFlee(traitable);
            if(traitable is Character character) {
                if (!character.behaviourComponent.isInVampireBatForm) {
                    if (!character.crimeComponent.HasNonHostileVillagerInRangeThatConsidersVampirismACrime()) {
                        //TransformToBat(character);
                        character.interruptComponent.TriggerInterrupt(INTERRUPT.Transform_To_Bat, character);
                    }
                }
            }
        }
        public override void OnAfterExitingCombat(ITraitable traitable) {
            base.OnAfterExitingCombat(traitable);
            if (traitable is Character character) {
                if (character.behaviourComponent.isInVampireBatForm) {
                    if (!character.crimeComponent.HasNonHostileVillagerInRangeThatConsidersVampirismACrime()) {
                        //RevertToNormal(character);
                        character.interruptComponent.TriggerInterrupt(INTERRUPT.Revert_From_Bat, character);
                    } else {
                        FleeToAllNonHostileVillagerInRangeThatConsidersVampirismACrime(character);
                    }
                }
            }
        }
        //public override void OnTickEnded(ITraitable traitable) {
        //    base.OnTickEnded(traitable);
        //    if (traitable is Character character) {
        //        if (character.behaviourComponent.isInVampireBatForm) {
        //            if (!HasNonHostileVillagerInRangeThatConsidersVampirismACrime(character)) {
        //                RevertToNormal(character);
        //            }
        //        }
        //    }
        //}
        public override string GetTestingData(ITraitable traitable = null) {
            string data = base.GetTestingData(traitable);
            data = $"{data} Dislikes Being Vampire: {dislikedBeingVampire.ToString()}";
            data = $"{data}\n Converted Villagers: {numOfConvertedVillagers.ToString()}";
            return data;
        }
        #endregion
        
        #region Loading
        public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadFirstWaveInstancedTrait(saveDataTrait);
            SaveDataVampire saveDataVampire = saveDataTrait as SaveDataVampire;
            Debug.Assert(saveDataVampire != null, nameof(saveDataVampire) + " != null");
            dislikedBeingVampire = saveDataVampire.dislikedBeingVampire;
            numOfConvertedVillagers = saveDataVampire.numOfConvertedVillagers;
        }
        public override void LoadSecondWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadSecondWaveInstancedTrait(saveDataTrait);
            SaveDataVampire saveDataVampire = saveDataTrait as SaveDataVampire;
            Assert.IsNotNull(saveDataVampire);
            awareCharacters.AddRange(SaveUtilities.ConvertIDListToCharacters(saveDataVampire.awareCharacters));
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                _owner = character;
            }
        }
        #endregion
        
        private void DetermineIfDesireOrDislike(Character character) {
            if(character.traitContainer.HasTrait("Hemophobic", "Chaste")) {
                dislikedBeingVampire = true;
                return;
            }
            if (character.traitContainer.HasTrait("Hemophiliac")) {
                dislikedBeingVampire = false;
                return;
            }
            if (character.traitContainer.HasTrait("Cultist") && GameUtilities.RollChance(75)) {
                dislikedBeingVampire = false;
                return;
            }
            if (character.characterClass.className == "Hero" && GameUtilities.RollChance(75)) {
                dislikedBeingVampire = true;
                return;
            }
            if (character.characterClass.className == "Shaman" && GameUtilities.RollChance(80)) {
                dislikedBeingVampire = true;
                return;
            }
            if (character.traitContainer.HasTrait("Lycanthrope") && GameUtilities.RollChance(80)) {
                dislikedBeingVampire = true;
                return;
            }
            if (character.traitContainer.HasTrait("Evil", "Treacherous") && GameUtilities.RollChance(75)) {
                dislikedBeingVampire = false;
                return;
            }
            if (GameUtilities.RollChance(50)) {
                dislikedBeingVampire = false;
            } else {
                dislikedBeingVampire = true;
            }
        }
        public void AdjustNumOfConvertedVillagers(int amount) {
            numOfConvertedVillagers += amount;
        }
        public void AddAwareCharacter(Character character) {
            if (!awareCharacters.Contains(character)) {
                awareCharacters.Add(character);
                if (character.traitContainer.HasTrait("Hemophiliac")) {
                    Hemophiliac hemophiliac = character.traitContainer.GetTraitOrStatus<Hemophiliac>("Hemophiliac");
                    hemophiliac.OnBecomeAwareOfVampire(_owner);
                } else if (character.traitContainer.HasTrait("Hemophobic")) {
                    Hemophobic hemophobic = character.traitContainer.GetTraitOrStatus<Hemophobic>("Hemophobic");
                    hemophobic.OnBecomeAwareOfVampire(_owner);
                }
            }
        }
        public bool DoesCharacterKnowThisVampire(Character character) {
            return awareCharacters.Contains(character);
        }
        public bool DoesFactionKnowThisVampire(Faction faction) {
            for (int i = 0; i < faction.characters.Count; i++) {
                Character member = faction.characters[i];
                if (member != _owner) {
                    if (DoesCharacterKnowThisVampire(member)) {
                        return true;
                    }
                }
            }
            return false;
        }
        private Character GetDrinkBloodTarget(Character vampire) {
            List<Character> targets = null;
            if(vampire.currentRegion != null) {
                for (int i = 0; i < vampire.currentRegion.charactersAtLocation.Count; i++) {
                    Character character = vampire.currentRegion.charactersAtLocation[i];
                    if(vampire != character) {
                        if(!character.traitContainer.HasTrait("Vampire") && character.isNormalCharacter && character.carryComponent.IsNotBeingCarried() && character.Advertises(INTERACTION_TYPE.DRINK_BLOOD) && !character.isDead && !vampire.relationshipContainer.IsFriendsWith(character)
                            && vampire.movementComponent.HasPathToEvenIfDiffRegion(character.gridTileLocation)) {
                            if(targets == null) { targets = new List<Character>(); }
                            targets.Add(character);
                        }
                    }
                }
            }
            if(targets != null && targets.Count > 0) {
                return UtilityScripts.CollectionUtilities.GetRandomElement(targets);  
            }
            return null;
        }
        private void FleeToAllNonHostileVillagerInRangeThatConsidersVampirismACrime(Character character) {
            if (character.marker) {
                for (int i = 0; i < character.marker.inVisionCharacters.Count; i++) {
                    Character inVision = character.marker.inVisionCharacters[i];
                    if (inVision != character) {
                        if (!character.IsHostileWith(inVision)) {
                            CRIME_SEVERITY severity = CrimeManager.Instance.GetCrimeSeverity(inVision, character, character, CRIME_TYPE.Vampire);
                            if (severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable) {
                                character.combatComponent.Flight(inVision, CombatManager.Vampire_Bat);
                            }
                        }
                    }
                }
            }
        }
    }
}

#region Save Data
public class SaveDataVampire : SaveDataTrait {
    public bool dislikedBeingVampire;
    public int numOfConvertedVillagers;
    public List<string> awareCharacters;

    public override void Save(Trait trait) {
        base.Save(trait);
        Vampire vampire = trait as Vampire;
        Assert.IsNotNull(vampire);
        awareCharacters = SaveUtilities.ConvertSavableListToIDs(vampire.awareCharacters);
        dislikedBeingVampire = vampire.dislikedBeingVampire;
        numOfConvertedVillagers = vampire.numOfConvertedVillagers;
    }
}
#endregion
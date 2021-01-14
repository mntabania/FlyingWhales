using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Traits {
    public class Alcoholic : Trait {

        private bool _hasDrankWithinTheDay;
        private Character owner;

        #region Getter
        public bool hasDrankWithinTheDay => _hasDrankWithinTheDay;
        public override Type serializedData => typeof(SaveDataAlcoholic);
        #endregion
        
        public Alcoholic() {
            name = "Alcoholic";
            description = "More than just a social drinker.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            canBeTriggered = true;
            _hasDrankWithinTheDay = true;
        }

        #region Loading
        public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadFirstWaveInstancedTrait(saveDataTrait);
            SaveDataAlcoholic saveDataAlcoholic = saveDataTrait as SaveDataAlcoholic;
            Assert.IsNotNull(saveDataAlcoholic);
            _hasDrankWithinTheDay = saveDataAlcoholic.hasDrankWithinTheDay;
        }
        #endregion
        
        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if(addedTo is Character) {
                owner = addedTo as Character;
            }
            Messenger.AddListener<int>(Signals.DAY_STARTED, OnDayStarted);
            Messenger.AddListener<ActualGoapNode>(JobSignals.STARTED_PERFORMING_ACTION, OnPerformAction);
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if(addTo is Character character) {
                owner = character;
                if (!character.isDead) {
                    Messenger.AddListener<int>(Signals.DAY_STARTED, OnDayStarted);
                    Messenger.AddListener<ActualGoapNode>(JobSignals.STARTED_PERFORMING_ACTION, OnPerformAction);
                }
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            Messenger.RemoveListener<int>(Signals.DAY_STARTED, OnDayStarted);
            Messenger.RemoveListener<ActualGoapNode>(JobSignals.STARTED_PERFORMING_ACTION, OnPerformAction);
            base.OnRemoveTrait(removedFrom, removedBy);
        }
        public override string TriggerFlaw(Character character) {
            //Will drink
            if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
                bool triggerBrokenhearted = false;
                Heartbroken heartbroken = character.traitContainer.GetTraitOrStatus<Heartbroken>("Heartbroken");
                if (heartbroken != null) {
                    triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < (25 * owner.traitContainer.stacks[heartbroken.name]);
                }
                if (!triggerBrokenhearted) {
                    if (character.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY)) {
                        character.jobQueue.CancelAllJobs(JOB_TYPE.HAPPINESS_RECOVERY);
                    }
                    if (character.homeSettlement != null && character.homeSettlement.HasStructure(STRUCTURE_TYPE.TAVERN)) {
                        LocationStructure tavern = character.homeSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.TAVERN);
                        List<TileObject> choices = tavern.GetTileObjectsThatAdvertise(INTERACTION_TYPE.DRINK);
                        if (choices.Count > 0) {
                            TileObject target = CollectionUtilities.GetRandomElement(choices);
                            GoapPlanJob drinkJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.DRINK, target, character);
                            character.jobQueue.AddJobInQueue(drinkJob);    
                        } else {
                            return "no_target";            
                        }
                    } else {
                        return "no_target";
                    }
                } else {
                    heartbroken.TriggerBrokenhearted();
                }
            } else {
                return "has_trigger_flaw";
            }
            return base.TriggerFlaw(character);
        }
        public override void ExecuteCostModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, ref int cost) {
            base.ExecuteCostModification(action, actor, poiTarget, otherData, ref cost);
            if (action == INTERACTION_TYPE.DRINK) {
                cost =  UtilityScripts.Utilities.Rng.Next(5, 20);
            }
        }
        #endregion

        private void OnDayStarted(int p_currentDay) {
            if (!hasDrankWithinTheDay) {
                owner.traitContainer.AddTrait(owner, "Withdrawal");
            }
            _hasDrankWithinTheDay = false;
        }
        private void OnPerformAction(ActualGoapNode node) {
            if(node.action.goapType == INTERACTION_TYPE.DRINK) {
                if (!hasDrankWithinTheDay) {
                    _hasDrankWithinTheDay = true;
                }
            }
        }
    }
}

#region Save Data
public class SaveDataAlcoholic : SaveDataTrait {
    public bool hasDrankWithinTheDay;
    public override void Save(Trait trait) {
        base.Save(trait);
        Traits.Alcoholic alcoholic = trait as Traits.Alcoholic;
        Assert.IsNotNull(alcoholic);
        hasDrankWithinTheDay = alcoholic.hasDrankWithinTheDay;
    }
}
#endregion
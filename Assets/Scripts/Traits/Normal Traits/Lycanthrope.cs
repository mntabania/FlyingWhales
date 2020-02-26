using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;

namespace Traits {
    public class Lycanthrope : Trait {
        public Character owner { get; private set; }
        public override bool isPersistent { get { return true; } }

        private int _level;
        public Lycanthrope() {
            name = "Lycanthrope";
            description = "Lycanthropes transform into wolves when they sleep.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            canBeTriggered = true;
            //effects = new List<TraitEffect>();
            //advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.TRANSFORM_TO_WOLF, INTERACTION_TYPE.REVERT_TO_NORMAL };
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            if (sourceCharacter is Character) {
                owner = sourceCharacter as Character;
                //AlterEgoData lycanthropeAlterEgo = _character.CreateNewAlterEgo("Lycanthrope");

                ////setup all alter ego data
                //lycanthropeAlterEgo.SetFaction(FactionManager.Instance.neutralFaction);
                //lycanthropeAlterEgo.SetRace(RACE.WOLF);
                //lycanthropeAlterEgo.SetRole(CharacterRole.BEAST);
                //lycanthropeAlterEgo.SetCharacterClass(CharacterManager.Instance.CreateNewCharacterClass(Utilities.GetRespectiveBeastClassNameFromByRace(RACE.WOLF)));
                //lycanthropeAlterEgo.SetLevel(level);
                //lycanthropeAlterEgo.AddTrait(new Nocturnal());

            }

            base.OnAddTrait(sourceCharacter);
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            //originalForm.RemoveAlterEgo("Lycanthrope");
            //originalForm = null;
            base.OnRemoveTrait(sourceCharacter, removedBy);
            owner.lycanData.EraseThisDataWhenTraitIsRemoved(owner);
        }
        //public override bool OnDeath(Character character) {
        //    if(character == owner.lycanData.lycanthropeForm) {
        //        owner.lycanData.LycanDies(character);
        //    } else if (character == owner.lycanData.originalForm) {
        //        return character.traitContainer.RemoveTrait(character, this);
        //    }
        //    return base.OnDeath(character);
        //}
        //public override bool OnAfterDeath(Character character, string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null, Log _deathLog = null, LogFiller[] deathLogFillers = null) {
        //    owner.lycanData.EraseThisDataWhenFormDies(owner, cause, deathFromAction, responsibleCharacter, _deathLog, deathLogFillers);
        //    return base.OnAfterDeath(character, cause, deathFromAction, responsibleCharacter, _deathLog, deathLogFillers);
        //}
        //public override void ExecuteActionPerTickEffects(INTERACTION_TYPE action, ActualGoapNode goapNode) {
        //    base.ExecuteActionPerTickEffects(action, goapNode);
        //    //if (action == INTERACTION_TYPE.NAP || action == INTERACTION_TYPE.SLEEP || action == INTERACTION_TYPE.SLEEP_OUTSIDE || action == INTERACTION_TYPE.NARCOLEPTIC_NAP) {
        //    if (originalForm.traitContainer.GetNormalTrait<Trait>("Resting") != null) {
        //            CheckForLycanthropy();
        //    }
        //}
        //public override void OnHourStarted() {
        //    base.OnHourStarted();
        //    if (activeForm.traitContainer.GetNormalTrait<Trait>("Resting") != null) {
        //        CheckForLycanthropy();
        //    }
        //}
        #endregion

        //public void CheckForLycanthropy() {
        //    int chance = UnityEngine.Random.Range(0, 100);
        //    //TODO:
        //    //if (restingTrait.lycanthropyTrait == null) {
        //    //    if (currentState.currentDuration == currentState.duration) {
        //    //        //If sleep will end, check if the actor is being targetted by Drink Blood action, if it is, do not end sleep
        //    //        bool isTargettedByDrinkBlood = false;
        //    //        for (int i = 0; i < actor.targettedByAction.Count; i++) {
        //    //            if (actor.targettedByAction[i].goapType == INTERACTION_TYPE.DRINK_BLOOD && !actor.targettedByAction[i].isDone && actor.targettedByAction[i].isPerformingActualAction) {
        //    //                isTargettedByDrinkBlood = true;
        //    //                break;
        //    //            }
        //    //        }
        //    //        if (isTargettedByDrinkBlood) {
        //    //            currentState.OverrideDuration(currentState.duration + 1);
        //    //        }
        //    //    }
        //    //} else {
        //    //    bool isTargettedByDrinkBlood = false;
        //    //    for (int i = 0; i < actor.targettedByAction.Count; i++) {
        //    //        if (actor.targettedByAction[i].goapType == INTERACTION_TYPE.DRINK_BLOOD && !actor.targettedByAction[i].isDone && actor.targettedByAction[i].isPerformingActualAction) {
        //    //            isTargettedByDrinkBlood = true;
        //    //            break;
        //    //        }
        //    //    }
        //    //    if (currentState.currentDuration == currentState.duration) {
        //    //        //If sleep will end, check if the actor is being targetted by Drink Blood action, if it is, do not end sleep
        //    //        if (isTargettedByDrinkBlood) {
        //    //            currentState.OverrideDuration(currentState.duration + 1);
        //    //        } else {
        //    //            if (!restingTrait.hasTransformed) {
        //    //                restingTrait.CheckForLycanthropy(true);
        //    //            }
        //    //        }
        //    //    } else {
        //    //        if (!isTargettedByDrinkBlood) {
        //    //            restingTrait.CheckForLycanthropy();
        //    //        }
        //    //    }
        //    //}
        //    if (activeForm.race == RACE.WOLF) {
        //        //Turn back to normal form
        //        if (chance < 25) {
        //            PlanRevertToNormal();
        //        }
        //    } else {
        //        //Turn to wolf
        //        if (chance < 25) {
        //            PlanTransformToWolf();
        //        }
        //    }
        //}

        //public void PlanTransformToWolf() {
        //    _character.currentActionNode?.EndPerTickEffect();
        //    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.TRANSFORM_TO_WOLF_FORM, _character, _character);
        //    _character.jobQueue.AddJobInQueue(job);
        //}
        //public void PlanRevertToNormal() {
        //    _character.currentActionNode?.EndPerTickEffect();
        //    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.REVERT_TO_NORMAL_FORM, _character, _character);
        //    _character.jobQueue.AddJobInQueue(job);
        //}
        //public void TurnToWolf() {
        //    ////Drop all plans except for the current action
        //    //_character.AdjustIsWaitingForInteraction(1);
        //    //_character.DropAllPlans(_character.currentActionNode.action.parentPlan);
        //    //_character.AdjustIsWaitingForInteraction(-1);

        //    ////Copy non delicate data
        //    //data.SetData(_character);

        //    //_character.SetHomeStructure(null);

        //    ////Reset needs
        //    //_character.ResetFullnessMeter();
        //    //_character.ResetHappinessMeter();
        //    //_character.ResetTirednessMeter();


        //    ////Remove all awareness then add all edible plants and small animals of current location to awareness
        //    //_character.awareness.Clear();
        //    //foreach (List<LocationStructure> structures in _character.specificLocation.structures.Values) {
        //    //    for (int i = 0; i < structures.Count; i++) {
        //    //        for (int j = 0; j < structures[i].pointsOfInterest.Count; j++) {
        //    //            IPointOfInterest poi = structures[i].pointsOfInterest[j];
        //    //            if(poi is TileObject) {
        //    //                TileObject tileObj = poi as TileObject;
        //    //                if(tileObj.tileObjectType == TILE_OBJECT_TYPE.SMALL_ANIMAL || tileObj.tileObjectType == TILE_OBJECT_TYPE.EDIBLE_PLANT) {
        //    //                    _character.AddAwareness(tileObj);
        //    //                }
        //    //            }
        //    //        }
        //    //    }
        //    //}

        //    ////Copy relationship data then remove them
        //    ////data.SetRelationshipData(_character);
        //    ////_character.RemoveAllRelationships(false);
        //    //foreach (Character target in _character.relationships.Keys) {
        //    //    CharacterManager.Instance.SetIsDisabledRelationshipBetween(_character, target, true);
        //    //}

        //    ////Remove race and class
        //    ////This is done first so that when the traits are copied, it will not copy the traits from the race and class because if it is copied and the race and character is brought back, it will be doubled, which is not what we want
        //    //_character.RemoveRace();
        //    //_character.RemoveClass();

        //    ////Copy traits and then remove them
        //    //data.SetTraits(_character);
        //    //_character.RemoveAllNonRelationshipTraits("Lycanthrope");

        //    ////Change faction and race
        //    //_character.ChangeFactionTo(FactionManager.Instance.neutralFaction);
        //    //_character.SetRace(RACE.WOLF);

        //    ////Change class and role
        //    //_character.AssignRole(CharacterRole.BEAST);
        //    //_character.AssignClassByRole(_character.role);

        //    //Messenger.Broadcast(Signals.CHARACTER_CHANGED_RACE, _character);

        //    //_character.CancelAllJobsTargettingThisCharacter("target is not found", false);
        //    //Messenger.Broadcast(Signals.CANCEL_CURRENT_ACTION, _character, "target is not found");

        //    _character.SwitchAlterEgo("Lycanthrope");
        //    //Plan idle stroll to the wilderness
        //    LocationStructure wilderness = _character.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
        //    LocationGridTile targetTile = wilderness.GetRandomTile();
        //    _character.PlanIdleStroll(wilderness, targetTile);
        //}

        //public void RevertToNormal() {
        //    ////Drop all plans except for the current action
        //    //_character.AdjustIsWaitingForInteraction(1);
        //    //_character.DropAllPlans(_character.currentActionNode.action.parentPlan);
        //    //_character.AdjustIsWaitingForInteraction(-1);

        //    ////Revert back data including awareness
        //    //_character.SetFullness(data.fullness);
        //    //_character.SetTiredness(data.tiredness);
        //    //_character.SetHappiness(data.happiness);
        //    //_character.CopyAwareness(data.awareness);
        //    //_character.SetHomeStructure(data.homeStructure);
        //    //_character.ChangeFactionTo(data.faction);
        //    //_character.ChangeRace(data.race);
        //    //_character.AssignRole(data.role);
        //    //_character.AssignClass(data.characterClass);

        //    ////Bring back lost relationships
        //    //foreach (Character target in _character.relationships.Keys) {
        //    //    CharacterManager.Instance.SetIsDisabledRelationshipBetween(_character, target, false);
        //    //}

        //    ////Revert back the traits
        //    //for (int i = 0; i < data.traits.Count; i++) {
        //    //    _character.AddTrait(data.traits[i]);
        //    //}

        //    _character.SwitchAlterEgo(CharacterManager.Original_Alter_Ego);
        //}

        public override string TriggerFlaw(Character character) {
            if (IsAlone()) {
                DoTransform();
            } else {
                //go to a random tile in the wilderness
                //then check if the character is alone, if not pick another random tile,
                //repeat the process until alone, then transform to wolf
                LocationStructure wilderness = character.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
                //LocationGridTile randomWildernessTile = wilderness.tiles[Random.Range(0, wilderness.tiles.Count)];
                //character.marker.GoTo(randomWildernessTile, CheckIfAlone);
                character.PlanAction(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.STEALTH_TRANSFORM, character, new object[] { wilderness });
            }
            return base.TriggerFlaw(character);
        }

        public void CheckIfAlone() {
            if (IsAlone()) {
                //alone
                DoTransform();
            } else {
                //go to a different tile
                LocationStructure wilderness = owner.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
                //LocationGridTile randomWildernessTile = wilderness.tiles[Random.Range(0, wilderness.tiles.Count)];
                //character.marker.GoTo(randomWildernessTile, CheckIfAlone);
                owner.PlanAction(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.STEALTH_TRANSFORM, owner, new object[] { wilderness });
            }
        }
        private bool IsAlone() {
            return owner.marker.inVisionCharacters.Count == 0;
        }
        private void DoTransform() {
            owner.lycanData.Transform(owner);
        }
    }

    //Lycanthrope data has only 1 instance but referenced by two characters: original and lycanthrope form
    //So if we need to do process something in this, we must always pass the character that referenced this data as a parameter because this data is shared
    public class LycanthropeData {
        public Character activeForm { get; private set; }
        public Character limboForm { get; private set; }

        public Character lycanthropeForm { get; private set; }
        public Character originalForm { get; private set; }

        public LycanthropeData(Character originalForm) {
            this.originalForm = originalForm;
            CreateLycanthropeForm();
            activeForm = originalForm;
            limboForm = lycanthropeForm;
            originalForm.traitContainer.AddTrait(originalForm, "Lycanthrope");
            lycanthropeForm.traitContainer.AddTrait(lycanthropeForm, "Lycanthrope");
            originalForm.SetLycanthropeData(this);
            lycanthropeForm.SetLycanthropeData(this);
        }

        private void CreateLycanthropeForm() {
            lycanthropeForm = CharacterManager.Instance.CreateNewLimboCharacter(RACE.WOLF, "Ravager", originalForm.gender, FactionManager.Instance.neutralFaction);
            lycanthropeForm.ConstructInitialGoapAdvertisementActions();
            lycanthropeForm.SetName(originalForm.name);
        }

        public void Transform(Character character) {
            if(character == originalForm) {
                character.interruptComponent.TriggerInterrupt(INTERRUPT.Transform_To_Wolf, character);
                //TurnToWolf();
            } else if(character == lycanthropeForm) {
                character.interruptComponent.TriggerInterrupt(INTERRUPT.Revert_To_Normal, character);
                //RevertToNormal();
            }
        }

        public void TurnToWolf() {
            activeForm = lycanthropeForm;
            limboForm = originalForm;
            LocationGridTile tile = originalForm.gridTileLocation;
            Region homeRegion = originalForm.homeRegion;
            PutToLimbo(originalForm);
            ReleaseFromLimbo(lycanthropeForm, tile, homeRegion);
            Messenger.Broadcast(Signals.ON_SWITCH_FROM_LIMBO, originalForm, lycanthropeForm);
        }

        public void RevertToNormal() {
            activeForm = originalForm;
            limboForm = lycanthropeForm;
            LocationGridTile tile = lycanthropeForm.gridTileLocation;
            Region homeRegion = lycanthropeForm.homeRegion;
            PutToLimbo(lycanthropeForm);
            ReleaseFromLimbo(originalForm, tile, homeRegion);
            Messenger.Broadcast(Signals.ON_SWITCH_FROM_LIMBO, lycanthropeForm, originalForm);
        }



        private void PutToLimbo(Character form) {
            if (UIManager.Instance.characterInfoInfoUi.isShowing && UIManager.Instance.characterInfoInfoUi.activeCharacter == form) {
                UIManager.Instance.characterInfoInfoUi.CloseMenu();
            }
            if (form.ownParty.icon.isTravelling) {
                form.marker.StopMovement();
            }
            if (form.trapStructure.structure != null) {
                form.trapStructure.SetStructureAndDuration(null, 0);
            }
            Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, this as IPointOfInterest, "");
            //ForceCancelAllJobsTargettingThisCharacter();
            //form.marker.ClearTerrifyingObjects();
            form.needsComponent.OnCharacterLeftLocation(form.currentRegion);

            form.CancelAllJobs();
            form.UnsubscribeSignals();
            form.SetIsConversing(false);
            form.SetPOIState(POI_STATE.INACTIVE);
            SchedulingManager.Instance.ClearAllSchedulesBy(this);
            if (form.marker) {
                for (int i = 0; i < form.marker.inVisionCharacters.Count; i++) {
                    Character otherCharacter = form.marker.inVisionCharacters[i];
                    if(otherCharacter.marker) {
                        otherCharacter.marker.RemovePOIFromInVisionRange(form);
                    }
                }
                for (int i = 0; i < form.marker.visionCollision.poisInRangeButDiffStructure.Count; i++) {
                    IPointOfInterest otherPOI = form.marker.visionCollision.poisInRangeButDiffStructure[i];
                    if (otherPOI is Character) {
                        (otherPOI as Character).marker.visionCollision.RemovePOIAsInRangeButDifferentStructure(form);
                    }
                }
                form.DestroyMarker();
            }
            form.currentRegion.RemoveCharacterFromLocation(form);
            form.homeRegion.RemoveResident(form);
            CharacterManager.Instance.AddNewLimboCharacter(form);
            CharacterManager.Instance.RemoveCharacter(form, false);
            Messenger.AddListener(Signals.TICK_STARTED, form.OnTickStartedWhileSeized);
        }
        private void ReleaseFromLimbo(Character form, LocationGridTile tileLocation, Region homeRegion) {
            if (Messenger.eventTable.ContainsKey(Signals.TICK_STARTED)) {
                Messenger.RemoveListener(Signals.TICK_STARTED, form.OnTickStartedWhileSeized);
            }
            homeRegion.AddResident(form);
            form.needsComponent.OnCharacterArrivedAtLocation(tileLocation.structure.location.coreTile.region);
            form.SubscribeToSignals();
            form.SetPOIState(POI_STATE.ACTIVE);
            if (!form.marker) {
                form.CreateMarker();
            }
            form.marker.InitialPlaceMarkerAt(tileLocation);
            //if (tileLocation.structure.location.coreTile.region != form.currentRegion) {
            //    if(form.currentRegion != null) {
            //        form.currentRegion.RemoveCharacterFromLocation(form);
            //    }
            //    form.marker.InitialPlaceMarkerAt(tileLocation);
            //} else {
            //    form.marker.InitialPlaceMarkerAt(tileLocation, false);
            //}
            form.needsComponent.CheckExtremeNeeds();
            CharacterManager.Instance.AddNewCharacter(form, false);
            CharacterManager.Instance.RemoveLimboCharacter(form);
        }

        //Parameter: which form is this data erased?
        public void EraseThisDataWhenTraitIsRemoved(Character form) {
            if(form != activeForm) {
                return;
            }
            if(form == lycanthropeForm) {
                originalForm.traitContainer.RemoveTrait(originalForm, "Lycanthrope");
                RevertToNormal();
            }
            CharacterManager.Instance.RemoveLimboCharacter(lycanthropeForm);
            originalForm.SetLycanthropeData(null);
            lycanthropeForm.SetLycanthropeData(null);
        }

        //Parameter: which form is this data erased?
        public void LycanDies(Character form, string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null, Log _deathLog = null, LogFiller[] deathLogFillers = null) {
            if (form != activeForm) {
                return;
            }
            originalForm.traitContainer.RemoveTrait(originalForm, "Lycanthrope");
            if (form == lycanthropeForm) {
                RevertToNormal();
                //CharacterManager.Instance.RemoveLimboCharacter(lycanthropeForm);
                originalForm.SetLycanthropeData(null);
                lycanthropeForm.SetLycanthropeData(null);
                originalForm.Death(cause, deathFromAction, responsibleCharacter, _deathLog, deathLogFillers);
            } 
            //else if (form == originalForm) {
            //    originalForm.traitContainer.RemoveTrait(originalForm, "Lycanthrope");
            //}
        }
    }
}

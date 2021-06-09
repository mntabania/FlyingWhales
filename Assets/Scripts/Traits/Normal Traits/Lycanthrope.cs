using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;
using Locations.Settlements;

namespace Traits {
    public class Lycanthrope : Trait {
        public Character owner { get; private set; }

        private Collider2D[] _triggerFlawNearbyTargets;
        
        #region getters
        public override bool isPersistent => true;
        #endregion
        
        public Lycanthrope() {
            name = "Lycanthrope";
            description = "Sometimes turns to plain wolf when it sleeps";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            canBeTriggered = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Per_Tick_While_Stationary_Unoccupied);
            AddTraitOverrideFunctionIdentifier(TraitManager.See_Poi_Cannot_Witness_Trait);
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.DISPEL };
            _triggerFlawNearbyTargets = new Collider2D[100];
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            if (sourceCharacter is Character character) {
                owner = character;
            }
            base.OnAddTrait(sourceCharacter);
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                owner = character;
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            base.OnRemoveTrait(sourceCharacter, removedBy);
            owner.RevertFromWerewolfForm();
            owner.lycanData.EraseThisDataWhenTraitIsRemoved(owner);
        }
        public override string GetTestingData(ITraitable traitable = null) {
            string data = base.GetTestingData(traitable);
            if (traitable is Character character) {
                data = $"{data}Is Master: {character.lycanData.isMaster}";
            }
            return data;
        }
        protected override string GetDescriptionInUI() {
            string data = base.GetDescriptionInUI();
            if (owner.lycanData.isMaster) {
                data = "Can transform into a powerful werewolf at will.";
            }
            data = owner.lycanData.dislikesBeingLycan ? 
                $"{data}\n{owner.visuals.GetCharacterNameWithIconAndColor()} loathes being a Lycanthrope" : 
                $"{data}\n{owner.visuals.GetCharacterNameWithIconAndColor()} enjoys being a Lycanthrope";
            if (owner.lycanData.awareCharacters.Count > 0) {
                data = $"{data}\nAware: ";
                for (int i = 0; i < owner.lycanData.awareCharacters.Count; i++) {
                    Character character = owner.lycanData.awareCharacters[i];
                    data = $"{data}{character.visuals.GetCharacterNameWithIconAndColor()}";
                    if (!CollectionUtilities.IsLastIndex(owner.lycanData.awareCharacters, i)) {
                        data = $"{data}, ";
                    }
                }    
            }
            return data;
        }
        public override void OnSeePOIEvenCannotWitness(IPointOfInterest targetPOI, Character character) {
            base.OnSeePOIEvenCannotWitness(targetPOI, character);
            if (IsHuntingForPrey() && targetPOI is Character seenCharacter && !seenCharacter.isDead && !owner.combatComponent.IsInActualCombatWith(seenCharacter)) {
                if (owner.relationshipContainer.IsFriendsWith(seenCharacter)) {
                    CRIME_SEVERITY severity = CrimeManager.Instance.GetCrimeSeverity(seenCharacter, owner, owner, CRIME_TYPE.Werewolf);
                    if (severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable) {
                        JobQueueItem huntPreyJob = owner.jobQueue.GetJob(JOB_TYPE.LYCAN_HUNT_PREY);
                        huntPreyJob?.ForceCancelJob("avoiding discovery");
                        owner.crimeComponent.FleeToAllVillagerInRangeThatConsidersCrimeTypeACrime(owner, CRIME_TYPE.Werewolf, "avoiding discovery");
                    }
                }
            }
        }
        public override bool PerTickWhileStationaryOrUnoccupied(Character p_character) {
            if (p_character.hasMarker && p_character.marker.isMoving && (p_character.lycanData.activeForm == p_character.lycanData.lycanthropeForm || p_character.lycanData.isInWerewolfForm)) {
                float roll = Random.Range(0f, 100f);
                float chance = 0.85f;
                if (p_character.currentRegion.GetTileObjectInRegionCount(TILE_OBJECT_TYPE.WEREWOLF_PELT) >= 3) {
                    chance = 0.5f;
                }
                if (roll < chance && p_character.gridTileLocation.tileObjectComponent.objHere == null) {
                    //spawn werewolf pelt
                    Messenger.Broadcast(CharacterSignals.LYCANTHROPE_SHED_WOLF_PELT, p_character);
                    p_character.interruptComponent.TriggerInterrupt(INTERRUPT.Shed_Pelt, p_character);
                }
            }
            if (p_character.needsComponent.isStarving && p_character.lycanData.isMaster && !IsHuntingForPrey() && GameUtilities.RollChance(1)) {
                Character huntPreyTarget = GetHuntPreyTarget();
                if (huntPreyTarget != null) {
                    p_character.jobComponent.TriggerHuntPreyJob(huntPreyTarget);    
                }
            }
            if (p_character.lycanData.dislikesBeingLycan && GameUtilities.RollChance(1)) { //1
                if (IsHuntingForPrey()) {
                    ResistHunger();
                }
            }
            return false;
        }
        private void ResistHunger() {
            JobQueueItem huntPreyJob = owner.jobQueue.GetJob(JOB_TYPE.LYCAN_HUNT_PREY);
            huntPreyJob?.ForceCancelJob("Resisted Hunger");
            
            owner.traitContainer.AddTrait(owner, "Ashamed");
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Trait", "Lycanthrope", "resist_hunger", null, LOG_TAG.Needs);
            log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddLogToDatabase(true);
            if (owner.lycanData.isInWerewolfForm) {
                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Revert_From_Werewolf, owner);    
            }
        }
        private bool IsHuntingForPrey() {
            if (owner.currentJob is GoapPlanJob && owner.currentJob.jobType == JOB_TYPE.LYCAN_HUNT_PREY) {
                return true;
            } else if (owner.currentJob is CharacterStateJob && owner.stateComponent.currentState is CombatState combatState) {
                if (combatState.currentClosestHostile != null) {
                    CombatData combatData = owner.combatComponent.GetCombatData(combatState.currentClosestHostile);
                    if (combatData != null && combatData.connectedAction != null && combatData.connectedAction.associatedJobType == JOB_TYPE.LYCAN_HUNT_PREY) {
                        return true;
                    }    
                }
            }
            return false;
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

        public override string TriggerFlaw(Character character) {
            if (!TryDoLycanBehaviour(true)) {
                return "fail_no_target";
            }
            return base.TriggerFlaw(character);
        }

        public void CheckIfAlone() {
            TryDoLycanBehaviour();
        }
        private bool TryDoLycanBehaviour(bool p_isFlawTriggered = false) {
            if (owner.lycanData.isMaster) {
                return TryMasterLycanHuntPrey(p_isFlawTriggered);
            } else {
                if (IsAlone()) {
                    DoTriggerFlawTransform();
                } else {
                    GoOutsideForStealthTransform();
                }
                return true;
            }
        }
        private bool IsAlone() {
            return !owner.crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(CRIME_TYPE.Werewolf);
            // return owner.marker.inVisionCharacters.Count == 0;
        }
        private void DoTriggerFlawTransform() {
            owner.lycanData.Transform(owner);
        }
        private void GoOutsideForStealthTransform() {
            //go to a random tile outside settlement or surrounding areas
            //then check if the character is alone, if not pick another random tile,
            //repeat the process until alone, then transform to wolf
            OtherData[] otherData = null;
            BaseSettlement currentSettlement = owner.currentSettlement;
            if (currentSettlement != null && currentSettlement.locationType == LOCATION_TYPE.VILLAGE) {
                otherData = new OtherData[] { new SettlementOtherData(currentSettlement) };
            } else {
                Area chosenArea = owner.areaLocation.neighbourComponent.GetRandomAdjacentNoSettlementHextileWithinRegion();
                if (chosenArea == null) {
                    chosenArea = owner.areaLocation;
                }
                otherData = new OtherData[] { new AreaOtherData(chosenArea) };
            }
            owner.PlanFixedJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.STEALTH_TRANSFORM, owner, otherData);
        }
        private bool TryMasterLycanHuntPrey(bool p_isFlawTriggered = false) {
            Character huntPreyTarget = GetHuntPreyTarget();
            if (huntPreyTarget != null) {
                owner.jobQueue.CancelAllJobs();
                return owner.jobComponent.TriggerHuntPreyJob(huntPreyTarget, p_isFlawTriggered);
            }
            return false;
        }

        private Character GetHuntPreyTarget() {
#if DEBUG_LOG
            string log = $"{GameManager.Instance.TodayLogString()} {owner.name} will try to get hunt prey target";
#endif
            WeightedDictionary<Character> choices = new WeightedDictionary<Character>();
            int animalCount = 0;
            var size = Physics2D.OverlapCircleNonAlloc(owner.worldPosition, 20f, _triggerFlawNearbyTargets, GameUtilities.Filtered_Layer_Mask);
            for (int i = 0; i < size; i++) {
                Collider2D collider2D = _triggerFlawNearbyTargets[i];
                POIVisionTrigger visionTrigger = collider2D.gameObject.GetComponent<POIVisionTrigger>();
                if (visionTrigger != null && visionTrigger.poi is Character otherCharacter) {
                    if (otherCharacter != owner && !otherCharacter.isDead) { 
                        int weight = 0; 
                        if (otherCharacter is Animal) { 
                            if (animalCount< 3) { 
                                weight = 10; 
                                animalCount++;    
                            } else { 
                                continue; //skip
                            }
                        } else if (otherCharacter.race.IsSapient()){ 
                            if (otherCharacter.faction != owner.faction && !owner.isDead) { 
                                if (!owner.relationshipContainer.IsFriendsWith(otherCharacter) && owner.movementComponent.HasPathToEvenIfDiffRegion(otherCharacter.gridTileLocation)) {
                                    weight = 10;
                                }
                            }
                        }
                        if (weight > 0) { 
                            choices.AddElement(otherCharacter, weight);
                        }
                    }
                }
            }



            // int animalCount = 0;
            // for (int i = 0; i < owner.currentRegion.charactersAtLocation.Count; i++) { 
            //     Character otherCharacter = owner.currentRegion.charactersAtLocation[i]; 
            //     if (otherCharacter != owner && !otherCharacter.isDead) { 
            //         int weight = 0; 
            //         if (otherCharacter is Animal) { 
            //             if (animalCount< 3) { 
            //                 weight = 10; 
            //                 animalCount++;    
            //             } else { 
            //                 continue; //skip
            //             }
            //         } else if (otherCharacter.race.IsSapient()){ 
            //             if (otherCharacter.faction != owner.faction && !owner.isDead) { 
            //                 if (!owner.relationshipContainer.IsFriendsWith(otherCharacter)) { 
            //                     weight = 10;    
            //                 }
            //             }
            //         }
            //         if (weight > 0) { 
            //             choices.AddElement(otherCharacter, weight);
            //         }
            //     }
            // }

#if DEBUG_LOG
            log += $"\n{choices.GetWeightsSummary("Weights are:")}";
#endif
            if (choices.GetTotalOfWeights() > 0) {
			    Character target = choices.PickRandomElementGivenWeights();
#if DEBUG_LOG
			    log += $"\nChosen target is {target.name}";
                owner.logComponent.PrintLogIfActive(log);
#endif
                return target;
            }
#if DEBUG_LOG
            owner.logComponent.PrintLogIfActive(log);
#endif
            return null;
        }
        public override string GetTriggerFlawEffectDescription(Character character, string key) {
            if (owner.lycanData.isMaster) {
                key = "flaw_effect_master";
            }
            return base.GetTriggerFlawEffectDescription(character, key);
        }
    }

    //Lycanthrope data has only 1 instance but referenced by two characters: original and lycanthrope form
    //So if we need to do process something in this, we must always pass the character that referenced this data as a parameter because this data is shared
    public class LycanthropeData {
        public Character activeForm { get; private set; }
        public Character limboForm { get; private set; }
        public Character lycanthropeForm { get; private set; }
        public Character originalForm { get; private set; }
        
        public bool dislikesBeingLycan { get; private set; }
        public bool isMaster { get; private set; }
        public List<Character> awareCharacters { get; private set; }
        public bool isInWerewolfForm { get; private set; }

        public Character plainWolf { get; private set; }
        public Character direWolf { get; private set; }

        public GameObject transformRevertEffectGO { get; private set; }

        public LycanthropeData(Character originalForm) {
            this.originalForm = originalForm;
            isMaster = false;
            CreatePlainWolfForm();
            UpdateLycanForm();
            activeForm = originalForm;
            limboForm = lycanthropeForm;
            originalForm.SetLycanthropeData(this);
            originalForm.traitContainer.AddTrait(originalForm, "Lycanthrope");
            awareCharacters = new List<Character>();
            DetermineIfDesireOrDislike(originalForm);
            Messenger.AddListener<SkillData>("LycanthropyLevelUp", OnLycanthropyLevelUp);
        }
        public LycanthropeData(Character originalForm, Character lycanthropeForm, Character activeForm, Character limboForm, Character plainWolf, Character direWolf) {
            this.originalForm = originalForm;
            this.lycanthropeForm = lycanthropeForm;
            this.activeForm = activeForm;
            this.limboForm = limboForm;
            this.plainWolf = plainWolf;
            this.direWolf = direWolf;
            originalForm.SetLycanthropeData(this);
            plainWolf?.SetLycanthropeData(this);
            direWolf?.SetLycanthropeData(this);
            Messenger.AddListener<SkillData>("LycanthropyLevelUp", OnLycanthropyLevelUp);
        }

        private void CreatePlainWolfForm() {
            plainWolf = CharacterManager.Instance.CreateNewLimboSummon(SUMMON_TYPE.Wolf, faction: FactionManager.Instance.neutralFaction);
            plainWolf.ConstructInitialGoapAdvertisementActions();
            plainWolf.SetFirstAndLastName(originalForm.firstName, originalForm.surName);
            plainWolf.SetLycanthropeData(this);
            plainWolf.traitContainer.AddTrait(plainWolf, "Lycanthrope");
        }
        private void CreateDireWolfForm() {
            direWolf = CharacterManager.Instance.CreateNewLimboSummon(SUMMON_TYPE.Dire_Wolf, faction: FactionManager.Instance.neutralFaction);
            direWolf.ConstructInitialGoapAdvertisementActions();
            direWolf.SetFirstAndLastName(originalForm.firstName, originalForm.surName);
            direWolf.SetLycanthropeData(this);
            direWolf.traitContainer.AddTrait(direWolf, "Lycanthrope");
        }

        private void UpdateLycanForm() {
            lycanthropeForm = plainWolf;
            if (originalForm.HasAfflictedByPlayerWith(PLAYER_SKILL_TYPE.LYCANTHROPY)) {
                int level = PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.LYCANTHROPY).currentLevel;
                if(level >= 2) {
                    SetIsMaster(true);
                }
                if (level >= 1) {
                    if(direWolf == null) {
                        CreateDireWolfForm();
                    }
                    lycanthropeForm = direWolf;
                }

            }
        }
        private void UpdateLycanFormName() {
            lycanthropeForm.SetFirstAndLastName(originalForm.firstName, originalForm.surName);
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
            if(UIManager.Instance.characterInfoUI.activeCharacter == activeForm) {
                UIManager.Instance.characterInfoUI.CloseMenu();
            }
            if (UIManager.Instance.monsterInfoUI.activeMonster == activeForm) {
                UIManager.Instance.monsterInfoUI.CloseMenu();
            }
            UpdateLycanForm();
            activeForm.traitContainer.RemoveTrait(activeForm, "Transforming");
            activeForm = lycanthropeForm;
            limboForm = originalForm;
            LocationGridTile tile = originalForm.gridTileLocation;
            Region homeRegion = originalForm.homeRegion;
            PutToLimbo(originalForm);
            UpdateLycanFormName();
            ReleaseFromLimbo(lycanthropeForm, tile, homeRegion);
            CopyImportantTraits(originalForm, lycanthropeForm);
            lycanthropeForm.needsComponent.ResetFullnessMeter();
            lycanthropeForm.needsComponent.ResetTirednessMeter();
            lycanthropeForm.needsComponent.ResetHappinessMeter();
            lycanthropeForm.needsComponent.ResetStaminaMeter();
            lycanthropeForm.needsComponent.ResetHopeMeter();
            lycanthropeForm.traitContainer.AddTrait(lycanthropeForm, "Transitioning");
            
            if (UIManager.Instance.IsContextMenuShowingForTarget(originalForm)) {
                UIManager.Instance.RefreshPlayerActionContextMenuWithNewTarget(lycanthropeForm);
            }
            
            Messenger.Broadcast(CharacterSignals.ON_SWITCH_FROM_LIMBO, originalForm, lycanthropeForm);
            activeForm.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(activeForm);
        }

        public void RevertToNormal() {
            if (UIManager.Instance.characterInfoUI.activeCharacter == activeForm) {
                UIManager.Instance.characterInfoUI.CloseMenu();
            }
            if (UIManager.Instance.monsterInfoUI.activeMonster == activeForm) {
                UIManager.Instance.monsterInfoUI.CloseMenu();
            }

            activeForm.traitContainer.RemoveTrait(activeForm, "Transforming");
            activeForm = originalForm;
            limboForm = lycanthropeForm;
            LocationGridTile tile = lycanthropeForm.gridTileLocation;
            Region homeRegion = lycanthropeForm.homeRegion;
            UpdateLycanFormName();
            PutToLimbo(lycanthropeForm);
            ReleaseFromLimbo(originalForm, tile, homeRegion);
            CopyImportantTraits(lycanthropeForm, originalForm);
            lycanthropeForm.traitContainer.RemoveTrait(lycanthropeForm, "Transitioning");
            
            if (UIManager.Instance.IsContextMenuShowingForTarget(lycanthropeForm)) {
                UIManager.Instance.RefreshPlayerActionContextMenuWithNewTarget(originalForm);
            }
            
            Messenger.Broadcast(CharacterSignals.ON_SWITCH_FROM_LIMBO, lycanthropeForm, originalForm);
            activeForm.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(activeForm);
        }
        private void CopyImportantTraits(Character p_copyFrom, Character p_copyTo) {
            if (p_copyFrom.traitContainer.HasTrait("Restrained")) {
                Trait restrained = p_copyFrom.traitContainer.GetTraitOrStatus<Trait>("Restrained");
                TraitManager.Instance.CopyTraitOrStatus(restrained, p_copyFrom, p_copyTo);
            }
            if (p_copyFrom.traitContainer.HasTrait("Prisoner")) {
                Trait prisoner = p_copyFrom.traitContainer.GetTraitOrStatus<Trait>("Prisoner");
                TraitManager.Instance.CopyTraitOrStatus(prisoner, p_copyFrom, p_copyTo);
            }
        }
        private void PutToLimbo(Character form) {
            if (UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter == form) {
                UIManager.Instance.characterInfoUI.CloseMenu();
            }
            if(form.marker && form.marker.isMoving) {
                form.marker.StopMovement();
            }
            if (form.trapStructure.IsTrapped()) {
                form.trapStructure.ResetAllTrapStructures();
            }
            if (form.trapStructure.IsTrappedInArea()) {
                form.trapStructure.ResetTrapArea();
            }
            //Added this because of this issue:
            //https://trello.com/c/Vx50lcFi/4344-nullreference-canseeobjectlocationhere
            if (form.partyComponent.hasParty) {
                form.partyComponent.currentParty.RemoveMemberThatJoinedQuest(form);
            }
            Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, form as IPointOfInterest, "");
            Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, form as IPointOfInterest, "");
            if (!form.carryComponent.IsNotBeingCarried()) {
                form.carryComponent.masterCharacter.UncarryPOI(form);
            }
            //ForceCancelAllJobsTargettingThisCharacter();
            //form.marker.ClearTerrifyingObjects();
            form.needsComponent.OnCharacterLeftLocation(form.currentRegion);

            form.jobQueue.CancelAllJobs();
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
                for (int i = 0; i < form.marker.inVisionPOIsButDiffStructure.Count; i++) {
                    IPointOfInterest otherPOI = form.marker.inVisionPOIsButDiffStructure[i];
                    if (otherPOI is Character) {
                        (otherPOI as Character).marker.RemovePOIAsInRangeButDifferentStructure(form);
                    }
                }
                form.DestroyMarker(removeFromMasterList: false);
            }
            form.currentRegion?.RemoveCharacterFromLocation(form);
            form.homeRegion?.RemoveResident(form);
            CharacterManager.Instance.AddNewLimboCharacter(form);
            CharacterManager.Instance.RemoveCharacter(form, false, false);
            Messenger.AddListener(Signals.TICK_STARTED, form.OnTickStartedWhileSeizedOrIsInLimbo);
        }
        private void ReleaseFromLimbo(Character form, LocationGridTile tileLocation, Region homeRegion) {
            if (Messenger.eventTable.ContainsKey(Signals.TICK_STARTED)) {
                Messenger.RemoveListener(Signals.TICK_STARTED, form.OnTickStartedWhileSeizedOrIsInLimbo);
            }
            homeRegion?.AddResident(form);
            form.needsComponent.OnCharacterArrivedAtLocation(tileLocation.structure.region.coreTile.region);
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
            CharacterManager.Instance.AddNewCharacter(form, false, false);
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
            lycanthropeForm.homeSettlement?.RemoveResident(lycanthropeForm);
            lycanthropeForm.faction?.LeaveFaction(lycanthropeForm);
            CharacterManager.Instance.RemoveLimboCharacter(lycanthropeForm);
            originalForm.SetLycanthropeData(null);
            plainWolf?.SetLycanthropeData(null);
            direWolf?.SetLycanthropeData(null);
            Messenger.RemoveListener<SkillData>("LycanthropyLevelUp", OnLycanthropyLevelUp);
        }

        //Parameter: which form is this data erased?
        public void LycanDies(Character form, string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null, Log _deathLog = default, LogFillerStruct[] deathLogFillers = null) {
            if (form != activeForm) {
                return;
            }
            originalForm.traitContainer.RemoveTrait(originalForm, "Lycanthrope");
            if (form == lycanthropeForm) {
                RevertToNormal();
                //CharacterManager.Instance.RemoveLimboCharacter(lycanthropeForm);
                originalForm.SetLycanthropeData(null);
                plainWolf?.SetLycanthropeData(null);
                direWolf?.SetLycanthropeData(null);
                Messenger.RemoveListener<SkillData>("LycanthropyLevelUp", OnLycanthropyLevelUp);
                originalForm.Death(cause, deathFromAction, responsibleCharacter, _deathLog, deathLogFillers);
            } 
            //else if (form == originalForm) {
            //    originalForm.traitContainer.RemoveTrait(originalForm, "Lycanthrope");
            //}
        }

        private void OnLycanthropyLevelUp(SkillData p_skill) {
            if (p_skill.currentLevel >= 2) {
                if (originalForm.HasAfflictedByPlayerWith(PLAYER_SKILL_TYPE.LYCANTHROPY)) {
                    SetIsMaster(true);
                }
            }
        }
        public void SetIsInWerewolfForm(bool state) {
            isInWerewolfForm = state;
        }

#region Additional Data
        public void SetDislikesBeingLycan(bool state) {
            dislikesBeingLycan = state;
        }
        public void SetIsMaster(bool state) {
            isMaster = state;
        }
        private void DetermineIfDesireOrDislike(Character character) {
            if(character.traitContainer.HasTrait("Lycanphobic", "Chaste")) {
                SetDislikesBeingLycan(true);
                return;
            }
            if (character.traitContainer.HasTrait("Lycanphiliac")) {
                SetDislikesBeingLycan(false);
                return;
            }
            if (character.traitContainer.HasTrait("Cultist") && GameUtilities.RollChance(75)) {
                SetDislikesBeingLycan(false);
                return;
            }
            if (character.characterClass.className == "Hero" && GameUtilities.RollChance(75)) {
                SetDislikesBeingLycan(true);
                return;
            }
            if (character.characterClass.className == "Shaman" && GameUtilities.RollChance(80)) {
                SetDislikesBeingLycan(true);
                return;
            }
            if (character.traitContainer.HasTrait("Vampire") && GameUtilities.RollChance(80)) {
                SetDislikesBeingLycan(true);
                return;
            }
            if (character.traitContainer.HasTrait("Evil", "Treacherous") && GameUtilities.RollChance(75)) {
                SetDislikesBeingLycan(false);
                return;
            }
            if (character.race == RACE.ELVES && GameUtilities.RollChance(75)) {
                SetDislikesBeingLycan(false);
                return;
            }
            SetDislikesBeingLycan(GameUtilities.RollChance(50));
        }
#endregion

#region Aware Characters
        public void AddAwareCharacter(Character character) {
            if (!awareCharacters.Contains(character)) {
                awareCharacters.Add(character);
                if (character.traitContainer.HasTrait("Lycanphiliac")) {
                    Lycanphiliac lycanphiliac = character.traitContainer.GetTraitOrStatus<Lycanphiliac>("Lycanphiliac");
                    lycanphiliac.OnBecomeAwareOfLycan(originalForm);
                } else if (character.traitContainer.HasTrait("Lycanphobic")) {
                    Lycanphobic lycanphobic = character.traitContainer.GetTraitOrStatus<Lycanphobic>("Lycanphobic");
                    lycanphobic.OnBecomeAwareOfLycan(originalForm);
                }
            }
        }
        public void LoadAwareCharacters(List<Character> characters) {
            awareCharacters = new List<Character>();
            if (characters.Count > 0) {
                awareCharacters.AddRange(characters);
            }
        }
        public bool DoesCharacterKnowThisLycan(Character character) {
            return awareCharacters.Contains(character);
        }
        public bool DoesFactionKnowThisLycan(Faction faction, bool includeDeadMembersInChecking = true) {
            for (int i = 0; i < faction.characters.Count; i++) {
                Character member = faction.characters[i];
                if (member != originalForm && (includeDeadMembersInChecking || !member.isDead)) {
                    if (DoesCharacterKnowThisLycan(member)) {
                        return true;
                    }
                }
            }
            return false;
        }
#endregion
    }

    [System.Serializable]
    public class SaveDataLycanthropeData : SaveData<LycanthropeData> {
        public string activeForm;
        public string limboForm;

        public string lycanthropeForm;
        public string originalForm;

        public string plainWolf;
        public string direWolf;

        public bool dislikesBeingLycan;
        public bool isMaster;
        public List<string> awareCharacterIDs;

        public bool isInWerewolfForm;

#region Overrides
        public override void Save(LycanthropeData data) {
            activeForm = data.activeForm.persistentID;
            limboForm = data.limboForm.persistentID;

            lycanthropeForm = data.lycanthropeForm.persistentID;
            originalForm = data.originalForm.persistentID;

            plainWolf = data.plainWolf?.persistentID;
            direWolf = data.direWolf?.persistentID;

            dislikesBeingLycan = data.dislikesBeingLycan;
            isMaster = data.isMaster;
            isInWerewolfForm = data.isInWerewolfForm;
            awareCharacterIDs = SaveUtilities.ConvertSavableListToIDs(data.awareCharacters);
        }
        public override LycanthropeData Load() {
            Character origForm = CharacterManager.Instance.GetCharacterByPersistentID(originalForm);
            Character lycanForm = CharacterManager.Instance.GetCharacterByPersistentID(lycanthropeForm);
            Character plainWolf = null;
            if (!string.IsNullOrEmpty(this.plainWolf)) {
                plainWolf = CharacterManager.Instance.GetCharacterByPersistentID(this.plainWolf);
            }
            Character direWolf = null;
            if (!string.IsNullOrEmpty(this.direWolf)) {
                direWolf = CharacterManager.Instance.GetCharacterByPersistentID(this.direWolf);
            }
            Character activeForm = origForm;
            Character limboForm = lycanForm;
            if (this.activeForm == this.lycanthropeForm) {
                activeForm = lycanForm;
                limboForm = origForm;
            } else {
                activeForm = origForm;
                limboForm = lycanForm;
            }
            LycanthropeData data = new LycanthropeData(origForm, lycanForm, activeForm, limboForm, plainWolf, direWolf);
            data.SetDislikesBeingLycan(dislikesBeingLycan);
            data.SetIsMaster(isMaster);
            data.SetIsInWerewolfForm(isInWerewolfForm);
            data.LoadAwareCharacters(SaveUtilities.ConvertIDListToCharacters(awareCharacterIDs));
            return data;
        }
#endregion
    }
}

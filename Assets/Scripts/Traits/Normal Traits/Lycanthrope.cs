using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;
namespace Traits {
    public class Lycanthrope : Trait {
        public Character owner { get; private set; }

        private int _level;

        #region getters
        public override bool isPersistent => true;
        #endregion
        
        public Lycanthrope() {
            name = "Lycanthrope";
            description = "Not a werewolf. Just sometimes turns to a plain ole wolf when it sleeps.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            canBeTriggered = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Per_Tick_Movement);
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.DISPEL };
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
                data = $"{data}Dislikes Being Lycan: {character.lycanData.dislikesBeingLycan.ToString()}";
                data = $"{data}\nIs Master: {character.lycanData.isMaster.ToString()}";
            }
            return data;
        }
        public override bool PerTickOwnerMovement() {
            if (owner.lycanData.activeForm == owner.lycanData.lycanthropeForm || owner.lycanData.isInWerewolfForm) {
                float roll = Random.Range(0f, 100f);
                float chance = 2f;
                if (owner.currentRegion.GetTileObjectInRegionCount(TILE_OBJECT_TYPE.WEREWOLF_PELT) >= 3) {
                    chance = 0.5f;
                }
                if (roll < chance && owner.gridTileLocation.objHere == null) {
                    //spawn werewolf pelt
                    owner.interruptComponent.TriggerInterrupt(INTERRUPT.Shed_Pelt, owner);
                }
            }
            if (owner.lycanData.dislikesBeingLycan && GameUtilities.RollChance(1) && owner.currentJob is GoapPlanJob goapPlanJob) { //1
                if (owner.currentJob.jobType == JOB_TYPE.LYCAN_HUNT_PREY) {
                    ResistHunger();
                } else if (owner.stateComponent.currentState is CombatState) {
                    CombatData combatData = owner.combatComponent.GetCombatData(goapPlanJob.targetPOI);
                    if (combatData != null && combatData.connectedAction != null && combatData.connectedAction.associatedJobType == JOB_TYPE.LYCAN_HUNT_PREY) {
                        ResistHunger();    
                    }
                }
            }
            return false;
        }
        private void ResistHunger() {
            owner.currentJob.ForceCancelJob(false, "Resisted Hunger");
            owner.traitContainer.AddTrait(owner, "Ashamed");
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Trait", "Lycanthrope", "resist_hunger", null, LOG_TAG.Needs);
            log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddLogToDatabase();
            if (owner.lycanData.isInWerewolfForm) {
                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Revert_From_Werewolf, owner);    
            }
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
            if (IsAlone()) {
                DoTransform();
            } else {
                //go to a random tile in the wilderness
                //then check if the character is alone, if not pick another random tile,
                //repeat the process until alone, then transform to wolf
                LocationStructure wilderness = character.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
                //LocationGridTile randomWildernessTile = wilderness.tiles[Random.Range(0, wilderness.tiles.Count)];
                //character.marker.GoTo(randomWildernessTile, CheckIfAlone);
                character.PlanAction(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.STEALTH_TRANSFORM, character, new OtherData[] { new LocationStructureOtherData(wilderness),  });
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
                owner.PlanAction(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.STEALTH_TRANSFORM, owner, new OtherData[] { new LocationStructureOtherData(wilderness) });
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
        
        public bool dislikesBeingLycan { get; private set; }
        public bool isMaster { get; private set; }
        public List<Character> awareCharacters { get; private set; }
        public bool isInWerewolfForm { get; private set; }

        public GameObject transformRevertEffectGO { get; private set; }

        public LycanthropeData(Character originalForm) {
            this.originalForm = originalForm;
            CreateLycanthropeForm();
            activeForm = originalForm;
            limboForm = lycanthropeForm;
            originalForm.traitContainer.AddTrait(originalForm, "Lycanthrope");
            lycanthropeForm.traitContainer.AddTrait(lycanthropeForm, "Lycanthrope");
            originalForm.SetLycanthropeData(this);
            lycanthropeForm.SetLycanthropeData(this);
            isMaster = false;
            awareCharacters = new List<Character>();
            DetermineIfDesireOrDislike(originalForm);
        }
        public LycanthropeData(Character originalForm, Character lycanthropeForm, Character activeForm, Character limboForm) {
            this.originalForm = originalForm;
            this.lycanthropeForm = lycanthropeForm;
            this.activeForm = activeForm;
            this.limboForm = limboForm;
        }

        private void CreateLycanthropeForm() {
            lycanthropeForm = CharacterManager.Instance.CreateNewLimboSummon(SUMMON_TYPE.Wolf, faction: FactionManager.Instance.neutralFaction);
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
            if(UIManager.Instance.characterInfoUI.activeCharacter == activeForm) {
                UIManager.Instance.characterInfoUI.CloseMenu();
            }
            if (UIManager.Instance.monsterInfoUI.activeMonster == activeForm) {
                UIManager.Instance.monsterInfoUI.CloseMenu();
            }

            activeForm.traitContainer.RemoveTrait(activeForm, "Transforming");
            activeForm = lycanthropeForm;
            limboForm = originalForm;
            LocationGridTile tile = originalForm.gridTileLocation;
            Region homeRegion = originalForm.homeRegion;
            PutToLimbo(originalForm);
            ReleaseFromLimbo(lycanthropeForm, tile, homeRegion);
            lycanthropeForm.needsComponent.ResetFullnessMeter();
            lycanthropeForm.needsComponent.ResetTirednessMeter();
            lycanthropeForm.needsComponent.ResetHappinessMeter();
            lycanthropeForm.needsComponent.ResetStaminaMeter();
            lycanthropeForm.needsComponent.ResetHopeMeter();
            lycanthropeForm.traitContainer.AddTrait(lycanthropeForm, "Transitioning");
            Messenger.Broadcast(Signals.ON_SWITCH_FROM_LIMBO, originalForm, lycanthropeForm);
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
            PutToLimbo(lycanthropeForm);
            ReleaseFromLimbo(originalForm, tile, homeRegion);
            lycanthropeForm.traitContainer.RemoveTrait(lycanthropeForm, "Transitioning");
            Messenger.Broadcast(Signals.ON_SWITCH_FROM_LIMBO, lycanthropeForm, originalForm);
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
            if (form.trapStructure.IsTrappedInHex()) {
                form.trapStructure.ResetAllTrapHexes();
            }
            Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, form as IPointOfInterest, "");
            if (form.carryComponent.isBeingCarriedBy != null) {
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
        public void LycanDies(Character form, string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null, Log _deathLog = default, LogFillerStruct[] deathLogFillers = null) {
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
                SetDislikesBeingLycan(true);
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
        public bool DoesFactionKnowThisLycan(Faction faction) {
            for (int i = 0; i < faction.characters.Count; i++) {
                Character member = faction.characters[i];
                if (member != originalForm) {
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
            dislikesBeingLycan = data.dislikesBeingLycan;
            isMaster = data.isMaster;
            isInWerewolfForm = data.isInWerewolfForm;
            awareCharacterIDs = SaveUtilities.ConvertSavableListToIDs(data.awareCharacters);
        }
        public override LycanthropeData Load() {
            Character origForm = CharacterManager.Instance.GetCharacterByPersistentID(originalForm);
            Character lycanForm = CharacterManager.Instance.GetCharacterByPersistentID(lycanthropeForm);
            Character activeForm = origForm;
            Character limboForm = lycanForm;
            if (this.activeForm == this.lycanthropeForm) {
                activeForm = lycanForm;
                limboForm = origForm;
            } else {
                activeForm = origForm;
                limboForm = lycanForm;
            }
            LycanthropeData data = new LycanthropeData(origForm, lycanForm, activeForm, limboForm);
            data.SetDislikesBeingLycan(dislikesBeingLycan);
            data.SetIsMaster(isMaster);
            data.SetIsInWerewolfForm(isInWerewolfForm);
            data.LoadAwareCharacters(SaveUtilities.ConvertIDListToCharacters(awareCharacterIDs));
            return data;
        }
        #endregion
    }
}

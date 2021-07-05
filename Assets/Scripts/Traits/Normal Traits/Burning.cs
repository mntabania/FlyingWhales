using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UtilityScripts;
using Random = UnityEngine.Random;
namespace Traits {
    public class Burning : Status, IElementalTrait {
        private ITraitable owner { get; set; }
        public BurningSource sourceOfBurning { get; private set; }
        public bool isPlayerSource { get; private set; }
        public override bool isPersistent => true;
        public Character douser { get; private set; } //the character that is going to douse this fire.
        private GameObject burningEffect;
        private readonly List<ITraitable> _burningSpreadChoices;
        private bool _hasBeenRemoved;

        #region getters
        public override Type serializedData => typeof(SaveDataBurning);
        #endregion

        public Burning() {
            name = "Burning";
            description = "On fire!";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(3);
            isTangible = true;
            hindersSocials = true;
            moodEffect = -25;
            _burningSpreadChoices = new List<ITraitable>();
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.EXTRACT_ITEM };
            AddTraitOverrideFunctionIdentifier(TraitManager.Initiate_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Destroy_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Execute_Pre_Effect_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Execute_Per_Tick_Effect_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Death_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Villager_Reaction);
        }

        #region Loading
        public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadFirstWaveInstancedTrait(saveDataTrait);
            SaveDataBurning data = saveDataTrait as SaveDataBurning;
            Assert.IsNotNull(data);
            BurningSource burningSource = DatabaseManager.Instance.burningSourceDatabase.GetOrCreateBurningSourceWithID(data.persistentID);
            LoadSourceOfBurning(burningSource);
            isPlayerSource = data.isPlayerSource;
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            owner = addTo;
            if (addTo is IPointOfInterest poi) {
                burningEffect = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Burning, false);
            } else if (addTo is ThinWall structureWallObject) {
                burningEffect = GameManager.Instance.CreateParticleEffectAt(structureWallObject, PARTICLE_EFFECT.Burning);
            }
            sourceOfBurning?.AddObjectOnFire(owner);
            Messenger.AddListener(Signals.TICK_ENDED, PerTickEnded);
            if (addTo is Character) {
                Messenger.AddListener<Character>(CharacterSignals.CHARACTER_MARKER_EXPIRED, OnCharacterMarkerExpired);
            }
        }
        #endregion
        
        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            owner = addedTo;
            if (addedTo is IPointOfInterest poi) {
                poi.AddAdvertisedAction(INTERACTION_TYPE.DOUSE_FIRE);
                burningEffect = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Burning, false);
                if (poi is Character character) {
                    character.AdjustDoNotRecoverHP(1);
                    if(character.limiterComponent.canMove && character.limiterComponent.canWitness && character.limiterComponent.canPerform) {
                        CreateJobsOnEnterVisionBasedOnTrait(character, character);
                    }
                    CharacterBurningProcess(character);
                } else {
                    poi.SetPOIState(POI_STATE.INACTIVE);
                }
                if(poi is WinterRose winterRose) {
                    winterRose.WinterRoseEffect();
                } else {
                    //Will not reprocess if winter rose since it will be destroyed anyway
                    Messenger.Broadcast(CharacterSignals.REPROCESS_POI, poi);
                }
            } else if (addedTo is ThinWall structureWallObject) {
                burningEffect = GameManager.Instance.CreateParticleEffectAt(structureWallObject, PARTICLE_EFFECT.Burning);
            }
            if (sourceOfBurning != null && !sourceOfBurning.objectsOnFire.Contains(owner)) {
                //this is so that addedTo will be added to the list of objects on fire of the burning source, if it isn't already.
                SetSourceOfBurning(sourceOfBurning, owner);
            }
            Messenger.AddListener(Signals.TICK_ENDED, PerTickEnded);
            if (addedTo is Character) {
                Messenger.AddListener<Character>(CharacterSignals.CHARACTER_MARKER_EXPIRED, OnCharacterMarkerExpired);
            }
            
            base.OnAddTrait(addedTo);
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            _hasBeenRemoved = true;
            SetDouser(null); //reset douser so that any signals related to that will be removed.
            SetSourceOfBurning(null, removedFrom);
            Messenger.RemoveListener(Signals.TICK_ENDED, PerTickEnded);
            if (removedFrom is Character) {
                Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_MARKER_EXPIRED, OnCharacterMarkerExpired);
            }
            if (burningEffect) {
                ObjectPoolManager.Instance.DestroyObject(burningEffect);
                burningEffect = null;
            }
            if (removedFrom is IPointOfInterest obj) {
                obj.RemoveAdvertisedAction(INTERACTION_TYPE.DOUSE_FIRE);
                if (removedFrom is Character character) {
                    // character.ForceCancelAllJobsTargettingThisCharacter(JOB_TYPE.REMOVE_STATUS);
                    character.AdjustDoNotRecoverHP(-1);
                    DisablePlayerSourceChaosOrb(character);
                } else {
                    if(obj is Bed bed) {
                        if (bed.IsSlotAvailable()) {
                            obj.SetPOIState(POI_STATE.ACTIVE);
                        }
                    } else {
                        obj.SetPOIState(POI_STATE.ACTIVE);
                    }
                }
            } 
        }
        public override void OnRemoveStatusBySchedule(ITraitable removedFrom) {
            base.OnRemoveStatusBySchedule(removedFrom);
            if (removedFrom is TileObject) {
                removedFrom.traitContainer.AddTrait(removedFrom, "Burnt");    
            }
        }
        public override bool OnDeath(Character character) {
            return character.traitContainer.RemoveTrait(character, this);
        }
        public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
            if (traitOwner.gridTileLocation != null 
                && characterThatWillDoJob.homeSettlement != null
                && traitOwner.gridTileLocation.IsPartOfSettlement(characterThatWillDoJob.homeSettlement)) {
                characterThatWillDoJob.homeSettlement.settlementJobTriggerComponent.TriggerDouseFire();
            }
            
            //pyrophobic handling
            Pyrophobic pyrophobic = characterThatWillDoJob.traitContainer.GetTraitOrStatus<Pyrophobic>("Pyrophobic");
            pyrophobic?.AddKnownBurningSource(sourceOfBurning, traitOwner);
            
            return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
        }
        public override string GetTestingData(ITraitable traitable = null) {
            return sourceOfBurning != null ? $"Douser: {douser?.name ?? "None"}. {sourceOfBurning}" : base.GetTestingData(traitable);
        }
        public override void ExecuteActionPreEffects(INTERACTION_TYPE action, ActualGoapNode goapNode) {
            base.ExecuteActionPreEffects(action, goapNode);
            if (goapNode.action.actionCategory == ACTION_CATEGORY.CONSUME || goapNode.action.actionCategory == ACTION_CATEGORY.DIRECT) {
                if (Random.Range(0, 100) < 10) { //5
                    goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Burning", out var trait, bypassElementalChance: true);
                    (trait as Burning)?.SetSourceOfBurning(sourceOfBurning, goapNode.actor);
                    Burning burning = goapNode.actor.traitContainer.GetTraitOrStatus<Burning>("Burning");
                    burning?.SetIsPlayerSource(isPlayerSource);
                }
            }
        }
        public override void ExecuteActionPerTickEffects(INTERACTION_TYPE action, ActualGoapNode goapNode) {
            base.ExecuteActionPerTickEffects(action, goapNode);
            if (goapNode.action.actionCategory == ACTION_CATEGORY.CONSUME || goapNode.action.actionCategory == ACTION_CATEGORY.DIRECT) {
                if (Random.Range(0, 100) < 10) { //5
                    goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Burning", out var trait, bypassElementalChance: true);
                    (trait as Burning)?.SetSourceOfBurning(sourceOfBurning, goapNode.actor);
                    Burning burning = goapNode.actor.traitContainer.GetTraitOrStatus<Burning>("Burning");
                    burning?.SetIsPlayerSource(isPlayerSource);
                }
            }
        }
        public override void OnInitiateMapObjectVisual(ITraitable traitable) {
            if (traitable is IPointOfInterest poi) {
                if (burningEffect) {
                    ObjectPoolManager.Instance.DestroyObject(burningEffect);
                    burningEffect = null;
                }
                burningEffect = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Burning, false);
            }
        }
        public override void OnDestroyMapObjectVisual(ITraitable traitable) {
            if (burningEffect) {
                ObjectPoolManager.Instance.DestroyObject(burningEffect);
                burningEffect = null;
            }
        }
        protected override string GetDescriptionInUI() {
            string desc = base.GetDescriptionInUI();
            desc += "\nIs Player Source: " + isPlayerSource;
            return desc;
        }
        #endregion

        public void LoadSourceOfBurning(BurningSource source) {
            sourceOfBurning = source;
        }
        public void SetSourceOfBurning(BurningSource source, ITraitable obj) {
            sourceOfBurning = source;
            if (sourceOfBurning != null) {
                source.AddObjectOnFire(obj);
            }
        }

        #region Listeners
        private void OnCharacterMarkerExpired(Character character) {
            if (owner == character) {
                //This is so that if the character that has this trait expires, he/she will lose this trait and be removed from the burning source's objects on fire. 
                character.traitContainer.RemoveTrait(character, this);
            }
        }
        private void PerTickEnded() {
            if (_hasBeenRemoved) {
                //if in case that this trait has been removed on the same tick that this runs, do not allow spreading.
                return;
            }
            //Every tick, a Burning tile, object or character has a 15% chance to spread to an adjacent flammable tile, flammable character, 
            //flammable object or the object in the same tile.
            if(PlayerManager.Instance.player.seizeComponent.seizedPOI == owner) {
                //Temporary fix only, if the burning object is seized, spreading of fire should not trigger
                return;
            }
            
            if(owner.gridTileLocation == null) {
                //Temporary fix only, if the burning object has no longer have a tile location (presumably destroyed), spreading of fire should not trigger
                return;
            }
#if DEBUG_PROFILER
            Profiler.BeginSample($"Burning - Tick Ended Part 1");
#endif
            owner.AdjustHP(-2, ELEMENTAL_TYPE.Normal, true, this, showHPBar: true, isPlayerSource: isPlayerSource);

            //Sleeping characters in bed should also receive damage
            //https://trello.com/c/kFZAHo11/1203-sleeping-characters-in-bed-should-also-receive-damage
            if (owner is BaseBed bed) {
                if(bed.users != null && bed.users.Length > 0) {
                    for (int i = 0; i < bed.users.Length; i++) {
                        Character user = bed.users[i];
                        if (user != null) {
                            user.AdjustHP(-2, ELEMENTAL_TYPE.Normal, true, this, showHPBar: true, isPlayerSource: isPlayerSource);
                        }
                    }
                }
            }
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif

            if (Random.Range(0, 100) >= 2) {
                return;
            }
#if DEBUG_PROFILER
            Profiler.BeginSample($"Burning - Tick Ended Part 2");
#endif
            _burningSpreadChoices.Clear();
            if (ShouldSpreadFire()) {
                LocationGridTile origin = owner.gridTileLocation;
                List<LocationGridTile> affectedTiles = RuinarchListPool<LocationGridTile>.Claim();
                origin.PopulateTilesInRadius(affectedTiles, 1, includeCenterTile: true, includeTilesInDifferentStructure: true);
                for (int i = 0; i < affectedTiles.Count; i++) {
                    List<ITraitable> traitablesOnTile = RuinarchListPool<ITraitable>.Claim();
                    affectedTiles[i].PopulateTraitablesOnTileThatCanHaveElementalTrait(traitablesOnTile, "Burning", true);
                    for (int j = 0; j < traitablesOnTile.Count; j++) {
                        _burningSpreadChoices.Add(traitablesOnTile[j]);
                    }
                    RuinarchListPool<ITraitable>.Release(traitablesOnTile);
                }
                if (_burningSpreadChoices.Count > 0) {
                    ITraitable chosen = _burningSpreadChoices[Random.Range(0, _burningSpreadChoices.Count)];
                    if (chosen.gridTileLocation != null) {
                        chosen.traitContainer.AddTrait(chosen, "Burning", out var trait, bypassElementalChance: true);
                        (trait as Burning)?.SetSourceOfBurning(sourceOfBurning, chosen);
                        Burning burning = chosen.traitContainer.GetTraitOrStatus<Burning>("Burning");
                        burning?.SetIsPlayerSource(isPlayerSource);
                    }
                }
                RuinarchListPool<LocationGridTile>.Release(affectedTiles);
            }
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif

        }
#endregion

#region Douser
        public void SetDouser(Character character) {
            douser = character;
            if (douser == null) {
                Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
            } else {
                Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
            }
        }
        private void OnJobRemovedFromCharacter(JobQueueItem jqi, Character character) {
            if (douser == character && jqi.jobType == JOB_TYPE.DOUSE_FIRE) {
                SetDouser(null); 
            }
        }
#endregion

#region Utilities
        public void CharacterBurningProcess(Character character) {
            if (character.isDead) {
                //Should not process if character is dead
                return;
            }
            if (character.traitContainer.HasTrait("Pyrophobic")) {
                character.traitContainer.AddTrait(character, "Traumatized");
                character.traitContainer.AddTrait(character, "Unconscious");

                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Trait", name, "pyrophobic_burn", null, LOG_TAG.Life_Changes);
                log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddLogToDatabase(true);
            }
        }
        private bool ShouldSpreadFire() {
            return owner is IPointOfInterest && owner.gridTileLocation != null; //only spread fire of this is owned by a POI
        }
#endregion

        #region IElementalTrait
        public void SetIsPlayerSource(bool p_state) {
            if (isPlayerSource != p_state) {
                isPlayerSource = p_state;
                if (owner is Character character) {
                    if (isPlayerSource) {
                        EnablePlayerSourceChaosOrb(character);
                    } else {
                        DisablePlayerSourceChaosOrb(character);
                    }
                }
            }
        }
        #endregion

        #region Reactions
        public override void VillagerReactionToTileObjectTrait(TileObject owner, Character actor, ref string debugLog) {
            base.VillagerReactionToTileObjectTrait(owner, actor, ref debugLog);
            Lazy lazy = actor.traitContainer.GetTraitOrStatus<Lazy>("Lazy");
            if (!actor.combatComponent.isInActualCombat && !actor.hasSeenFire) {
                bool hasHigherPrioJob = actor.jobQueue.jobsInQueue.Count > 0 && actor.jobQueue.jobsInQueue[0].priority > JOB_TYPE.DOUSE_FIRE.GetJobTypePriority();
                if (!hasHigherPrioJob
                    && owner.gridTileLocation != null
                    && actor.homeSettlement != null
                    && owner.gridTileLocation.IsPartOfSettlement(actor.homeSettlement)
                    && !actor.traitContainer.HasTrait("Pyrophobic")
                    && !actor.traitContainer.HasTrait("Dousing")
                    && !actor.jobQueue.HasJob(JOB_TYPE.DOUSE_FIRE)) {
#if DEBUG_LOG
                    debugLog = $"{debugLog}\n-Target is Burning and Character is not Pyrophobic";
#endif
                    actor.SetHasSeenFire(true);
                    if (lazy == null || !lazy.TryIgnoreUrgentTask(JOB_TYPE.DOUSE_FIRE)) {
                        actor.homeSettlement.settlementJobTriggerComponent.TriggerDouseFire();
                        if (!actor.homeSettlement.HasJob(JOB_TYPE.DOUSE_FIRE)) {
#if DEBUG_LOG
                            Debug.LogWarning($"{actor.name} saw a fire in a settlement but no douse fire jobs were created.");
#endif
                        }

                        JobQueueItem douseJob = actor.homeSettlement.GetFirstJobOfTypeThatCanBeAssignedTo(JOB_TYPE.DOUSE_FIRE, actor);
                        if (douseJob != null) {
                            actor.jobQueue.AddJobInQueue(douseJob);
                        } else {
                            if (actor.combatComponent.combatMode == COMBAT_MODE.Aggressive) {
                                actor.combatComponent.Flight(owner, "saw fire");
                            }
                        }
                    }
                }
            }
        }
        #endregion

    }
}

#region Save Data
public class SaveDataBurning : SaveDataTrait {
    public string burningSourceID;
    public bool isPlayerSource;
    public override void Save(Trait trait) {
        base.Save(trait);
        Burning data = trait as Burning;
        Assert.IsNotNull(data);
        burningSourceID = data.persistentID;
        isPlayerSource = data.isPlayerSource;
    }
}
#endregion
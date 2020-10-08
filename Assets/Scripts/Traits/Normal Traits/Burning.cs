using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;
namespace Traits {
    public class Burning : Status {
        private ITraitable owner { get; set; }
        public BurningSource sourceOfBurning { get; private set; }
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
            moodEffect = -25;
            _burningSpreadChoices = new List<ITraitable>();
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.EXTRACT_ITEM };
            AddTraitOverrideFunctionIdentifier(TraitManager.Initiate_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Destroy_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Execute_Pre_Effect_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Execute_Per_Tick_Effect_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Death_Trait);
        }

        #region Loading
        public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadFirstWaveInstancedTrait(saveDataTrait);
            SaveDataBurning saveDataBurning = saveDataTrait as SaveDataBurning;
            Assert.IsNotNull(saveDataBurning);
            BurningSource burningSource = DatabaseManager.Instance.burningSourceDatabase.GetOrCreateBurningSourceWithID(saveDataBurning.persistentID);
            LoadSourceOfBurning(burningSource);
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            owner = addTo;
            if (addTo is IPointOfInterest poi) {
                burningEffect = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Burning, false);
            } else if (addTo is StructureWallObject structureWallObject) {
                burningEffect = GameManager.Instance.CreateParticleEffectAt(structureWallObject, PARTICLE_EFFECT.Burning);
            }
            sourceOfBurning?.AddObjectOnFire(owner);
            Messenger.AddListener(Signals.TICK_ENDED, PerTickEnded);
            if (addTo is Character) {
                Messenger.AddListener<Character>(Signals.CHARACTER_MARKER_EXPIRED, OnCharacterMarkerExpired);
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
                    if(character.canMove && character.canWitness && character.canPerform) {
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
                    Messenger.Broadcast(Signals.REPROCESS_POI, poi);
                }
            } else if (addedTo is StructureWallObject structureWallObject) {
                burningEffect = GameManager.Instance.CreateParticleEffectAt(structureWallObject, PARTICLE_EFFECT.Burning);
            }
            if (sourceOfBurning != null && !sourceOfBurning.objectsOnFire.Contains(owner)) {
                //this is so that addedTo will be added to the list of objects on fire of the burning source, if it isn't already.
                SetSourceOfBurning(sourceOfBurning, owner);
            }
            Messenger.AddListener(Signals.TICK_ENDED, PerTickEnded);
            if (addedTo is Character) {
                Messenger.AddListener<Character>(Signals.CHARACTER_MARKER_EXPIRED, OnCharacterMarkerExpired);
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
                Messenger.RemoveListener<Character>(Signals.CHARACTER_MARKER_EXPIRED, OnCharacterMarkerExpired);
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
            removedFrom.traitContainer.AddTrait(removedFrom, "Burnt");
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
                    goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Burning", out var trait);
                    (trait as Burning)?.SetSourceOfBurning(sourceOfBurning, goapNode.actor);
                }
            }
        }
        public override void ExecuteActionPerTickEffects(INTERACTION_TYPE action, ActualGoapNode goapNode) {
            base.ExecuteActionPerTickEffects(action, goapNode);
            if (goapNode.action.actionCategory == ACTION_CATEGORY.CONSUME || goapNode.action.actionCategory == ACTION_CATEGORY.DIRECT) {
                if (Random.Range(0, 100) < 10) { //5
                    goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Burning", out var trait);
                    (trait as Burning)?.SetSourceOfBurning(sourceOfBurning, goapNode.actor);
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
            owner.AdjustHP(-(int) (owner.maxHP * 0.02f), ELEMENTAL_TYPE.Normal, true, this, showHPBar: true);

            //Sleeping characters in bed should also receive damage
            //https://trello.com/c/kFZAHo11/1203-sleeping-characters-in-bed-should-also-receive-damage
            if (owner is Bed bed) {
                if(bed.users != null && bed.users.Length > 0) {
                    for (int i = 0; i < bed.users.Length; i++) {
                        Character user = bed.users[i];
                        user.AdjustHP(-(int) (user.maxHP * 0.02f), ELEMENTAL_TYPE.Normal, true, this, showHPBar: true);
                    }
                }
            }

            if (Random.Range(0, 100) >= 4) {
                return;
            }
            _burningSpreadChoices.Clear();
            if (ShouldSpreadFire()) {
                LocationGridTile origin = owner.gridTileLocation;
                List<LocationGridTile> affectedTiles = origin.GetTilesInRadius(1, includeCenterTile: true, includeTilesInDifferentStructure: true);
                for (int i = 0; i < affectedTiles.Count; i++) {
                    _burningSpreadChoices.AddRange(affectedTiles[i].GetTraitablesOnTile());
                }
                if (_burningSpreadChoices.Count > 0) {
                    ITraitable chosen = _burningSpreadChoices[Random.Range(0, _burningSpreadChoices.Count)];
                    chosen.traitContainer.AddTrait(chosen, "Burning", out var trait, bypassElementalChance: true);
                    (trait as Burning)?.SetSourceOfBurning(sourceOfBurning, chosen);
                }    
            }

        }
        #endregion

        #region Douser
        public void SetDouser(Character character) {
            douser = character;
            if (douser == null) {
                Messenger.RemoveListener<JobQueueItem, Character>(Signals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
            } else {
                Messenger.AddListener<JobQueueItem, Character>(Signals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
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
            if (character.traitContainer.HasTrait("Pyrophobic")) {
                character.traitContainer.AddTrait(character, "Traumatized");
                character.traitContainer.AddTrait(character, "Unconscious");

                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Trait", name, "pyrophobic_burn", null, LOG_TAG.Life_Changes);
                log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddLogToDatabase();
            }
        }
        private bool ShouldSpreadFire() {
            return owner is IPointOfInterest && owner.gridTileLocation != null; //only spread fire of this is owned by a POI
        }
        #endregion

    }
}

#region Save Data
public class SaveDataBurning : SaveDataTrait {
    public string burningSourceID;
    public override void Save(Trait trait) {
        base.Save(trait);
        Burning burning = trait as Burning;
        Assert.IsNotNull(burning);
        burningSourceID = burning.persistentID;
    }
}
#endregion
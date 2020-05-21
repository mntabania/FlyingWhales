using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

namespace Traits {
    public class Burning : Status {
        private ITraitable owner { get; set; }
        public BurningSource sourceOfBurning { get; private set; }
        public override bool isPersistent => true;
        public Character douser { get; private set; } //the character that is going to douse this fire.
        private GameObject burningEffect;
        private readonly List<ITraitable> _burningSpreadChoices;
        private bool _hasBeenRemoved;

        public Burning() {
            name = "Burning";
            description = "This character is on fire!";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(1);
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
                } 
                // else {
                //     poi.SetPOIState(POI_STATE.INACTIVE);
                // }
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
            
            base.OnAddTrait(addedTo);
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            _hasBeenRemoved = true;
            SetDouser(null); //reset douser so that any signals related to that will be removed.
            SetSourceOfBurning(null, removedFrom);
            Messenger.RemoveListener(Signals.TICK_ENDED, PerTickEnded);
            if (burningEffect) {
                ObjectPoolManager.Instance.DestroyObject(burningEffect);
                burningEffect = null;
            }
            if (removedFrom is IPointOfInterest obj) {
                obj.RemoveAdvertisedAction(INTERACTION_TYPE.DOUSE_FIRE);
                if (removedFrom is Character character) {
                    // character.ForceCancelAllJobsTargettingThisCharacter(JOB_TYPE.REMOVE_STATUS);
                    character.AdjustDoNotRecoverHP(-1);
                }
                // else {
                //     obj.SetPOIState(POI_STATE.ACTIVE);   
                // }
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
            Pyrophobic pyrophobic = characterThatWillDoJob.traitContainer.GetNormalTrait<Pyrophobic>("Pyrophobic");
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
            //TODO: CAN BE OPTIMIZED?
            _burningSpreadChoices.Clear();
            if (ShouldSpreadFire()) {
                LocationGridTile origin = owner.gridTileLocation;
                List<LocationGridTile> affectedTiles = origin.GetTilesInRadius(1, includeCenterTile: true, includeTilesInDifferentStructure: true);
                for (int i = 0; i < affectedTiles.Count; i++) {
                    _burningSpreadChoices.AddRange(affectedTiles[i].GetTraitablesOnTile());
                }
                if (_burningSpreadChoices.Count > 0) {
                    ITraitable chosen = _burningSpreadChoices[Random.Range(0, _burningSpreadChoices.Count)];
                    chosen.traitContainer.AddTrait(chosen, "Burning", out var trait);
                    (trait as Burning)?.SetSourceOfBurning(sourceOfBurning, chosen);
                }    
            }

            owner.AdjustHP(-(int)(owner.maxHP * 0.02f), ELEMENTAL_TYPE.Normal, true, this, showHPBar: true);
        }

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

        private bool ShouldSpreadFire() {
            return owner is IPointOfInterest; //only spread fire of this is owned by a POI
        }

    }

    public class SaveDataBurning : SaveDataTrait {
        public int burningSourceID;

        public override void Save(Trait trait) {
            base.Save(trait);
            Burning derivedTrait = trait as Burning;
            burningSourceID = derivedTrait.sourceOfBurning.id;
        }

        public override Trait Load(ref Character responsibleCharacter) {
            Trait trait = base.Load(ref responsibleCharacter);
            // Burning derivedTrait = trait as Burning;
            // derivedTrait.LoadSourceOfBurning(LandmarkManager.Instance.GetBurningSourceByID(burningSourceID));
            return trait;
        }
    }
}

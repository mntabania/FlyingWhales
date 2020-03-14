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
            moodEffect = -25;
            _burningSpreadChoices = new List<ITraitable>();
            AddTraitOverrideFunctionIdentifier(TraitManager.Initiate_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Destroy_Map_Visual_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            owner = addedTo;
            if (addedTo is IPointOfInterest poi) {
                burningEffect = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Burning, false);
                if (poi is Character character) {
                    character.AdjustDoNotRecoverHP(1);
                    if(character.canMove && character.canWitness && character.canPerform) {
                        CreateJobsOnEnterVisionBasedOnTrait(character, character);
                    }
                } else {
                    IPointOfInterest obj = poi;
                    obj.SetPOIState(POI_STATE.INACTIVE);
                }
                Messenger.Broadcast(Signals.REPROCESS_POI, poi);
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
            if (removedFrom is IPointOfInterest) {
                if (removedFrom is Character character) {
                    character.ForceCancelAllJobsTargettingThisCharacter(JOB_TYPE.DOUSE_FIRE);
                    character.AdjustDoNotRecoverHP(-1);
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
            Pyrophobic pyrophobic = characterThatWillDoJob.traitContainer.GetNormalTrait<Pyrophobic>("Pyrophobic");
            pyrophobic?.AddKnownBurningSource(sourceOfBurning, traitOwner);
            
            return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
        }
        public override bool IsTangible() {
            return true;
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
            if (sourceOfBurning != null && obj is IPointOfInterest poiOnFire) {
                source.AddObjectOnFire(poiOnFire);
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
                //Messenger.RemoveListener(Signals.TICK_ENDED, PerTickEnded);
                //Temporary fix only, if the burning object has no longer have a tile location (presumably destroyed), spreading of fire should not trigger, and remove listener for per tick
                return;
            }
            //TODO: CAN BE OPTIMIZED?
            _burningSpreadChoices.Clear();
            LocationGridTile origin = owner.gridTileLocation;
            List<LocationGridTile> affectedTiles = origin.GetTilesInRadius(1, includeCenterTile: true, includeTilesInDifferentStructure: true);
            for (int i = 0; i < affectedTiles.Count; i++) {
                _burningSpreadChoices.AddRange(affectedTiles[i].GetTraitablesOnTile());
            }
            //choices.AddRange(origin.GetTraitablesOnTileWithTrait("Flammable"));
            //List<LocationGridTile> neighbours = origin.FourNeighbours();
            //for (int i = 0; i < neighbours.Count; i++) {
            //    choices.AddRange(neighbours[i].GetTraitablesOnTileWithTrait("Flammable"));
            //}
            //choices = choices.Where(x => !x.traitContainer.HasTrait("Burning", "Burnt", "Wet", "Fireproof")).ToList();
            if (_burningSpreadChoices.Count > 0) {
                ITraitable chosen = _burningSpreadChoices[Random.Range(0, _burningSpreadChoices.Count)];
                chosen.traitContainer.AddTrait(chosen, "Burning", out var trait);
                (trait as Burning)?.SetSourceOfBurning(sourceOfBurning, chosen);
            }

            owner.AdjustHP(-(int)(owner.maxHP * 0.02f), ELEMENTAL_TYPE.Normal, true, this);
            //if (owner is Character) {
            //    //Burning characters reduce their current hp by 2% of maxhp every tick. 
            //    //They also have a 6% chance to remove Burning effect but will not gain a Burnt trait afterwards. 
            //    //If a character dies and becomes a corpse, it may still continue to burn.
            //    if (Random.Range(0, 100) < 6) {
            //        owner.traitContainer.RemoveTrait(owner, this);
            //    }
            //} else {
            //    if (owner.currentHP == 0) {
            //        owner.traitContainer.RemoveTrait(owner, this);
            //        // owner.traitContainer.AddTrait(owner, "Burnt");
            //    } else {
            //        //Every tick, a Burning tile or object also has a 3% chance to remove Burning effect. 
            //        //Afterwards, it will have a Burnt trait, which disables its Flammable trait (meaning it can no longer gain a Burning status).
            //        if (Random.Range(0, 100) < 3) {
            //            owner.traitContainer.RemoveTrait(owner, this);
            //            owner.traitContainer.AddTrait(owner, "Burnt");
            //        }
            //    }
                
            //}
        }

        #region Douser
        public void SetDouser(Character character) {
            douser = character;
            if (douser == null) {
                Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, OnCharacterExitedState);
            } else {
                Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, OnCharacterExitedState);
            }
        }
        private void OnCharacterExitedState(Character character, CharacterState state) {
            if (state.characterState == CHARACTER_STATE.DOUSE_FIRE && douser == character) {
                SetDouser(null); //character that exited douse fire state is this fires' douser, set douser to null.
            }
        }
        #endregion

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

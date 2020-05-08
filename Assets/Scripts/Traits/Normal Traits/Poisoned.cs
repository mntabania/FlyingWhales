using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;
namespace Traits {
    public class Poisoned : Status {

        public List<Character> awareCharacters { get; } //characters that know about this trait
        private ITraitable traitable { get; set; } //poi that has the poison
        private Character characterOwner;
        private StatusIcon _statusIcon;
        private GameObject _poisonedEffect;
        public Character cleanser { get; private set; }

        private bool _isVenomous;

        public Poisoned() {
            name = "Poisoned";
            description = "This object is poisoned.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(4);
            isTangible = true;
            //effects = new List<TraitEffect>();
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.REMOVE_POISON, };
            awareCharacters = new List<Character>();
            mutuallyExclusive = new string[] { "Robust" };
            moodEffect = -12;
            isStacking = true;
            stackLimit = 5;
            stackModifier = 0.5f;
            SetLevel(1);
            AddTraitOverrideFunctionIdentifier(TraitManager.Initiate_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Destroy_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Execute_After_Effect_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Tick_Started_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            traitable = addedTo;
            _isVenomous = addedTo.traitContainer.HasTrait("Venomous");
            UpdateVisualsOnAdd(addedTo);
            if(traitable is Character character) {
                characterOwner = character;
                if (!_isVenomous) {
                    characterOwner.AdjustDoNotRecoverHP(1);
                }
            } 
            //else if (addedTo is TileObject) {
            //    ticksDuration = GameManager.Instance.GetTicksBasedOnHour(24);
            //}
        }
        public override void OnStackStatus(ITraitable addedTo) {
            base.OnStackStatus(addedTo);
            UpdateVisualsOnAdd(addedTo);
        }
        public override void OnStackStatusAddedButStackIsAtLimit(ITraitable traitable) {
            base.OnStackStatusAddedButStackIsAtLimit(traitable);
            UpdateVisualsOnAdd(traitable);
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            UpdateVisualsOnRemove(removedFrom);
            if (!_isVenomous) {
                characterOwner?.AdjustDoNotRecoverHP(-1);
            }
            awareCharacters.Clear();
            responsibleCharacters?.Clear(); //Cleared list, for garbage collection
        }
        public override void ExecuteActionAfterEffects(INTERACTION_TYPE action, ActualGoapNode goapNode, ref bool isRemoved) {
            base.ExecuteActionAfterEffects(action, goapNode, ref isRemoved);
            if (goapNode.action.actionCategory == ACTION_CATEGORY.CONSUME) {
                if(traitable is IPointOfInterest poi && goapNode.poiTarget == poi) {
                    Assert.IsFalse(goapNode.actor == poi, $"Consume action ({goapNode.action.name}) " +
                        $"performed on {goapNode.poiTarget.name} by {goapNode.actor.name} is trying to remove poisoned " +
                        $"stacks from the actor rather than the target!");
                    goapNode.actor.interruptComponent.TriggerInterrupt(INTERRUPT.Ingested_Poison, poi);
                    poi.traitContainer.RemoveStatusAndStacks(poi, this.name);
                    isRemoved = true;
                }
            }
        }
        public override void OnTickStarted() {
            base.OnTickStarted();
            if (!_isVenomous) {
                characterOwner?.AdjustHP(-Mathf.RoundToInt(characterOwner.maxHP * (0.005f * characterOwner.traitContainer.stacks[name])),
                ELEMENTAL_TYPE.Normal, true, showHPBar: true);
            }
        }
        public override void OnInitiateMapObjectVisual(ITraitable traitable) {
            if (traitable is IPointOfInterest poi) {
                if (_poisonedEffect) {
                    ObjectPoolManager.Instance.DestroyObject(_poisonedEffect);
                    _poisonedEffect = null;
                }
                _poisonedEffect = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Poison, false);
            }
        }
        public override void OnDestroyMapObjectVisual(ITraitable traitable) {
            if (_poisonedEffect) {
                ObjectPoolManager.Instance.DestroyObject(_poisonedEffect);
                _poisonedEffect = null;
            }
        }
        public override string GetTestingData(ITraitable traitable = null) {
            string data = base.GetTestingData(traitable);
            data += $"\n\tCleanser: {cleanser?.name ?? "None"}";
            return data;
        }
        #endregion

        #region Aware Characters
        public void AddAwareCharacter(Character character) {
            if (awareCharacters.Contains(character)) {
                awareCharacters.Add(character);
            }
        }
        public void RemoveAwareCharacter(Character character) {
            awareCharacters.Remove(character);
        }
        #endregion

        //This is only called if there is already a Poisoned status before adding the Venomous trait
        public void SetIsVenomous() {
            if (!_isVenomous) {
                _isVenomous = true;
                characterOwner.AdjustDoNotRecoverHP(1);
            }
        }

        private void UpdateVisualsOnAdd(ITraitable addedTo) {
            if(addedTo is IPointOfInterest pointOfInterest && _poisonedEffect == null && (pointOfInterest is MovingTileObject) == false) {
                _poisonedEffect = GameManager.Instance.CreateParticleEffectAt(pointOfInterest, PARTICLE_EFFECT.Poison, false);
            }
            if (addedTo is TileObject tileObject) {
                if (tileObject is GenericTileObject) {
                    tileObject.gridTileLocation.parentMap.SetUpperGroundVisual(tileObject.gridTileLocation.localPlace, InnerMapManager.Instance.assetManager.poisonRuleTile);
                }
            }
        }
        private void UpdateVisualsOnRemove(ITraitable removedFrom) {
            if(_poisonedEffect != null) {
                ObjectPoolManager.Instance.DestroyObject(_poisonedEffect);
                _poisonedEffect = null;
            }
            if (removedFrom is TileObject tileObject) {
                if (tileObject is GenericTileObject) {
                    tileObject.gridTileLocation.parentMap.SetUpperGroundVisual(tileObject.gridTileLocation.localPlace, 
                        null);
                }
            }
        }
        
        #region Cleanser
        public void SetCleanser(Character character) {
            cleanser = character;
            if (cleanser == null) {
                Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, OnCharacterChangedState);
                Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_PAUSED_STATE, OnCharacterChangedState);
            } else {
                Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, OnCharacterChangedState);
                Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_PAUSED_STATE, OnCharacterChangedState);
            }
        }
        #endregion
        
        #region Listeners
        private void OnCharacterChangedState(Character character, CharacterState state) {
            if (state.characterState == CHARACTER_STATE.CLEANSE_TILES && cleanser == character) {
                SetCleanser(null); 
            }
        }
        #endregion
    }

    public class SaveDataPoisoned : SaveDataTrait {
        public List<int> awareCharacterIDs;

        public override void Save(Trait trait) {
            base.Save(trait);
            Poisoned derivedTrait = trait as Poisoned;
            for (int i = 0; i < derivedTrait.awareCharacters.Count; i++) {
                awareCharacterIDs.Add(derivedTrait.awareCharacters[i].id);
            }
        }

        public override Trait Load(ref Character responsibleCharacter) {
            Trait trait = base.Load(ref responsibleCharacter);
            Poisoned derivedTrait = trait as Poisoned;
            for (int i = 0; i < awareCharacterIDs.Count; i++) {
                derivedTrait.AddAwareCharacter(CharacterManager.Instance.GetCharacterByID(awareCharacterIDs[i]));
            }
            return trait;
        }
    }
}

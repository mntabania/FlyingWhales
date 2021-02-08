using UnityEngine.Assertions;
using UnityEngine;
using System;
using Traits;
using UtilityScripts;
using Random = UnityEngine.Random;

namespace Traits {
    public class FiniteZombie : Trait {
        protected Character owner;
        protected bool _hasTurnedAtLeastOnce;

        #region getters
        public override bool isPersistent => true;
        public override Type serializedData => typeof(SaveDataFiniteZombie);
        public bool hasTurnedAtLeastOnce => _hasTurnedAtLeastOnce;
        #endregion

        public FiniteZombie() {
            name = "Finite Zombie";
            description = "Finite Zombie";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
        }

        #region Loading
        public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadFirstWaveInstancedTrait(saveDataTrait);
            if (saveDataTrait is SaveDataFiniteZombie finiteZombie) {
                _hasTurnedAtLeastOnce = finiteZombie.hasTurnedAtLeastOnce;
            } else {
                //Had to do this because had to handle incompatible saves
                _hasTurnedAtLeastOnce = true;
            }
        }
        public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait) {
            base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
            if (owner is Summon || owner.minion != null) {
                owner.visuals.UsePreviousClassAsset(true);
                owner.visuals.UpdateAllVisuals(owner);
                owner.marker?.SetMarkerColor(Color.grey);
            } else {
                if (!_hasTurnedAtLeastOnce) {
                    owner.visuals.UsePreviousClassAsset(true);
                    owner.visuals.UpdateAllVisuals(owner);
                }
            }
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                owner = character;
                if (!_hasTurnedAtLeastOnce) {
                    Messenger.AddListener(Signals.HOUR_STARTED, HourlyCheck);
                }
            }
        }
        #endregion

        #region Override
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                owner = character;
                Messenger.AddListener(Signals.HOUR_STARTED, HourlyCheck);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (owner != null) {
                if (Messenger.eventTable.ContainsKey(Signals.HOUR_STARTED)) {
                    Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyCheck);
                }
            }
        }
        #endregion

        private void HourlyCheck() {
            if (owner.isDead) {
                if (GameUtilities.RollChance(50)) {
                    if (owner.isBeingSeized || (owner.grave != null && owner.grave.isBeingSeized)) {
                        return;
                    }
                    if (owner.marker == null && owner.grave == null) {
                        Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyCheck);
                        return;
                    }
                    DropTransformingInfected();
                    Reanimate();
                    //Remove listener here because this must be triggered only once
                    Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyCheck);
                }
            }
        }
        private void DropTransformingInfected() {
            if (owner.isBeingCarriedBy != null && owner.isBeingCarriedBy.carryComponent.IsPOICarried(owner)) {
                owner.isBeingCarriedBy.StopCurrentActionNode();
            } else if (owner.grave != null && owner.grave.isBeingCarriedBy != null && owner.grave.isBeingCarriedBy.carryComponent.IsPOICarried(owner.grave)) {
                owner.grave.isBeingCarriedBy.StopCurrentActionNode();
            }
            if (owner.isBeingCarriedBy != null) {
                owner.isBeingCarriedBy.UncarryPOI(owner);
            } else if (owner.grave != null && owner.grave.isBeingCarriedBy != null) {
                owner.grave.isBeingCarriedBy.UncarryPOI(owner.grave);
            }
        }
        private void Reanimate() {
            _hasTurnedAtLeastOnce = true;
            if (owner is Summon || owner.minion != null) {
                //if character is not a villager, use previous class asset and change color to grey.
                owner.visuals.UsePreviousClassAsset(true);
                owner.marker?.SetMarkerColor(Color.grey);
            } else {
                owner.visuals.UsePreviousClassAsset(false);
            }
            CharacterManager.Instance.RaiseFromDeadRetainCharacterInstance(owner, FactionManager.Instance.undeadFaction, owner.race, owner.characterClass.className);
            //owner.visuals.UpdateAllVisuals(owner);
        }
    }
}


#region Save Data
public class SaveDataFiniteZombie : SaveDataTrait {
    public bool hasTurnedAtLeastOnce;
    public override void Save(Trait trait) {
        base.Save(trait);
        FiniteZombie walkerZombie = trait as FiniteZombie;
        Assert.IsNotNull(walkerZombie);
        hasTurnedAtLeastOnce = walkerZombie.hasTurnedAtLeastOnce;
    }
}
#endregion
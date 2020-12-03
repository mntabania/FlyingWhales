using UnityEngine.Assertions;
using UnityEngine;
using System;
using Traits;
using UtilityScripts;
using Random = UnityEngine.Random;

namespace Traits {
    public class WalkerZombie : Trait {
        private Character owner;
        private bool _hasTurnedAtLeastOnce;

        #region getters
        public override bool isPersistent => true;
        public override Type serializedData => typeof(SaveDataWalkerZombie);
        public bool hasTurnedAtLeastOnce => _hasTurnedAtLeastOnce;
        #endregion

        public WalkerZombie() {
            name = "Walker Zombie";
            description = "Walker Zombie";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
        }

        #region Loading
        public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait) {
            base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
            if (p_saveDataTrait is SaveDataWalkerZombie walkerZombie) {
                _hasTurnedAtLeastOnce = walkerZombie.hasTurnedAtLeastOnce;
            } else {
                //Had to do this because had to handle incompatible saves
                _hasTurnedAtLeastOnce = true;
            }
            if (!_hasTurnedAtLeastOnce) {
                owner.visuals.UsePreviousClassAsset(true);
                owner.visuals.UpdateAllVisuals(owner);
            }
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                owner = character;
                Messenger.AddListener(Signals.HOUR_STARTED, HourlyCheck);
            }
        }
        #endregion

        #region Override
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                owner = character;
                SetMovementSpeed();
                Messenger.AddListener(Signals.HOUR_STARTED, HourlyCheck);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (owner != null) {
                UnsetMovementSpeed();
                // SetColor(Color.white);
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

        // private void UpdateColor() {
        //     if (!owner.isDead) {
        //         SetColor(Color.grey);
        //     } else {
        //         SetColor(Color.white);
        //     }
        // }
        // private void SetColor(Color color) {
        //     if (owner.marker) {
        //         owner.marker.SetMarkerColor(color);    
        //     }
        // }
        private void Reanimate() {
            _hasTurnedAtLeastOnce = true;
            owner.visuals.UsePreviousClassAsset(false);
            CharacterManager.Instance.RaiseFromDeadRetainCharacterInstance(owner, FactionManager.Instance.undeadFaction, owner.race, "Walker Zombie");
            owner.visuals.UpdateAllVisuals(owner);    
            // SetColor(Color.grey);
        }
        private void SetMovementSpeed() {
            owner.movementComponent.AdjustRunSpeedModifier(-0.2f);
            owner.movementComponent.AdjustWalkSpeedModifier(-0.5f);
        }
        private void UnsetMovementSpeed() {
            owner.movementComponent.AdjustRunSpeedModifier(0.2f);
            owner.movementComponent.AdjustWalkSpeedModifier(0.5f);
        }
    }
}


#region Save Data
public class SaveDataWalkerZombie : SaveDataTrait {
    public bool hasTurnedAtLeastOnce;
    public override void Save(Trait trait) {
        base.Save(trait);
        WalkerZombie walkerZombie = trait as WalkerZombie;
        Assert.IsNotNull(walkerZombie);
        hasTurnedAtLeastOnce = walkerZombie.hasTurnedAtLeastOnce;
    }
}
#endregion
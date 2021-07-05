using UnityEngine.Assertions;
using UnityEngine;
using System;
using Traits;
using Random = UnityEngine.Random;

namespace Traits {
    public class NightZombie : Trait {
        private Character owner;
        private bool _hasTurnedAtLeastOnce;

        #region getters
        public override bool isPersistent => true;
        public override Type serializedData => typeof(SaveDataNightZombie);
        public bool hasTurnedAtLeastOnce => _hasTurnedAtLeastOnce;
        #endregion

        public NightZombie() {
            name = "Night Zombie";
            description = "Night Zombie";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
        }

        #region Loading
        public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait) {
            base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
            if (p_saveDataTrait is SaveDataNightZombie nightZombie) {
                _hasTurnedAtLeastOnce = nightZombie.hasTurnedAtLeastOnce;
            } else {
                //Had to do this because had to handle incompatible saves
                _hasTurnedAtLeastOnce = true;
            }
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
            int todayTick = GameManager.Instance.currentTick;
            if (todayTick == 120 || todayTick == 360) {
                if (owner.isBeingSeized || (owner.grave != null && owner.grave.isBeingSeized)) {
                    return;
                }
                if (owner.marker == null && owner.grave == null) {
                    Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyCheck);
                    return;
                }

                if (todayTick == 120) {
                    if (!owner.isDead) {
                        DropTransformingInfected();
                        owner.Death();
                    }
                } else {
                    if (owner.isDead) {
                        DropTransformingInfected();
                        Reanimate();
                    }
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
            // if (!owner.isDead) {
            //     SetColor(Color.grey);
            // } else {
            //     SetColor(Color.white);
            // }
        // }
        // private void SetColor(Color color) {
        //     owner.marker.SetMarkerColor(color);
        // }
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
            // SetColor(Color.grey);
        }
        private void SetMovementSpeed() {
            owner.movementComponent.AdjustRunSpeedModifier(1f);
            owner.movementComponent.AdjustWalkSpeedModifier(-0.5f);
        }
        private void UnsetMovementSpeed() {
            owner.movementComponent.AdjustRunSpeedModifier(-1f);
            owner.movementComponent.AdjustWalkSpeedModifier(0.5f);
        }
    }
}

#region Save Data
public class SaveDataNightZombie : SaveDataTrait {
    public bool hasTurnedAtLeastOnce;
    public override void Save(Trait trait) {
        base.Save(trait);
        NightZombie nightZombie = trait as NightZombie;
        Assert.IsNotNull(nightZombie);
        hasTurnedAtLeastOnce = nightZombie.hasTurnedAtLeastOnce;
    }
}
#endregion
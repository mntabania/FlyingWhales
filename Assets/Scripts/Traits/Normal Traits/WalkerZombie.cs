﻿using UnityEngine.Assertions;
using UnityEngine;
using System;
using Traits;
using UtilityScripts;
using Random = UnityEngine.Random;

namespace Traits {
    public class WalkerZombie : Trait {
        private Character owner;

        #region getters
        public override bool isPersistent => true;
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
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                owner = character;
                UpdateColor();
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
                SetColor(Color.white);
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

        private void UpdateColor() {
            if (!owner.isDead) {
                SetColor(Color.grey);
            } else {
                SetColor(Color.white);
            }
        }
        private void SetColor(Color color) {
            if (owner.marker) {
                owner.marker.SetMarkerColor(color);    
            }
        }
        private void Reanimate() {
            owner.RaiseFromDeath(faction: FactionManager.Instance.undeadFaction, race: owner.race, className: "Walker Zombie");
            SetColor(Color.grey);
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
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Infected : Status {

        private Character owner;
        private bool _hasAlreadyDied;
        public bool isLiving { get; private set; }

        public override bool isPersistent { get { return true; } }

        public Infected() {
            name = "Infected";
            description = "This character has the zombie virus.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CURE_CHARACTER, };
            mutuallyExclusive = new string[] { "Robust" };
            moodEffect = -5;
            _hasAlreadyDied = false;
            AddTraitOverrideFunctionIdentifier(TraitManager.Death_Trait);
            //AddTraitOverrideFunctionIdentifier(TraitManager.Hour_Started_Trait);
        }

        #region Override
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            owner = addedTo as Character;
            //owner.needsComponent.AdjustStaminaDecreaseRate(2);
            Messenger.AddListener(Signals.HOUR_STARTED, HourlyCheck);
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            owner.marker.SetMarkerColor(Color.white);
            //owner.needsComponent.AdjustStaminaDecreaseRate(-2);
            if (Messenger.eventTable.ContainsKey(Signals.HOUR_STARTED)) {
                Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyCheck);
            }
        }
        public override bool OnDeath(Character character) {
            if (_hasAlreadyDied) {
                SetIsLiving(false);
            } else {
                SetHasAlreadyDied(true);
            }
            return base.OnDeath(character);
        }
        #endregion

        public void SetHasAlreadyDied(bool state) {
            _hasAlreadyDied = state;
        }

        private void HourlyCheck() {
            if (!_hasAlreadyDied) {
                if(UnityEngine.Random.Range(0, 100) < 20) { //20
                    owner.interruptComponent.TriggerInterrupt(INTERRUPT.Zombie_Death, owner);
                    owner.movementComponent.AdjustRunSpeedModifier(1f);
                    owner.movementComponent.AdjustWalkSpeedModifier(-0.5f);
                }
            } else {
                int todayTick = GameManager.Instance.Today().tick;
                if (todayTick == 72 || todayTick == 216) {
                    if (owner.isBeingSeized || (owner.grave != null && owner.grave.isBeingSeized)) {
                        return;
                    }
                    if (owner.marker == null && owner.grave == null) {
                        Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyCheck);
                        return;
                    }

                    if (todayTick == 72) {
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
        }
        private void DropTransformingInfected() {
            if (owner.isBeingCarriedBy != null && owner.isBeingCarriedBy.ownParty.IsPOICarried(owner)) {
                owner.isBeingCarriedBy.StopCurrentActionNode();
            } else if (owner.grave != null && owner.grave.isBeingCarriedBy != null && owner.grave.isBeingCarriedBy.ownParty.IsPOICarried(owner.grave)) {
                owner.grave.isBeingCarriedBy.StopCurrentActionNode();
            }
            if (owner.isBeingCarriedBy != null) {
                owner.isBeingCarriedBy.UncarryPOI(owner);
            } else if (owner.grave != null && owner.grave.isBeingCarriedBy != null) {
                owner.grave.isBeingCarriedBy.UncarryPOI(owner.grave);
            }
        }

        public void SetIsLiving(bool state) {
            if(isLiving != state) {
                isLiving = state;
                if (isLiving) {
                    owner.marker.SetMarkerColor(Color.grey);
                    Messenger.AddListener<Character, Character>(Signals.CHARACTER_WAS_HIT, OnCharacterHit);
                } else {
                    Messenger.RemoveListener<Character, Character>(Signals.CHARACTER_WAS_HIT, OnCharacterHit);
                    owner.marker.SetMarkerColor(Color.white);
                }
            }
        }

        private void Reanimate() {
            owner.RaiseFromDeath(faction: FactionManager.Instance.undeadFaction, race: owner.race, className: "Zombie");
            SetIsLiving(true);
        }

        private void OnCharacterHit(Character hitCharacter, Character hitBy) {
            if (hitBy == owner) {
                //a character was hit by the owner of this trait, check if the character that was hit becomes infected.
                string summary = $"{hitCharacter.name} was hit by {hitBy.name}. Rolling for infect...";
                int roll = Random.Range(0, 100);
                summary += $"\nRoll is {roll}";
                int chance = 35;
                if (hitCharacter.isDead) {
                    chance = 80;
                }
                // chance = 100;
                if (roll < chance) { //15
                    summary += $"\nChance met, {hitCharacter.name} will turn into a zombie.";
                    if (hitCharacter.traitContainer.AddTrait(hitCharacter, "Infected", out var infectedTrait, characterResponsible: hitBy)) {
                        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "contracted_zombie");
                        log.AddToFillers(hitCharacter, hitCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        log.AddToFillers(hitBy, hitBy.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        log.AddLogToInvolvedObjects();
                        if(hitCharacter.isDead && infectedTrait != null && infectedTrait is Infected infected) {
                            infected.SetHasAlreadyDied(true);
                        }
                        // PlayerManager.Instance.player.ShowNotification(log);
                        //Debug.Log(GameManager.Instance.TodayLogString() + Utilities.LogReplacer(log));
                    } else {
                        summary += $"\n{hitCharacter.name} is already a zombie!";
                    }
                }
                Debug.Log(GameManager.Instance.TodayLogString() + summary);
            }
        }
    }
}


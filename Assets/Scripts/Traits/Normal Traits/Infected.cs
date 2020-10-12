using System;
using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;
namespace Traits {
    public class Infected : Status {

        public bool isLiving { get; private set; }
        
        private Character owner;
        private bool _hasAlreadyDied;
        private GameObject _infectedEffectGO;

        #region getters
        public override bool isPersistent => true;
        public bool hasAlreadyDied => _hasAlreadyDied;
        public override Type serializedData => typeof(SaveDataInfected);
        #endregion
        
        public Infected() {
            name = "Infected";
            description = "Zombie Virus is circulating inside it.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CURE_CHARACTER };
            mutuallyExclusive = new string[] { "Robust" };
            moodEffect = -5;
            _hasAlreadyDied = false;
            AddTraitOverrideFunctionIdentifier(TraitManager.Death_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Initiate_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Destroy_Map_Visual_Trait);
            //AddTraitOverrideFunctionIdentifier(TraitManager.Hour_Started_Trait);
        }

        #region Loading
        public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadFirstWaveInstancedTrait(saveDataTrait);
            SaveDataInfected saveDataInfected = saveDataTrait as SaveDataInfected;
            Assert.IsNotNull(saveDataInfected);
            _hasAlreadyDied = saveDataInfected.hasAlreadyDied;
        }
        public override void LoadSecondWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadSecondWaveInstancedTrait(saveDataTrait);
            SaveDataInfected saveDataInfected = saveDataTrait as SaveDataInfected;
            Assert.IsNotNull(saveDataInfected);
            SetIsLiving(saveDataInfected.isLiving);
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if(addTo is Character character) {
                owner = character;
                Messenger.AddListener(Signals.HOUR_STARTED, HourlyCheck);
                _infectedEffectGO = GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Infected, false);
            }
        }
        #endregion

        #region Override
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if(addedTo is Character character) {
                owner = character;
                //owner.needsComponent.AdjustStaminaDecreaseRate(2);
                Messenger.AddListener(Signals.HOUR_STARTED, HourlyCheck);
                _infectedEffectGO = GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Infected, false);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (_infectedEffectGO) {
                ObjectPoolManager.Instance.DestroyObject(_infectedEffectGO);
                _infectedEffectGO = null;
            }
            if (owner != null) {
                owner.marker.SetMarkerColor(Color.white);
                //owner.needsComponent.AdjustStaminaDecreaseRate(-2);
                if (Messenger.eventTable.ContainsKey(Signals.HOUR_STARTED)) {
                    Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyCheck);
                }

                //When this character got rid of infection and its class is still a zombie, transform it back to normal because it means that he was healed of being a zombie
                if(owner.characterClass.className == "Zombie") {
                    owner.AssignClass(owner.previousClassName);
                }
            }
        }
        public override bool OnDeath(Character character) {
            if (hasAlreadyDied) {
                SetIsLiving(false);
            } else {
                SetHasAlreadyDied(true);
            }
            return base.OnDeath(character);
        }
        public override void OnInitiateMapObjectVisual(ITraitable traitable) {
            if (traitable is Character character) {
                if (_infectedEffectGO) {
                    ObjectPoolManager.Instance.DestroyObject(_infectedEffectGO);
                    _infectedEffectGO = null;
                }
                _infectedEffectGO = GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Infected, false);
                if (isLiving) {
                    owner.marker.SetMarkerColor(Color.grey);
                }
            }
        }
        public override void OnDestroyMapObjectVisual(ITraitable traitable) {
            if (_infectedEffectGO) {
                ObjectPoolManager.Instance.DestroyObject(_infectedEffectGO);
                _infectedEffectGO = null;
            }
        }
        #endregion

        public void SetHasAlreadyDied(bool state) {
            _hasAlreadyDied = state;
        }

        private void HourlyCheck() {
            if (!hasAlreadyDied) {
                if(UnityEngine.Random.Range(0, 100) < 5) { //20
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
                int chance = 20;
                if (hitCharacter.isDead) {
                    chance = 80;
                }
                // chance = 100;
                if (roll < chance) { //15
                    summary += $"\nChance met, {hitCharacter.name} will turn into a zombie.";
                    InfectTarget(hitCharacter);
                }
                Debug.Log(GameManager.Instance.TodayLogString() + summary);
            }
        }
        public void InfectTarget(Character target) {
            if (target.race == RACE.SKELETON) { return; } //Prevent skeletons from getting infected
            if (target.traitContainer.AddTrait(target, "Infected", out var infectedTrait, characterResponsible: owner)) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "contracted_zombie", null, LOG_TAG.Life_Changes);
                log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                log.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFrom(owner, log);
                if (target.isDead && infectedTrait != null && infectedTrait is Infected infected) {
                    infected.SetHasAlreadyDied(true);
                }
            }
        }
    }
}

#region Save Data
public class SaveDataInfected : SaveDataTrait {
    public bool hasAlreadyDied;
    public bool isLiving;
    public override void Save(Trait trait) {
        base.Save(trait);
        Infected infected = trait as Infected;
        Assert.IsNotNull(infected);
        hasAlreadyDied = infected.hasAlreadyDied;
        isLiving = infected.isLiving;
    }
}
#endregion
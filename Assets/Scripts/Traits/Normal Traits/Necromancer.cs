using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine.Assertions;
namespace Traits {
    public class Necromancer : Trait {

        public const int MaxSkeletonFollowers = 25;
        
        public Character owner { get; private set; }
        public LocationStructure lairStructure { get; private set; }
        public NPCSettlement attackVillageTarget { get; private set; }
        public string prevClassName { get; private set; }
        public int lifeAbsorbed { get; private set; }
        public int energy { get; private set; }
        public bool doNotSpawnLair { get; private set; }
        public GameDate spawnLairDate { get; private set; }

        #region getters
        //Commented this out since nothing uses it anymore, and technically GetNumOfSkeletonFollowersInSameRegion()
        //is same as number of skeleton followers since we only have 1 region now
        // public int numOfSkeletonFollowers => GetNumOfSkeletonFollowers(); 
        public int numOfSkeletonFollowers => GetNumOfSkeletonFollowersInSameRegion();
        public override Type serializedData => typeof(SaveDataNecromancer);
        public override bool affectsNameIcon => true;
        #endregion

        public Necromancer() {
            name = "Necromancer";
            description = "This is a necromancer.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.BUILD_LAIR, INTERACTION_TYPE.SPAWN_SKELETON, INTERACTION_TYPE.READ_NECRONOMICON, INTERACTION_TYPE.MEDITATE, INTERACTION_TYPE.REGAIN_ENERGY };
            AddTraitOverrideFunctionIdentifier(TraitManager.See_Poi_Trait);
        }

        #region Loading
        public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadFirstWaveInstancedTrait(saveDataTrait);
            SaveDataNecromancer saveDataNecromancer = saveDataTrait as SaveDataNecromancer;
            Assert.IsNotNull(saveDataNecromancer);
            if (!string.IsNullOrEmpty(saveDataNecromancer.lairStructureID)) {
                lairStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataNecromancer.lairStructureID);
            }
            if (!string.IsNullOrEmpty(saveDataNecromancer.attackVillageID)) {
                attackVillageTarget = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(saveDataNecromancer.attackVillageID) as NPCSettlement;
            }
            prevClassName = saveDataNecromancer.prevClassName;
            lifeAbsorbed = saveDataNecromancer.lifeAbsorbed;
            energy = saveDataNecromancer.energy;
            doNotSpawnLair = saveDataNecromancer.doNotSpawnLair;
            spawnLairDate = saveDataNecromancer.spawnLairDate;
        }
        public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait) {
            base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
            if (doNotSpawnLair) {
                SchedulingManager.Instance.AddEntry(spawnLairDate, () => SetDoNotSpawnLair(false), null);
            }
        }
        #endregion

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            owner = addedTo as Character;
            prevClassName = owner.characterClass.className;
            //NOTE: Removed assigning class here because Necromancer trait should be added when Necromancer class is assigned, not the other way around. This means, that the class controls the trait not vice-versa.
            //owner.AssignClass("Necromancer");
            owner.behaviourComponent.AddBehaviourComponent(typeof(NecromancerBehaviour));
            owner.SetNecromancerTrait(this);
            //NOTE: The changing of faction and clearing out home is moved in Necromantic Transformation interrupt, the reason is the necromancer must not change faction every time he changes classes because he can be a master lycan
            //The creation of lair must also be done only once
            AdjustEnergy(10);
            owner.jobQueue.CancelAllJobs();
            owner.movementComponent.SetEnableDigging(true);
            owner.movementComponent.SetAvoidSettlements(true);
            Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, owner as IPlayerActionTarget);
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                owner = character;
                CharacterManager.Instance.SetNecromancerInTheWorld(owner);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            //owner.AssignClass(prevClassName);
            owner.behaviourComponent.RemoveBehaviourComponent(typeof(NecromancerBehaviour));
            owner.SetNecromancerTrait(null);
            //owner.ChangeFactionTo(FactionManager.Instance.vagrantFaction);
            if (FactionManager.Instance.undeadFaction.leader == owner) {
                FactionManager.Instance.undeadFaction.OnlySetLeader(null);
            }
            CharacterManager.Instance.SetNecromancerInTheWorld(null);
            owner.movementComponent.SetEnableDigging(false);
            owner.movementComponent.SetAvoidSettlements(false);
            Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, owner as IPlayerActionTarget);
        }
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is Character || targetPOI is Tombstone) {
                Character targetCharacter = null;
                if (targetPOI is Character character) {
                    if (character is Summon summon) {
                        if (!summon.hasBeenRaisedFromDead) {
                            targetCharacter = character;        
                        }
                    } else {
                        targetCharacter = character;
                    }
                } else if (targetPOI is Tombstone tombstone) {
                    targetCharacter = tombstone.character;
                }
                if (targetCharacter != null && targetCharacter.isDead && targetCharacter.hasMarker && numOfSkeletonFollowers < MaxSkeletonFollowers) {
                    return characterThatWillDoJob.jobComponent.TriggerRaiseCorpse(targetCharacter);
                }
            }
            return false;
        }
        #endregion

        #region Utilities
        public void SetLairStructure(LocationStructure structure) {
            lairStructure = structure;
        }
        public void AdjustLifeAbsorbed(int amount) {
            lifeAbsorbed += amount;
        }
        public void AdjustEnergy(int amount) {
            energy += amount;
            energy = Mathf.Clamp(energy, 0, 10);
        }
        // private int GetNumOfSkeletonFollowers() {
        //     int count = 0;
        //     for (int i = 0; i < owner.faction.characters.Count; i++) {
        //         if(owner.faction.characters[i].race == RACE.SKELETON) {
        //             count++;
        //         }
        //     }
        //     return count;
        // }
        private int GetNumOfSkeletonFollowersInSameRegion() {
            int count = 0;
            for (int i = 0; i < owner.faction.characters.Count; i++) {
                Character member = owner.faction.characters[i];
                if (member.race == RACE.SKELETON && member.currentRegion == owner.currentRegion) {
                    count++;
                }
            }
            return count;
        }
        public int GetNumOfSkeletonFollowersThatAreNotAttackingAndIsAlive() {
            int count = 0;
            for (int i = 0; i < owner.faction.characters.Count; i++) {
                Character factionMember = owner.faction.characters[i];
                if (factionMember.race == RACE.SKELETON && !factionMember.isDead && !factionMember.behaviourComponent.HasBehaviour(typeof(AttackVillageBehaviour))) {
                    count++;
                }
            }
            return count;
        }
        public void SetAttackVillageTarget(NPCSettlement npcSettlement) {
            attackVillageTarget = npcSettlement;
        }
        public void SetDoNotSpawnLair(bool p_state) {
            if (doNotSpawnLair != p_state) {
                doNotSpawnLair = p_state;
                if (owner.isDead) {
                    return;
                }
                if (doNotSpawnLair) {
                    spawnLairDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(4));
                    SchedulingManager.Instance.AddEntry(spawnLairDate, () => SetDoNotSpawnLair(false), null);
                }
            }
        }
        #endregion
    }
}

#region Save Data
public class SaveDataNecromancer : SaveDataTrait {
    public string lairStructureID;
    public string attackVillageID;
    public string prevClassName;
    public int lifeAbsorbed;
    public int energy;
    public bool doNotSpawnLair;
    public GameDate spawnLairDate;
    public override void Save(Trait trait) {
        base.Save(trait);
        Necromancer necromancer = trait as Necromancer;
        Assert.IsNotNull(necromancer);
        lairStructureID = necromancer.lairStructure == null ? string.Empty : necromancer.lairStructure.persistentID;
        attackVillageID = necromancer.attackVillageTarget == null ? string.Empty : necromancer.attackVillageTarget.persistentID;
        prevClassName = necromancer.prevClassName;
        lifeAbsorbed = necromancer.lifeAbsorbed;
        energy = necromancer.energy;
        doNotSpawnLair = necromancer.doNotSpawnLair;
        spawnLairDate = necromancer.spawnLairDate;
    }
}
#endregion
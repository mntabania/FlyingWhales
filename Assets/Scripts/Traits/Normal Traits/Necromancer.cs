using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine.Assertions;
namespace Traits {
    public class Necromancer : Trait {
        public Character owner { get; private set; }
        public LocationStructure lairStructure { get; private set; }
        public NPCSettlement attackVillageTarget { get; private set; }
        public string prevClassName { get; private set; }
        public int lifeAbsorbed { get; private set; }
        public int energy { get; private set; }

        #region getters
        public int numOfSkeletonFollowers => GetNumOfSkeletonFollowers();
        public int numOfSkeletonFollowersInSameRegion => GetNumOfSkeletonFollowersInSameRegion();
        public override Type serializedData => typeof(SaveDataNecromancer);
        #endregion

        public Necromancer() {
            name = "Necromancer";
            description = "This is a necromancer.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.BUILD_LAIR, INTERACTION_TYPE.SPAWN_SKELETON, INTERACTION_TYPE.READ_NECRONOMICON, INTERACTION_TYPE.MEDITATE, INTERACTION_TYPE.REGAIN_ENERGY };
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
            //Temporary fix: Change faction to undead when the necromancer sucessfully built his lair so that the other characters in the village will not attack him
            //EDIT NOTE: Faction changing is brought back when necromancer trait is added not when the character built a lair because we now have the Transitioning trait
            //This means that the character will no longer be hostile to villagers if he/she is transitioning
            owner.ChangeFactionTo(FactionManager.Instance.undeadFaction);
            FactionManager.Instance.undeadFaction.OnlySetLeader(owner);
            CharacterManager.Instance.SetNecromancerInTheWorld(owner);
            owner.MigrateHomeStructureTo(null);
            owner.ClearTerritory();
            AdjustEnergy(5);
            owner.jobQueue.CancelAllJobs();
            owner.movementComponent.SetEnableDigging(true);
            owner.movementComponent.SetAvoidSettlements(true);
            Messenger.Broadcast(SpellSignals.RELOAD_PLAYER_ACTIONS, owner as IPlayerActionTarget);
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
            owner.ChangeFactionTo(FactionManager.Instance.vagrantFaction);
            FactionManager.Instance.undeadFaction.OnlySetLeader(null);
            CharacterManager.Instance.SetNecromancerInTheWorld(null);
            owner.movementComponent.SetEnableDigging(false);
            owner.movementComponent.SetAvoidSettlements(false);
            Messenger.Broadcast(SpellSignals.RELOAD_PLAYER_ACTIONS, owner as IPlayerActionTarget);
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
        }
        private int GetNumOfSkeletonFollowers() {
            int count = 0;
            for (int i = 0; i < owner.faction.characters.Count; i++) {
                if(owner.faction.characters[i].race == RACE.SKELETON) {
                    count++;
                }
            }
            return count;
        }
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
    public override void Save(Trait trait) {
        base.Save(trait);
        Necromancer necromancer = trait as Necromancer;
        Assert.IsNotNull(necromancer);
        lairStructureID = necromancer.lairStructure == null ? string.Empty : necromancer.lairStructure.persistentID;
        attackVillageID = necromancer.attackVillageTarget == null ? string.Empty : necromancer.attackVillageTarget.persistentID;
        prevClassName = necromancer.prevClassName;
        lifeAbsorbed = necromancer.lifeAbsorbed;
        energy = necromancer.energy;
    }
}
#endregion
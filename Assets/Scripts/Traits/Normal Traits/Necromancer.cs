using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

namespace Traits {
    public class Necromancer : Trait {
        public Character owner { get; private set; }
        public LocationStructure lairStructure { get; private set; }
        public NPCSettlement attackVillageTarget { get; private set; }
        public string prevClassName { get; private set; }
        public int lifeAbsorbed { get; private set; }
        public int energy { get; private set; }

        #region getters
        public int numOfSkeletonFollowers { get { return GetNumOfSkeletonFollowers(); } }
        #endregion

        public Necromancer() {
            name = "Necromancer";
            description = "This is a necromancer.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.BUILD_LAIR, INTERACTION_TYPE.SPAWN_SKELETON, INTERACTION_TYPE.READ_NECRONOMICON, INTERACTION_TYPE.MEDITATE, INTERACTION_TYPE.REGAIN_ENERGY };
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            owner = addedTo as Character;
            prevClassName = owner.characterClass.className;
            owner.AssignClass("Necromancer");
            owner.behaviourComponent.AddBehaviourComponent(typeof(NecromancerBehaviour));
            owner.SetNecromancerTrait(this);
            //Temporary fix: Change faction to undead when the necromancer sucessfully built his lair so that the other characters in the village will not attack him
            //owner.ChangeFactionTo(FactionManager.Instance.undeadFaction);
            //FactionManager.Instance.undeadFaction.OnlySetLeader(owner);
            CharacterManager.Instance.SetNecromancerInTheWorld(owner);
            owner.MigrateHomeStructureTo(null);
            owner.ClearTerritory();
            AdjustEnergy(5);
            owner.CancelAllJobs();
            owner.movementComponent.SetEnableDigging(true);
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            owner.AssignClass(prevClassName);
            owner.behaviourComponent.RemoveBehaviourComponent(typeof(NecromancerBehaviour));
            owner.SetNecromancerTrait(null);
            owner.ChangeFactionTo(FactionManager.Instance.vagrantFaction);
            FactionManager.Instance.undeadFaction.OnlySetLeader(null);
            CharacterManager.Instance.SetNecromancerInTheWorld(null);
            owner.movementComponent.SetEnableDigging(false);
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

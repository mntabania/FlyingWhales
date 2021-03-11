using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inner_Maps.Location_Structures {
    public class PartyStructure : DemonicStructure {

        public List<IStoredTarget> allPossibleTargets = new List<IStoredTarget>();

        public PartyStructure(STRUCTURE_TYPE structure, Region location) : base(structure, location) {
            Debug.LogError("CALLED");
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        }
        public PartyStructure(Region location, SaveDataDemonicStructure data) : base(location, data) {
            Debug.LogError("CALLED");
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        }

        protected Party m_party;

        //summon list
        public List<Character> deployedSummons = new List<Character>();
        public List<SummonSettings> deployedSummonSettings = new List<SummonSettings>();
        public List<CharacterClass> deployedCSummonlass = new List<CharacterClass>();
        public List<SUMMON_TYPE> deployedSummonType = new List<SUMMON_TYPE>();

        //minion list
        public List<Character> deployedMinions = new List<Character>();
        public List<PLAYER_SKILL_TYPE> deployedMinionsSkillType = new List<PLAYER_SKILL_TYPE>();
        public List<CharacterClass> deployedMinionClass = new List<CharacterClass>();

        //targets
        public List<IStoredTarget> deployedTargets = new List<IStoredTarget>();

        public int deployedSummonCount => deployedCSummonlass.Count;
        public int deployedMinionCount => deployedMinionsSkillType.Count;

        public int deployedTargetCount => deployedTargets.Count;

        public int maxSummonLimitDeployCount = 5;
        public int readyForDeploySummonCount;
        public int readyForDeployMinionCount;
        public int readyForDeployTargetCount;

        public void RemoveItemOnRight(DeployedMonsterItemUI p_itemUI) {
            if (!p_itemUI.isMinion) {
                deployedCSummonlass.Remove(p_itemUI.characterClass);
                deployedSummonSettings.Remove(p_itemUI.summonSettings);
                deployedSummonType.Remove(p_itemUI.summonType);
            } else {
                deployedMinionClass.Remove(p_itemUI.characterClass);
                deployedMinionsSkillType.Remove(p_itemUI.playerSkillType);
            }
        }

        public void AddDeployedItem(DeployedMonsterItemUI p_itemUI) {
            if (!p_itemUI.isMinion) {
                deployedCSummonlass.Add(p_itemUI.characterClass);
                deployedSummonSettings.Add(p_itemUI.summonSettings);
                deployedSummonType.Add(p_itemUI.summonType);
                AddSummonOnCharacterList(p_itemUI.deployedCharacter);
            } else {
                deployedMinionClass.Add(p_itemUI.characterClass);
                deployedMinionsSkillType.Add(p_itemUI.playerSkillType);
                AddMinionOnCharacterList(p_itemUI.deployedCharacter);
            }
        }

        public void AddMinionOnCharacterList(Character p_newSummon) {
            deployedMinions.Add(p_newSummon);
        }

        public void AddSummonOnCharacterList(Character p_newSummon) {
            deployedSummons.Add(p_newSummon);
        }

        public void AddTargetOnCurrentList(IStoredTarget p_newTarget) {
            if (!deployedTargets.Contains(p_newTarget)) {
                deployedTargets.Add(p_newTarget);
            }
        }

        public void RemoveTargetOnCurrentList(IStoredTarget p_newTarget) {
            if (deployedTargets.Contains(p_newTarget)) {
                deployedTargets.Remove(p_newTarget);
            }
        }

        public void RemoveCharacterOnList(Character p_removeSummon) {
            deployedSummons.Remove(p_removeSummon);
        }

        public virtual void UnDeployAll() {
            deployedSummons.ForEach((eachSummon) => {
                m_party.RemoveMember(eachSummon);
                PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge((eachSummon as Summon).summonType, 1);
                m_party.RemoveMemberThatJoinedQuest(eachSummon);
                CharacterManager.Instance.RemoveCharacter(eachSummon, true);
            });
            m_party.RemoveMemberThatJoinedQuest(deployedMinions[0]);
            CharacterManager.Instance.RemoveCharacter(deployedMinions[0], true);
            deployedSummons.Clear();
            deployedSummonSettings.Clear();
            deployedCSummonlass.Clear();
            deployedSummonType.Clear();
            deployedMinionClass.Clear();
            deployedMinionsSkillType.Clear();
            deployedMinions.Clear();
            deployedTargets.Clear();
            readyForDeployMinionCount = 0;
            readyForDeploySummonCount = 0;
            readyForDeployTargetCount = 0;
        }

        public virtual void OnCharacterDied(Character p_deadMonster) {
            Debug.LogError(p_deadMonster.name);
            for (int x = 0; x < deployedSummons.Count; ++x) {
                if (p_deadMonster == deployedSummons[x]) {
                    PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge((p_deadMonster as Summon).summonType, 1);
                    deployedSummons.RemoveAt(x);
                    deployedSummonSettings.RemoveAt(x);
                    deployedCSummonlass.RemoveAt(x);
                    deployedSummonType.RemoveAt(x);
                    break;
                }
            }
            for (int x = 0; x < deployedMinions.Count; ++x) {
                Debug.LogError(p_deadMonster.name + " -- " + deployedMinions[x].name);
                if (p_deadMonster == deployedMinions[x]) {
                    deployedMinionClass.RemoveAt(x);
                    deployedMinions.RemoveAt(x);
                    deployedMinionsSkillType.RemoveAt(x);
                    break;
                }
            }

            if(deployedSummonCount <= 0 && deployedMinionCount <= 0) {
                deployedTargets.Clear();
			}
        }

        public virtual void DeployParty() {

        }
    }
}

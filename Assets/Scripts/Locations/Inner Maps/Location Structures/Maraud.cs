using UnityEngine;
using System.Collections.Generic;

namespace Inner_Maps.Location_Structures {
    public class Maraud : DemonicStructure {
        public Maraud(Region location) : base(STRUCTURE_TYPE.MARAUD, location) {
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        }
        public Maraud(Region location, SaveDataDemonicStructure data) : base(location, data) {
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        }
        public int deployedSummonCount => deployedCSummonlass.Count;
        public int deployedMinionCount => deployedMinionsSkillType.Count;

        public int maxSummonLimitDeployCount = 2;
        public int readyForDeploySummonCount;
        public int readyForDeployMinionCount;

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
            } else {
                deployedMinionClass.Add(p_itemUI.characterClass);
                deployedMinionsSkillType.Add(p_itemUI.playerSkillType);
            }
        }

        public void AddCharacterOnList(Character p_newSummon) {
            deployedMinions.Add(p_newSummon);
        }

        public void RemoveCharacterOnList(Character p_removeSummon) {
            deployedMinions.Remove(p_removeSummon);
            CharacterManager.Instance.RemoveCharacter(p_removeSummon, true);
        }

        //summon list
        public List<Character> deployedSummons = new List<Character>();
        public List<SummonSettings> deployedSummonSettings = new List<SummonSettings>();
        public List<CharacterClass> deployedCSummonlass = new List<CharacterClass>();
        public List<SUMMON_TYPE> deployedSummonType = new List<SUMMON_TYPE>();

        //minion list
        public List<Character> deployedMinions = new List<Character>();
        public List<PLAYER_SKILL_TYPE> deployedMinionsSkillType = new List<PLAYER_SKILL_TYPE>();
        public List<CharacterClass> deployedMinionClass = new List<CharacterClass>();

        void OnCharacterDied(Character p_deadMonster) {
            for (int x = 0; x < deployedSummons.Count; ++x) {
                if (p_deadMonster == deployedSummons[x]) {
                    PlayerManager.Instance.player.underlingsComponent.monsterUnderlingCharges[deployedSummonType[x]].currentCharges++;
                    deployedSummons.RemoveAt(x);
                    deployedSummonSettings.RemoveAt(x);
                    deployedCSummonlass.RemoveAt(x);
                    deployedSummonType.RemoveAt(x);
                    break;
                }
            }
            for (int x = 0; x < deployedMinions.Count; ++x) {
                if (p_deadMonster == deployedMinions[x]) {
                    deployedMinionClass.RemoveAt(x);
                    deployedMinions.RemoveAt(x);
                    deployedMinionsSkillType.RemoveAt(x);
                    break;
                }
            }
        }
    }
}
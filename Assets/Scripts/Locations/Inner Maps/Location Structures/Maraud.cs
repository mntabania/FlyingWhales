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
                deployedMinionsSkillType.Remove(p_itemUI.playerSkillType);
            }
        }

        public void AddDeployedItem(DeployedMonsterItemUI p_itemUI) {
            if (!p_itemUI.isMinion) {
                deployedCSummonlass.Add(p_itemUI.characterClass);
                deployedSummonSettings.Add(p_itemUI.summonSettings);
                deployedSummonType.Add(p_itemUI.summonType);
            } else {
                deployedMinionsSkillType.Add(p_itemUI.playerSkillType);
            }
        }
        //summon list
        public List<Character> deployedSummons = new List<Character>();
        public List<SummonSettings> deployedSummonSettings = new List<SummonSettings>();
        public List<CharacterClass> deployedCSummonlass = new List<CharacterClass>();
        public List<SUMMON_TYPE> deployedSummonType = new List<SUMMON_TYPE>();

        //minion list
        public List<Character> deployedMinions = new List<Character>();
        public List<PLAYER_SKILL_TYPE> deployedMinionsSkillType = new List<PLAYER_SKILL_TYPE>();

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
        }
    }
}
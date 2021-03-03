using UnityEngine;
using System.Collections.Generic;

namespace Inner_Maps.Location_Structures {
    public class DefensePoint : DemonicStructure {
        public DefensePoint(Region location) : base(STRUCTURE_TYPE.DEFENSE_POINT, location) {
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        }
        public DefensePoint(Region location, SaveDataDemonicStructure data) : base(location, data) {
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        }

        public int deployedCount => deployedClass.Count;

        public int maxLimitDeployedCount = 3;
        public int readyForDeployCount;

        public void RemoveItemOnRight(DeployedMonsterItemUI p_itemUI) {
            deployedClass.Remove(p_itemUI.characterClass);
            deployedSettings.Remove(p_itemUI.summonSettings);
            deployedSummonType.Remove(p_itemUI.summonType);
        }

        public void AddDeployedItem(DeployedMonsterItemUI p_itemUI) {
            deployedClass.Add(p_itemUI.characterClass);
            deployedSettings.Add(p_itemUI.summonSettings);
            deployedSummonType.Add(p_itemUI.summonType);
        }
        //this list has different count to the set of lists above
        public List<Character> deployedMonsters = new List<Character>();
        public List<SummonSettings> deployedSettings = new List<SummonSettings>();
        public List<CharacterClass> deployedClass = new List<CharacterClass>();
        public List<SUMMON_TYPE> deployedSummonType = new List<SUMMON_TYPE>();

        void OnCharacterDied(Character p_deadMonster) { 
            for(int x = 0; x < deployedMonsters.Count; ++x) {
                if (p_deadMonster == deployedMonsters[x]) {
                    PlayerManager.Instance.player.underlingsComponent.monsterUnderlingCharges[deployedSummonType[x]].currentCharges++;
                    deployedMonsters.RemoveAt(x);
                    deployedSettings.RemoveAt(x);
                    deployedClass.RemoveAt(x);
                    deployedSummonType.RemoveAt(x);
                    break;
                }
			}
        }
    }
}
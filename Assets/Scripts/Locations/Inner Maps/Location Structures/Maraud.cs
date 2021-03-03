using UnityEngine;
using System.Collections.Generic;

namespace Inner_Maps.Location_Structures {
    public class Maraud : DemonicStructure {
        public Maraud(Region location) : base(STRUCTURE_TYPE.MARAUD, location) { }
        public Maraud(Region location, SaveDataDemonicStructure data) : base(location, data) { }
        public int deployedCount => deployedMonsterItemUI.Count;

        public int maxLimitDeployedCount = 3;
        public int currentDeployedCount;

        public void RemoveDeployedItem(DeployedMonsterItemUI p_itemUI) {
            if (deployedMonsterItemUI.Contains(p_itemUI)) {
                deployedMonsterItemUI.Remove(p_itemUI);
                deployedClass.Remove(p_itemUI.characterClass);
                deployedSettings.Remove(p_itemUI.summonSettings);
            }
        }

        public void AddDeployedItem(DeployedMonsterItemUI p_itemUI) {
            if (!deployedMonsterItemUI.Contains(p_itemUI)) {
                deployedMonsterItemUI.Add(p_itemUI);
                deployedClass.Add(p_itemUI.characterClass);
                deployedSettings.Add(p_itemUI.summonSettings);
            }
        }

        public List<Character> deployedMonsters = new List<Character>();
        public List<SummonSettings> deployedSettings = new List<SummonSettings>();
        public List<CharacterClass> deployedClass = new List<CharacterClass>();
        public List<DeployedMonsterItemUI> deployedMonsterItemUI = new List<DeployedMonsterItemUI>();
    }
}
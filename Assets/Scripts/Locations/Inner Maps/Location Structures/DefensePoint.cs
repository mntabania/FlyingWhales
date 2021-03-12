using UnityEngine;
using System.Collections.Generic;

namespace Inner_Maps.Location_Structures {
    public class DefensePoint : PartyStructure {
        public DefensePoint(Region location) : base(STRUCTURE_TYPE.DEFENSE_POINT, location) {
            
        }
        public DefensePoint(Region location, SaveDataDemonicStructure data) : base(location, data) {
            
        }

        public override void RemoveCharacterOnList(Character p_removeSummon) {
            deployedSummons.Remove(p_removeSummon);
            p_removeSummon.SetDestroyMarkerOnDeath(true);
            p_removeSummon.Death();
        }

        public override void OnCharacterDied(Character p_deadMonster) {
            for (int x = 0; x < deployedSummons.Count; ++x) {
                if (p_deadMonster == deployedSummons[x]) {
                    PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge((p_deadMonster as Summon).summonType, 1);
                    deployedSummons.RemoveAt(x);
                    deployedSummonSettings.RemoveAt(x);
                    deployedCSummonlass.RemoveAt(x);
                    deployedSummonType.RemoveAt(x);
                }
            }    
        }

        public override void UnDeployAll() {
            deployedSummons.ForEach((eachSummon) => {
                PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge((eachSummon as Summon).summonType, 1);
                eachSummon.SetDestroyMarkerOnDeath(true);
                eachSummon.Death();
            });
            deployedSummons.Clear();
            deployedSummonSettings.Clear();
            deployedCSummonlass.Clear();
            deployedSummonType.Clear();
            readyForDeploySummonCount = 0;
        }
    }
}
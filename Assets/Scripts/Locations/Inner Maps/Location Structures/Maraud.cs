using UnityEngine;
using System.Collections.Generic;

namespace Inner_Maps.Location_Structures {
    public class Maraud : PartyStructure {
        public Maraud(Region location) : base(STRUCTURE_TYPE.MARAUD, location) {
            allPossibleTargets.Clear();
            LandmarkManager.Instance.allNonPlayerSettlements.ForEach((eachVillage) => {
                if (eachVillage.locationType == LOCATION_TYPE.VILLAGE) {
                    allPossibleTargets.Add(eachVillage);
                }
            });
        }
        public Maraud(Region location, SaveDataDemonicStructure data) : base(location, data) {
            allPossibleTargets.Clear();
            LandmarkManager.Instance.allNonPlayerSettlements.ForEach((eachVillage) => {
                if (eachVillage.locationType == LOCATION_TYPE.VILLAGE) {
                    allPossibleTargets.Add(eachVillage);
                }                
            });
        }

        public override void DeployParty() {
            m_party = PartyManager.Instance.CreateNewParty(deployedMinions[0]);
            deployedSummons.ForEach((eachSummon) => m_party.AddMember(eachSummon));
            deployedMinions[0].faction.partyQuestBoard.CreateRaidPartyQuest(deployedMinions[0],
                    deployedMinions[0].homeSettlement, deployedTargets[0] as Locations.Settlements.BaseSettlement);
            m_party.TryAcceptQuest();
            m_party.AddMemberThatJoinedQuest(deployedMinions[0]);
            deployedSummons.ForEach((eachSummon) => m_party.AddMemberThatJoinedQuest(eachSummon));
        }
    }
}
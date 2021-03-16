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
            base.DeployParty();
            party = PartyManager.Instance.CreateNewParty(partyData.deployedMinions[0]);
            partyData.deployedSummons.ForEach((eachSummon) => party.AddMember(eachSummon));
            partyData.deployedMinions[0].faction.partyQuestBoard.CreateDemonRaidPartyQuest(partyData.deployedMinions[0],
                    partyData.deployedMinions[0].homeSettlement, partyData.deployedTargets[0] as Locations.Settlements.BaseSettlement);
            party.TryAcceptQuest();
            party.AddMemberThatJoinedQuest(partyData.deployedMinions[0]);
            partyData.deployedSummons.ForEach((eachSummon) => party.AddMemberThatJoinedQuest(eachSummon));
            ListenToParty();
        }
    }
}
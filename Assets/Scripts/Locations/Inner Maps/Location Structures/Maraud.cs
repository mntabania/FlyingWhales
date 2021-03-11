using UnityEngine;
using System.Collections.Generic;

namespace Inner_Maps.Location_Structures {
    public class Maraud : PartyStructure {
        public Maraud(Region location) : base(STRUCTURE_TYPE.MARAUD, location) {
            allPossibleTargets = PlayerManager.Instance.player.storedTargetsComponent.storedCharacters;
        }
        public Maraud(Region location, SaveDataDemonicStructure data) : base(location, data) {
            
        }

        public override void DeployParty() {
            m_party = PartyManager.Instance.CreateNewParty(deployedMinions[0]);
            deployedSummons.ForEach((eachSummon) => m_party.AddMember(eachSummon));
            deployedMinions[0].faction.partyQuestBoard.CreateRaidPartyQuest(deployedMinions[0],
                    deployedMinions[0].homeSettlement, LandmarkManager.Instance.allNonPlayerSettlements[0]);
            m_party.TryAcceptQuest();
            m_party.AddMemberThatJoinedQuest(deployedMinions[0]);
            deployedSummons.ForEach((eachSummon) => m_party.AddMemberThatJoinedQuest(eachSummon));
        }
    }
}
using UnityEngine;
using System.Collections.Generic;

namespace Inner_Maps.Location_Structures {
    public class DefensePoint : PartyStructure {
        public DefensePoint(Region location) : base(STRUCTURE_TYPE.DEFENSE_POINT, location) {
            
        }
        public DefensePoint(Region location, SaveDataDemonicStructure data) : base(location, data) {
            
        }

        public override void RemoveCharacterOnList(Character p_removeSummon) {
            partyData.deployedSummons.Remove(p_removeSummon);
            if (p_removeSummon.partyComponent.IsAMemberOfParty(party)) {
                party.RemoveMember(p_removeSummon);
                party.RemoveMemberThatJoinedQuest(p_removeSummon);
            }
            p_removeSummon.SetDestroyMarkerOnDeath(true);
            p_removeSummon.Death();
        }

        public override void OnCharacterDied(Character p_deadMonster) {
            for (int x = 0; x < partyData.deployedSummons.Count; ++x) {
                if (p_deadMonster == partyData.deployedSummons[x]) {
                    PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge((p_deadMonster as Summon).summonType, 1);
                    partyData.deployedSummons.RemoveAt(x);
                    partyData.deployedSummonSettings.RemoveAt(x);
                    partyData.deployedCSummonlass.RemoveAt(x);
                }
            }    
        }

        public override void UnDeployAll() {
            partyData.deployedSummons.ForEach((eachSummon) => {
                PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge((eachSummon as Summon).summonType, 1);
                eachSummon.SetDestroyMarkerOnDeath(true);
                eachSummon.Death();
            });
            partyData.deployedSummons.Clear();
            partyData.deployedSummonSettings.Clear();
            partyData.deployedCSummonlass.Clear();
            partyData.readyForDeploySummonCount = 0;
            Messenger.Broadcast(PartySignals.UNDEPLOY_PARTY, party);
        }

		public override void DeployParty() {
            if (party == null) {
                party = PartyManager.Instance.CreateNewParty(partyData.deployedSummons[0]);
                partyData.deployedSummons[0].faction.partyQuestBoard.CreateDemonDefendPartyQuest(partyData.deployedSummons[0],
                        partyData.deployedSummons[0].homeSettlement, this);
                party.TryAcceptQuest();
            }
            partyData.deployedSummons.ForEach((eachSummon) => {
                party.AddMember(eachSummon);
                party.AddMemberThatJoinedQuest(eachSummon);
            });
        }
    }
}
using UnityEngine;
using System.Collections.Generic;
using UtilityScripts;

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
            if (!m_isUndeployUserAction) {
                m_isUndeployUserAction = true;
                List<Character> deployed = RuinarchListPool<Character>.Claim();
                if (partyData.deployedSummons.Count > 0) {
                    deployed.AddRange(partyData.deployedSummons);
                }
                for (int x = 0; x < deployed.Count; x++) {
                    deployed[x].Death();
                }
                partyData.deployedSummons.Clear();
                partyData.deployedSummonSettings.Clear();
                partyData.deployedCSummonlass.Clear();
                partyData.readyForDeploySummonCount = 0;
                Messenger.Broadcast(PartySignals.UNDEPLOY_PARTY, party);
            }
     
        }

		public override void DeployParty() {
            if (party == null) {
                party = PartyManager.Instance.CreateNewParty(partyData.deployedSummons[0]);
                partyData.deployedSummons.ForEach((eachSummon) => party.AddMember(eachSummon));
                partyData.deployedSummons[0].faction.partyQuestBoard.CreateDemonDefendPartyQuest(partyData.deployedSummons[0],
                        partyData.deployedSummons[0].homeSettlement, this);
                party.TryAcceptQuest();
            }
            partyData.deployedSummons.ForEach((eachSummon) => {
                if (!eachSummon.partyComponent.IsAMemberOfParty(party)) {
                    party.AddMember(eachSummon);
                    party.AddMemberThatJoinedQuest(eachSummon);
                }
            });
        }
    }
}
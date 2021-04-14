﻿using UnityEngine;
using System.Collections.Generic;
using UtilityScripts;

namespace Inner_Maps.Location_Structures {
    public class DefensePoint : PartyStructure {
        public override string nameplateName => "Prism";
        public DefensePoint(Region location) : base(STRUCTURE_TYPE.DEFENSE_POINT, location) {
            startingSummonCount = 2;
            nameWithoutID = "Prism";
            name = $"{nameWithoutID} {id.ToString()}";
            Messenger.RemoveListener<IStoredTarget>(PlayerSignals.PLAYER_REMOVED_STORED_TARGET, OnTargetRemoved);
        }
        public DefensePoint(Region location, SaveDataPartyStructure data) : base(location, data) {
            Messenger.RemoveListener<IStoredTarget>(PlayerSignals.PLAYER_REMOVED_STORED_TARGET, OnTargetRemoved);
        }

        public override void RemoveCharacterOnList(Character p_removeSummon) {
            partyData.deployedSummons.Remove(p_removeSummon);
            partyData.deployedSummonUnderlings.Remove(PlayerManager.Instance.player.underlingsComponent.GetSummonUnderlingChargesBySummonType((p_removeSummon as Summon).summonType));
            if (p_removeSummon.partyComponent.IsAMemberOfParty(party)) {
                party.RemoveMember(p_removeSummon);
                party.RemoveMemberThatJoinedQuest(p_removeSummon);
                PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge((p_removeSummon as Summon).summonType, 1);
            }
            p_removeSummon.SetDestroyMarkerOnDeath(true);
            p_removeSummon.Death();
        }

        public override void OnCharacterDied(Character p_deadMonster) {
            if (m_isUndeployUserAction) {
                return;
            }
            for (int x = 0; x < partyData.deployedSummons.Count; ++x) {
                if (p_deadMonster == partyData.deployedSummons[x]) {
                    PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge((p_deadMonster as Summon).summonType, 1);
                    partyData.deployedSummons.RemoveAt(x);
                    partyData.deployedSummonUnderlings.RemoveAt(x);
                }
            }    
        }

		public override void DeployParty() {
            if (party == null) {
                party = PartyManager.Instance.CreateNewParty(partyData.deployedSummons[0]);
                partyData.deployedSummons[0].faction.partyQuestBoard.CreateDemonDefendPartyQuest(partyData.deployedSummons[0],
                        partyData.deployedSummons[0].homeSettlement, this);
                party.TryAcceptQuest();
                PlayerManager.Instance.player.bookmarkComponent.AddBookmark(party, BOOKMARK_CATEGORY.Player_Parties);
            }
            partyData.deployedSummons.ForEach((eachSummon) => {
                party.AddMember(eachSummon);
                party.AddMemberThatJoinedQuest(eachSummon);
            });
            partyData.readyForDeploySummonCount = 0;
        }
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(PLAYER_SKILL_TYPE.DEFEND);
        }
    }
}
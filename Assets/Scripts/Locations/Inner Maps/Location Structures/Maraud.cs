using UnityEngine;
using System.Collections.Generic;

namespace Inner_Maps.Location_Structures {
    public class Maraud : PartyStructure {
        public override List<IStoredTarget> allPossibleTargets => _allVillages;

        public override string scenarioDescription => "The Maraud allows the player to summon Raid Parties. Raid Parties harass Villages and is primarily used to generate some Chaos Orbs. A Raid Party needs a Lesser Demon as its leader";

        private List<IStoredTarget> _allVillages;
        public Maraud(Region location) : base(STRUCTURE_TYPE.MARAUD, location) {
            SetMaxHPAndReset(5000);
            _allVillages = new List<IStoredTarget>();
            UpdateTargetsList();
        }
        public Maraud(Region location, SaveDataPartyStructure data) : base(location, data) {
            _allVillages = new List<IStoredTarget>();
            UpdateTargetsList();
        }
        public override void DeployParty() {
            base.DeployParty();
            party = PartyManager.Instance.CreateNewParty(partyData.deployedMinions[0], PARTY_QUEST_TYPE.Demon_Raid);
            partyData.deployedSummons.ForEach((eachSummon) => party.AddMember(eachSummon));
            partyData.deployedMinions[0].faction.partyQuestBoard.CreateDemonRaidPartyQuest(partyData.deployedMinions[0],
                    partyData.deployedMinions[0].homeSettlement, partyData.deployedTargets[0] as Locations.Settlements.BaseSettlement);
            party.TryAcceptQuest();
            party.AddMemberThatJoinedQuest(partyData.deployedMinions[0]);
            partyData.deployedSummons.ForEach((eachSummon) => party.AddMemberThatJoinedQuest(eachSummon));
            ListenToParty();
            PlayerManager.Instance.player.bookmarkComponent.AddBookmark(party, BOOKMARK_CATEGORY.Player_Parties);
        }
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(PLAYER_SKILL_TYPE.RAID);
        }
        protected override void SubscribeListeners() {
            base.SubscribeListeners();
            Messenger.AddListener<NPCSettlement>(SettlementSignals.SETTLEMENT_CREATED, OnSettlementCreated);
        }
        protected override void UnsubscribeListeners() {
            base.UnsubscribeListeners();
            Messenger.RemoveListener<NPCSettlement>(SettlementSignals.SETTLEMENT_CREATED, OnSettlementCreated);
        }
        private void OnSettlementCreated(NPCSettlement p_settlement) {
            UpdateTargetsList(p_settlement);
        }

        #region Targets
        private void UpdateTargetsList(NPCSettlement p_settlement = null) {
            _allVillages.Clear();
            LandmarkManager.Instance.allNonPlayerSettlements.ForEach((eachVillage) => {
                if (eachVillage.locationType == LOCATION_TYPE.VILLAGE && eachVillage.areas.Count > 0) {
                    _allVillages.Add(eachVillage);
                }
            });
            if (p_settlement != null) {
                if (!LandmarkManager.Instance.allNonPlayerSettlements.Contains(p_settlement)) {
                    if (p_settlement.locationType == LOCATION_TYPE.VILLAGE) {
                        if (!_allVillages.Contains(p_settlement)) {
                            _allVillages.Add(p_settlement);
                        }
                    }
                }
            }
        }
        #endregion
        
        #region Structure Object
        public override void SetStructureObject(LocationStructureObject structureObj) {
            base.SetStructureObject(structureObj);
            Vector3 position = structureObj.transform.position;
            position.x -= 0.5f;
            position.y -= 0.5f;
            worldPosition = position;
        }
        #endregion
    }
}
using UnityEngine;
using System.Collections.Generic;

namespace Inner_Maps.Location_Structures {
    public class Maraud : PartyStructure {
        public override List<IStoredTarget> allPossibleTargets => _allVillages;
        
        private List<IStoredTarget> _allVillages;
        public Maraud(Region location) : base(STRUCTURE_TYPE.MARAUD, location) {
            _allVillages = new List<IStoredTarget>();
            UpdateTargetsList();
        }
        public Maraud(Region location, SaveDataDemonicStructure data) : base(location, data) {
            _allVillages = new List<IStoredTarget>();
            UpdateTargetsList();
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
            partyData.initialDeployedMinionCount = partyData.deployedMinions.Count;
            partyData.initialDeployedSummonCount = partyData.deployedSummons.Count;
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
            UpdateTargetsList();
        }

        #region Targets
        private void UpdateTargetsList() {
            _allVillages.Clear();
            LandmarkManager.Instance.allNonPlayerSettlements.ForEach((eachVillage) => {
                if (eachVillage.locationType == LOCATION_TYPE.VILLAGE && eachVillage.areas.Count > 0) {
                    _allVillages.Add(eachVillage);
                }
            });
        }
        #endregion
        
        #region Structure Object
        public override void SetStructureObject(LocationStructureObject structureObj) {
            base.SetStructureObject(structureObj);
            Vector3 position = structureObj.transform.position;
            position.y -= 0.5f;
            worldPosition = position;
        }
        #endregion
    }
}
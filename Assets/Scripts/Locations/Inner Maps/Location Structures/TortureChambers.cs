using System.Collections.Generic;

namespace Inner_Maps.Location_Structures {
    public class TortureChambers : PartyStructure {
        private TortureChamberStructureObject _tortureChamberStructureObject;
        public LocationGridTile entrance => _tortureChamberStructureObject.entrance;
        public override List<IStoredTarget> allPossibleTargets => PlayerManager.Instance.player.storedTargetsComponent.storedVillagers;
        public override string nameplateName => "Prison";
        public TortureChambers(Region location) : base(STRUCTURE_TYPE.TORTURE_CHAMBERS, location){
            nameWithoutID = "Prison";
            name = $"{nameWithoutID} {id.ToString()}";
        }
        public TortureChambers(Region location, SaveDataPartyStructure data) : base(location, data) { }

        #region Overrides
        public override void OnCharacterUnSeizedHere(Character character) {
            if (character.isNormalCharacter) {
                if (character.gridTileLocation != null && IsTilePartOfARoom(character.gridTileLocation, out var room)) {
                    DoorTileObject door = room.GetTileObjectInRoom<DoorTileObject>(); //close door in room
                    door?.Close();
                }
                // character.traitContainer.RestrainAndImprison(character, null, PlayerManager.Instance.player.playerFaction);
                if (character.partyComponent.hasParty) {
                    //We remove the character from the party quest if he is put in the defiler so he will not dig out of it and do the quest
                    character.partyComponent.currentParty.RemoveMemberThatJoinedQuest(character);
                }
                if (character.gridTileLocation != null && !character.gridTileLocation.charactersHere.Contains(character)) {
                    character.gridTileLocation.AddCharacterHere(character);
                }
            }
        }
        protected override void DestroyStructure() {
            StopDrainingCharactersHere();
            base.DestroyStructure();
        }
        public override void DeployParty() {
            base.DeployParty();
            party = PartyManager.Instance.CreateNewParty(partyData.deployedMinions[0]);
            partyData.deployedMinions[0].combatComponent.SetCombatMode(COMBAT_MODE.Defend);
            partyData.deployedSummons.ForEach((eachSummon) => party.AddMember(eachSummon));
            partyData.deployedSummons.ForEach((eachSummon) => eachSummon.combatComponent.SetCombatMode(COMBAT_MODE.Defend));

            partyData.deployedMinions[0].faction.partyQuestBoard.CreateDemonSnatchPartyQuest(partyData.deployedMinions[0],
                    partyData.deployedMinions[0].homeSettlement, partyData.deployedTargets[0] as Character, this);
            party.TryAcceptQuest();
            partyData.deployedTargets[0].isTargetted = true;
            party.AddMemberThatJoinedQuest(partyData.deployedMinions[0]);
            partyData.deployedSummons.ForEach((eachSummon) => party.AddMemberThatJoinedQuest(eachSummon));
            ListenToParty();
            PlayerManager.Instance.player.bookmarkComponent.AddBookmark(party, BOOKMARK_CATEGORY.Player_Parties);
        }
        #endregion

        protected override void AfterCharacterAddedToLocation(Character p_character) {
            base.AfterCharacterAddedToLocation(p_character);
            p_character.movementComponent.SetEnableDigging(false);
            if (p_character.isNormalCharacter && IsTilePartOfARoom(p_character.gridTileLocation, out var room) && room is PrisonCell prisonCell && prisonCell.skeleton == null) {
                DoorTileObject door = room.GetTileObjectInRoom<DoorTileObject>(); //close door in room
                door?.Close();
            }
            Messenger.Broadcast(SpellSignals.FORCE_RELOAD_PLAYER_ACTIONS);
        }
        protected override void AfterCharacterRemovedFromLocation(Character p_character) {
            base.AfterCharacterRemovedFromLocation(p_character);
            p_character.movementComponent.SetEnableDigging(true);
            Messenger.Broadcast(SpellSignals.FORCE_RELOAD_PLAYER_ACTIONS);
        }
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(PLAYER_SKILL_TYPE.SNATCH_VILLAGER);
        }
        
        #region Structure Object
        public override void SetStructureObject(LocationStructureObject structureObj) {
            base.SetStructureObject(structureObj);
            _tortureChamberStructureObject = structureObj as TortureChamberStructureObject;
        }
        public override void OnBuiltNewStructure() {
            base.OnBuiltNewStructure();
            _tortureChamberStructureObject.SetEntrance(region.innerMap);
        }
        public override void OnDoneLoadStructure() {
            _tortureChamberStructureObject.SetEntrance(region.innerMap);
        }
        #endregion

        #region Rooms
        protected override StructureRoom CreteNewRoomForStructure(List<LocationGridTile> tilesInRoom) {
            return new PrisonCell(tilesInRoom);
        }
        #endregion
        
        private void StopDrainingCharactersHere() {
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                character.traitContainer.RemoveTrait(character, "Being Drained");
            }
        }
    }
}
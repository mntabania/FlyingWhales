using System.Collections.Generic;
using System.Linq;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
namespace Inner_Maps.Location_Structures {
    public class TortureChambers : DemonicStructure {
        private TortureChamberStructureObject _tortureChamberStructureObject;
        public LocationGridTile entrance => _tortureChamberStructureObject.entrance;
        public override string nameplateName => "Prison";
        public TortureChambers(Region location) : base(STRUCTURE_TYPE.TORTURE_CHAMBERS, location){
            nameWithoutID = "Prison";
            name = $"{nameWithoutID} {id.ToString()}";
        }
        public TortureChambers(Region location, SaveDataDemonicStructure data) : base(location, data) { }

        #region Overrides
        public override void OnCharacterUnSeizedHere(Character character) {
            if (character.isNormalCharacter) {
                character.traitContainer.RestrainAndImprison(character, null, PlayerManager.Instance.player.playerFaction);
                //character.traitContainer.AddTrait(character, "Restrained");
                //Prisoner prisonerTrait = character.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
                //if (prisonerTrait != null) {
                //    prisonerTrait.SetPrisonerOfFaction(PlayerManager.Instance.player.playerFaction);
                //}
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
        #endregion
        
        #region Listeners
        protected override void SubscribeListeners() {
            base.SubscribeListeners();
            Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        }
        protected override void UnsubscribeListeners() {
            base.UnsubscribeListeners();
            Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        }
        #endregion
        
        private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
            if (structure == this && character.isNormalCharacter && IsTilePartOfARoom(character.gridTileLocation, out var room) && room is PrisonCell prisonCell && prisonCell.skeleton == null) {
                DoorTileObject door = room.GetTileObjectInRoom<DoorTileObject>(); //close door in room
                door?.Close();
            }
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
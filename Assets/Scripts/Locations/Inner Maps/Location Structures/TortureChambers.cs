using System.Collections.Generic;
using System.Linq;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
namespace Inner_Maps.Location_Structures {
    public class TortureChambers : DemonicStructure {
        public override Vector2 selectableSize { get; }
        private TortureChamberStructureObject _tortureChamberStructureObject;
        public LocationGridTile entrance => _tortureChamberStructureObject.entrance;


        public TortureChambers(Region location) : base(STRUCTURE_TYPE.TORTURE_CHAMBERS, location){
            selectableSize = new Vector2(10f, 10f);
        }

        #region Listeners
        protected override void SubscribeListeners() {
            base.SubscribeListeners();
            Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
            Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_LEFT_STRUCTURE, OnCharacterLeftStructure);
        }
        protected override void UnsubscribeListeners() {
            base.UnsubscribeListeners();
            Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
            Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_LEFT_STRUCTURE, OnCharacterLeftStructure);
        }
        #endregion

        #region Structure Object
        public override void SetStructureObject(LocationStructureObject structureObj) {
            base.SetStructureObject(structureObj);
            _tortureChamberStructureObject = structureObj as TortureChamberStructureObject;
        }
        public override void OnBuiltStructure() {
            _tortureChamberStructureObject.SetEntrance(location.innerMap);
        }
        #endregion

        #region Listeners
        private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
            if (structure == this && character.isNormalCharacter) {
                // character.trapStructure.SetForcedStructure(this);
                character.DecreaseCanTakeJobs();
                character.traitContainer.AddTrait(character, "Restrained");
            }
        }
        private void OnCharacterLeftStructure(Character character, LocationStructure structure) {
            if (structure == this && character.isNormalCharacter) {
                // character.trapStructure.SetForcedStructure(null);
                character.IncreaseCanTakeJobs();
            }
        }
       #endregion

        #region Rooms
        protected override StructureRoom CreteNewRoomForStructure(List<LocationGridTile> tilesInRoom) {
            return new TortureRoom(tilesInRoom);
        }
        #endregion
    }
}
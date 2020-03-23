using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class DemonicPrison : LocationStructure {
        
        public override Vector2 selectableSize { get; }
        public DemonicPrison(Region location) : base(STRUCTURE_TYPE.DEMONIC_PRISON, location){
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
        
        private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
            if (structure == this && character.IsNormalCharacter()) {
                character.trapStructure.SetForcedStructure(this);
                character.DecreaseCanTakeJobs();
            }
        }
        private void OnCharacterLeftStructure(Character character, LocationStructure structure) {
            if (structure == this && character.IsNormalCharacter()) {
                character.trapStructure.SetForcedStructure(null);
                character.IncreaseCanTakeJobs();
            }
        }
    }
}
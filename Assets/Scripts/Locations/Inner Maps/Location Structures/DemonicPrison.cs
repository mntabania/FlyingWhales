using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class DemonicPrison : DemonicStructure {
        
        public DemonicPrison(Region location) : base(STRUCTURE_TYPE.DEMONIC_PRISON, location){ }
        public DemonicPrison(Region location, SaveDataDemonicStructure data) : base(location, data) { }
        
        #region Listeners
        protected override void SubscribeListeners() {
            base.SubscribeListeners();
            Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
            Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_LEFT_STRUCTURE, OnCharacterLeftStructure);
        }
        protected override void UnsubscribeListeners() {
            base.UnsubscribeListeners();
            Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
            Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_LEFT_STRUCTURE, OnCharacterLeftStructure);
        }
        #endregion
        
        private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
            if (structure == this && character.isNormalCharacter) {
                character.trapStructure.SetForcedStructure(this);
                character.limiterComponent.DecreaseCanTakeJobs();
            }
        }
        private void OnCharacterLeftStructure(Character character, LocationStructure structure) {
            if (structure == this && character.isNormalCharacter) {
                character.trapStructure.SetForcedStructure(null);
                character.limiterComponent.IncreaseCanTakeJobs();
            }
        }
    }
}
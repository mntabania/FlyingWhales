namespace Quests {
    public class CharacterDisguised : QuestCriteria {

        public Character disguiser { get; private set; }
        public Character targetCharacter { get; private set; }
        
        public override void Enable() {
            Messenger.AddListener<Character, Character>(Signals.CHARACTER_DISGUISED, OnCharacterDisguised);
        }
        public override void Disable() {
            Messenger.RemoveListener<Character, Character>(Signals.CHARACTER_DISGUISED, OnCharacterDisguised);
        }

        private void OnCharacterDisguised(Character disguiser, Character targetCharacter) {
            this.disguiser = disguiser;
            this.targetCharacter = targetCharacter;
            SetCriteriaAsMet();
        }
    }
}
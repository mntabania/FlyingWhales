namespace Quests {
    public class DragonAwakenedCriteria : QuestCriteria {
        
        public Character targetCharacter { get; private set; }
        
        public override void Enable() {
            Messenger.AddListener<Character>(MonsterSignals.AWAKEN_DRAGON, OnDragonAwakened);
        }
        public override void Disable() {
            Messenger.RemoveListener<Character>(MonsterSignals.AWAKEN_DRAGON, OnDragonAwakened);
        }
        
        private void OnDragonAwakened(Character character) {
            targetCharacter = character;
            SetCriteriaAsMet();
        }
    }
}
namespace Quests {
    public class DragonLeftCriteria : QuestCriteria {
        
        public Character targetCharacter { get; private set; }
        
        public override void Enable() {
            Messenger.AddListener<Character>(Signals.DRAGON_LEFT_WORLD, OnDragonLeftWorld);
        }
        public override void Disable() {
            Messenger.RemoveListener<Character>(Signals.DRAGON_LEFT_WORLD, OnDragonLeftWorld);
        }
        
        private void OnDragonLeftWorld(Character character) {
            targetCharacter = character;
            SetCriteriaAsMet();
        }
    }
}
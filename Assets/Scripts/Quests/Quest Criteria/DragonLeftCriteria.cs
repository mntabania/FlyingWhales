namespace Quests {
    public class DragonLeftCriteria : QuestCriteria {
        
        public Character targetCharacter { get; private set; }
        public Region region { get; private set; }
        
        public override void Enable() {
            Messenger.AddListener<Character, Region>(MonsterSignals.DRAGON_LEFT_WORLD, OnDragonLeftWorld);
        }
        public override void Disable() {
            Messenger.RemoveListener<Character, Region>(MonsterSignals.DRAGON_LEFT_WORLD, OnDragonLeftWorld);
        }
        
        private void OnDragonLeftWorld(Character character, Region currentRegion) {
            targetCharacter = character;
            region = currentRegion;
            SetCriteriaAsMet();
        }
    }
}
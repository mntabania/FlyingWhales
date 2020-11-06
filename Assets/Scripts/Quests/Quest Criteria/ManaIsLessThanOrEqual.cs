using Tutorial;
namespace Quests {
    public class ManaIsLessThanOrEqual : QuestCriteria {

        private int _neededMana;
        
        public ManaIsLessThanOrEqual(int neededMana) {
            _neededMana = neededMana;
        }
        
        public override void Enable() {
            Messenger.AddListener<int, int>(Signals.PLAYER_ADJUSTED_MANA, OnManaAdjusted);
        }
        public override void Disable() {
            Messenger.RemoveListener<int, int>(Signals.PLAYER_ADJUSTED_MANA, OnManaAdjusted);
        }

        private void OnManaAdjusted(int amount, int mana) {
            if (mana <= _neededMana) {
                SetCriteriaAsMet();
            }
        }
    }
}
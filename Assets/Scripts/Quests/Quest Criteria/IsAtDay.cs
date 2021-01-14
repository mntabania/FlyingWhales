namespace Quests {
    public class IsAtDay : QuestCriteria {

        private readonly int _validDay;
        
        public IsAtDay(int validDay) {
            _validDay = validDay;
        }
        
        public override void Enable() {
            if (_validDay == GameManager.Instance.Today().day) {
                CheckCriteria(GameManager.Instance.Today().day);
            }
            Messenger.AddListener<int>(Signals.DAY_STARTED, CheckCriteria);
        }
        public override void Disable() {
            Messenger.RemoveListener<int>(Signals.DAY_STARTED, CheckCriteria);
        }

        private void CheckCriteria(int p_currentDay) {
            if (_validDay == p_currentDay) {
                if (hasCriteriaBeenMet == false) {
                    SetCriteriaAsMet();    
                }
            } else {
                if (hasCriteriaBeenMet) {
                    SetCriteriaAsUnMet();    
                }
            }
        }
    }
}
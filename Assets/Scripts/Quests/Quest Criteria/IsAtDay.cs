namespace Quests {
    public class IsAtDay : QuestCriteria {

        private readonly int _validDay;
        
        public IsAtDay(int validDay) {
            _validDay = validDay;
        }
        
        public override void Enable() {
            if (_validDay == GameManager.Instance.Today().day) {
                CheckCriteria();
            }
            Messenger.AddListener(Signals.DAY_STARTED, CheckCriteria);
        }
        public override void Disable() {
            Messenger.RemoveListener(Signals.DAY_STARTED, CheckCriteria);
        }

        private void CheckCriteria() {
            if (_validDay == GameManager.Instance.Today().day) {
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
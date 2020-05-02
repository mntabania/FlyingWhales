using System;
namespace Tutorial {
    public class PlayerIsInInnerMap : TutorialQuestCriteria {
        private readonly Func<Region, bool> _validityChecker;
        public PlayerIsInInnerMap(Func<Region, bool> validityChecker = null) {
            _validityChecker = validityChecker;
        }
        public override void Enable() {
            Messenger.AddListener<Region>(Signals.LOCATION_MAP_OPENED, OnLocationMapOpened);
            Messenger.AddListener<Region>(Signals.LOCATION_MAP_CLOSED, OnLocationMapClosed);
        }
        public override void Disable() {
            Messenger.RemoveListener<Region>(Signals.LOCATION_MAP_OPENED, OnLocationMapOpened);
            Messenger.RemoveListener<Region>(Signals.LOCATION_MAP_CLOSED, OnLocationMapClosed);
        }
        
        private void OnLocationMapOpened(Region location) {
            if (_validityChecker != null) {
                if (_validityChecker.Invoke(location)) {
                    SetCriteriaAsMet();
                }
            } else {
                SetCriteriaAsMet();    
            }
        }
        private void OnLocationMapClosed(Region location) {
            SetCriteriaAsUnMet();
        }
    }
}
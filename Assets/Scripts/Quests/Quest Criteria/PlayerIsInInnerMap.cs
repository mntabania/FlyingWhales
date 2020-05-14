using System;
using Inner_Maps;
namespace Quests {
    public class PlayerIsInInnerMap : QuestCriteria {
        private readonly Func<Region, bool> _validityChecker;
        public PlayerIsInInnerMap(Func<Region, bool> validityChecker = null) {
            _validityChecker = validityChecker;
        }
        public override void Enable() {
            Messenger.AddListener<Region>(Signals.LOCATION_MAP_OPENED, OnLocationMapOpened);
            Messenger.AddListener<Region>(Signals.LOCATION_MAP_CLOSED, OnLocationMapClosed);
            if (InnerMapManager.Instance.currentlyShowingMap != null) {
                SetCriteriaAsMet();
            }
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
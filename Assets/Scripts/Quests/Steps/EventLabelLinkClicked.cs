using System;
using Ruinarch.Custom_UI;
namespace Quests.Steps {
    public class EventLabelLinkClicked : QuestStep {
        
        private readonly string _eventLblName;
        
        public EventLabelLinkClicked(string eventLblName, string stepDescription) 
            : base(stepDescription) {
            _eventLblName = eventLblName;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<EventLabel>(UISignals.EVENT_LABEL_LINK_CLICKED, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<EventLabel>(UISignals.EVENT_LABEL_LINK_CLICKED, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(EventLabel eventLabel) {
            if (eventLabel.name == _eventLblName) {
                Complete();
            }
        }
        #endregion
    }
}
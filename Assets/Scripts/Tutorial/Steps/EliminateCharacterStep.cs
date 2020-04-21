using System.Collections.Generic;
using System.Linq;
namespace Tutorial {
    public class EliminateCharacterStep : TutorialQuestStep {
        private readonly List<Character> _targets;
        public EliminateCharacterStep(string stepDescription = "Eliminate characters", string tooltip = "", 
            params Character[] targets) : base(stepDescription, tooltip) {
            _targets = new List<Character>(targets);
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(Character character) {
            if (_targets.Remove(character) && _targets.Count == 0) {
                Complete();
            }
        }
        #endregion
    }
}
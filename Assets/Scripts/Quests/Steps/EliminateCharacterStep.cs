using System.Collections.Generic;
namespace Quests.Steps {
    public class EliminateCharacterStep : QuestStep {
        private readonly List<Character> _targets;
        public EliminateCharacterStep(string stepDescription = "Eliminate characters", 
            params Character[] targets) : base(stepDescription) {
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
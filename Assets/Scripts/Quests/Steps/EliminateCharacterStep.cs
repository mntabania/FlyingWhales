using System;
using System.Collections.Generic;
namespace Quests.Steps {
    public class EliminateCharacterStep : QuestStep {
        private readonly Func<List<Character>, int, string> _descriptionGetter;
        private readonly List<Character> _targets;
        private readonly int _initialCharactersToEliminate;
        
        public EliminateCharacterStep(string stepDescription = "Eliminate characters", 
            params Character[] targets) : base(stepDescription) {
            _targets = new List<Character>(targets);
            _initialCharactersToEliminate = _targets.Count;
        }
        public EliminateCharacterStep(Func<List<Character>, int, string> descriptionGetter, List<Character> targets) 
            : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
            _targets = targets;
            _initialCharactersToEliminate = _targets.Count;
        }
        
        
        protected override void SubscribeListeners() {
            Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(Character character) {
            if (_targets.Remove(character)) {
                Messenger.Broadcast(Signals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
                if (_targets.Count == 0) {
                    Complete();    
                }
            }
        }
        #endregion

        #region Description
        protected override string GetStepDescription() {
            if (_descriptionGetter != null) {
                return _descriptionGetter.Invoke(_targets, _initialCharactersToEliminate);
            }
            return base.GetStepDescription();
        }
        #endregion
    }
}
using System;
using System.Collections.Generic;
namespace Quests.Steps {
    public class EliminateAngelStep : QuestStep {
        private readonly Func<List<Character>, int, string> _descriptionGetter;
        private readonly List<Character> _targets;
        private readonly int _initialCharactersToEliminate;
        
        public EliminateAngelStep(Func<List<Character>, int, string> descriptionGetter, List<Character> targets) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
            _targets = new List<Character>(targets);
            _initialCharactersToEliminate = _targets.Count;
            for (int i = 0; i < targets.Count; i++) {
                Character target = targets[i];
                if (target.isDead) {
                    CheckForCompletion(target);
                }
            }
        }
        
        
        protected override void SubscribeListeners() {
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(Character character) {
            //remove character if character is dead or if he/she is no longer part of a major non player faction
            if (character.isDead) {
                if (_targets.Remove(character)) {
                    objectsToCenter?.Remove(character);
                    Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
                    if (_targets.Count == 0) {
                        Complete();    
                    }
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
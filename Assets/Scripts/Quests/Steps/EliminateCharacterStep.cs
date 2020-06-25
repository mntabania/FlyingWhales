using System;
using System.Collections.Generic;
using Traits;
namespace Quests.Steps {
    public class EliminateCharacterStep : QuestStep {
        private readonly Func<List<Character>, int, string> _descriptionGetter;
        private readonly List<Character> _targets;
        private readonly int _initialCharactersToEliminate;
        
        public EliminateCharacterStep(Func<List<Character>, int, string> descriptionGetter, List<Character> targets) 
            : base(string.Empty) {
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
            Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, CheckForCompletion);
            Messenger.AddListener<Character>(Signals.FACTION_SET, CheckForCompletion);
            Messenger.AddListener<Character>(Signals.CHARACTER_ALLIANCE_WITH_PLAYER_CHANGED, CheckForCompletion);
        }
        
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, CheckForCompletion);
            Messenger.RemoveListener<Character>(Signals.FACTION_SET, CheckForCompletion);
            Messenger.RemoveListener<Character>(Signals.CHARACTER_ALLIANCE_WITH_PLAYER_CHANGED, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(Character character) {
            //remove character if character is dead or if he/she is no longer part of a major non player faction
            if (character.isDead || (character.faction != null && character.faction.isMajorNonPlayerFriendlyNeutral == false) || character.isAlliedWithPlayer) {
                if (_targets.Remove(character)) {
                    objectsToCenter?.Remove(character);
                    Messenger.Broadcast(Signals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
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
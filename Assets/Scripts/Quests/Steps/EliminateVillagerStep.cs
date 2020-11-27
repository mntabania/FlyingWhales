using System;
using System.Collections.Generic;
using Traits;
namespace Quests.Steps {
    public class EliminateVillagerStep : QuestStep {
        private readonly Func<List<Character>, int, string> _descriptionGetter;
        private readonly List<Character> _targets;
        private int _initialCharactersToEliminate;

        public EliminateVillagerStep(Func<List<Character>, int, string> descriptionGetter, List<Character> targets) : base(string.Empty) {
            _descriptionGetter = descriptionGetter;
            _targets = new List<Character>(targets);
            _initialCharactersToEliminate = _targets.Count;
            for (int i = 0; i < targets.Count; i++) {
                Character target = targets[i];
                if (EliminateAllVillagers.ShouldConsiderCharacterAsEliminated(target)) {
                    CheckForCompletion(target);
                }
            }
        }
        
        
        protected override void SubscribeListeners() {
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, CheckForCompletion);
            Messenger.AddListener<Character>(FactionSignals.FACTION_SET, CheckForCompletion);
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_ALLIANCE_WITH_PLAYER_CHANGED, CheckForCompletion);
            Messenger.AddListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnNewVillagerArrived);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, CheckForCompletion);
            Messenger.RemoveListener<Character>(FactionSignals.FACTION_SET, CheckForCompletion);
            Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_ALLIANCE_WITH_PLAYER_CHANGED, CheckForCompletion);
            Messenger.RemoveListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnNewVillagerArrived);
        }

        #region Listeners
        private void CheckForCompletion(Character character) {
            //remove character if character is dead or if he/she is no longer part of a major non player faction
            if (EliminateAllVillagers.ShouldConsiderCharacterAsEliminated(character)) {
                if (_targets.Remove(character)) {
                    objectsToCenter?.Remove(character);
                    Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
                    if (_targets.Count == 0) {
                        Complete();
                        Messenger.Broadcast(PlayerSignals.WIN_GAME);
                    }
                }    
            }
        }
        private void OnNewVillagerArrived(Character newVillager) {
            _targets.Add(newVillager);
            objectsToCenter?.Add(newVillager);
            _initialCharactersToEliminate++;
            Messenger.Broadcast(UISignals.UPDATE_QUEST_STEP_ITEM, this as QuestStep);
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
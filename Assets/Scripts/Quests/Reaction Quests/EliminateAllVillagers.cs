using System.Collections.Generic;
using System.Linq;
using Quests.Steps;
using UnityEngine;
namespace Quests {
    public class EliminateAllVillagers : ReactionQuest {


        private EliminateVillagerStep _eliminateVillagerStep;
        
        public EliminateAllVillagers() : base($"Eliminate All Villagers") { }
        protected override void ConstructSteps() {
            List<Character> villagers = CharacterManager.Instance.GetAllNormalCharacters();
            _eliminateVillagerStep = new EliminateVillagerStep(GetEliminateAllVillagersDescription, villagers);
            _eliminateVillagerStep.SetObjectsToCenter(villagers.Where(x => x.isDead == false).Select(x => x as ISelectable).ToList());
            
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(_eliminateVillagerStep),
            };
            
            Messenger.AddListener<KeyCode>(Signals.KEY_DOWN, OnKeyPressed);
        }
        private void OnKeyPressed(KeyCode keyCode) {
            if (keyCode == KeyCode.Tab) {
                if (!_eliminateVillagerStep.isCompleted) {
                    _eliminateVillagerStep.CenterCycle();    
                }
            }
        }

        #region Step Helpers
        private string GetEliminateAllVillagersDescription(List<Character> remainingTargets, int totalCharactersToEliminate) {
            return $"Villagers Remaining: {remainingTargets.Count.ToString()}"; // /{totalCharactersToEliminate.ToString()}
        }
        #endregion

        #region Utilities
        public static bool ShouldConsiderCharacterAsEliminated(Character character) {
            return character.isDead ||
                   (character.faction != null && character.faction.isMajorNonPlayerFriendlyNeutral == false) ||
                   character.isAlliedWithPlayer;
        }
        #endregion
    }
}
using System.Collections.Generic;
using System.Linq;
using Quests.Steps;
namespace Quests {
    public class EliminateAllVillagers : ReactionQuest {
        
        public EliminateAllVillagers() : base($"Eliminate All Villagers") { }
        protected override void ConstructSteps() {
            List<Character> villagers = CharacterManager.Instance.GetAllNormalCharacters();
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new EliminateCharacterStep(GetEliminateAllVillagersDescription, villagers)
                        .SetObjectsToCenter(villagers.Where(x => x.isDead == false).Select(x => x as ISelectable).ToList())
                ),
            };
        }

        #region Step Helpers
        private string GetEliminateAllVillagersDescription(List<Character> remainingTargets, int totalCharactersToEliminate) {
            return $"Villagers Remaining: {remainingTargets.Count.ToString()}"; // /{totalCharactersToEliminate.ToString()}
        }
        #endregion
    }
}
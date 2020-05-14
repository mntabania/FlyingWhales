using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Quests.Steps;
namespace Quests {
    public class DivineIntervention : ReactionQuest {

        private readonly List<Character> _angels;

        #region getters
        public List<Character> angels => _angels;
        #endregion
        
        public DivineIntervention(List<Character> angels) : base("Divine Intervention") {
            _angels = angels;
        }
        
        protected override void ConstructSteps() {
            CharacterBehaviourComponent attackDemonicStructureBehaviour =
                CharacterManager.Instance.GetCharacterBehaviourComponent(typeof(AttackDemonicStructureBehaviour));
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new EliminateCharacterStep(GetStopCharactersDescription, angels)
                        .SetHoverOverAction(OnHoverEliminateItem)
                        .SetHoverOutAction(() => UIManager.Instance.HideSmallInfo())
                        .SetObjectsToCenter(angels.Select(x => x as ISelectable).ToArray())    
                )
            };
        }

        #region Step Helpers
        private string GetStopCharactersDescription(List<Character> targets, int initialTargetCount) {
            return $"Eliminate {(initialTargetCount - targets.Count).ToString()}/{initialTargetCount.ToString()} Angels.";
        }
        private void OnHoverEliminateItem(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo(
                $"The Divine has sent down {angels.Count.ToString()} Angels to stop you from further transgressions. " +
                "Eliminate all of them before they destroy your demonic structures.",
                pos: item.hoverPosition
            );
        }
        #endregion
    }
}
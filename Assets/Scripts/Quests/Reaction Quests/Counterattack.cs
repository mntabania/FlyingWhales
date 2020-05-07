using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Quests.Steps;
namespace Quests {
    public class Counterattack : ReactionQuest {

        private List<Character> _attackers;
        private DemonicStructure _targetStructure;

        public Counterattack(List<Character> attackers, DemonicStructure targetStructure) : base("Counterattack!") {
            _attackers = attackers;
            _targetStructure = targetStructure;
        }
        protected override void ConstructSteps() {
            CharacterBehaviourComponent attackDemonicStructureBehaviour =
                CharacterManager.Instance.GetCharacterBehaviourComponent(typeof(AttackDemonicStructureBehaviour));
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new CharactersRemovedBehaviourStep($"Defend {_targetStructure.name}", 
                            _attackers, attackDemonicStructureBehaviour)
                        .SetObjectsToCenter(_targetStructure),
                    new CharactersRemovedBehaviourStep(GetStopCharactersDescription, _attackers, 
                            attackDemonicStructureBehaviour)
                        .SetHoverOverAction(OnHoverStopItem)
                        .SetHoverOutAction(() => UIManager.Instance.HideSmallInfo())
                        .SetObjectsToCenter(_attackers.Select(x => x as ISelectable).ToArray())    
                )
            };
        }
        public override void Activate() {
            base.Activate();
            Messenger.AddListener<LocationStructure>(Signals.STRUCTURE_DESTROYED, OnStructureDestroyed);
        }
        public override void Deactivate() {
            base.Deactivate();
            Messenger.RemoveListener<LocationStructure>(Signals.STRUCTURE_DESTROYED, OnStructureDestroyed);
        }

        #region Step Helpers
        private string GetStopCharactersDescription(List<Character> targets, int initialTargetCount) {
            return $"Stop {(initialTargetCount - targets.Count).ToString()}/{initialTargetCount.ToString()} Villagers.";
        }
        private void OnHoverStopItem(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo(
                "There are several ways to distract a Villager from their tasks. " +
                 "An Exhausted, Starving or Sulking villager will drop what it's doing to recover. " +
                 "Status Effects that temporarily stops a Villager from moving " +
                 "(ex: Unconscious, Zapped, Frozen) will also make it forget its current action. " +
                 "Killing a Villager, of course, is a permanent distraction.",
                pos: item.hoverPosition
            );
        }
        #endregion

        #region Failure
        private void OnStructureDestroyed(LocationStructure structure) {
            if (_targetStructure == structure) {
                FailQuest();
            }
        }
        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Inner_Maps.Location_Structures;
using Quests.Steps;
using UnityEngine.Assertions;
namespace Quests {
    public class Counterattack : ReactionQuest {

        private readonly List<Character> _attackers;
        private readonly DemonicStructure _targetStructure;

        #region getters
        public List<Character> attackers => _attackers;
        public DemonicStructure targetStructure => _targetStructure;
        public override Type serializedData => typeof(SaveDataCounterattack);
        #endregion
        
        public Counterattack(List<Character> attackers, DemonicStructure targetStructure) : base("Counterattack!") {
            _attackers = attackers;
            _targetStructure = targetStructure;
        }
        public Counterattack(SaveDataCounterattack saveData) : base("Counterattack!") {
            _attackers = SaveUtilities.ConvertIDListToCharacters(saveData.attackerIDs);
            _targetStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveData.targetStructureID) as DemonicStructure;
            Assert.IsNotNull(_targetStructure, $"Could not find demonic structure with persistent ID {saveData.targetStructureID}");
        }
        
        protected override void ConstructSteps() {
            CharacterBehaviourComponent attackDemonicStructureBehaviour = CharacterManager.Instance.GetCharacterBehaviourComponent(typeof(AttackDemonicStructureBehaviour));
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new CharactersRemovedBehaviourStep($"Defend {_targetStructure.name}", new List<Character>(attackers), attackDemonicStructureBehaviour)
                        .SetObjectsToCenter(_targetStructure),
                    new CharactersRemovedBehaviourStep(GetStopCharactersDescription, new List<Character>(attackers), attackDemonicStructureBehaviour)
                        .SetHoverOverAction(OnHoverStopItem)
                        .SetHoverOutAction(() => UIManager.Instance.HideSmallInfo())
                        .SetObjectsToCenter(attackers.Select(x => x as ISelectable).ToArray())    
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
            return $"Stop {targets.Count.ToString()} Villagers.";
        }
        private void OnHoverStopItem(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo(
                $"There are several ways to distract a Villager from their tasks. " +
                 "An Exhausted, Starving or Sulking villager will drop what it's doing to recover. " +
                 $"Status Effects that temporarily stops a Villager from moving " +
                 "(ex: Unconscious, Zapped, Frozen) will also make it forget its current action. " +
                 $"Killing a Villager, of course, is a permanent distraction.",
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

    public class SaveDataCounterattack : SaveDataReactionQuest {
        public List<string> attackerIDs;
        public string targetStructureID;
        public override void Save(ReactionQuest data) {
            base.Save(data);
            Counterattack counterattack = data as Counterattack;
            Debug.Assert(counterattack != null, nameof(counterattack) + " != null");
            attackerIDs = SaveUtilities.ConvertSavableListToIDs(counterattack.attackers);
            targetStructureID = counterattack.targetStructure.persistentID;
        }
        public override ReactionQuest Load() {
            return new Counterattack(this);
        }
    }
}
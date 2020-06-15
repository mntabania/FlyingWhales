using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using JetBrains.Annotations;
using Quests.Steps;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Quests {
    [UsedImplicitly]
    public class DemonicStructureDiscovered : ReactionQuest {

        private LocationStructure _targetStructure;
        private readonly Character _reporter;
        private readonly GoapPlanJob _targetJob;
        
        public DemonicStructureDiscovered(LocationStructure targetStructure, Character reporter, GoapPlanJob targetJob)
            : base($"{targetStructure.nameWithoutID} Discovered!") {
            _targetStructure = targetStructure;
            _reporter = reporter;
            _targetJob = targetJob;
        }
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new GoapJobFailed($"Stop {_reporter.name}!", _reporter, _targetJob)
                        .SetHoverOverAction(OnHoverStopActor)
                        .SetHoverOutAction(() => UIManager.Instance.HideSmallInfo())
                        .SetObjectsToCenter(_reporter)    
                )
            };
        }
        public override void Activate() {
            base.Activate();
            Messenger.AddListener<Character, GoapPlanJob>(Signals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJobSuccessfully);
        }
        public override void Deactivate() {
            base.Deactivate();
            Messenger.RemoveListener<Character, GoapPlanJob>(Signals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJobSuccessfully);
        }

        #region Step Helpers
        private void OnHoverStopActor(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo(
                $"{_reporter.name} has discovered your {_targetStructure.name}! " +
                $"{UtilityScripts.Utilities.GetPronounString(_reporter.gender, PRONOUN_TYPE.SUBJECTIVE, true)} " +
                $"is returning home to report it. If successful, your threat will increase by 100. " +
                $"Find a way to stop {UtilityScripts.Utilities.GetPronounString(_reporter.gender, PRONOUN_TYPE.OBJECTIVE, false)}!",
                pos: item.hoverPosition
            );
        }
        #endregion

        #region Failure
        private void OnCharacterFinishedJobSuccessfully(Character character, GoapPlanJob job) {
            if (character == _reporter && job == _targetJob) {
                FailQuest();
            }
        }
        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Inner_Maps.Location_Structures;
using JetBrains.Annotations;
using Quests.Steps;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Quests {
    [UsedImplicitly]
    public class DemonicStructureDiscovered : ReactionQuest {

        private readonly LocationStructure _targetStructure;
        private readonly Character _reporter;
        private readonly GoapPlanJob _targetJob;

        #region getters
        public override Type serializedData => typeof(SaveDataDemonicStructureDiscovered);
        public LocationStructure targetStructure => _targetStructure;
        public Character reporter => _reporter;
        public GoapPlanJob targetJob => _targetJob;
        #endregion
        
        public DemonicStructureDiscovered(LocationStructure targetStructure, Character reporter, GoapPlanJob targetJob) : base($"{targetStructure.nameWithoutID} Discovered!") {
            _targetStructure = targetStructure;
            _reporter = reporter;
            _targetJob = targetJob;
        }
        public DemonicStructureDiscovered(SaveDataDemonicStructureDiscovered data) : base(data.questName) {
            _targetStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(data.targetStructureID);
            _reporter = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(data.reporterID);
            _targetJob = DatabaseManager.Instance.jobDatabase.GetJobWithPersistentID(data.jobID) as GoapPlanJob;
        }
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new GoapJobFailed($"Stop {reporter.name}!", reporter, targetJob)
                        .SetHoverOverAction(OnHoverStopActor)
                        .SetHoverOutAction(() => UIManager.Instance.HideSmallInfo())
                        .SetObjectsToCenter(reporter)    
                )
            };
        }
        public override void Activate() {
            base.Activate();
            Messenger.AddListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJobSuccessfully);
        }
        public override void Deactivate() {
            base.Deactivate();
            Messenger.RemoveListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJobSuccessfully);
        }

        #region Step Helpers
        private void OnHoverStopActor(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo(
                $"{reporter.name} has discovered your {targetStructure.name}! " +
                $"{UtilityScripts.Utilities.GetPronounString(reporter.gender, PRONOUN_TYPE.SUBJECTIVE, true)} " +
                $"is returning home to report it. If successful, your threat will increase by 20, " +
                $"and {reporter.name} will post a job to attack your {targetStructure.name}. " +
                $"Find a way to stop {UtilityScripts.Utilities.GetPronounString(reporter.gender, PRONOUN_TYPE.OBJECTIVE, false)}!",
                pos: item.hoverPosition
            );
        }
        #endregion

        #region Failure
        private void OnCharacterFinishedJobSuccessfully(Character character, GoapPlanJob job) {
            if (character == reporter && job == targetJob) {
                FailQuest();
            }
        }
        #endregion
    }
    
    public class SaveDataDemonicStructureDiscovered : SaveDataReactionQuest {
        public string questName;
        public string reporterID;
        public string jobID;
        public string targetStructureID;
        public override void Save(ReactionQuest data) {
            base.Save(data);
            DemonicStructureDiscovered demonicStructureDiscovered = data as DemonicStructureDiscovered;
            Debug.Assert(demonicStructureDiscovered != null, nameof(demonicStructureDiscovered) + " != null");
            questName = demonicStructureDiscovered.questName;
            reporterID = demonicStructureDiscovered.reporter.persistentID;
            jobID = demonicStructureDiscovered.targetJob.persistentID;
            targetStructureID = demonicStructureDiscovered.targetStructure.persistentID;
        }
        public override ReactionQuest Load() {
            return new DemonicStructureDiscovered(this);
        }
    }
}
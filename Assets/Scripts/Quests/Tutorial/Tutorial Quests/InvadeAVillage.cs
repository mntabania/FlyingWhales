using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Quests;
using Quests.Steps;
using UnityEngine;
namespace Tutorial {
    public class InvadeAVillage : TutorialQuest {
        
        public InvadeAVillage() : base("Invade A Village", TutorialManager.Tutorial.Invade_A_Village) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new HasCompletedTutorialQuest(TutorialManager.Tutorial.Build_A_Kennel)
            };
            Messenger.AddListener<PlayerAction>(Signals.ON_EXECUTE_PLAYER_ACTION, OnPlayerActionExecuted);
        }
        protected override bool HasMetAllCriteria() {
            bool hasMetAllCriteria = base.HasMetAllCriteria();
            if (hasMetAllCriteria) {
                return PlayerManager.Instance.player.playerSkillComponent.minionsSkills.Count > 0 
                       || PlayerManager.Instance.player.playerSkillComponent.summonsSkills.Count > 0;
            }
            return false;
        }
        #endregion

        #region Overrides
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(new ClickOnAreaStep("Click on a Village area", validityChecker: IsHexTileAVillage)),
                new QuestStepCollection(
                    new ActivateInvadeStep(),
                    new ExecutedPlayerActionStep(SPELL_TYPE.INVADE, "Summon at least one invader")
                )
            };
        }
        public override void Activate() {
            base.Activate();
            Messenger.RemoveListener<PlayerAction>(Signals.ON_EXECUTE_PLAYER_ACTION, OnPlayerActionExecuted);
        }
        public override void Deactivate() {
            base.Deactivate();
            Messenger.RemoveListener<PlayerAction>(Signals.ON_EXECUTE_PLAYER_ACTION, OnPlayerActionExecuted);
        }
        #endregion

        #region Step Helpers
        private bool IsHexTileAVillage(HexTile tile) {
            return tile.landmarkOnTile != null && tile.landmarkOnTile.specificLandmarkType.GetStructureType().IsSettlementStructure();
        }
        #endregion

        #region Completion
        private void OnPlayerActionExecuted(PlayerAction playerAction) {
            if (playerAction.type == SPELL_TYPE.INVADE) {
                CompleteQuest();
            }
        }
        #endregion
    }
}
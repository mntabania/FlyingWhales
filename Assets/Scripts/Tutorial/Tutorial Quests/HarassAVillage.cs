using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
namespace Tutorial {
    public class HarassAVillage : TutorialQuest {
        
        private Coroutine availabilityTimer;

        private bool hasNotCasted;
        
        public override int priority => 5;
        public HarassAVillage() : base("Harass A Village", TutorialManager.Tutorial.Harass_A_Village) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<TutorialQuestCriteria>() {
                new HasCompletedTutorialQuest(TutorialManager.Tutorial.Basic_Controls),
                new PlayerHasNotCastedForSeconds(15f),
                new PlayerHasNotCompletedTutorialInSeconds(15f),
                new PlayerIsInInnerMap(DoesLocationHaveAVillage),
            };
        }
        protected override bool HasMetAllCriteria() {
            bool hasMetAllCriteria = base.HasMetAllCriteria();
            if (hasMetAllCriteria) {
                return PlayerManager.Instance.player.playerSkillComponent.minionsSkills.Count > 0 
                       && TutorialManager.Instance.HasActiveTutorial() == false;
            }
            return false;
        }
        private bool DoesLocationHaveAVillage(Region location) {
            return location.HasActiveSettlement();
        }
        #endregion
        
        
        #region Overrides
        protected override void ConstructSteps() {
            steps = new List<TutorialQuestStepCollection>() {
                new TutorialQuestStepCollection(new ClickOnAreaStep("Click on a Village area", validityChecker: IsHexTileAVillage)),
                new TutorialQuestStepCollection(
                    new ActivateHarassStep(),
                    new ExecutedPlayerActionStep(SPELL_TYPE.HARASS, "Summon at least one harasser")
                )
            };
        }
        #endregion
        

        #region Step Helpers
        private bool IsHexTileAVillage(HexTile tile) {
            return tile.landmarkOnTile != null && tile.landmarkOnTile.specificLandmarkType.GetStructureType().IsSettlementStructure();
        }
        #endregion
    }
}
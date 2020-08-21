using System;
using System.Collections.Generic;
using Quests;
using Quests.Steps;
namespace Tutorial {
    public class WorldMap : BonusTutorial {
        public WorldMap() : base("World Map", TutorialManager.Tutorial.World_Map) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new IsAtTime(new[] {
                    GameManager.Instance.GetTicksBasedOnHour(3),
                    GameManager.Instance.GetTicksBasedOnHour(11)
                }),
                new PlayerIsInInnerMap()
            };
        }
        protected override bool HasMetAllCriteria() {
            bool hasMetCriteria = base.HasMetAllCriteria();
            if (hasMetCriteria) {
                return GridMap.Instance.allRegions.Length >= 2 && TutorialManager.Instance.GetAllActiveTutorialsCount() + QuestManager.Instance.GetActiveQuestsCount() <= 1;
            }
            return false;
        }
        #endregion
        
        #region Steps
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new ButtonClickedStep("ToggleMapBtn", "Click on World Map button")
                        .SetOnTopmostActions(OnTopMostWorldMap, OnNoLongerTopMostWorldMap),
                    new DoubleClickHexTileStep("Double click on any tile")
                ),
            };
        }
        #endregion
        
        #region Availability
        protected override void MakeAvailable() {
            if (isAvailable) {
                return;
            }
            isAvailable = true;
            ConstructSteps();
            TutorialManager.Instance.ActivateTutorial(this);
        }
        #endregion
        
        #region World Map
        private void OnTopMostWorldMap() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "ToggleMapBtn");
        }
        private void OnNoLongerTopMostWorldMap() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "ToggleMapBtn");
        }
        #endregion
        
    }
}
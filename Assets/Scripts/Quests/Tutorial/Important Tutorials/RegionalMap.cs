using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Quests;
using Quests.Steps;
using UnityEngine;
using UnityEngine.PlayerLoop;
namespace Tutorial {
    public class RegionalMap : ImportantTutorial {
        
        public override int priority => 20;
        private float _notCastingTime;
        
        public RegionalMap() : base("Regional Map", TutorialManager.Tutorial.Regional_Map) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new HasCompletedTutorialQuest(TutorialManager.Tutorial.Basic_Controls),
                new PlayerHasNotCastedForSeconds(15f),
                new PlayerHasNotCompletedTutorialInSeconds(15f),
                new PlayerIsInInnerMap()
            };
            Messenger.AddListener<HexTile>(Signals.TILE_DOUBLE_CLICKED, OnTileDoubleClicked);
        }
        #endregion
        
        #region Overrides
        public override void Activate() {
            base.Activate();
            Messenger.RemoveListener<HexTile>(Signals.TILE_DOUBLE_CLICKED, OnTileDoubleClicked);
        }
        public override void Deactivate() {
            base.Deactivate();
            Messenger.RemoveListener<HexTile>(Signals.TILE_DOUBLE_CLICKED, OnTileDoubleClicked);
        }
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(new HideRegionMapStep("Click on globe icon")),
                new QuestStepCollection(new SelectRegionStep()),
                new QuestStepCollection(new DoubleClickHexTileStep())
            };
        }
        #endregion

        #region Listeners
        private void OnTileDoubleClicked(HexTile hexTile) {
            CompleteQuest();
        }
        #endregion
    }
}
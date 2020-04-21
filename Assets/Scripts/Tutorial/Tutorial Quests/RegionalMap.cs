using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.PlayerLoop;
namespace Tutorial {
    public class RegionalMap : TutorialQuest {
        
        public override int priority => 20;
        private float _notCastingTime;
        
        public RegionalMap() : base("Regional Map", TutorialManager.Tutorial.Regional_Map) { }
        
        #region Overrides
        public override void WaitForAvailability() {
            Messenger.AddListener<SpellData>(Signals.ON_EXECUTE_SPELL, OnSpellCast);
            Messenger.AddListener<SpellData>(Signals.ON_EXECUTE_AFFLICTION, OnSpellCast);
            Messenger.AddListener<PlayerAction>(Signals.ON_EXECUTE_PLAYER_ACTION, OnSpellCast);
            Messenger.AddListener<HexTile>(Signals.TILE_DOUBLE_CLICKED, OnTileDoubleClicked);
        }
        public override void Activate() {
            base.Activate();
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_SPELL, OnSpellCast);
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_AFFLICTION, OnSpellCast);
            Messenger.RemoveListener<PlayerAction>(Signals.ON_EXECUTE_PLAYER_ACTION, OnSpellCast);
            Messenger.RemoveListener<HexTile>(Signals.TILE_DOUBLE_CLICKED, OnTileDoubleClicked);
        }
        protected override void ConstructSteps() {
            steps = new List<TutorialQuestStepCollection>() {
                new TutorialQuestStepCollection(new HideRegionMapStep("Click on globe icon")),
                new TutorialQuestStepCollection(new SelectRegionStep()),
                new TutorialQuestStepCollection(new DoubleClickHexTileStep())
            };
        }
        public override void PerFrameActions() {
            _notCastingTime += Time.deltaTime;
            if (isAvailable == false) {
                if (_notCastingTime >= 10f) {
                    if (InnerMapManager.Instance.currentlyShowingLocation != null) {
                        MakeAvailable();
                    }
                }
            }
        }
        #endregion

        #region Listeners
        private void OnSpellCast(SpellData spellData) {
            _notCastingTime = 0f;
            if (TutorialManager.Instance.IsInWaitList(this)) {
                MakeUnavailable();
            }
        }
        private void OnTileDoubleClicked(HexTile hexTile) {
            CompleteTutorial();
        }
        #endregion
    }
}
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Quests.Special_Popups {
    [UsedImplicitly]
    public class ActivatedAnkh : SpecialPopup {

        private TileObject _targetObject;

        public ActivatedAnkh() : base("Activated Ankh", QuestManager.Special_Popup.Activated_Ankh) { }
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>(
                new [] {
                    new TileObjectActivated(TILE_OBJECT_TYPE.ANKH_OF_ANUBIS).SetOnMeetAction(SetTargetObject), 
                }    
            );
        }
        protected override bool HasMetAllCriteria() {
            bool criteriaMet = base.HasMetAllCriteria();
            if (criteriaMet) {
                return WorldSettings.Instance.worldSettingsData.worldType != WorldSettingsData.World_Type.Tutorial;
            }
            return false;
        }
        private void SetTargetObject(QuestCriteria criteria) {
            if (criteria is TileObjectActivated tileObjectActivated) {
                _targetObject = tileObjectActivated.activatedObject;
            }
        }
        public override void Activate() {
            StopCheckingCriteria();
            UIManager.Instance.ShowYesNoConfirmation("Ankh of Anubis", 
                $"The {UtilityScripts.Utilities.ColorizeAction("Ankh of Anubis")} is acting strangely! Something must have happened to activate its powers. Watch out!", 
                onClickYesAction: () => UIManager.Instance.ShowTileObjectInfo(_targetObject), 
                yesBtnText: $"Center on {_targetObject.name}", noBtnText: "OK", pauseAndResume: true, showCover: true);
            CompleteQuest();
        }
    }
}
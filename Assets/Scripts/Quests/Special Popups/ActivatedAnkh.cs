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
                    new TileObjectActivated<AnkhOfAnubis>().SetOnMeetAction(SetTargetObject), 
                }    
            );
        }
        protected override bool HasMetAllCriteria() {
            bool criteriaMet = base.HasMetAllCriteria();
            if (criteriaMet) {
                return WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona;
            }
            return false;
        }
        private void SetTargetObject(QuestCriteria criteria) {
            if (criteria is TileObjectActivated<AnkhOfAnubis> tileObjectActivated) {
                _targetObject = tileObjectActivated.activatedObject;
            }
        }
        public override void Activate() {
            StopCheckingCriteria();
            PlayerUI.Instance.ShowGeneralConfirmation("Ankh of Anubis", 
                $"The {UtilityScripts.Utilities.ColorizeAction("Ankh of Anubis")} is acting strangely! Something must have happened to activate its powers. Watch out!", 
                onClickCenter: () => UIManager.Instance.ShowTileObjectInfo(_targetObject));
            CompleteQuest();
        }
    }
}
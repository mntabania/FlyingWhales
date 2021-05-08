using System;
using System.Collections.Generic;
using Inner_Maps;
namespace Quests.Special_Popups {
    public class TheNecronomicon : SpecialPopup {

        private TileObject _necronomicon;
        
        public TheNecronomicon() : base("The Necronomicon", QuestManager.Special_Popup.The_Necronomicon) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _necronomicon = InnerMapManager.Instance.GetFirstArtifact(ARTIFACT_TYPE.Necronomicon);
            _activationCriteria = new List<QuestCriteria>(
                new QuestCriteria[] {
                    new IsAtDay(1),
                    new IsAtTime(new[] {GameManager.Instance.GetTicksBasedOnHour(18)}), 
                }    
            );
        }
        protected override bool HasMetAllCriteria() {
            bool hasMetCriteria = base.HasMetAllCriteria();
            if (hasMetCriteria) {
                return false; //WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo;
            }
            return false;
        }
        #endregion

        #region Activation
        public override void Activate() {
            PlayerUI.Instance.ShowGeneralConfirmation("The Necronomicon", 
                $"The {UtilityScripts.Utilities.ColorizeAction("Necronomicon")} is here. A dark hearted individual might be able to tap its full potential!", 
                onClickCenter: () => UIManager.Instance.ShowTileObjectInfo(_necronomicon)
            );
            CompleteQuest();
        }
        public override void Deactivate() {
            base.Deactivate();
            StopCheckingCriteria();
        }
        #endregion
    }
}
using System;
using System.Collections.Generic;
using Inner_Maps;
namespace Quests.Special_Popups {
    public class TheSword : SpecialPopup {

        private TileObject _excalibur;
        
        public TheSword() : base("The Sword", QuestManager.Special_Popup.The_Sword) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _excalibur = InnerMapManager.Instance.GetFirstTileObject(TILE_OBJECT_TYPE.EXCALIBUR);
            _activationCriteria = new List<QuestCriteria>(
                new QuestCriteria[] {
                    new IsAtDay(2),
                    new IsAtTime(new[] {GameManager.Instance.GetTicksBasedOnHour(13)}), 
                }    
            );
            Messenger.AddListener<ISelectable>(Signals.SELECTABLE_LEFT_CLICKED, OnSelectableLeftClicked);
            Messenger.AddListener<TileObject, Character>(Signals.CHARACTER_OBTAINED_ITEM, OnCharacterObtainedItem);
        }
        protected override bool HasMetAllCriteria() {
            bool hasMetCriteria = base.HasMetAllCriteria();
            if (hasMetCriteria) {
                return WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa;
            }
            return false;
        }
        #endregion

        #region Completion
        private void OnSelectableLeftClicked(ISelectable selectable) {
            if (selectable is Excalibur) {
                //complete quest when excalibur is selected
                CompleteQuest();
            }
        }
        private void OnCharacterObtainedItem(TileObject tileObject, Character character) {
            if (tileObject.tileObjectType == TILE_OBJECT_TYPE.EXCALIBUR) {
                CompleteQuest();
            }
        }
        #endregion
        
        #region Activation
        public override void Activate() {
            Messenger.RemoveListener<ISelectable>(Signals.SELECTABLE_LEFT_CLICKED, OnSelectableLeftClicked);
            Messenger.RemoveListener<TileObject, Character>(Signals.CHARACTER_OBTAINED_ITEM, OnCharacterObtainedItem);
            PlayerUI.Instance.ShowGeneralConfirmation("Excalibur", 
                $"There is a {UtilityScripts.Utilities.ColorizeAction("legendary sword")} at the center of this region. Is it something we can use, or something that can be used against us?", 
                onClickCenter: () => UIManager.Instance.ShowTileObjectInfo(_excalibur)
            );
            CompleteQuest();
        }
        public override void Deactivate() {
            base.Deactivate();
            StopCheckingCriteria();
            Messenger.RemoveListener<ISelectable>(Signals.SELECTABLE_LEFT_CLICKED, OnSelectableLeftClicked);
            Messenger.RemoveListener<TileObject, Character>(Signals.CHARACTER_OBTAINED_ITEM, OnCharacterObtainedItem);
        }
        #endregion
    }
}
using System;
using System.Collections.Generic;
namespace Quests.Special_Popups {
    public class SleepingDragon : SpecialPopup {

        private Dragon _targetDragon;
        
        public SleepingDragon() : base("A Dragon", QuestManager.Special_Popup.Sleeping_Dragon) { }

        #region Criteria
        protected override void ConstructCriteria() {
            for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                Character character = CharacterManager.Instance.allCharacters[i];
                if (character is Dragon dragon) {
                    _targetDragon = dragon;
                    break;
                }
            }
            _activationCriteria = new List<QuestCriteria>(
                new QuestCriteria[] {
                    new IsAtDay(1),
                    new IsAtTime(new[] {GameManager.Instance.GetTicksBasedOnHour(13)}), 
                }    
            );
            Messenger.AddListener<ISelectable>(ControlsSignals.SELECTABLE_LEFT_CLICKED, OnSelectableLeftClicked);
            Messenger.AddListener<Character>(MonsterSignals.AWAKEN_DRAGON, OnDragonAwakened);
        }
        protected override bool HasMetAllCriteria() {
            bool hasMetCriteria = base.HasMetAllCriteria();
            if (hasMetCriteria) {
                return WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona ;
            }
            return false;
        }
        #endregion

        #region Completion
        private void OnSelectableLeftClicked(ISelectable selectable) {
            if (selectable is Dragon) {
                //complete quest when dragon is selected
                CompleteQuest();
            }
        }
        private void OnDragonAwakened(Character dragon) {
            CompleteQuest();
        }
        #endregion

        #region Activation
        public override void Activate() {
            Messenger.RemoveListener<ISelectable>(ControlsSignals.SELECTABLE_LEFT_CLICKED, OnSelectableLeftClicked);
            PlayerUI.Instance.ShowGeneralConfirmation("A Dragon", 
                $"There is a {UtilityScripts.Utilities.ColorizeAction("sleeping dragon")} in this region. It has been hibernating for hundreds of years. " +
                $"If you manage to {UtilityScripts.Utilities.ColorizeAction("awaken")} it, it may assist you in wiping out the village.", 
                onClickCenter: () => UIManager.Instance.ShowCharacterInfo(_targetDragon, true)
            );
            CompleteQuest();
        }
        public override void Deactivate() {
            base.Deactivate();
            StopCheckingCriteria();
            Messenger.RemoveListener<ISelectable>(ControlsSignals.SELECTABLE_LEFT_CLICKED, OnSelectableLeftClicked);
        }
        #endregion
    }
}
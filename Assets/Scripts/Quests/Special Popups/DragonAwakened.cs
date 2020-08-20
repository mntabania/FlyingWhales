using System.Collections.Generic;
using JetBrains.Annotations;

namespace Quests.Special_Popups {
    [UsedImplicitly]
    public class DragonAwakened : SpecialPopup {

        private Character _targetCharacter;

        public DragonAwakened() : base("Dragon Awakened", QuestManager.Special_Popup.Dragon_Awakened) {
            isRepeatable = true;
        }
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>(
                new [] {
                    new DragonAwakenedCriteria().SetOnMeetAction(SetTargetCharacter), 
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
        private void SetTargetCharacter(QuestCriteria criteria) {
            if (criteria is DragonAwakenedCriteria dragonAwakenedCriteria) {
                _targetCharacter = dragonAwakenedCriteria.targetCharacter;
            }
        }
        public override void Activate() {
            StopCheckingCriteria();
            PlayerUI.Instance.ShowGeneralConfirmation("Dragon Awakened", 
                $"{UtilityScripts.Utilities.ColorizeAction(_targetCharacter.name)} has been awakened from its long slumber!", 
                onClickCenter: () => UIManager.Instance.ShowCharacterInfo(_targetCharacter, true));
            CompleteQuest();
        }
    }
}
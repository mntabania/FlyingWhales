using System.Collections.Generic;
using JetBrains.Annotations;

namespace Quests.Special_Popups {
    [UsedImplicitly]
    public class DragonLeft : SpecialPopup {

        private Character _targetCharacter;

        public DragonLeft() : base("Dragon Left", QuestManager.Special_Popup.Dragon_Left) {
            isRepeatable = true;
        }
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>(
                new [] {
                    new DragonLeftCriteria().SetOnMeetAction(SetTargetCharacter), 
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
        private void SetTargetCharacter(QuestCriteria criteria) {
            if (criteria is DragonLeftCriteria dragonLeftCriteria) {
                _targetCharacter = dragonLeftCriteria.targetCharacter;
            }
        }
        public override void Activate() {
            StopCheckingCriteria();
            PlayerUI.Instance.ShowGeneralConfirmation("Dragon Left", 
                $"The dragon {_targetCharacter.name} left {_targetCharacter.currentRegion.name} and has not been seen again!");
            CompleteQuest();
        }
    }
}
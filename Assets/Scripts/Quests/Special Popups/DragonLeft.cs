using System.Collections.Generic;
using JetBrains.Annotations;

namespace Quests.Special_Popups {
    [UsedImplicitly]
    public class DragonLeft : SpecialPopup {

        private Character _targetCharacter;
        private Region _targetRegion;

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
                return WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona;
            }
            return false;
        }
        private void SetTargetCharacter(QuestCriteria criteria) {
            if (criteria is DragonLeftCriteria dragonLeftCriteria) {
                _targetCharacter = dragonLeftCriteria.targetCharacter;
                _targetRegion = _targetCharacter.homeRegion;
            }
        }
        public override void Activate() {
            StopCheckingCriteria();
            PlayerUI.Instance.ShowGeneralConfirmation("Dragon Left", 
                $"Looks like {UtilityScripts.Utilities.ColorizeAction(_targetCharacter.name)} grew bored of all the destruction and has left {_targetRegion.name}. It doesn't look like it's coming back.");
            CompleteQuest();
        }
    }
}
using System.Collections.Generic;
using JetBrains.Annotations;
using Tutorial;
namespace Quests.Special_Popups {
    [UsedImplicitly]
    public class ExcaliburObtained : SpecialPopup {

        private Character _targetCharacter;
        
        public ExcaliburObtained() : base("Excalibur Obtained", QuestManager.Special_Popup.Excalibur_Obtained) { }
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>(
                new [] {
                    new CharacterObtainedItem(TILE_OBJECT_TYPE.EXCALIBUR).SetOnMeetAction(SetTargetCharacter), 
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
            if (criteria is CharacterObtainedItem characterObtainedItem) {
                _targetCharacter = characterObtainedItem.characterThatObtainedItem;
            }
        }
        public override void Activate() {
            StopCheckingCriteria();
            PlayerUI.Instance.ShowGeneralConfirmation("Excalibur", 
                $"{_targetCharacter.name} has successfully pulled out the Excalibur from its stone and has " +
                $"become a powerful {UtilityScripts.Utilities.ColorizeAction("Hero")}! Watch out!", 
                onClickCenter: () => UIManager.Instance.ShowCharacterInfo(_targetCharacter, true));
            CompleteQuest();
        }
    }
}
using System.Collections.Generic;
using JetBrains.Annotations;
using Tutorial;
namespace Quests.Special_Popups {
    [UsedImplicitly]
    public class DisguisedSuccubus : SpecialPopup {

        private Character _disguiser;
        private Character _targetCharacter;
        
        public DisguisedSuccubus() : base("Disguised Succubus", QuestManager.Special_Popup.Disguised_Succubus) { }
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>(
                new [] {
                    new CharacterDisguised().SetOnMeetAction(SetTargetCharacter), 
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
            if (criteria is CharacterDisguised characterDisguised) {
                _disguiser = characterDisguised.disguiser;
                _targetCharacter = characterDisguised.targetCharacter;
            }
        }
        public override void Activate() {
            StopCheckingCriteria();
            PlayerUI.Instance.ShowGeneralConfirmation("Succubus", 
                $"The succubus {_disguiser.name} has disguised itself as {_targetCharacter.name} and is planning to do something naughty!", 
                onClickCenter: () => UIManager.Instance.ShowCharacterInfo(_disguiser, true));
            CompleteQuest();
        }
    }
}
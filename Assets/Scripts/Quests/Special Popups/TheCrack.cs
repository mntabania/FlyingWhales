using System.Collections.Generic;
using Inner_Maps;
namespace Quests.Special_Popups {
    public class TheCrack : SpecialPopup {

        private Character _evilVillager;
        
        public TheCrack() : base("The Crack", QuestManager.Special_Popup.The_Crack) { }

        #region Criteria
        protected override void ConstructCriteria() {
            for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                Character character = CharacterManager.Instance.allCharacters[i];
                if (character.isNormalCharacter && character.traitContainer.HasTrait("Evil")) {
                    _evilVillager = character;
                    break;
                }
            }
            _activationCriteria = new List<QuestCriteria>(
                new QuestCriteria[] {
                    new IsAtDay(1),
                    new IsAtTime(new[] {GameManager.Instance.GetTicksBasedOnHour(13)}), 
                }    
            );
        }
        protected override bool HasMetAllCriteria() {
            bool hasMetCriteria = base.HasMetAllCriteria();
            if (hasMetCriteria) {
                return WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa;
            }
            return false;
        }
        #endregion

        #region Activation
        public override void Activate() {
            PlayerUI.Instance.ShowGeneralConfirmation("The Crack", 
                $"This town is full of {UtilityScripts.Utilities.ColorizeAction("Blessed")} or {UtilityScripts.Utilities.ColorizeAction("Robust")} villagers but there is one that you may find more cooperative.", 
                onClickCenter: () => UIManager.Instance.ShowCharacterInfo(_evilVillager, true)
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
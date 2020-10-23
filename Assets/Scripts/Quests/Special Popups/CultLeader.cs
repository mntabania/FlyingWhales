using System.Collections.Generic;
using JetBrains.Annotations;

namespace Quests.Special_Popups {
    [UsedImplicitly]
    public class CultLeader : SpecialPopup {

        private Character _targetCharacter;

        public CultLeader() : base("Cult Leader", QuestManager.Special_Popup.Cult_Leader) { }
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>(
                new [] {
                    new InterruptFinishedCriteria(INTERRUPT.Become_Cult_Leader).SetOnMeetAction(SetTargetCharacter), 
                }    
            );
        }
        private void SetTargetCharacter(QuestCriteria criteria) {
            if (criteria is InterruptFinishedCriteria interruptFinishedCriteria) {
                _targetCharacter = interruptFinishedCriteria.character;
            }
        }
        public override void Activate() {
            StopCheckingCriteria();
            PlayerUI.Instance.ShowGeneralConfirmation("Cult Leader", 
                $"The devotion of Cultist {_targetCharacter.visuals.GetCharacterNameWithIconAndColor()} towards your cause has bore fruit! " +
                $"{UtilityScripts.Utilities.GetPronounString(_targetCharacter.gender, PRONOUN_TYPE.SUBJECTIVE, true)} " +
                $"is now a Cult Leader. You can instruct {UtilityScripts.Utilities.GetPronounString(_targetCharacter.gender, PRONOUN_TYPE.OBJECTIVE, true)} to create a new Demon Cult Faction!",
                onClickCenter: () => UIManager.Instance.ShowCharacterInfo(_targetCharacter, true));
            CompleteQuest();
        }
    }
}
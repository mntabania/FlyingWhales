using System.Collections;
using System.Collections.Generic;
using Quests;
using Quests.Steps;
using UnityEngine;
namespace Tutorial {
    public class CastMeteor : TutorialQuest {

        public override int priority => 1;
        
        public CastMeteor() : base("Cast Meteor", TutorialManager.Tutorial.Cast_Meteor) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new HasCompletedTutorialQuest(TutorialManager.Tutorial.Basic_Controls),
                new PlayerHasNotCastedForSeconds(15f),
                new PlayerHasNotCompletedTutorialInSeconds(15f)
            };
        }
        #endregion
        
        #region Overrides
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(new ShowSpellMenuStep()
                    .SetHoverOverAction(OnHoverShowSpell)
                    .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                ),
                new QuestStepCollection(new ChooseSpellStep(SPELL_TYPE.METEOR, "Click on Meteor")),
                new QuestStepCollection(new ExecuteSpellStep(SPELL_TYPE.METEOR, "Cast on any tile")
                    .SetCompleteAction(OnCompleteExecuteSpell))
            };
        }
        #endregion
        
        #region Step Helpers
        private void OnHoverShowSpell(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo("The spells tab can be accessed on the top left of your screen.",
                TutorialManager.Instance.spellsTabVideoClip, "Spells Tab", item.hoverPosition);
        }
        private void OnCompleteExecuteSpell() {
            TutorialManager.Instance.StartCoroutine(DelayedOnExecuteSpellPopup());
        }
        private IEnumerator DelayedOnExecuteSpellPopup() {
            yield return new WaitForSecondsRealtime(1.5f);
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Spells",
                "These are powerful magic that you may cast on a tile or an area of the map. " +
                "Spells do not have any Mana Cost but they have a limited number of Charges. " +
                "All Spells also have a short Cooldown.", TutorialManager.Instance.spellsVideoClip);
        }
        #endregion
    }
}
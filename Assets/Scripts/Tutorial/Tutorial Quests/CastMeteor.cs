using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Tutorial {
    public class CastMeteor : TutorialQuest {

        public override int priority => 1;
        
        public CastMeteor() : base("Cast Meteor", TutorialManager.Tutorial.Cast_Meteor) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<TutorialQuestCriteria>() {
                new HasCompletedTutorialQuest(TutorialManager.Tutorial.Basic_Controls),
                new PlayerHasNotCastedForSeconds(15f),
                new PlayerHasNotCompletedTutorialInSeconds(15f)
            };
        }
        #endregion
        
        #region Overrides
        protected override void ConstructSteps() {
            steps = new List<TutorialQuestStepCollection>() {
                new TutorialQuestStepCollection(new ShowSpellMenuStep()),
                new TutorialQuestStepCollection(new ChooseSpellStep(SPELL_TYPE.METEOR, "Click on Meteor")),
                new TutorialQuestStepCollection(new ExecuteSpellStep(SPELL_TYPE.METEOR, "Cast on any tile")
                    .SetCompleteAction(OnCompleteExecuteSpell))
            };
        }
        #endregion
        

        #region Step Helpers
        private void OnCompleteExecuteSpell() {
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Spells",
                "These are powerful magic that you may cast on a tile or an area of the map. " +
                "Spells do not have any Mana Cost but they have a limited number of Charges. " +
                "All Spells also have a short Cooldown.", TutorialManager.Instance.spellsVideoClip);
        }
        #endregion
    }
}
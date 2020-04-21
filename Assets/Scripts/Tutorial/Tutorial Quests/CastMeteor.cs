using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Tutorial {
    public class CastMeteor : TutorialQuest {

        private Coroutine availabilityTimer;
        public override int priority => 1;
        
        public CastMeteor() : base("Cast Meteor", TutorialManager.Tutorial.Cast_Meteor) { }

        #region Overrides
        public override void WaitForAvailability() {
            Messenger.AddListener<SpellData>(Signals.ON_EXECUTE_SPELL, OnSpellExecutedWhileWaitingForAvailability);
            availabilityTimer = TutorialManager.Instance.StartCoroutine(WaitForSeconds());
        }
        protected override void StopWaitingForAvailability() {
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_SPELL, OnSpellExecutedWhileWaitingForAvailability);
            if (availabilityTimer != null) {
                TutorialManager.Instance.StopCoroutine(availabilityTimer);
            }
            Messenger.AddListener<SpellData>(Signals.ON_EXECUTE_SPELL, OnSpellExecutedWhileInWaitList);
        }
        public override void Activate() {
            base.Activate();
            //stop listening for spell execution while in wait list.
            Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_SPELL, OnSpellExecutedWhileInWaitList);
        }
        protected override void ConstructSteps() {
            steps = new List<TutorialQuestStepCollection>() {
                new TutorialQuestStepCollection(new ShowSpellMenuStep()),
                new TutorialQuestStepCollection(new ChooseSpellStep(SPELL_TYPE.METEOR, "Click on Meteor")),
                new TutorialQuestStepCollection(new ExecuteSpellStep(SPELL_TYPE.METEOR, "Cast on any tile")
                    .SetCompleteAction(() => UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Spells", 
                        "These are powerful magic that you may cast on a tile or an area of the map. " +
                        "Spells do not have any Mana Cost but they have a limited number of Charges. " +
                        "All Spells also have a short Cooldown.", TutorialManager.Instance.spellPopUpPicture)))
            };
        }
        #endregion

        #region Availability Functions
        private void OnSpellExecutedWhileWaitingForAvailability(SpellData spellType) {
            //reset wait time
            TutorialManager.Instance.StopCoroutine(availabilityTimer);
            availabilityTimer = TutorialManager.Instance.StartCoroutine(WaitForSeconds());
        }
        private IEnumerator WaitForSeconds() {
            yield return new WaitForSecondsRealtime(10);
            MakeAvailable();
        }
        #endregion

        #region Wait List Functions
        private void OnSpellExecutedWhileInWaitList(SpellData spellData) {
            MakeUnavailable();
        }
        #endregion
        
    }
}
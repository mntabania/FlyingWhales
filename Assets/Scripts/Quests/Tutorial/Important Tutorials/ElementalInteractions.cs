using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Quests;
using Quests.Steps;
using UnityEngine;
namespace Tutorial {
    [UsedImplicitly]
    public class ElementalInteractions : ImportantTutorial {

        public override int priority => 1;
        
        public ElementalInteractions() : base("Elemental Interactions", TutorialManager.Tutorial.Elemental_Interactions) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new QuestStepActivated<StoreIntelStep>()
            };
        }
        #endregion
        
        #region Overrides
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new ShowSpellMenuStep()
                        .SetHoverOverAction(OnHoverShowSpell)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                        .SetOnTopmostActions(OnTopMostSpellTab, OnNoLongerTopMostSpellTab),
                    new ExecuteSpellStep(SPELL_TYPE.RAIN, "Cast Rain")
                        .SetCompleteAction(OnCompleteExecuteSpell)
                        .SetOnTopmostActions(OnTopMostRainSpell, OnNoLongerTopMostRainSpell),
                    new TriggerVaporFromWindStep()
                        .SetHoverOverAction(OnHoverWind)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                        .SetOnTopmostActions(OnTopMostWindBlast, OnNoLongerTopMostWindBlast),
                    new TriggerElectricChainStep("Electrocute Wet Floor")
                        .SetHoverOverAction(OnHoverElectric)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                ),
                new QuestStepCollection(
                    new ExecuteSpellStep(SPELL_TYPE.SPLASH_POISON, "Cast Splash Poison")
                        .SetOnTopmostActions(OnTopMostSplashPoison, OnNoLongerTopMostSplashPoison),
                     new TriggerPoisonExplosionStep("Burn Poisoned Floor")
                        .SetHoverOverAction(OnHoverFire)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                ),
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
        private void OnHoverElectric(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo("You can spread Electric damage and zap characters on Wet tiles. Try to cast Lightning on a Wet tile.", 
                item.hoverPosition, "Electric");
        }
        private void OnHoverWind(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo("You can produce Water Vapor from Wet tiles and Poison Clouds from Poisoned tiles. Try to cast Wind Blast on a Wet tile.", 
                item.hoverPosition, "Wind");
        }
        private void OnHoverFire(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo("You can trigger a powerful explosion by applying Fire damage to a Poisoned tile or object. Try to cast a Meteor on a Poisoned tile.", 
                item.hoverPosition, "Fire");
        }
        #endregion

        #region Spell Tab
        private void OnTopMostSpellTab() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Spells Tab");
        }
        private void OnNoLongerTopMostSpellTab() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Spells Tab");
        }
        #endregion

        #region Rain Spell
        private void OnTopMostRainSpell() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Rain");
        }
        private void OnNoLongerTopMostRainSpell() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Rain");
        }
        #endregion

        #region Splash Poison
        private void OnTopMostSplashPoison() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Splash Poison");
        }
        private void OnNoLongerTopMostSplashPoison() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Splash Poison");
        }
        #endregion

        #region Wind Blast
        private void OnTopMostWindBlast() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Wind Blast");
        }
        private void OnNoLongerTopMostWindBlast() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Wind Blast");
        }
        #endregion
    }
}
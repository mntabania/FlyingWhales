using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Quests;
using Quests.Steps;
using Settings;
namespace Tutorial {
    public class BiolabTutorial : BonusTutorial {

        public BiolabTutorial() : base("Biolab", TutorialManager.Tutorial.Biolab_Tutorial) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new StructureBuiltCriteria(STRUCTURE_TYPE.BIOLAB)
            };
        }
        #endregion
      
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                        new ClickOnStructureStep("Click on the Biolab", validityChecker: structure => structure.structureType == STRUCTURE_TYPE.BIOLAB)
                            .SetCompleteAction(OnCompleteClickBiolab),
                        new ButtonClickedStep("Upgrade", "Click on Upgrade button")
                            .SetOnTopmostActions(OnTopMostClickUpgrade, OnNoLongerTopMostClickUpgrade),
                        new ToggleTurnedOnStep("Lifespan_Tab", "Click on the Lifespan Tab")
                            .SetOnTopmostActions(OnTopMostClickLifespan, OnNoLongerTopMostLifespan)
                            .SetCompleteAction(OnCompleteClickLifespan),
                        new ButtonClickedStep("Plague Upgrade Btn", "Upgrade your Plague once")
                            .SetOnTopmostActions(OnTopMostClickUpgradeObjectsLifespan, OnNoLongerTopMostClickUpgradeObjectsLifespan)
                ),
                new QuestStepCollection(
                    new ToggleTurnedOnStep("Monsters Tab", "Click on the Monsters tab")
                        .SetOnTopmostActions(OnTopMostClickMonstersTab, OnNoLongerTopMostClickMonstersTab)
                        .SetCompleteAction(OnCompleteClickMonstersTab),
                    new ExecuteSpellStep(PLAYER_SKILL_TYPE.PLAGUED_RAT, "Summon a Plagued Rat")
                )
            };
        }
        protected override void MakeAvailable() {
            base.MakeAvailable();
            TutorialManager.Instance.ActivateTutorial(this);
        }
        public override void Activate() {
            base.Activate();
            Messenger.Broadcast(UISignals.UPDATE_BUILD_LIST);
        }

        #region Step Completion Actions
        private void OnCompleteClickBiolab() {
            PlayerUI.Instance.ShowGeneralConfirmation(
            "Biolab", $"The Biolab allows you to customize and improve your {UtilityScripts.Utilities.ColorizeAction("Plague")} spell. " +
                      $"Spread your Plague and let it wreak havoc to produce {UtilityScripts.Utilities.ColorizeAction("Chaotic Energy")} that you can then use for upgrade."
            );
        }
        private void OnCompleteClickLifespan() {
            PlayerUI.Instance.ShowGeneralConfirmation(
                "Upgrades", $"You gain a significant amount of {UtilityScripts.Utilities.ColorizeAction("Chaotic Energy")} each time your Plague spreads. " +
                            $"You gain a small amount when a Plagued victim dies. Some {UtilityScripts.Utilities.ColorizeAction("Symptoms")} may also give you Plague Points when they get triggered." +
                            $"\n\nUse these Points to upgrade various aspects of your Plague!" 
            );
        }
        private void OnCompleteClickMonstersTab() {
            PlayerUI.Instance.ShowGeneralConfirmation(
                "Plagued Rats", $"The Biolab produces {UtilityScripts.Utilities.ColorizeAction("Plagued Rats")} around once every three days - " +
                                $"up to 3 charges max. These pests can be used to spread the Plague as it visits Villages and transmit the disease to any {UtilityScripts.Utilities.ColorizeAction("food")} it consumes." 
            );
        }
        #endregion

        #region Highlighters
        private void OnTopMostClickUpgrade() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Upgrade");
        }
        private void OnNoLongerTopMostClickUpgrade() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Upgrade");
        }
        private void OnTopMostClickLifespan() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Lifespan_Tab");
        }
        private void OnNoLongerTopMostLifespan() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Lifespan_Tab");
        }
        private void OnTopMostClickUpgradeObjectsLifespan() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Plague Upgrade Btn");
        }
        private void OnNoLongerTopMostClickUpgradeObjectsLifespan() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Plague Upgrade Btn");
        }
        private void OnTopMostClickMonstersTab() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Monsters Tab");
        }
        private void OnNoLongerTopMostClickMonstersTab() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Monsters Tab");
        }
        #endregion
    }
}
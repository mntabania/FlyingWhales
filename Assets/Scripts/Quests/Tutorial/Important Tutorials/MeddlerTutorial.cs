using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Quests;
using Quests.Steps;

namespace Tutorial {
    public class MeddlerTutorial : BonusTutorial {

        public MeddlerTutorial() : base("Meddler", TutorialManager.Tutorial.Meddler_Tutorial) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new StructureBuiltCriteria(STRUCTURE_TYPE.MEDDLER)
            };
        }
        // protected override bool HasMetAllCriteria() {
        //     bool hasMetCriteria = base.HasMetAllCriteria();
        //     if (hasMetCriteria) {
        //         return PlayerSkillManager.Instance.GetPlayerSkillData(PLAYER_SKILL_TYPE.SEIZE_CHARACTER).isInUse;
        //     }
        //     return false;
        // }
        #endregion
      
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new PlayerActionContextMenuShown(target => target is Character character && character.isNormalCharacter, $"Right click on a Villager"),
                    new ButtonClickedStep("Scheme", "Click on the Scheme Action")
                        .SetOnTopmostActions(OnTopmostSchemeAction, OnNoLongerTopmostSchemeAction),
                    new ParameterlessSignalReceived(UISignals.SCHEME_UI_SHOWN, "Click any available Scheme")
                        .SetOnTopmostActions(OnTopmostAllSchemeActions, OnNoLongerTopmostAllSchemeActions),
                    new ParameterlessSignalReceived(UISignals.TEMPTATIONS_POPUP_SHOWN, "Click on the Tempt button")
                        .SetOnTopmostActions(OnTopmostTemptation, OnNoLongerTopmostTemptation),
                    new ParameterlessSignalReceived(UISignals.TEMPTATIONS_OFFERED, "Offer any Boon")
                        .SetOnTopmostActions(OnTopmostBoons, OnNoLongerTopmostBoons),
                    new ExecutedPlayerActionStep(PLAYER_SKILL_TYPE.SCHEME, "Click on the Confirm button")
                        .SetOnTopmostActions(OnTopmostConfirmBtn, OnNoLongerTopmostConfirmBtn)
                        .SetCompleteAction(OnCompleteScheme)
                )
            };
        }
        protected override void MakeAvailable() {
            base.MakeAvailable();
            PlayerUI.Instance.ShowGeneralConfirmation("Schemes", 
                $"The {UtilityScripts.Utilities.ColorizeAction("Meddler")} allows you to perform various {UtilityScripts.Utilities.ColorizeAction("Schemes")}! " +
                $"Provide {UtilityScripts.Utilities.ColorizeAction("Blackmail material")} or offer {UtilityScripts.Utilities.ColorizeAction("Boons")} to convince Villagers to do your bidding.", 
                onClickOK: () => TutorialManager.Instance.ActivateTutorial(this));
        }
        public override void Activate() {
            base.Activate();
            Messenger.Broadcast(UISignals.UPDATE_BUILD_LIST);
        }
        
        #region Step Completion Actions
        private void OnCompleteScheme() {
            PlayerUI.Instance.ShowGeneralConfirmation("Village Schemes",
                $"Aside from Schemes targeting Villagers, you can also launch {UtilityScripts.Utilities.ColorizeAction("Schemes")} on " +
                $"Villages to {UtilityScripts.Utilities.ColorizeAction("induce or stifle Migration")}. Right-click on a Village name or Town Center to perform these. These Schemes cost Mana.");
        }
        #endregion

        #region Scheme Action
        private void OnTopmostSchemeAction() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Scheme");   
        }
        private void OnNoLongerTopmostSchemeAction() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Scheme");
        }
        #endregion

        #region All Schemes
        private void OnTopmostAllSchemeActions() {
            for (int i = 0; i < PlayerSkillManager.Instance.allSchemes.Length; i++) {
                PLAYER_SKILL_TYPE scheme = PlayerSkillManager.Instance.allSchemes[i];
                SkillData spellData = PlayerSkillManager.Instance.GetSkillData(scheme);
                Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, spellData.name);    
            }
        }
        private void OnNoLongerTopmostAllSchemeActions() {
            for (int i = 0; i < PlayerSkillManager.Instance.allSchemes.Length; i++) {
                PLAYER_SKILL_TYPE scheme = PlayerSkillManager.Instance.allSchemes[i];
                SkillData spellData = PlayerSkillManager.Instance.GetSkillData(scheme);
                Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, spellData.name);    
            }
        }
        #endregion

        #region Temptation
        private void OnTopmostTemptation() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "TemptationButton");   
        }
        private void OnNoLongerTopmostTemptation() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "TemptationButton");
        }
        #endregion
        
        #region Boons
        private void OnTopmostBoons() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Dark Blessing");
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Empower");
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Cleanse Flaws");
        }
        private void OnNoLongerTopmostBoons() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Dark Blessing");
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Empower");
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Cleanse Flaws");
        }
        #endregion
        
        #region Confirm
        private void OnTopmostConfirmBtn() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "TemptConfirmBtn");   
        }
        private void OnNoLongerTopmostConfirmBtn() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "TemptConfirmBtn");
        }
        #endregion
    }
}
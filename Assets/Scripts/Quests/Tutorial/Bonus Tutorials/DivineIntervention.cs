using System.Collections.Generic;
using JetBrains.Annotations;
using Quests;
namespace Tutorial {
    [UsedImplicitly]
    public class DivineIntervention : BonusTutorial {
        public DivineIntervention() : base("Divine Intervention", TutorialManager.Tutorial.Divine_Intervention) { }
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>(
                new [] {
                    new ReactionQuestActivated<Quests.DivineIntervention>(), 
                }    
            );
        }
        protected override void MakeAvailable() {
            base.MakeAvailable();
            TutorialManager.Instance.ActivateTutorialButDoNotShow(this);
        }
        public override void Activate() {
            StopCheckingCriteria();
            Quests.DivineIntervention divineIntervention = QuestManager.Instance.GetActiveQuest<Quests.DivineIntervention>();
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Divine Intervention!", 
                "Your Threat Level has reached maximum! " +
                $"Because there aren't enough combatant Villagers to mount a counterattack, " +
                $"The Divine has sent down {divineIntervention.angels.Count.ToString()} Angels to stop you from further transgressions. " +
                "Eliminate all of them before they destroy your demonic structures.", 
                TutorialManager.Instance.divineInterventionPicture);
            CompleteQuest();
        }
    }
}
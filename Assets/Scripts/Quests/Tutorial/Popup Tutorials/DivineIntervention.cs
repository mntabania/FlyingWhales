using System.Collections.Generic;
using JetBrains.Annotations;
using Quests;
namespace Tutorial {
    [UsedImplicitly]
    public class DivineIntervention : PopupTutorial {
        public DivineIntervention() : base("Divine Intervention", TutorialManager.Tutorial.Divine_Intervention) { }
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>(
                new [] {
                    new ReactionQuestActivated<Quests.DivineIntervention>(), 
                }    
            );
        }
        public override void Activate() {
            base.Activate();
            Quests.DivineIntervention divineIntervention = QuestManager.Instance.GetActiveQuest<Quests.DivineIntervention>();
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Divine Intervention!", 
                "Your Threat Level has reached maximum! " +
                $"Because there aren't enough combatant {UtilityScripts.Utilities.VillagerIcon()}Villagers to mount a counterattack, " +
                $"The Divine has sent down {divineIntervention.angels.Count.ToString()} Angels to stop you from further transgressions. " +
                "Eliminate all of them before they destroy your demonic structures.", 
                TutorialManager.Instance.divineInterventionPicture);
            CompleteQuest();
        }
    }
}
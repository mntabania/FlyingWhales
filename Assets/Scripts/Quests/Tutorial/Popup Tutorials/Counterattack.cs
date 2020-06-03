using System.Collections.Generic;
using JetBrains.Annotations;
using Quests;
namespace Tutorial {
    [UsedImplicitly]
    public class Counterattack : PopupTutorial {
        public Counterattack() : base("Counterattack", TutorialManager.Tutorial.Counterattack) { }
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>(
                new [] {
                    new ReactionQuestActivated<Quests.Counterattack>(), 
                }    
            );
        }
        public override void Activate() {
            base.Activate();
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Counterattack!", 
                "Your Threat Level has reached maximum and has triggered a counterattack. " +
                "Several hostile villagers are on their way to clear out your corruption!\n\n" +
                $"There are several ways to distract a {UtilityScripts.Utilities.VillagerIcon()}Villager from its task. " +
                "An Exhausted, Starving or Sulking villager will drop what it's doing to recover. " +
                $"Status Effects that temporarily stop a {UtilityScripts.Utilities.VillagerIcon()}Villager from moving (ex: Zapped) " +
                $"will also make it forget its current action. Killing a {UtilityScripts.Utilities.VillagerIcon()}Villager, of course, is a permanent distraction.", 
                TutorialManager.Instance.counterattackPicture);
            CompleteQuest();
        }
    }
}
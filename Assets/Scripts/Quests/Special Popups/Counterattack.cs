using System.Collections.Generic;
using JetBrains.Annotations;
using Tutorial;
namespace Quests.Special_Popups {
    [UsedImplicitly]
    public class Counterattack : SpecialPopup {
        public Counterattack() : base("Counterattack", QuestManager.Special_Popup.Counterattack) { }
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>(
                new [] {
                    new ReactionQuestActivated<Quests.Counterattack>(), 
                }    
            );
        }
        public override void Activate() {
            StopCheckingCriteria();
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Counterattack!", 
                "Your Threat Level has reached maximum and has triggered a counterattack. " +
                "Several hostile villagers are on their way to clear out your corruption!\n\n" +
                $"There are several ways to distract a Villager from its task. " +
                "An Exhausted, Starving or Sulking villager will drop what it's doing to recover. " +
                $"Status Effects that temporarily stop a Villager from moving (ex: Zapped) " +
                $"will also make it forget its current action. Killing a Villager, of course, is a permanent distraction.", 
                TutorialManager.Instance.counterattackPicture);
            CompleteQuest();
        }
    }
}
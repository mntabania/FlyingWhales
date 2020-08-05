using System.Collections.Generic;
using JetBrains.Annotations;
using Tutorial;
namespace Quests.Special_Popups {
    [UsedImplicitly]
    public class Threat : SpecialPopup {
        public Threat() : base("Threat", QuestManager.Special_Popup.Threat) { }
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>(
                new [] {
                    new ThreatIncreased(), 
                }    
            );
        }
        public override void Activate() {
            StopCheckingCriteria();
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Threat", 
                $"Careful, your {UtilityScripts.Utilities.ColorizeAction("Threat Level")} has increased! Many of the things you do generate only a " +
                $"small amount of Threat but it goes down very slowly every hour. " +
                $"Something really bad will happen once your Threat Level has reached {UtilityScripts.Utilities.ColorizeAction(ThreatComponent.MAX_THREAT.ToString())}. Be ready!", 
                TutorialManager.Instance.threatPicture);
            CompleteQuest();
        }
    }
}
using System.Collections.Generic;
using JetBrains.Annotations;
using Tutorial;
namespace Quests.Special_Popups {
    [UsedImplicitly]
    public class LowMana : SpecialPopup {
        public LowMana() : base("Low Mana", QuestManager.Special_Popup.Low_Mana) { }
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>(
                new [] {
                    new ManaIsLessThanOrEqual(45), 
                }    
            );
        }
        public override void Activate() {
            StopCheckingCriteria();
            PlayerUI.Instance.ShowGeneralConfirmation("Low Mana", 
                $"You are low on {UtilityScripts.Utilities.ColorizeAction("Mana")}! Though you gain a small amount of Mana per hour when you have less than 45 Mana, " +
                $"the primary way of gaining more is by finding {UtilityScripts.Utilities.ColorizeAction("Mana Orbs")} produced by Villagers through criminal or sad acts.");
            CompleteQuest();
        }
    }
}
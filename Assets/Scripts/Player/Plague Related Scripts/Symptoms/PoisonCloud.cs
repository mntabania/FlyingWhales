using Traits;
using UnityEngine;
using UtilityScripts;

namespace Plague.Symptom {
    public class PoisonCloud : PlagueSymptom {
        
        public override PLAGUE_SYMPTOM symptomType => PLAGUE_SYMPTOM.Poison_Cloud;

        protected override void ActivateSymptom(Character p_character) {
            int randomStacks = GameUtilities.RandomBetweenTwoNumbers(2, 5);
            //Is this dependency? Is this bad practice? If it is, we need to find a better way
            Inner_Maps.InnerMapManager.Instance.SpawnPoisonCloud(p_character.gridTileLocation, randomStacks);
            if (PlayerManager.Instance.player.plagueComponent.CanGainPlaguePoints()) {
                PlayerManager.Instance.player.plagueComponent.GainPlaguePointFromCharacter(1, p_character);    
            }
            Debug.Log("Activated Poison Cloud Symptom");
        }
        public override void PerTickWhileStationaryOrUnoccupied(Character p_character) {
            if (GameUtilities.RollChance(1.5f)) {
                ActivateSymptomOn(p_character);
            }
        }
    }
}
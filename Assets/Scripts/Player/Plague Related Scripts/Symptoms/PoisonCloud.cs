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
            Debug.Log("Activated Poison Cloud Symptom");
        }
        public override void PerTickMovement(Character p_character) {
            if (GameUtilities.RollChance(1)) {
                ActivateSymptom(p_character);
            }
        }
    }
}
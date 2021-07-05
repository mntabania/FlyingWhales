using Traits;
using UnityEngine;
using UtilityScripts;

namespace Plague.Symptom {
    public class Paralysis : PlagueSymptom {
        
        public override PLAGUE_SYMPTOM symptomType => PLAGUE_SYMPTOM.Paralysis;

        protected override void ActivateSymptom(Character p_character) {
            if (!p_character.traitContainer.HasTrait("Paralyzed")) {
                p_character.traitContainer.AddTrait(p_character, "Paralyzed");
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Plague", "plague_paralysis", null, LOG_TAG.Life_Changes);
                log.AddToFillers(p_character, p_character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddLogToDatabase(true);
            }
#if DEBUG_LOG
            Debug.Log("Activated Paralysis Symptom");
#endif
        }
        public override void HourStarted (Character p_character, int p_numOfHoursPassed) {
            if (p_numOfHoursPassed == 49) {
                if (GameUtilities.RollChance(25)) {
                    ActivateSymptomOn(p_character);
                }
            }
        }
    }
}
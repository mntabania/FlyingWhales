using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
namespace Traits {
    public class Nocturnal : Trait {
        public override bool isSingleton => true;

        public Nocturnal() {
            name = "Nocturnal";
            description = "Awake at night and asleep during the day.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            base.OnAddTrait(sourcePOI);
            if (sourcePOI is Character) {
                Character character = sourcePOI as Character;
                // character.needsComponent.SetForcedFullnessRecoveryTimeInWords(TIME_IN_WORDS.EARLY_NIGHT);
                // character.needsComponent.SetForcedTirednessRecoveryTimeInWords(TIME_IN_WORDS.MORNING);
                // character.needsComponent.SetForcedHappinessRecoveryTimeChoices(TIME_IN_WORDS.LATE_NIGHT, TIME_IN_WORDS.EARLY_NIGHT, TIME_IN_WORDS.MORNING);
                // character.needsComponent.SetFullnessForcedTick();
                // character.needsComponent.SetTirednessForcedTick();
                // character.needsComponent.SetHappinessForcedTick();
                character.dailyScheduleComponent.OnCharacterGainedNocturnal(character);
            }
        }
        public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
            base.OnRemoveTrait(sourcePOI, removedBy);
            if (sourcePOI is Character) {
                Character character = sourcePOI as Character;
                // character.needsComponent.SetForcedFullnessRecoveryTimeInWords(TIME_IN_WORDS.LUNCH_TIME);
                // character.needsComponent.SetForcedTirednessRecoveryTimeInWords(TIME_IN_WORDS.LATE_NIGHT);
                // character.needsComponent.SetForcedHappinessRecoveryTimeChoices(TIME_IN_WORDS.MORNING, TIME_IN_WORDS.AFTERNOON, TIME_IN_WORDS.EARLY_NIGHT);
                // character.needsComponent.SetFullnessForcedTick();
                // character.needsComponent.SetTirednessForcedTick();
                // character.needsComponent.SetHappinessForcedTick();
                character.dailyScheduleComponent.OnCharacterLostNocturnal(character);
            }
        }
        #endregion
    }
}


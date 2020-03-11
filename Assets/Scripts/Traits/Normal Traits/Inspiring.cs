using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Inspiring : Trait {
        public Inspiring() {
            name = "Inspiring";
            description = "Inspring characters make people around them feel happier.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
        }

        #region Overrides
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is Character) {
                Character targetCharacter = targetPOI as Character;
                string debugLog = $"{characterThatWillDoJob.name} saw {targetPOI.name} and has {name}";
                debugLog += "\n-20% chance to trigger Inspired interrupt if Target is part of Faction or NPCSettlement";
                if (characterThatWillDoJob.faction == targetCharacter.faction || characterThatWillDoJob.homeSettlement == targetCharacter.homeSettlement) {
                    debugLog += "\n-Target is part of Faction or NPCSettlement";
                    int chance = UnityEngine.Random.Range(0, 100);
                    debugLog += $"\n-Roll: {chance}";
                    if (chance < 8) {
                        debugLog += "\n-Triggered Inspired interrupt";
                        characterThatWillDoJob.logComponent.PrintLogIfActive(debugLog);
                        targetCharacter.interruptComponent.TriggerInterrupt(INTERRUPT.Inspired, characterThatWillDoJob);
                        return true;
                    }
                } else {
                    debugLog += "\n-Target is part of Faction or NPCSettlement";
                }
                characterThatWillDoJob.logComponent.PrintLogIfActive(debugLog);
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        #endregion
    }
}
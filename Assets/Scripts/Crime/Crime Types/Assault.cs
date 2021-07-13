using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class Assault : CrimeType {
        public Assault() : base(CRIME_TYPE.Assault) { }

        #region Overrides
        public override CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target) {
            //Witness of an assault should not accuse actor if the actor attacked the target because of a committed crime and the witness is aware of any active crime committed by the target
            //This means that since the witness is aware that the actor is only attacking the target because of a crime, he should not accuse the actor of assault
            //https://trello.com/c/r7EoP912/5005-crime-data-changes
            if (target is Character targetCharacter) {
                bool isWitnessAlsoAWitnessOfTargetCrime = targetCharacter.crimeComponent.IsAnActiveCrimeWitnessedBy(witness); //If the witness of the actor's assault also witnessed a crime committed by target
                bool didActorAttackTargetBecauseOfCrime = false;
                CombatData actorToTargetCombatData = actor.combatComponent.GetCombatData(targetCharacter);
                if (actorToTargetCombatData != null && actorToTargetCombatData.attackBecauseOfCrime) {
                    didActorAttackTargetBecauseOfCrime = true;
                }
                if (isWitnessAlsoAWitnessOfTargetCrime && didActorAttackTargetBecauseOfCrime) {
                    return CRIME_SEVERITY.None;
                }
            }
            return base.GetCrimeSeverity(witness, actor, target);
        }
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " is violent";
        }
        #endregion
    }
}
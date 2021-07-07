using Crime_System;
using UnityEngine.Assertions;
namespace Interrupts {
    public class BecomeVampireLord : Interrupt {
        public BecomeVampireLord() : base(INTERRUPT.Become_Vampire_Lord) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Major};
            shouldShowNotif = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            interruptHolder.actor.structureComponent.TryUnassignFromCurrentWorkStructureOnClassChange(interruptHolder.actor, "Vampire Lord");
            interruptHolder.actor.classComponent.AssignClass("Vampire Lord");
            Traits.Vampire vampire = interruptHolder.actor.traitContainer.GetTraitOrStatus<Traits.Vampire>("Vampire");
            Assert.IsNotNull(vampire, $"{interruptHolder.actor.name}");
            vampire.SetHasBecomeVampireLord(true);
            if (interruptHolder.actor.faction != null && interruptHolder.actor.faction.GetCrimeSeverity(interruptHolder.actor, interruptHolder.actor, CRIME_TYPE.Vampire) != CRIME_SEVERITY.None) {
                interruptHolder.actor.MigrateHomeStructureTo(null);
                if (interruptHolder.actor.faction.isMajorFaction) {
                    //Banned vampire lord from its previous faction because when the vampire lord leaves it's faction after this
                    //it has a chance to rejoin it because of FactionRelatedBehaviour if it is not banned.
                    //only banned it from major factions because we don't want to ban the character from the Vagrant faction, Undead faction or any other minor factions.
                    interruptHolder.actor.faction.AddBannedCharacter(interruptHolder.actor);    
                }
                interruptHolder.actor.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, interruptHolder.actor, "left_faction_vampire");
            }
            return true;
        }
        #endregion
    }
}

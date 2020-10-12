using Crime_System;

namespace Interrupts {
    public class BecomeVampireLord : Interrupt {
        public BecomeVampireLord() : base(INTERRUPT.Become_Vampire_Lord) {
            duration = 0;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            interruptHolder.actor.AssignClass("Vampire Lord");
            if (interruptHolder.actor.faction != null && interruptHolder.actor.faction.GetCrimeSeverity(null, interruptHolder.actor, interruptHolder.actor, CRIME_TYPE.Vampire) != CRIME_SEVERITY.None) {
                interruptHolder.actor.MigrateHomeStructureTo(null);
                interruptHolder.actor.faction.AddBannedCharacter(interruptHolder.actor);
                interruptHolder.actor.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, interruptHolder.actor, "left_faction_vampire");
            }
            return base.ExecuteInterruptEndEffect(interruptHolder);
        }
        #endregion
    }
}

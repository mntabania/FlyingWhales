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
            if (interruptHolder.actor.faction != null && 
                interruptHolder.actor.faction.GetCrimeSeverity(null, interruptHolder.actor, interruptHolder.actor, CRIME_TYPE.Vampire) != CRIME_SEVERITY.None) {
                interruptHolder.actor.MigrateHomeStructureTo(null);
                interruptHolder.actor.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Faction, interruptHolder.actor);
            }
            return base.ExecuteInterruptEndEffect(interruptHolder);
        }
        #endregion
    }
}

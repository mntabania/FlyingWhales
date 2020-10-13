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
                if (interruptHolder.actor.faction.isMajorFaction) {
                    //Banned vampire lord from its previous faction because when the vampire lord leaves it's faction after this
                    //it has a chance to rejoin it because of FactionRelatedBehaviour if it is not banned.
                    //only banned it from major factions because we don't want to ban the character from the Vagrant faction, Undead faction or any other minor factions.
                    interruptHolder.actor.faction.AddBannedCharacter(interruptHolder.actor);    
                }
                interruptHolder.actor.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, interruptHolder.actor, "left_faction_vampire");
            }
            return base.ExecuteInterruptEndEffect(interruptHolder);
        }
        #endregion
    }
}

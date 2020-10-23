namespace Interrupts {
    public class BecomeCultLeader : Interrupt {
        public BecomeCultLeader() : base(INTERRUPT.Become_Cult_Leader) {
            duration = 0;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            interruptHolder.actor.AssignClass("Cult Leader");
            return base.ExecuteInterruptEndEffect(interruptHolder);
        }
        #endregion
    }
}
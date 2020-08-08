using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class Wary : Interrupt {
        public Wary() : base(INTERRUPT.Wary) {
            interruptIconString = GoapActionStateDB.Shock_Icon;
            duration = 2;
            doesStopCurrentAction = true;
            shouldAddLogs = false;
            shouldShowNotif = true;
        }
    }
}
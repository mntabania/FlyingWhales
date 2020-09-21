using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Traits;
using Tutorial;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Interrupts {
    public class BeingBrainwashed : Interrupt {
        
        public BeingBrainwashed() : base(INTERRUPT.Being_Brainwashed) {
            duration = 24;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.Sad_Icon;
            logTags = new[] {LOG_TAG.Player, LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            if (interruptHolder.actor.gridTileLocation.structure.IsTilePartOfARoom(interruptHolder.actor.gridTileLocation, out var room) && room is DefilerRoom defilerRoom) {
                Log log;
                if (defilerRoom.WasBrainwashSuccessful(interruptHolder.actor)) {
                    //successfully converted
                    interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Cultist");
                    log = new Log(GameManager.Instance.Today(), "Interrupt", "Being Brainwashed", "converted", null, LOG_TAG.Life_Changes, LOG_TAG.Player);
                } else {
                    interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Unconscious");
                    log = new Log(GameManager.Instance.Today(), "Interrupt", "Being Brainwashed", "not_converted", null, LOG_TAG.Life_Changes, LOG_TAG.Player);
                }
                log.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
            }
            return true;
        }
        #endregion
    }
}
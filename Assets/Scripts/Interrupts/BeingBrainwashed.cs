using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Object_Pools;
using Traits;
using Tutorial;
using UnityEngine.Assertions;
using UtilityScripts;
using Prison = Tutorial.Prison;
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
            if (interruptHolder.actor.gridTileLocation.structure.IsTilePartOfARoom(interruptHolder.actor.gridTileLocation, out var room) && room is PrisonCell defilerRoom) {
                Log log;
                if (defilerRoom.WasBrainwashSuccessful(interruptHolder.actor)) {
                    //successfully converted
                    LocationStructure currentStructure = interruptHolder.actor.currentStructure;
                    if (currentStructure != null && currentStructure is TortureChambers) {
                        interruptHolder.actor.movementComponent.LetGo();
                    }
                    interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Cultist");
                    interruptHolder.actor.ResetNeeds();
                    log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Being Brainwashed", "converted", null, LOG_TAG.Major);
                } else {
                    interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Unconscious");
                    log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Being Brainwashed", "not_converted", null, logTags);
                }
                log.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
            }
            return true;
        }
        #endregion
    }
}
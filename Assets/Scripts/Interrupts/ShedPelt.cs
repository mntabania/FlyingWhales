using Inner_Maps;
using UnityEngine;

namespace Interrupts {
    public class ShedPelt : Interrupt {
        public ShedPelt() : base(INTERRUPT.Shed_Pelt) {
            duration = 3;
            doesStopCurrentAction = true;
            interruptIconString = GoapActionStateDB.Work_Icon;
            isIntel = true;
            logTags = new[] {LOG_TAG.Misc};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            LocationGridTile tileLocation = interruptHolder.actor.gridTileLocation;
            TileObject pelt = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.WEREWOLF_PELT);
            tileLocation.structure.AddPOI(pelt, tileLocation);
            
            overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "effect", null, logTags);
            overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideEffectLog.AddToFillers(pelt, pelt.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            return true;
        }
        #endregion
    }
}
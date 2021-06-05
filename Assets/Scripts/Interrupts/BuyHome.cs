using Inner_Maps.Location_Structures;
using Object_Pools;
using UnityEngine.Assertions;
namespace Interrupts {
    public class BuyHome : Interrupt {
        
        public BuyHome() : base(INTERRUPT.Buy_Home) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Life_Changes};
        }
        
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character actor = interruptHolder.actor;
            LocationStructure currentHomeStructure = actor.homeStructure;
            GenericTileObject genericTileObject = interruptHolder.target as GenericTileObject;
            Assert.IsNotNull(genericTileObject, $"Target of buy home interrupt of {actor?.name} is not a GenericTileObject! Target is {interruptHolder.target?.name}");
            Assert.IsFalse(genericTileObject.gridTileLocation.structure is Wilderness, $"Set home interrupt of {actor.name} will set home to wilderness! Provided tile object is {genericTileObject} at {genericTileObject.gridTileLocation}");
            actor.MigrateHomeStructureTo(genericTileObject.gridTileLocation.structure);
            
            //Subtract dwelling cost from actors money
            actor.moneyComponent.AdjustCoins(-50);
            
            //TODO: Also migrate lover.
            
            //Do not log if the new home is same as previous/current home so that it will not spam in the log tab
            //This is also the fix for this: https://trello.com/c/Ecjx7j55/3762-live-v03502-cultist-found-new-home-loop
            if (actor.homeStructure != null && actor.homeStructure != actor.previousCharacterDataComponent.previousHomeStructure && actor.homeStructure != currentHomeStructure) {
                if (overrideEffectLog != null) { LogPool.Release(overrideEffectLog); }
                overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "buy_new_home_structure", null, logTags);
                overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                overrideEffectLog.AddToFillers(actor.homeStructure, actor.homeStructure.name, LOG_IDENTIFIER.LANDMARK_1);
            }
            return true;
        }
    }
}
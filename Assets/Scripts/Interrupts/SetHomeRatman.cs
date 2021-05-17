using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Logs;
using UnityEngine.Assertions;
using Inner_Maps;
using Object_Pools;
using UtilityScripts;

namespace Interrupts {
    public class SetHomeRatman : Interrupt {
        public SetHomeRatman() : base(INTERRUPT.Set_Home_Ratman) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character actor = interruptHolder.actor;
            SetNewHomeStructure(actor);
            if(actor.homeStructure != null) {
                //if (overrideEffectLog != null) { LogPool.Release(overrideEffectLog); }
                overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "set_new_home", null, logTags);
                overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                overrideEffectLog.AddToFillers(null, actor.homeStructure.name, LOG_IDENTIFIER.STRING_1);
            }
            return true;
        }
        #endregion

        private void SetNewHomeStructure(Character actor) {
#if DEBUG_LOG
            string log = "Setting new home for ratman " + actor.name;
#endif
            Region currentRegion = actor.currentRegion;
            if(currentRegion != null) {
                List<BaseSettlement> settlementChoices = ObjectPoolManager.Instance.CreateNewSettlementList();
                //List<Region> adjacentRegions = currentRegion.neighbours;

                PopulateSettlementChoices(settlementChoices, currentRegion);

                //for (int i = 0; i < adjacentRegions.Count; i++) {
                //    Region region = adjacentRegions[i];
                //    PopulateSettlementChoices(settlementChoices, region);
                //}
                if(settlementChoices.Count > 0) {
                    BaseSettlement chosenSettlement = CollectionUtilities.GetRandomElement(settlementChoices);
                    actor.ClearTerritoryAndMigrateHomeSettlementTo(chosenSettlement);
                }
            } else {
#if DEBUG_LOG
                log += "\n-Character has no current region";
#endif
            }
            //If all else fails, check if character has home structure and if it is already destroyed, set it to null
            if (actor.homeStructure != null && actor.homeStructure.hasBeenDestroyed) {
                actor.MigrateHomeStructureTo(null, affectSettlement: false);
            }
#if DEBUG_LOG
            actor.logComponent.PrintLogIfActive(log);
#endif
        }

        private void PopulateSettlementChoices(List<BaseSettlement> settlementChoices, Region region) {
            for (int i = 0; i < region.allStructures.Count; i++) {
                LocationStructure structure = region.allStructures[i];
                if (structure.settlementLocation != null && structure.settlementLocation.owner == null && structure.settlementLocation.residents.Count <= 0
                    && (structure.structureType == STRUCTURE_TYPE.CAVE || structure.structureType == STRUCTURE_TYPE.MONSTER_LAIR || structure.settlementLocation.locationType == LOCATION_TYPE.VILLAGE)) {
                    if (!settlementChoices.Contains(structure.settlementLocation)) {
                        settlementChoices.Add(structure.settlementLocation);
                    }
                }
            }
        }
    }
}
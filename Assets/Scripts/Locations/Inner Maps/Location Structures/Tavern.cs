using System;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

namespace Inner_Maps.Location_Structures {
    public class Tavern : ManMadeStructure {
        
        // public override Vector2 selectableSize { get; }
        // public override Vector3 worldPosition => structureObj.transform.position;

        public Tavern(Region location) : base(STRUCTURE_TYPE.TAVERN, location) {
            SetMaxHPAndReset(8000);
        }
        public Tavern(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(8000);
        }

        #region Overrides
        protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob) {
            producedJob = null;
            string log = string.Empty;

#if DEBUG_LOG
            log += $"\n{p_worker.name} will do work for {name}";
            log += $"\n15% chance to check food capacity";
#endif
            if (GameUtilities.RollChance(15, ref log)) {
                List<TileObject> tables = GetTileObjectsOfType(TILE_OBJECT_TYPE.TABLE);
                Table tableWithNoFood = null;
                if (tables != null && tables.Count > 0) {
                    for (int i = 0; i < tables.Count; i++) {
                        Table t = tables[i] as Table;
                        if (t != null && t.food <= 0) {
                            tableWithNoFood = t;
                            break;
                        }
                    }
                }
                if (tableWithNoFood != null) {
#if DEBUG_LOG
                    log += $"\nTable with No Food: {tableWithNoFood.nameWithID}";
#endif
                    p_worker.jobComponent.TryCreateBuyFoodForTavernTable(tableWithNoFood, out producedJob);
                    if (producedJob != null) {
#if DEBUG_LOG
                        log += $"\nCreated Buy Food For Tavern Job";
                        p_worker.logComponent.PrintLogIfActive(log);
#endif
                        return;
                    } else {
#if DEBUG_LOG
                        log += $"\nDid not create Buy Food For Tavern Job";
#endif
                    }
                } else {
#if DEBUG_LOG
                    log += $"\nDid not find table with No Food";
#endif
                }

            }
#if DEBUG_LOG
            log += $"\n100% chance to Clean";
#endif
            int chanceToClean = 100;
            if (p_worker.traitContainer.HasTrait("Lazy")) {
                chanceToClean = 6;
#if DEBUG_LOG
                log += $"\nWorker is Lazy, chance to Clean is now 6%";
#endif
            }
            if (GameUtilities.RollChance(chanceToClean, ref log)) {
                p_worker.jobComponent.TryCreateCleanItemJob(this, out producedJob);
                if (producedJob != null) {
#if DEBUG_LOG
                    log += $"\nCreated Clean Job: {producedJob.ToString()} with target {producedJob.poiTarget?.nameWithID}";
#endif
                }
            }
#if DEBUG_LOG
            p_worker.logComponent.PrintLogIfActive(log);
#endif
        }
        #endregion

        #region Worker
        public override bool CanHireAWorker() {
            return true;
        }
        #endregion
    }
}
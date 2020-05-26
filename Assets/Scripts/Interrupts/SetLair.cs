using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

namespace Interrupts {
    public class SetLair : Interrupt {
        public SetLair() : base(INTERRUPT.Set_Lair) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            string log = "Setting new lair for " + actor.name;
            log += "\n-Setting lair in current region";
            Region currentRegion = actor.currentRegion;
            LocationStructure lair = GetLairInRegion(currentRegion);
            if(lair != null) {
                actor.necromancerTrait.SetLairStructure(lair);
            } else {
                log += "\n-Setting lair in all regions";
                actor.necromancerTrait.SetLairStructure(GetLairInAllRegions());
            }
            if(actor.necromancerTrait.lairStructure != null) {
                log += "\n-Lair is set: " + actor.necromancerTrait.lairStructure.GetNameRelativeTo(actor) + " in " + actor.necromancerTrait.lairStructure.location.name;
                log += "\n-Migrating home to lair";
                actor.MigrateHomeStructureTo(actor.necromancerTrait.lairStructure);
                actor.ClearTerritory();
                actor.logComponent.PrintLogIfActive(log);
                return true;
            }
            actor.logComponent.PrintLogIfActive(log);
            return false;
        }
        #endregion

        private LocationStructure GetLairInAllRegions() {
            LocationStructure chosenLair = null;
            for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
                Region region = GridMap.Instance.allRegions[i];
                chosenLair = GetLairInRegion(region);
                if(chosenLair != null) {
                    return chosenLair;
                }
            }
            return chosenLair;
        }
        private LocationStructure GetLairInRegion(Region region) {
            //TODO: Add Temple to the pool of lairs to be chosen
            LocationStructure chosenLair = region.GetFirstUnoccupiedStructureOfType(STRUCTURE_TYPE.MAGE_TOWER);
            if (chosenLair == null) {
                chosenLair = region.GetFirstUnoccupiedStructureOfType(STRUCTURE_TYPE.MONSTER_LAIR);
            }
            return null;
        }
    }
}
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
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            string log = "Setting new lair for " + interruptHolder.actor.name;
            log += "\n-Setting lair in current region";
            Region currentRegion = interruptHolder.actor.currentRegion;
            LocationStructure lair = GetLairInRegion(currentRegion);
            if(lair != null) {
                interruptHolder.actor.necromancerTrait.SetLairStructure(lair);
            } else {
                log += "\n-Setting lair in all regions";
                interruptHolder.actor.necromancerTrait.SetLairStructure(GetLairInAllRegions());
            }
            if(interruptHolder.actor.necromancerTrait.lairStructure != null) {
                log += "\n-Lair is set: " + interruptHolder.actor.necromancerTrait.lairStructure.GetNameRelativeTo(interruptHolder.actor) + " in " + interruptHolder.actor.necromancerTrait.lairStructure.location.name;
                log += "\n-Migrating home to lair";
                interruptHolder.actor.MigrateHomeStructureTo(interruptHolder.actor.necromancerTrait.lairStructure);
                interruptHolder.actor.ClearTerritory();
                interruptHolder.actor.logComponent.PrintLogIfActive(log);
                return true;
            }
            interruptHolder.actor.logComponent.PrintLogIfActive(log);
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
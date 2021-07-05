using System;
using System.Collections;
using System.Collections.Generic;
using Locations.Area_Features;
using Logs;
using Object_Pools;
using UnityEngine;

namespace Interrupts {
    public class LossOfControl : Interrupt {
        public LossOfControl() : base(INTERRUPT.Loss_Of_Control) {
            duration = 20;
            doesDropCurrentJob = true;
            doesStopCurrentAction = true;
            isSimulateneous = false;
            interruptIconString = GoapActionStateDB.Shock_Icon;
            logTags = new[] {LOG_TAG.Needs, LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            //if (overrideEffectLog != null) { LogPool.Release(overrideEffectLog); }
            overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Mental Break", "break", null, logTags);
            overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideEffectLog.AddToFillers(null, "Loss of Control", LOG_IDENTIFIER.STRING_1);

            Area area = interruptHolder.actor.areaLocation;
            if (interruptHolder.actor.characterClass.className.Equals("Druid")) {
                //Electric storm
                if (area.spellsComponent.hasElectricStorm) {
                    //reset electric storm
                    area.spellsComponent.ResetElectricStormDuration();
                } else {
                    area.spellsComponent.SetHasElectricStorm(true, false);
                }
            } else if (interruptHolder.actor.characterClass.className.Equals("Shaman")) {
                //Poison Bloom
                PoisonBloomFeature poisonBloomFeature = area.featureComponent.GetFeature<PoisonBloomFeature>();
                if (poisonBloomFeature != null) {
                    poisonBloomFeature.ResetDuration();
                } else {
                    area.featureComponent.AddFeature(AreaFeatureDB.Poison_Bloom_Feature, area);
                }
            } else if (interruptHolder.actor.characterClass.className.Equals("Mage")) { 
                //Brimstones
                if (area.spellsComponent.hasBrimstones) {
                    //reset brim stone
                    area.spellsComponent.ResetBrimstoneDuration();
                } else {
                    area.spellsComponent.SetHasBrimstones(true, false);
                }
            }
            else {
                throw new Exception($"No spell type for Loss of Control interrupt for character {interruptHolder.actor.name} with class {interruptHolder.actor.characterClass.className}");
            }
            
            
            return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
        }
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            //interruptHolder.actor.IncreaseCanMove();
            interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Catharsis");
            return base.ExecuteInterruptEndEffect(interruptHolder);
        }
        #endregion
    }
}
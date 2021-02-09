﻿using System;
using System.Collections;
using System.Collections.Generic;
using Locations.Tile_Features;
using Logs;
using UnityEngine;

namespace Interrupts {
    public class LossOfControl : Interrupt {
        public LossOfControl() : base(INTERRUPT.Loss_Of_Control) {
            duration = 12;
            doesDropCurrentJob = true;
            doesStopCurrentAction = true;
            isSimulateneous = false;
            interruptIconString = GoapActionStateDB.Shock_Icon;
            logTags = new[] {LOG_TAG.Needs, LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            //interruptHolder.actor.DecreaseCanMove();
            overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Mental Break", "break", null, logTags);
            overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideEffectLog.AddToFillers(null, "Loss of Control", LOG_IDENTIFIER.STRING_1);
            //Removed this because overridden logs are already added after in AddEffectLog
            //interruptHolder.actor.logComponent.RegisterLog(overrideEffectLog, onlyClickedCharacter: false);

            HexTile hexTile = interruptHolder.actor.hexTileLocation;
            if (interruptHolder.actor.characterClass.className.Equals("Druid")) {
                //Electric storm
                if (hexTile.spellsComponent.hasElectricStorm) {
                    //reset electric storm
                    hexTile.spellsComponent.ResetElectricStormDuration();
                } else {
                    hexTile.spellsComponent.SetHasElectricStorm(true);
                    // PlayerSkillManager.Instance.GetSpellData(SPELL_TYPE.ELECTRIC_STORM).ActivateAbility(hexTile);
                }
            } else if (interruptHolder.actor.characterClass.className.Equals("Shaman")) {
                //Poison Bloom
                PoisonBloomFeature poisonBloomFeature = hexTile.featureComponent.GetFeature<PoisonBloomFeature>();
                if (poisonBloomFeature != null) {
                    poisonBloomFeature.ResetDuration();
                } else {
                    hexTile.featureComponent.AddFeature(TileFeatureDB.Poison_Bloom_Feature, hexTile);
                    // PlayerSkillManager.Instance.GetSpellData(SPELL_TYPE.POISON_BLOOM).ActivateAbility(hexTile);
                }
            } else if (interruptHolder.actor.characterClass.className.Equals("Mage")) { 
                //Brimstones
                if (hexTile.spellsComponent.hasBrimstones) {
                    //reset electric storm
                    hexTile.spellsComponent.ResetBrimstoneDuration();
                } else {
                    hexTile.spellsComponent.SetHasBrimstones(true);
                    // PlayerSkillManager.Instance.GetSpellData(SPELL_TYPE.BRIMSTONES).ActivateAbility(hexTile);
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
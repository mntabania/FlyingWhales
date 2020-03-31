using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class LossOfControl : Interrupt {
        public LossOfControl() : base(INTERRUPT.Loss_Of_Control) {
            duration = 12;
            doesDropCurrentJob = true;
            doesStopCurrentAction = true;
            isSimulateneous = false;
            interruptIconString = GoapActionStateDB.Exclamation_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target, ref Log overrideEffectLog) {
            actor.DecreaseCanMove();
            overrideEffectLog = new Log(GameManager.Instance.Today(), "Interrupt", "Mental Break", "break");
            overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideEffectLog.AddToFillers(null, "Loss of Control", LOG_IDENTIFIER.STRING_1);
            actor.logComponent.RegisterLog(overrideEffectLog, onlyClickedCharacter: false);
            
            if (actor.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                HexTile hexTile = actor.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
                if (actor.characterClass.className.Equals("Druid")) {
                    //Electric storm
                    if (hexTile.spellsComponent.hasElectricStorm) {
                        //reset electric storm
                        hexTile.spellsComponent.ResetElectricStormDuration();
                    } else {
                        PlayerManager.Instance.GetSpellData(SPELL_TYPE.ELECTRIC_STORM).ActivateAbility(hexTile);
                    }
                } else if (actor.characterClass.className.Equals("Shaman")) {
                    //Poison Bloom
                    PoisonBloomFeature poisonBloomFeature = hexTile.featureComponent.GetFeature<PoisonBloomFeature>();
                    if (poisonBloomFeature != null) {
                        poisonBloomFeature.ResetDuration();
                    } else {
                        PlayerManager.Instance.GetSpellData(SPELL_TYPE.POISON_BLOOM).ActivateAbility(hexTile);
                    }
                } else if (actor.characterClass.className.Equals("Mage")) { 
                    //Brimstones
                    if (hexTile.spellsComponent.hasBrimstones) {
                        //reset electric storm
                        hexTile.spellsComponent.ResetBrimstoneDuration();
                    } else {
                        PlayerManager.Instance.GetSpellData(SPELL_TYPE.BRIMSTONES).ActivateAbility(hexTile);
                    }
                }
                else {
                    throw new Exception($"No spell type for Loss of Control interrupt for character {actor.name} with class {actor.characterClass.className}");
                }
            }
            
            return base.ExecuteInterruptStartEffect(actor, target, ref overrideEffectLog);
        }
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            actor.IncreaseCanMove();
            actor.traitContainer.AddTrait(actor, "Catharsis");
            return base.ExecuteInterruptEndEffect(actor, target);
        }
        #endregion
    }
}
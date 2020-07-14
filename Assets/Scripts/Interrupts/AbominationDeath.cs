using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class AbominationDeath : Interrupt {
        public AbominationDeath() : base(INTERRUPT.Abomination_Death) {
            duration = 3;
            doesStopCurrentAction = true;
            interruptIconString = GoapActionStateDB.Injured_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            LocationGridTile gridTileLocation = actor.gridTileLocation;
            actor.SetDestroyMarkerOnDeath(true);
            actor.Death("Abomination Germ", interrupt: this);
            Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Abomination,
                    FactionManager.Instance.neutralFaction);
            summon.SetName(actor.name);
            summon.CreateMarker();
            summon.InitialCharacterPlacement(gridTileLocation, true);
            summon.logComponent.AddHistory(actor.deathLog);
            if (UIManager.Instance.characterInfoUI.isShowing && 
                UIManager.Instance.characterInfoUI.activeCharacter == actor) {
                UIManager.Instance.characterInfoUI.CloseMenu();    
            }
            return true;
        }
        #endregion
    }
}
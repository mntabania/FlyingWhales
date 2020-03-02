using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;

public class Lullaby : PlayerSpell {
    public Lullaby() : base(SPELL_TYPE.LULLABY) {
        tier = 2;
        abilityRadius = 1;
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
        //abilityTags.Add(ABILITY_TAG.NONE);
    }

    #region Overrides
    protected override void OnLevelUp() {
        base.OnLevelUp();
        abilityRadius = level;
    }
    public override void ActivateAction(LocationGridTile targetTile) {
        base.ActivateAction(targetTile);
        //List<ITraitable> flammables = new List<ITraitable>();
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(abilityRadius, includeCenterTile: true, includeTilesInDifferentStructure: true);

        List<Character> charactersInHighlightedTiles = new List<Character>();
        for (int i = 0; i < InnerMapManager.Instance.currentlyShowingLocation.charactersAtLocation.Count; i++) {
            Character currCharacter = InnerMapManager.Instance.currentlyShowingLocation.charactersAtLocation[i];
            if (tiles.Contains(currCharacter.gridTileLocation)) {
                charactersInHighlightedTiles.Add(currCharacter);
            }
        }

        for (int i = 0; i < charactersInHighlightedTiles.Count; i++) {
            Character character = charactersInHighlightedTiles[i];

            if(!character.traitContainer.HasTrait("Resting")) {
                if (character.stateComponent.currentState != null) {
                    character.stateComponent.ExitCurrentState();
                    //This call is doubled so that it will also exit the previous major state if there's any
                    //if (character.stateComponent.currentState != null) {
                    //    character.stateComponent.currentState.OnExitThisState();
                    //}
                } 
                //else if (character.stateComponent.stateToDo != null) {
                //    character.stateComponent.SetStateToDo(null);
                //}
                else {
                    if (character.currentParty.icon.isTravelling) {
                        if (character.currentParty.icon.travelLine == null) {
                            character.marker.StopMovement();
                        }
                    }
                    if (character.currentActionNode != null) {
                        character.currentActionNode.StopActionNode(true);
                    }
                }
                character.marker.UpdateActionIcon();
            }
            character.needsComponent.ExhaustCharacter(character);
        }
    }
    public override void ShowRange(LocationGridTile targetTile) {
        base.ShowRange(targetTile);
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(abilityRadius, includeCenterTile: true, includeTilesInDifferentStructure: true);
        InnerMapManager.Instance.HighlightTiles(tiles);
    }
    public override void HideRange(LocationGridTile targetTile) {
        base.HideRange(targetTile);
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(abilityRadius, includeCenterTile: true, includeTilesInDifferentStructure: true);
        InnerMapManager.Instance.UnhighlightTiles(tiles);
    }
    #endregion
}

public class LullabyData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.LULLABY;
    public override string name { get { return "Lullaby"; } }
    public override string description { get { return "Makes characters in an settlement exhausted."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.SABOTAGE; } }
    public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public LullabyData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }
}
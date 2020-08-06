using System.Collections.Generic;
using Inner_Maps;
using Traits;

public class Disable : GoapAction {
    public Disable() : base(INTERACTION_TYPE.DISABLE) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Stealth_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.DEMON };
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Disable Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override bool IsInvalidOnVision(ActualGoapNode node) {
        //this action should not be considered invalid if the target is in combat.
        return false;
    }
    #endregion
    
    #region State Effects
    public void PreDisableSuccess(ActualGoapNode goapNode) {
        //Spawn Particle Effect
        GameManager.Instance.CreateParticleEffectAt(goapNode.actor.gridTileLocation, PARTICLE_EFFECT.Disabler);
    }
    public void AfterDisableSuccess(ActualGoapNode goapNode) {
        List<LocationGridTile> tilesInRange =
            goapNode.actor.gridTileLocation.GetTilesInRadius(3, includeCenterTile: true,
                includeTilesInDifferentStructure: true);

        for (int i = 0; i < tilesInRange.Count; i++) {
            LocationGridTile tile = tilesInRange[i];
            tile.PerformActionOnTraitables(traitable =>  DisableEffect(traitable, goapNode.actor));
        }
        goapNode.actor.AdjustHP(-goapNode.actor.maxHP, ELEMENTAL_TYPE.Normal, true);
    }
    private void DisableEffect(ITraitable traitable, Character actor) {
        if (traitable is Character targetCharacter && actor.IsHostileWith(targetCharacter)) {
            targetCharacter.traitContainer.AddTrait(traitable, "Ensnared", actor);  
            Log log = new Log(GameManager.Instance.Today(), "GoapAction", "Disable", "effect");
            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            log.AddLogToInvolvedObjects();
        }
    }
    #endregion
}
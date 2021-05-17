using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UtilityScripts;
public class DecreaseMood : GoapAction {
    public DecreaseMood() : base(INTERACTION_TYPE.DECREASE_MOOD) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Magic_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.DEMON, RACE.LESSER_DEMON, RACE.GHOST };
        logTags = new[] {LOG_TAG.Player, LOG_TAG.Life_Changes};
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Decrease Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
#endregion
    
#region State Effects
    public void PreDecreaseSuccess(ActualGoapNode goapNode) {
        //Spawn Particle Effect
        GameManager.Instance.CreateParticleEffectAt(goapNode.actor.gridTileLocation, PARTICLE_EFFECT.Demooder);
    }
    public void AfterDecreaseSuccess(ActualGoapNode goapNode) {
        List<LocationGridTile> tilesInRange = RuinarchListPool<LocationGridTile>.Claim();
        goapNode.actor.gridTileLocation.PopulateTilesInRadius(tilesInRange, 3, includeCenterTile: true,
                includeTilesInDifferentStructure: true);

        for (int i = 0; i < tilesInRange.Count; i++) {
            LocationGridTile tile = tilesInRange[i];
            tile.PerformActionOnTraitables(traitable =>  DecreaseEffect(traitable, goapNode.actor));
        }
        RuinarchListPool<LocationGridTile>.Release(tilesInRange);
        goapNode.actor.AdjustHP(-goapNode.actor.maxHP, ELEMENTAL_TYPE.Normal, true);
    }
    private void DecreaseEffect(ITraitable traitable, Character actor) {
        if (traitable is Character targetCharacter && actor.IsHostileWith(targetCharacter)) {
            targetCharacter.traitContainer.AddTrait(traitable, "Dolorous", actor);  
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", "Decrease Mood", "effect", null, logTags);
            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            log.AddLogToDatabase(true);
        }
    }
#endregion
}
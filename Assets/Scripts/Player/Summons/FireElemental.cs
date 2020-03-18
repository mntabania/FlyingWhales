using Inner_Maps;
using Traits;

public class FireElemental : Summon {

    public const string ClassName = "FireElemental";
    
    public override string raceClassName => $"Fire Elemental";
    
    public FireElemental() : base(SUMMON_TYPE.FireElemental, "FireElemental", RACE.ELEMENTAL,
        UtilityScripts.Utilities.GetRandomGender()) { }
    public FireElemental(SaveDataCharacter data) : base(data) { }
    
    public override void SubscribeToSignals() {
        base.SubscribeToSignals();
        Messenger.AddListener<ActualGoapNode>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
    }
    public override void UnsubscribeSignals() {
        base.UnsubscribeSignals();
        Messenger.RemoveListener<ActualGoapNode>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
    }
    private void OnCharacterFinishedAction(ActualGoapNode goapNode) {
        if (goapNode.actor == this && goapNode.action.goapType == INTERACTION_TYPE.STAND) {
            Burning burning = new Burning();
            burning.SetSourceOfBurning(new BurningSource(gridTileLocation.parentMap.region), gridTileLocation.genericTileObject);
            gridTileLocation.genericTileObject.traitContainer.AddTrait(gridTileLocation.genericTileObject, burning, this, bypassElementalChance: true);
        }
    }
    
    
}


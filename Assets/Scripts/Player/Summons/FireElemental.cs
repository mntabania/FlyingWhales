using Inner_Maps;
using Traits;

public class FireElemental : Summon {

    public const string ClassName = "Fire Elemental";
    
    public override string raceClassName => $"Fire Elemental";
    
    public FireElemental() : base(SUMMON_TYPE.Fire_Elemental, ClassName, RACE.ELEMENTAL, UtilityScripts.Utilities.GetRandomGender()) { }
    public FireElemental(string className) : base(SUMMON_TYPE.Fire_Elemental, className, RACE.ELEMENTAL, UtilityScripts.Utilities.GetRandomGender()) { }
    public FireElemental(SaveDataSummon data) : base(data) { }

    public override void Initialize() {
        base.Initialize();
        traitContainer.AddTrait(this, "Fire Resistant");
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Fire_Elemental_Behaviour);
    }
    public override void SubscribeToSignals() {
        if (hasSubscribedToSignals) {
            return;
        }
        base.SubscribeToSignals();
        Messenger.AddListener<Character, IPointOfInterest, INTERACTION_TYPE, ACTION_STATUS>(JobSignals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
    }
    public override void UnsubscribeSignals() {
        if (!hasSubscribedToSignals) {
            return;
        }
        base.UnsubscribeSignals();
        Messenger.RemoveListener<Character, IPointOfInterest, INTERACTION_TYPE, ACTION_STATUS>(JobSignals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
    }
    private void OnCharacterFinishedAction(Character p_actor, IPointOfInterest p_target, INTERACTION_TYPE p_type, ACTION_STATUS p_status) {
        if (p_actor == this && p_type == INTERACTION_TYPE.STAND) {
            Burning burning = TraitManager.Instance.CreateNewInstancedTraitClass<Burning>("Burning");
            burning.SetSourceOfBurning(new BurningSource(), gridTileLocation.tileObjectComponent.genericTileObject);
            gridTileLocation.tileObjectComponent.genericTileObject.traitContainer.AddTrait(gridTileLocation.tileObjectComponent.genericTileObject, burning, this, bypassElementalChance: true);
        }
    }
}


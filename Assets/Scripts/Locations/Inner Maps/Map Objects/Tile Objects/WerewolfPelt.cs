public class WerewolfPelt : TileObject {
    public WerewolfPelt() {
        Initialize(TILE_OBJECT_TYPE.WEREWOLF_PELT, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.SCRAP);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
        AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
        AddAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
        traitContainer.AddTrait(this, "Interesting");
    }
    public WerewolfPelt(SaveDataTileObject data) : base(data) { }
    protected override string GenerateName() { return "Werewolf Pelt"; }

    #region Ownership
    public override void SetInventoryOwner(Character p_newOwner) {
        base.SetInventoryOwner(p_newOwner);
        if (p_newOwner != null) {
            if (!p_newOwner.traitContainer.HasTrait("Lycanthrope")) {
                if (p_newOwner.HasItem(TILE_OBJECT_TYPE.PHYLACTERY)) {
                    p_newOwner.UnobtainItem(TILE_OBJECT_TYPE.PHYLACTERY);
                    p_newOwner.UnobtainItem(TILE_OBJECT_TYPE.WEREWOLF_PELT);
                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Tile Object", "Werewolf Pelt", "activated phylactery");
                    log.AddToFillers(p_newOwner, p_newOwner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(this, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddLogToDatabase(true);
                } else {
                    p_newOwner.UnobtainItem(TILE_OBJECT_TYPE.WEREWOLF_PELT);
                    p_newOwner.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Lycanthrope, this);
                }
            }
        }
    }
    #endregion
}

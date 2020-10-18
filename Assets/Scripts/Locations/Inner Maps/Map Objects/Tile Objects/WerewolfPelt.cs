public class WerewolfPelt : TileObject {
    public WerewolfPelt() {
        Initialize(TILE_OBJECT_TYPE.WEREWOLF_PELT, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.SCRAP);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
        AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
        traitContainer.AddTrait(this, "Interesting");
    }
    public WerewolfPelt(SaveDataTileObject data) { }
    protected override string GenerateName() { return "Werewolf Pelt"; }

    #region Ownership
    public override void SetInventoryOwner(Character character) {
        base.SetInventoryOwner(character);
        if (character != null) {
            if (!character.traitContainer.HasTrait("Lycanthrope")) {
                if (character.HasItem(TILE_OBJECT_TYPE.PHYLACTERY)) {
                    character.UnobtainItem(TILE_OBJECT_TYPE.PHYLACTERY);
                    character.UnobtainItem(TILE_OBJECT_TYPE.WEREWOLF_PELT);
                    Log log = new Log(GameManager.Instance.Today(), "Tile Object", "Werewolf Pelt", "activated phylactery");
                    log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(this, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddLogToDatabase();
                } else {
                    character.UnobtainItem(TILE_OBJECT_TYPE.WEREWOLF_PELT);
                    character.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Lycanthrope, this);
                }
            }
        }
    }
    #endregion
}

namespace Characters.Villager_Wants {
    public abstract class FurnitureWant : VillagerWant {
        public abstract TILE_OBJECT_TYPE furnitureWanted { get; }
    }
}
public class SmallTreeObject : TreeObject {
    public SmallTreeObject() : base(TILE_OBJECT_TYPE.SMALL_TREE_OBJECT) { }
    public SmallTreeObject(SaveDataTreeObject data) : base(data) { }
    
    public override string ToString() {
        return $"Tree {id.ToString()}";
    }
    protected override string GenerateName() { return "Tree"; }
}

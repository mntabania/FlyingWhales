public class GoapNode {
    //public GoapNode parent;
    //public int index;
    public int cost;
    public int level;
    public GoapAction action;
    public IPointOfInterest target;

    public void Initialize(int cost, int level, GoapAction action, IPointOfInterest target) {
        this.cost = cost;
        this.level = level;
        this.action = action;
        this.target = target;
    }

    public void Reset() {
        this.cost = 0;
        this.level = 0;
        this.action = null;
        this.target = null;
    }
}
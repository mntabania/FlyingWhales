namespace Inner_Maps.Location_Structures {
    public class Warehouse : ManMadeStructure{
        public Warehouse(Region location) : base(STRUCTURE_TYPE.WAREHOUSE, location) { }
        public Warehouse(Region location, SaveDataLocationStructure data) : base(location, data) { }
    }
}
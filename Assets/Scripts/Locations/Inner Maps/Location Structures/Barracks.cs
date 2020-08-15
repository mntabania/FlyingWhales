namespace Inner_Maps.Location_Structures {
     public class Barracks : ManMadeStructure{
         public Barracks(Region location) : base(STRUCTURE_TYPE.BARRACKS, location) {
             SetMaxHPAndReset(8000);
         }
         public Barracks(Region location, SaveDataLocationStructure data) : base(location, data) {
             SetMaxHP(8000);
         }
     }
 }
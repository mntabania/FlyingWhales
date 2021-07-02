using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class AncientGraveyardStructureObject : LocationStructureObject {
        private readonly string[] _classChoices = new[] {"Barbarian", "Archer", "Noble", "Farmer"};
        protected override void PreplacedObjectProcessing(StructureTemplateObjectData preplacedObj,
            LocationGridTile tile, LocationStructure structure, TileObject newTileObject) { 
            
            if (newTileObject is Tombstone tombstone) {
                Character character = CharacterManager.Instance.CreateNewCharacter(
                    CollectionUtilities.GetRandomElement(_classChoices), 
                    GameUtilities.RollChance(50) ? RACE.HUMANS : RACE.ELVES, 
                    GameUtilities.RollChance(50) ? GENDER.MALE : GENDER.FEMALE, homeRegion: structure.region, faction: FactionManager.Instance.vagrantFaction);
                character.SetIsPreplaced(true);
                character.CreateMarker();
                character.InitialCharacterPlacement(tile);
                character.marker.UpdatePosition();
                character.Death();
                tombstone.SetCharacter(character);
            }
            
            base.PreplacedObjectProcessing(preplacedObj, tile, structure, newTileObject);
        }
    }
}
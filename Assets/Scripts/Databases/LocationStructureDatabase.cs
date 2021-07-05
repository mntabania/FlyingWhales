using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
namespace Databases {
    public class LocationStructureDatabase {
        public Dictionary<string, LocationStructure> structuresByGUID { get; }
        public List<LocationStructure> allStructures { get; }

        public LocationStructureDatabase() {
            structuresByGUID = new Dictionary<string, LocationStructure>();
            allStructures = new List<LocationStructure>();
        }

        public void RegisterStructure(LocationStructure locationStructure) {
            structuresByGUID.Add(locationStructure.persistentID, locationStructure);
            allStructures.Add(locationStructure);
        }
        public LocationStructure GetStructureByPersistentID(string id) {
            if (structuresByGUID.ContainsKey(id)) {
                return structuresByGUID[id];
            }
            throw new Exception($"There is no structure with persistent ID {id}");
        }
        public LocationStructure GetStructureByPersistentIDSafe(string id) {
            if (structuresByGUID.ContainsKey(id)) {
                return structuresByGUID[id];
            }
            return null;
        }
    }
}
using System.Collections.Generic;
using Traits;
namespace Databases {
    //This is the burning source database
    public class BurningSourceDatabase {
        public Dictionary<string, BurningSource> burningSourcesByID { get; }
        public List<BurningSource> burningSources { get; }
        
        public BurningSourceDatabase() {
            burningSourcesByID = new Dictionary<string, BurningSource>();
            burningSources = new List<BurningSource>();
        }

        public void Register(BurningSource burningSource) {
            burningSourcesByID.Add(burningSource.persistentID, burningSource);
            burningSources.Add(burningSource);
        }
        public void UnRegister(BurningSource burningSource) {
            burningSourcesByID.Remove(burningSource.persistentID);
            burningSources.Remove(burningSource);
        }

        public BurningSource GetOrCreateBurningSourceWithID(string id) {
            if (burningSourcesByID.ContainsKey(id)) {
                return burningSourcesByID[id];
            }
            return new BurningSource(id);
        }
    }
}
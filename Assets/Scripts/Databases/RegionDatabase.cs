using System;
using System.Collections.Generic;

public class RegionDatabase {
    //public Dictionary<string, Region> regionByGUID { get; }
    //public Region[] allRegions { get; private set; }

    public Region mainRegion { get; private set; }
    public RegionDatabase() {
        //regionByGUID = new Dictionary<string, Region>();
    }

    //public void RegisterRegions(Region[] regions) {
    //    allRegions = regions;
    //    for (int i = 0; i < allRegions.Length; i++) {
    //        Region region = allRegions[i];
    //        RegisterRegion(region);
    //    }
    //}
    public void RegisterRegion(Region region) {
        mainRegion = region;
        //regionByGUID.Add(region.persistentID, region);
    }
    //public Region GetRegionByPersistentID(string id) {
    //    if (regionByGUID.ContainsKey(id)) {
    //        return regionByGUID[id];    
    //    }
    //    throw new Exception($"There is no region with persistent id {id}");
    //}
}

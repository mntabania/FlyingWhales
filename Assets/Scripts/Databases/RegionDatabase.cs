using System.Collections.Generic;

public class RegionDatabase {
    public Dictionary<string, Region> regionByGUID { get; }
    public Region[] allRegions;


    public RegionDatabase() {
        regionByGUID = new Dictionary<string, Region>();
    }

    public void RegisterRegions(Region[] regions) {
        allRegions = regions;
        for (int i = 0; i < allRegions.Length; i++) {
            Region region = allRegions[i];
            RegisterRegion(region);
        }
    }
    private void RegisterRegion(Region region) {
        regionByGUID.Add(region.persistentID, region);
    }
}

using System;
using System.Collections.Generic;
public class AreaDatabase {
    
    public Dictionary<string, Area> areaByGUID { get; }
    public List<Area> allAreas { get; }

    public AreaDatabase() {
        areaByGUID = new Dictionary<string, Area>();
        allAreas = new List<Area>();
    }

    public void RegisterArea(Area p_area) {
        areaByGUID.Add(p_area.areaData.persistentID, p_area);
        allAreas.Add(p_area);
    }

    public Area GetAreaByPersistentID(string id) {
        if (areaByGUID.ContainsKey(id)) {
            return areaByGUID[id];
        }
        throw new Exception($"There is no area with persistent ID {id}");
    }
}
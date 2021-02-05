using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class RegionDivisionComponent {
    private const int MAX_FAUNA_LIST_CAPACITY = 5;

    public List<RegionDivision> divisions { get; private set; }

    public RegionDivisionComponent() {
        divisions = new List<RegionDivision>();
    }
    public RegionDivisionComponent(SaveDataRegionDivisionComponent data) {
        divisions = new List<RegionDivision>();
        for (int i = 0; i < data.divisions.Count; i++) {
            divisions.Add(data.divisions[i].Load());
        }
    }
    public void AddRegionDivision(RegionDivision p_division) {
        divisions.Add(p_division);
        if (WorldSettings.Instance.worldSettingsData.IsScenarioMap()) {
            if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pitto) {
                //https://www.notion.so/ruinarch/08b940d81ce74b24826650200ad2df0c?v=05a2de69f70c4e27987aeb65ff0727d5&p=e347e3b46ac64198b144ec6707374f11
                p_division.PopulateFaunaList(8);
            } else {
                ScenarioData scenarioData = WorldSettings.Instance.GetScenarioDataByWorldType(WorldSettings.Instance.worldSettingsData.worldType);
                if(scenarioData.faunaList == null || scenarioData.faunaList.Length <= 0) {
                    p_division.PopulateFaunaList(MAX_FAUNA_LIST_CAPACITY);
                } else {
                    p_division.PopulateFaunaList(scenarioData.faunaList);
                }    
            }
            
        } else {
            p_division.PopulateFaunaList(MAX_FAUNA_LIST_CAPACITY);
        }
    }
}

[System.Serializable]
public class SaveDataRegionDivisionComponent : SaveData<RegionDivisionComponent> {
    public List<SaveDataRegionDivision> divisions { get; private set; }

    #region Overrides
    public override void Save(RegionDivisionComponent data) {
        divisions = new List<SaveDataRegionDivision>();
        for (int i = 0; i < data.divisions.Count; i++) {
            SaveDataRegionDivision save = new SaveDataRegionDivision();
            save.Save(data.divisions[i]);
            divisions.Add(save);
        }
    }

    public override RegionDivisionComponent Load() {
        RegionDivisionComponent component = new RegionDivisionComponent(this);
        return component;
    }
    #endregion
}
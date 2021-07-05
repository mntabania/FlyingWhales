using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class BiomeDivisionComponent {
    private const int MAX_FAUNA_LIST_CAPACITY = 5;

    public List<BiomeDivision> divisions { get; private set; }

    public BiomeDivisionComponent() {
        divisions = new List<BiomeDivision>();
    }
    public BiomeDivisionComponent(SaveDataRegionDivisionComponent data) {
        divisions = new List<BiomeDivision>();
        for (int i = 0; i < data.divisions.Count; i++) {
            divisions.Add(data.divisions[i].Load());
        }
    }
    public void AddBiomeDivision(BiomeDivision p_division) {
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
    public BiomeDivision GetBiomeDivisionThatTileBelongsTo(LocationGridTile p_tile) {
        for (int i = 0; i < divisions.Count; i++) {
            BiomeDivision biomeDivision = divisions[i];
            if (biomeDivision.biome == p_tile.mainBiomeType && biomeDivision.tiles.Contains(p_tile)) {
                return biomeDivision;
            }
        }
        return null;
    }
    public BiomeDivision GetBiomeDivision(BIOMES p_biome) {
        for (int i = 0; i < divisions.Count; i++) {
            BiomeDivision division = divisions[i];
            if (division.biome == p_biome) {
                return division;    
            }
        }
        return null;
    }
}

[System.Serializable]
public class SaveDataRegionDivisionComponent : SaveData<BiomeDivisionComponent> {
    public List<SaveDataRegionDivision> divisions { get; private set; }

    #region Overrides
    public override void Save(BiomeDivisionComponent data) {
        divisions = new List<SaveDataRegionDivision>();
        for (int i = 0; i < data.divisions.Count; i++) {
            SaveDataRegionDivision save = new SaveDataRegionDivision();
            save.Save(data.divisions[i]);
            divisions.Add(save);
        }
    }

    public override BiomeDivisionComponent Load() {
        BiomeDivisionComponent component = new BiomeDivisionComponent(this);
        return component;
    }
    #endregion
}
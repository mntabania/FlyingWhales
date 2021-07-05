using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Grid_Tile_Features;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace Inner_Maps.Grid_Tile_Features {
    public class GridTileFeatureComponent {
        private List<GridTileFeature> _features;

        #region getters
        public List<GridTileFeature> features => _features;
        #endregion
        

        public GridTileFeatureComponent() {
            _features = new List<GridTileFeature>();
        }

        #region Loading
        public void LoadReferences(SaveDataGridTileFeatureComponent data) {
            for (int i = 0; i < data.features.Length; i++) {
                SaveDataGridTileFeature saveDataGridTileFeature = data.features[i];
                GridTileFeature gridTileFeature = saveDataGridTileFeature.Load();
                _features.Add(gridTileFeature);
                gridTileFeature.Initialize();
                gridTileFeature.LoadReferences(saveDataGridTileFeature);
            }
        }
        #endregion

        public void Initialize() {
            //create dictionary
            List<GridTileFeature> allFeatures = ReflectiveEnumerator.GetEnumerableOfType<GridTileFeature>().ToList();
            for (int i = 0; i < allFeatures.Count; i++) {
                GridTileFeature feature = allFeatures[i];
                _features.Add(feature);
                feature.Initialize();
            }
        }
        public void AddFeatureToTile<T>(LocationGridTile p_tile) where T : GridTileFeature {
            GridTileFeature feature = GetFeature<T>();
            feature.AddTile(p_tile);
        }
        public void RemoveFeatureFromTile<T>(LocationGridTile p_tile) where T : GridTileFeature {
            GridTileFeature feature = GetFeature<T>();
            feature.RemoveTile(p_tile);
        }
        public T GetFeature<T>() {
            for (int i = 0; i < _features.Count; i++) {
                GridTileFeature feature = _features[i];
                if (feature is T tileFeature) {
                    return tileFeature;
                }
            }
            return default;
        }
    }
}

#region Save Data
public class SaveDataGridTileFeatureComponent : SaveData<GridTileFeatureComponent> {
    public SaveDataGridTileFeature[] features;
    public override void Save(GridTileFeatureComponent data) {
        base.Save(data);
        features = new SaveDataGridTileFeature[data.features.Count];
        for (int i = 0; i < data.features.Count; i++) {
            GridTileFeature feature = data.features[i];
            SaveDataGridTileFeature saveData = Activator.CreateInstance(feature.serializedData) as SaveDataGridTileFeature;
            saveData.Save(feature);
            features[i] = saveData;
        }
    }
    public override GridTileFeatureComponent Load() {
        return new GridTileFeatureComponent();
    }
}
#endregion
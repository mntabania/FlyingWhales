using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Locations.Features;
using UnityEngine;

public class TileFeatureComponent {

	public List<TileFeature> features { get; private set; }

	public TileFeatureComponent() {
		features = new List<TileFeature>();
	}
	
	public void AddFeature(TileFeature feature, HexTile tile) {
		if (!features.Contains(feature)) {
			features.Add(feature);
			Debug.Log($"{GameManager.Instance.TodayLogString()}{feature.name} was added to {tile}");
			feature.OnAddFeature(tile);
		}
	}
	public void AddFeature(string featureName, HexTile tile) {
		AddFeature(LandmarkManager.Instance.CreateTileFeature<TileFeature>(featureName), tile);
	}
	public bool RemoveFeature(TileFeature feature, HexTile tile) {
		if (features.Remove(feature)) {
			Debug.Log($"{GameManager.Instance.TodayLogString()}{feature.name} was removed from {tile}");
			feature.OnRemoveFeature(tile);
			return true;
		}
		return false;
	}
	public bool RemoveFeature(string featureName, HexTile tile) {
		TileFeature feature = GetFeature(featureName);
		if (feature != null) {
			return RemoveFeature(feature, tile);
		}
		return false;
	}
	public void RemoveAllFeatures(HexTile tile) {
		for (int i = 0; i < features.Count; i++) {
			if (RemoveFeature(features[i], tile)) {
				i--;
			}
		}
	}
	public void RemoveAllFeaturesExcept(HexTile tile, params string[] except) {
		for (int i = 0; i < features.Count; i++) {
			TileFeature feature = features[i];
			if (except.Contains(feature.name)) {
				continue;
			}
			if (RemoveFeature(feature, tile)) {
				i--;
			}
		}
	}
	public TileFeature GetFeature(string featureName) {
		for (int i = 0; i < features.Count; i++) {
			TileFeature f = features[i];
			string typeString = f.GetType().Name; 
			if (typeString == featureName || f.name == featureName) {
				return f;
			}
		}
		return null;
	}
	public T GetFeature<T>() where T : TileFeature{
		for (int i = 0; i < features.Count; i++) {
			TileFeature f = features[i];
			if (f is T feature) {
				return feature;
			}
		}
		return null;
	}
	public bool HasFeature(string featureName) {
		return GetFeature(featureName) != null;
	}
}

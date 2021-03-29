using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Locations.Area_Features;
using UnityEngine;

public class AreaFeatureComponent {
	public List<AreaFeature> features { get; private set; }

	public AreaFeatureComponent() {
		features = new List<AreaFeature>();
	}
	
	public void AddFeature(AreaFeature feature, Area p_area) {
		if (!features.Contains(feature)) {
			features.Add(feature);
			// Debug.Log($"{GameManager.Instance.TodayLogString()}{feature.name} was added to {tile}");
			feature.OnAddFeature(p_area);
		}
	}
	public AreaFeature AddFeature(string featureName, Area p_area) {
		AreaFeature feature = LandmarkManager.Instance.CreateAreaFeature<AreaFeature>(featureName);
		AddFeature(feature, p_area);
		return feature;
	}
	public bool RemoveFeature(AreaFeature feature, Area p_area) {
		if (features.Remove(feature)) {
			// Debug.Log($"{GameManager.Instance.TodayLogString()}{feature.name} was removed from {tile}");
			feature.OnRemoveFeature(p_area);
			return true;
		}
		return false;
	}
	public bool RemoveFeature(string featureName, Area p_area) {
		AreaFeature feature = GetFeature(featureName);
		if (feature != null) {
			return RemoveFeature(feature, p_area);
		}
		return false;
	}
	public void RemoveAllFeatures(Area p_area) {
		for (int i = 0; i < features.Count; i++) {
			if (RemoveFeature(features[i], p_area)) {
				i--;
			}
		}
	}
	public void RemoveAllFeaturesExcept(Area p_area, params string[] except) {
		for (int i = 0; i < features.Count; i++) {
			AreaFeature feature = features[i];
			if (except.Contains(feature.name)) {
				continue;
			}
			if (RemoveFeature(feature, p_area)) {
				i--;
			}
		}
	}
	public AreaFeature GetFeature(string featureName) {
		for (int i = 0; i < features.Count; i++) {
			AreaFeature f = features[i];
			string typeString = f.GetType().Name; 
			if (typeString == featureName || f.name == featureName) {
				return f;
			}
		}
		return null;
	}
	public T GetFeature<T>() where T : AreaFeature{
		for (int i = 0; i < features.Count; i++) {
			AreaFeature f = features[i];
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

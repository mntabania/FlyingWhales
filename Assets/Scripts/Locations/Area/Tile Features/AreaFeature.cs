namespace Locations.Area_Features {
	public class AreaFeature  {

		public string name { get; protected set; }
		public string description { get; protected set; }

		#region Virtuals
		public virtual void OnAddFeature(Area p_area) { }
		public virtual void OnRemoveFeature(Area p_area) { }
		public virtual void OnDemolishLandmark(Area p_area, LANDMARK_TYPE demolishedLandmarkType) { }
		public virtual void GameStartActions(Area p_area) { }
		#endregion

		#region Loading
		public virtual void LoadedGameStartActions(Area p_area) {
			GameStartActions(p_area); //by default features will behave the same as normal when loaded, override this if specific feature has a specific implementation for loading
		}
		#endregion

	}
	
	public class SaveDataAreaFeature {

		public string areaFeatureName;

		public virtual void Save(AreaFeature areaFeature) {
			areaFeatureName = areaFeature.GetType().Name;
		}

		public virtual AreaFeature Load() {
			return LandmarkManager.Instance.CreateAreaFeature<AreaFeature>(areaFeatureName);
		}
	}
}



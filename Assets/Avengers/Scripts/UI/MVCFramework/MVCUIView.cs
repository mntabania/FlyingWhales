using UnityEngine;

namespace Ruinarch.MVCFramework {
	public abstract class MVCUIView : MonoBehaviour 
	{

		protected MVCUIModel _baseAssetModel;

		protected virtual void Init(Canvas canvas, MVCUIModel assets) 
		{
			_baseAssetModel = assets;
			_baseAssetModel.transform.SetParent(canvas.transform, false);
		}

		public virtual void Destroy() 
		{
			GameObject.Destroy(gameObject);
			if (_baseAssetModel != null) {
				GameObject.Destroy(_baseAssetModel.gameObject);
				_baseAssetModel = null;
			}
		}

		public virtual void HideUI() 
		{
			_baseAssetModel.parentDisplay.gameObject.SetActive(false);
		}

		public virtual void ShowUI() 
		{
			_baseAssetModel.parentDisplay.gameObject.SetActive(true);
		}

		public void MakeLastSibling() {
			_baseAssetModel.transform.SetAsLastSibling();
		}

	}
}

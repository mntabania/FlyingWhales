using UnityEngine;
using System;

namespace Ruinarch.MVCFramework
{
	public abstract class MVCUIController : MonoBehaviour
	{
		public Action onUIINstantiated;

		protected MVCUIView m_mvcUIView;
		protected MVCUIModel m_mvcUIModel;

		[SerializeField]
		protected Canvas _canvas;

		public virtual void InstantiateUI()
		{

		}

		public void InitUI(MVCUIModel p_newModel, MVCUIView p_newView) {
			m_mvcUIView = p_newView;
			m_mvcUIModel = p_newModel;
			m_mvcUIView.transform.SetParent(GameObject.FindGameObjectWithTag("MainCamera").transform);
			onUIINstantiated?.Invoke();
		}

		public virtual void ShowUI() 
		{
			m_mvcUIView.ShowUI();
			m_mvcUIModel.transform.SetAsLastSibling();
		}

		public virtual void HideUI()
		{
			m_mvcUIView.HideUI();
		}

		public virtual void SetParent(Transform p_newParent)
		{
			m_mvcUIModel.parentDisplay.transform.parent.SetParent(p_newParent, false);
		}	
	}
}
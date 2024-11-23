//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace Utage
{

	/// <summary>
	/// トランジションの管理
	/// </summary>
	[AddComponentMenu("Utage/ADV/EffectManager")]
	public class AdvEffectManager : MonoBehaviour
	{
		public AdvEngine Engine { get { return engine ?? (engine = this.GetComponentInParent<AdvEngine>()); } }
		AdvEngine engine;

		/// <summary>
		/// メッセージウィンドウ
		/// </summary>
		public AdvUguiMessageWindowManager MessageWindow { get { return messageWindow ?? (messageWindow = FindObjectOfType<AdvUguiMessageWindowManager>()); } }
		[SerializeField]
		AdvUguiMessageWindowManager messageWindow;

		/// <summary>
		/// カメラのルート
		/// </summary>
		public GameObject CameraRoot
		{
			get
			{
				UguiLetterBoxCamera camera = FindObjectOfType<UguiLetterBoxCamera>();
				if (camera)
				{
					cameraRoot = camera.transform.parent.gameObject;
				}
				return cameraRoot;
			}
		}
		[SerializeField]
		GameObject cameraRoot;

		public GameObject FindTarget(AdvEffectData data)
		{
			switch (data.Target)
			{
				case AdvEffectData.TargetType.MessageWindow:
					return MessageWindow.gameObject;
				case AdvEffectData.TargetType.Graphics:
					return Engine.GraphicManager.gameObject;
				case AdvEffectData.TargetType.Camera:
						return CameraRoot;
				case AdvEffectData.TargetType.Default:
				default:
						{
							AdvGraphicObject obj = engine.GraphicManager.FindObject(data.TargetName);
							if (obj != null && obj.gameObject != null) return obj.gameObject;
							AdvGraphicLayer layer = engine.GraphicManager.FindLayer(data.TargetName);
							if (layer != null) return layer.gameObject;
							Transform ui = engine.UiManager.transform.Find(data.TargetName);
							if (ui != null) return ui.gameObject;
							return null;
						}
			}
		}

		public void Play(AdvEffectData effectData)
		{
			currentEffectList.Add(effectData);
			effectData.Play(this, OnComplete);
		}

		void OnComplete(AdvEffectData effectData)
		{
			currentEffectList.Remove(effectData);
		}

		public bool IsPageWaiting
		{
			get
			{
				foreach (AdvEffectData effect in currentEffectList)
				{
					if (effect.IsPageWaiting)
					{
						return true;
					}
				}
				return false;
			}
		}

		List<AdvEffectData> currentEffectList = new List<AdvEffectData>();


		internal bool IsCommandWaiting(AdvEffectData effectData)
		{
			//今のコマンドがWaitのとき、エフェクトの終了待ちをする
			if (effectData.Wait == AdvEffectData.WaitType.Wait)
			{
				foreach (AdvEffectData effect in currentEffectList)
				{
					if (effect.IsCommandWaiting)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}

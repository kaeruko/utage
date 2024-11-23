//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Utage
{

	/// <summary>
	/// 表示言語切り替え用のクラス
	/// </summary>
	[AddComponentMenu("Utage/Lib/UI/LocalizeRectTransform")]
	public class UguiLocalizeRectTransform : UguiLocalizeBase
	{
		[Serializable]
		public class Setting
		{
			public string language;
			public Vector2 anchoredPosition;
			public Vector2 size = new Vector2(100,100);
		};

		[SerializeField]
		List<Setting> settingList = new List<Setting>();

		[NonSerialized]
		Setting defaultSetting = null;

		/// <summary>
		/// スプライトコンポーネント(アタッチされてない場合はnull)
		/// </summary>
		RectTransform CachedRectTransform { get { if (null == cachedRectTransform) cachedRectTransform = this.GetComponent<RectTransform>(); return cachedRectTransform; } }
		RectTransform cachedRectTransform;

		protected override void RefreshSub()
		{
			Setting setting = settingList.Find(x => x.language == currentLanguage);
			if (setting == null)
			{
				setting = defaultSetting;
			}
			CachedRectTransform.anchoredPosition = setting.anchoredPosition;
			CachedRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, setting.size.x);
			CachedRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, setting.size.y);
		}

		protected override void InitDefault()
		{
			defaultSetting = new Setting();
			defaultSetting.anchoredPosition = CachedRectTransform.anchoredPosition;
			defaultSetting.size = CachedRectTransform.rect.size;
		}
		public override void ResetDefault()
		{
			if (defaultSetting == null) return;

			CachedRectTransform.anchoredPosition = defaultSetting.anchoredPosition;
			CachedRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, defaultSetting.size.x);
			CachedRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, defaultSetting.size.y);
		}
	}
}


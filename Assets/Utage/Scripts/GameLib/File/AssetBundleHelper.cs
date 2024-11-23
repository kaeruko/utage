//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using System.Collections.Generic;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utage
{
	[System.Flags]
	public enum AssetBundleTargetFlags
	{
		Android			= 0x1 << 0,
		iOS				= 0x1 << 1,
		WebPlayer		= 0x1 << 2,
		WebGL			= 0x1 << 3,
		Windows			= 0x1 << 4,
		OSX				= 0x1 << 5,
		// 他のプラットフォームは必要に応じて追加
	};

	//アセットバンドルのヘルパー
	public class AssetBundleHelper
	{
		public static AssetBundleTargetFlags RuntimeAssetBundleTraget()
		{
#if UNITY_EDITOR
			return BuildTargetToBuildTargetFlag(EditorUserBuildSettings.activeBuildTarget);
#else
			return RuntimePlatformToBuildTargetFlag(Application.platform);
#endif
		}

		//ランタイムのプラットフォームを、ターゲットフラグにに変換
		public static AssetBundleTargetFlags RuntimePlatformToBuildTargetFlag(RuntimePlatform platform)
		{
			switch (platform)
			{
				case RuntimePlatform.Android:
					return AssetBundleTargetFlags.Android;
				case RuntimePlatform.IPhonePlayer:
					return AssetBundleTargetFlags.iOS;
#if UNITY_5_4_OR_NEWER
#else
				case RuntimePlatform.WindowsWebPlayer:
				case RuntimePlatform.OSXWebPlayer:
					return AssetBundleTargetFlags.WebPlayer;
#endif

				case RuntimePlatform.WebGLPlayer:
					return AssetBundleTargetFlags.WebGL;

				case RuntimePlatform.WindowsPlayer:
					return AssetBundleTargetFlags.Windows;

				case RuntimePlatform.OSXPlayer:
					return AssetBundleTargetFlags.OSX;
				default:
					Debug.LogError("Not support " + platform.ToString());
					return 0;
			}
		}
#if UNITY_EDITOR
		//ターゲットフラグスを、ビルドターゲットのリストに変換
		public static List<BuildTarget> BuildTargetFlagsToBuildTargetList(AssetBundleTargetFlags flags)
		{
			List<BuildTarget> list = new List<BuildTarget>();
			foreach (AssetBundleTargetFlags flag in Enum.GetValues(typeof(AssetBundleTargetFlags)))
			{
				if ((flags & flag) == flag)
				{
					list.Add(BuildTargetFlagToBuildTarget(flag));
				}
			}
			return list;
		}

		//ターゲットフラグを、ビルドターゲットに変換
		public static BuildTarget BuildTargetFlagToBuildTarget(AssetBundleTargetFlags flag)
		{
			switch (flag)
			{
				case AssetBundleTargetFlags.Android:
					return BuildTarget.Android;
				case AssetBundleTargetFlags.iOS:
					return BuildTarget.iOS;
#if UNITY_5_4_OR_NEWER
#else
				case AssetBundleTargetFlags.WebPlayer:
					return BuildTarget.WebPlayer;
#endif
				case AssetBundleTargetFlags.WebGL:
					return BuildTarget.WebGL;
				case AssetBundleTargetFlags.Windows:
					return BuildTarget.StandaloneWindows64;
				case AssetBundleTargetFlags.OSX:
					return BuildTarget.StandaloneOSX;
				default:
					Debug.LogError("Not support " + flag.ToString() );
					return 0;
			}
		}

		//ビルドターゲットを、ターゲットフラグに変換
		public static AssetBundleTargetFlags BuildTargetToBuildTargetFlag(BuildTarget target)
		{
			switch (target)
			{
				case BuildTarget.Android:
					return AssetBundleTargetFlags.Android;
				case BuildTarget.iOS:
					return AssetBundleTargetFlags.iOS;
#if UNITY_5_4_OR_NEWER
#else
				case BuildTarget.WebPlayer:
					return AssetBundleTargetFlags.WebPlayer;
#endif
				case BuildTarget.WebGL:
					return AssetBundleTargetFlags.WebGL;

				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
					return AssetBundleTargetFlags.Windows;

				case BuildTarget.StandaloneOSXIntel:
				case BuildTarget.StandaloneOSXIntel64:
				case BuildTarget.StandaloneOSX:
					return AssetBundleTargetFlags.OSX;

				default:
					return 0;
			}
		}
#endif
	}
}
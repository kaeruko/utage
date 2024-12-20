//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using UnityEngine;
using Utage;
using System.Collections;
using System.Collections.Generic;

namespace Utage
{

	/// <summary>
	/// ゲーム起動処理のサンプル
	/// </summary>
	[AddComponentMenu("Utage/ADV/EngineStarter")]
	public class AdvEngineStarter : MonoBehaviour
	{
		public enum LoadType
		{
			Local,
			Server,
		};

		//Awake時にロードする
		[SerializeField]
		bool isLoadOnAwake = true;

		/// <summary>開始フレームで自動でADVエンジンを起動する</summary>
		[SerializeField]
		bool isAutomaticPlay = false;

		/// <summary>開始フレームで自動でADVエンジンを起動する</summary>
		[SerializeField]
		string startScenario = "";

		/// <summary>ADVエンジン</summary>
		public AdvEngine Engine { get { return this.engine ?? (this.engine = FindObjectOfType<AdvEngine>() as AdvEngine); } }
		[SerializeField]
		AdvEngine engine;

		/// <summary>シナリオデータのロードタイプ</summary>
		public LoadType ScenarioDataLoadType
		{
			get { return scenarioDataLoadType; }
			set { scenarioDataLoadType = value; }
		}
		[SerializeField]
		LoadType scenarioDataLoadType;

		/// <summary>サーバーから起動する場合の開始ファイルのパス</summary>
		public string UrlScenarioData
		{
			get { return urlScenarioData; }
			set { urlScenarioData = value; }
		}
		[SerializeField]
		string urlScenarioData;

		/// <summary>サーバーから起動する場合の開始ファイルのバージョン(-1なら毎回ダウンロードしなおす)</summary>
		public int ScenarioVersion
		{
			get { return scenarioVersion; }
			set { scenarioVersion = value; }
		}
		[SerializeField]
		int scenarioVersion = -1;

		/// <summary>各章のURL</summary>
		[SerializeField]
		List<string> chapterUrlList;

		/// <summary>
		/// シナリオ
		/// </summary>
		public AdvImportScenarios Scenarios { get { return scenarios; } set { scenarios = value; }}
		[SerializeField]
		AdvImportScenarios scenarios;

		/// <summary>
		/// エクスポートしたシナリオデータがあればここに設定可能
		/// </summary>
//		[SerializeField]
//		AdvScenarioDataExported[] exportedScenarioDataTbl;

		/// <summary>リソースのロードタイプ</summary>
		public LoadType ResourceLoadType
		{
			get { return resourceLoadType; }
			set { resourceLoadType = value; }
		}
		[SerializeField]
		LoadType resourceLoadType;

		/// <summary>リソースディレクトリのサーバーアドレス</summary>
		[SerializeField]
		string urlResourceDir;

		/// <summary>リソースディレクトリのルートパス</summary>
		[SerializeField]
		string rootResourceDir;

		public string ResourceDir
		{ 
			get { return (ResourceLoadType == LoadType.Server ? urlResourceDir : rootResourceDir); }
			set
			{
				if (ResourceLoadType == LoadType.Server)
				{
					urlResourceDir = value;
				}
				else
				{
					rootResourceDir = value;
				}
			}
		}

		/// <summary>コンバートファイルリストを使うか</summary>
		[SerializeField]
		bool useConvertFileListOnLocal;
		[SerializeField]
		bool useConvertFileListOnServer;

		/// <summary>アセットバンドルリストを使うか</summary>
		[SerializeField]
		bool useAssetBundleListOnLocal;
		[SerializeField]
		bool useAssetBundleListOnServer;

		/// <summary>ロード設定を自動で設定</summary>
		[SerializeField]
		bool isAutomaticInitFileLoadSetting;

		/// <summary>コンバートファイルリストのパス</summary>
		[SerializeField, LimitEnum("Local", "LocalCompressed", "LocalCompressed2")]
		AssetFileManagerSettings.LoadType localLoadSetting = AssetFileManagerSettings.LoadType.Local;

		/// <summary>コンバートファイルリストのURL</summary>
		[SerializeField, LimitEnum("Server", "ServerPure")]
		AssetFileManagerSettings.LoadType serverLoadSetting = AssetFileManagerSettings.LoadType.ServerPure;


		void Awake()
		{
			if (isLoadOnAwake)
			{
				LoadEngine();
			}
		}

		public void LoadEngine()
		{
			LoadEngine(ScenarioVersion);
		}

		public void LoadEngine(int version)
		{
			//ロードタイプの設定
			if (isAutomaticInitFileLoadSetting)
			{
				switch (ResourceLoadType)
				{
					case LoadType.Local:
						AssetFileManager.InitLoadTypeSetting(localLoadSetting);
						break;
					case LoadType.Server:
						AssetFileManager.InitLoadTypeSetting(serverLoadSetting);
						break;
				}
			}

			LoadInitFileList(version);

			if (!string.IsNullOrEmpty (startScenario)) 
			{
				Engine.StartScenarioLabel = startScenario;
			}

			//ADVエンジンの初期化を開始
			switch(ScenarioDataLoadType)
			{
				case LoadType.Server:
					if (ScenarioDataLoadType != LoadType.Server)
					{
						Debug.LogError("ScenarioDataLoadType is Not Server");
					}
					else
					{
						if (string.IsNullOrEmpty(urlScenarioData)) { Debug.LogError("Not set URL ScenarioData", this); return; }
						if (string.IsNullOrEmpty(ResourceDir)) { Debug.LogError("Not set ResourceData", this); return; }
						Engine.BootFromCsv(urlScenarioData, ResourceDir, version);
					}
					break;
				case LoadType.Local:
					if (scenarios == null)
					{
						Debug.LogError("Scenarios is Blank. Please set .scenarios Asset", this);
						return;
					}
					if (string.IsNullOrEmpty(ResourceDir)) { Debug.LogError("Not set ResourceData", this); return; }
					Engine.BootFromExportData(this.scenarios, ResourceDir);
					break;
			}

			if (isAutomaticPlay)
			{
				StartCoroutine(CoPlayEngine());
			}
		}

		public void LoadInitFileList(int version)
		{
			string dir = ResourceDir;
			bool useConvertFileList = ResourceLoadType == LoadType.Server ? useConvertFileListOnServer : useConvertFileListOnLocal;
			bool useAssetBundleFileList = ResourceLoadType == LoadType.Server ? useAssetBundleListOnServer : useAssetBundleListOnLocal;
			List<string> pathList = new List<string>();
			if(useConvertFileList)
			{
				string path = FilePathUtil.Combine( dir, FilePathUtil.GetDirectoryNameOnly(ResourceDir+"/") + ExtensionUtil.ConvertFileList);
				pathList.Add(path);
			}
			if(useAssetBundleFileList)
			{
				string AssetBundleTarget = AssetBundleHelper.RuntimeAssetBundleTraget().ToString();
				string assetBundlePath = FilePathUtil.Combine( AssetBundleTarget, AssetBundleTarget + ExtensionUtil.ConvertFileList);
				string path = FilePathUtil.Combine(dir, assetBundlePath);
				pathList.Add(path);
			}
			AssetFileManager.LoadInitFileList(pathList, version);
		}

		public void StartEngine()
		{
			StartCoroutine(CoPlayEngine());
		}

		IEnumerator CoPlayEngine()
		{
			while (Engine.IsWaitBootLoading) yield return 0;
			if (string.IsNullOrEmpty(startScenario))
			{
				Engine.StartGame();
			}
			else
			{
				Engine.StartGame(startScenario);
			}
		}

#if UNITY_EDITOR
		const int Version = 1;
		[SerializeField, HideInInspector]
		int version = 0;

		/// <summary>シナリオデータプロジェクト</summary>
		public Object ScenarioDataProject { get { return scenarioDataProject; } set { scenarioDataProject = value; } }
		[SerializeField]
		Object scenarioDataProject;

		//スクリプトから初期化
		public void InitOnCreate(AdvEngine engine, AdvImportScenarios scenarios, string rootResourceDir)
		{
			this.engine = engine;
			this.scenarios = scenarios;
			this.rootResourceDir = rootResourceDir;
			EditorVersionUp();
		}

		//最新バージョンかチェック
		public bool EditorCheckVersion()
		{
			if (version == Version)
			{
				if (this.scenarios != null && !this.scenarios.CheckVersion())
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		//最新バージョンにバージョンアップ
		public void EditorVersionUp()
		{
			version = Version;
		}
#endif
	}
}
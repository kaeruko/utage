//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace Utage
{

	//「Utage」のシナリオデータ用のエクセルファイルインポーター
	public class AdvExcelImporter : AssetPostprocessor
	{
		static void OnPostprocessAllAssets(
			string[] importedAssets,
			string[] deletedAssets,
			string[] movedAssets,
			string[] movedFromAssetPaths)
		{
			//制御エディタを通して、管理対象のデータのみインポートする
			AdvScenarioDataBuilderWindow.Import(importedAssets);
		}
		public const string BookAssetExt = ".book.asset";
		public const string ScenarioAssetExt = ".asset";

		//シナリオデータ
		Dictionary<string, AdvScenarioData> scenarioDataTbl;
		AdvMacroManager	macroManager;
		AdvImportScenarios scenariosAsset;

		//ファイルの読み込み
		public void Import(AdvScenarioDataProject project)
		{
			if (project.ChapterDataList.Count <= 0)
			{
				Debug.LogError("ChapterDataList is zeo");
				return;
			}

			AssetFileManager.IsEditorErrorCheck = true;
			AdvCommand.IsEditorErrorCheck = true;

			AdvEngine engine = UtageEditorToolKit.FindComponentAllInTheScene<AdvEngine>();
			if (engine != null)
			{
				engine.BootInitCustomCommand();
			}
			this.scenarioDataTbl = new Dictionary<string, AdvScenarioData>();
			this.macroManager = new AdvMacroManager();

			AdvScenarioDataBuilderWindow.ProjectData.CreateScenariosIfMissing();
			this.scenariosAsset = project.Scenarios;

			this.scenariosAsset.ClearOnImport();
			//チャプターデータのインポート
			project.ChapterDataList.ForEach(x => ImportChapter(x.chapterName, x.ExcelPathList, project.CheckTextCount));
			EditorUtility.SetDirty(this.scenariosAsset);
			AssetDatabase.Refresh();
			AdvCommand.IsEditorErrorCheck = false;
			AssetFileManager.IsEditorErrorCheck = false;
		}


		bool ImportChapter(string chapterName, List<string> pathList, bool checkTextCount)
		{
			//末尾の空白文字をチェック
			//対象のエクセルファイルを全て読み込み
			Dictionary<string, StringGridDictionary> bookDictionary = ReadExcels(pathList);
			if (bookDictionary.Count <= 0) return false;

			CheckWhiteSpaceEndOfCell(bookDictionary);

			List<AdvImportBook> bookAssetList = new List<AdvImportBook>();
			//シナリオデータをインポート
			foreach (string path in bookDictionary.Keys)
			{
				bookAssetList.Add( ImportBook(bookDictionary[path], path) );
			}

			//シナリオのエラーチェック
			ErrorCheckScenario(chapterName, bookAssetList, checkTextCount);

			return true;
		}

		//対象のエクセルファイルを全て読み込み
		Dictionary<string, StringGridDictionary> ReadExcels( List<string> pathList )
		{
			Dictionary<string, StringGridDictionary> bookDictionary = new Dictionary<string, StringGridDictionary>();
			foreach (string path in pathList)
			{
				if (!string.IsNullOrEmpty(path))
				{
					StringGridDictionary book = ExcelParser.Read(path, '#');
					book.RemoveSheets(@"^#");
					if (book.List.Count > 0)
					{
						bookDictionary.Add(path, book);
					}
				}
			}
			return bookDictionary;
		}


		//末尾の空白文字をチェック
		private void CheckWhiteSpaceEndOfCell(Dictionary<string, StringGridDictionary> bookDictionary)
		{
			AdvEditorSettingWindow editorSetting = AdvEditorSettingWindow.GetInstance();
			if ( UnityEngine.Object.ReferenceEquals(editorSetting,null)) return;
			if( !editorSetting.CheckWhiteSpaceOnImport ) return;

			List<string> ignoreHeader = new List<string>();
			ignoreHeader.Add("Text");
			if (LanguageManagerBase.Instance != null)
			{
				foreach( string language in LanguageManagerBase.Instance.Languages )
				{
					ignoreHeader.Add(language);
				}
			}

			foreach( StringGridDictionary book in bookDictionary.Values )
			{
				foreach( var sheet in book.Values )
				{
					List<int> ignoreIndex = new List<int>();
					foreach( var item in ignoreHeader )
					{
						int index;
						if (sheet.Grid.TryGetColumnIndex(item, out index))
						{
							ignoreIndex.Add(index);
						}
					}
					foreach (var row in sheet.Grid.Rows)
					{
						if (row.RowIndex == 0) continue;

						for (int i = 0; i < row.Strings.Length; ++i )
						{
							string str = row.Strings[i];
							if (str.Length <= 0) continue;
							if (ignoreIndex.Contains(i)) continue;

							int endIndex = str.Length-1;
							if (char.IsWhiteSpace(str[endIndex]))
							{
								Debug.LogWarning(row.ToErrorString("Last characer is white space [" + ColorUtil.AddColorTag(str,ColorUtil.Red)  + "]  \n" ));
							}
						}
					}
				}
			}
		}

		//ブックのインポート
		AdvImportBook ImportBook(StringGridDictionary book, string path)
		{
			//シナリオデータ用のスクリプタブルオブジェクトを宣言
			string bookAssetPath = Path.ChangeExtension(path, BookAssetExt);
			AdvImportBook asset = UtageEditorToolKit.GetImportedAssetCreateIfMissing<AdvImportBook>(bookAssetPath);
			asset.hideFlags = HideFlags.NotEditable;
			asset.Clear();

			foreach (var sheet in book.List)
			{
				asset.AddData(sheet.Grid);
			}

			//変更を反映
			Debug.Log(LanguageAdvErrorMsg.LocalizeTextFormat(AdvErrorMsg.Import, bookAssetPath));
			EditorUtility.SetDirty(asset);
			return asset;
		}
		//シナリオのエラーチェック
		void ErrorCheckScenario(string chapterName, List<AdvImportBook> books, bool checkTextCount)
		{
			this.scenariosAsset.AddImportData(chapterName,books);

			AdvSettingDataManager setting = new AdvSettingDataManager();
			setting.ImportedScenarios = this.scenariosAsset;
			setting.BootInit("");

			GraphicInfo.CallbackExpression = setting.DefaultParam.CalcExpressionBoolean;
			TextParser.CallbackCalcExpression = setting.DefaultParam.CalcExpressionNotSetParam;
			iTweenData.CallbackGetValue = setting.DefaultParam.GetParameter;

			List<AdvScenarioData> scenarioList = new List<AdvScenarioData>();
			foreach (var book in books)
			{
				foreach (var grid in book.GridList)
				{
					string sheetName = grid.SheetName;
					if (!AdvSettingDataManager.IsScenarioSheet(sheetName)) continue;
					if (!macroManager.TryAddMacroData(sheetName, grid))
					{
						if (scenarioDataTbl.ContainsKey(sheetName))
						{
							Debug.LogError(sheetName + " is already contains in the sheets");
						}
						else
						{
							AdvScenarioData scenario = new AdvScenarioData(sheetName, grid);
							scenarioDataTbl.Add(sheetName, scenario);
							scenarioList.Add(scenario);
						}
					}
				}
			}

			//シナリオデータとして解析、初期化
			foreach (AdvScenarioData data in scenarioList)
			{
				data.Init(setting, this.macroManager);
			}

			GraphicInfo.CallbackExpression = null;
			TextParser.CallbackCalcExpression = null;
			iTweenData.CallbackGetValue = null;

			//シナリオラベルのリンクチェック
			ErrorCheckScenarioLabel(scenarioList);

			//文字数カウント
			if (checkTextCount)
			{
				CheckCharacterCount(scenarioList);
			}
		}

		/// <summary>
		/// シナリオラベルのリンクチェック
		/// </summary>
		/// <param name="label">シナリオラベル</param>
		/// <returns>あればtrue。なければfalse</returns>
		void ErrorCheckScenarioLabel(List<AdvScenarioData> scenarioList)
		{
			//リンク先のシナリオラベルがあるかチェック
			foreach (AdvScenarioData data in scenarioList)
			{
				foreach (AdvScenarioJumpData jumpData in data.JumpScenarioData)
				{
					if (!IsExistScenarioLabel(jumpData.ToLabel))
					{
						Debug.LogError( 
							jumpData.FromRow.ToErrorString( 
							LanguageAdvErrorMsg.LocalizeTextFormat(AdvErrorMsg.NotLinkedScenarioLabel, jumpData.ToLabel, "")
							));
					}
				}
			}

			//シナリオラベルが重複しているかチェック
			foreach (AdvScenarioData data in scenarioList)
			{
				foreach (AdvScenarioLabelData labelData in data.ScenarioLabelData)
				{
					if (IsExistScenarioLabel(labelData.ScenaioLabel, data))
					{
						string error = labelData.ToErrorString(LanguageAdvErrorMsg.LocalizeTextFormat(AdvErrorMsg.RedefinitionScenarioLabel, labelData.ScenaioLabel,""), data.DataGridName );
						Debug.LogError(error);
					}
				}
			}
		}


		/// <summary>
		/// シナリオラベルがあるかチェック
		/// </summary>
		/// <param name="label">シナリオラベル</param>
		/// <param name="egnoreData">チェックを無視するデータ</param>
		/// <returns>あればtrue。なければfalse</returns>
		bool IsExistScenarioLabel(string label, AdvScenarioData egnoreData = null )
		{
			foreach (AdvScenarioData data in scenarioDataTbl.Values)
			{
				if (data == egnoreData) continue;
				if (data.IsContainsScenarioLabel(label))
				{
					return true;
				}
			}
			return false;
		}

		// 文字数オーバー　チェック
		internal void CheckCharacterCount(List<AdvScenarioData> scenarioList)
		{
			int count;
			if (TryCheckCharacterCount(scenarioList,out count))
			{
				Debug.Log(LanguageAdvErrorMsg.LocalizeTextFormat(AdvErrorMsg.ChacacterCountOnImport, count));
			}
		}

		// 文字数オーバー　チェック
		internal bool TryCheckCharacterCount( List<AdvScenarioData> scenarioList, out int count )
		{
			count = 0;
			AdvEngine engine = UtageEditorToolKit.FindComponentAllInTheScene<AdvEngine>();
			if (engine == null) return false;
			
			AdvUguiManager uguiManager = UtageEditorToolKit.FindComponentAllInTheScene<AdvUguiManager>();
			if (uguiManager == null) return false;

			bool isActive = uguiManager.gameObject.activeSelf;
			if (!isActive)
			{
				uguiManager.gameObject.SetActive(true);
				//				UguiLetterBoxCanvasScaler scaler = uguiManager.GetComponent<UguiLetterBoxCanvasScaler>();
				//				if (scaler != null)					scaler.SetLayoutHorizontal();
			}
			AdvUguiMessageWindow[] messageWindows = uguiManager.GetComponentsInChildren<AdvUguiMessageWindow>(true);
			Dictionary<string, AdvUguiMessageWindow> windows = new Dictionary<string, AdvUguiMessageWindow>();
			foreach (var window in messageWindows)
			{
				windows.Add(window.name, window);
			}

			foreach (AdvScenarioData data in scenarioList)
			{
				count += data.EditorCheckCharacterCount(engine, windows);
			}
			if (!isActive) uguiManager.gameObject.SetActive(false);
			return true;
		}
	}
}
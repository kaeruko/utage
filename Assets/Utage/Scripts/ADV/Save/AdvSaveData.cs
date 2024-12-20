//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.IO;

namespace Utage
{

	/// <summary>
	/// セーブデータ
	/// </summary>
	[System.Serializable]
	public class AdvSaveData
	{
		public enum SaveDataType
		{
			Default,
			Quick,
			Auto,
		};

		public SaveDataType Type{ get{ return type;}}
		SaveDataType type;

		/// <summary>
		/// 現在のシナリオラベル
		/// </summary>
		public string CurrentSenarioLabel { get { return this.currentSenarioLabel; } }
		string currentSenarioLabel;

		/// <summary>
		/// 現在のページ
		/// </summary>
		public int CurrentPage { get { return this.currentPage; } }
		int currentPage;

		/// <summary>
		/// 現在の、シーン回想用のシーンラベル
		/// </summary>
		public string CurrentGallerySceneLabel { get { return this.currentGallerySceneLabel; } }
		string currentGallerySceneLabel = "";

		/// <summary>
		/// JumpManger用のデータ
		/// </summary>
		public byte[] JumpMangerBuffer { get; private set; }


		/// <summary>
		/// 独自拡張のバイナリデータ
		/// </summary>
		public AdvCustomSaveData CustomBuffer { get; protected set; }

		/// <summary>
		/// バージョンアップで追加していくデータ（本来はこの形式で統一すべきだった･･･）
		/// </summary>
		public AdvCustomSaveData VersionUpBuffer { get; protected set; }
/*		
		/// <summary>
		/// スプライトの取得
		/// </summary>
		public Sprite GetSprite(float pixelsToUnits)
		{
			if (sprite == null )
			{
				if (Texture == null)
				{
					return null;
				}

				sprite = UtageToolKit.CreateSprite(Texture, pixelsToUnits);
			}
			return sprite;
		}
		Sprite sprite;
*/
		/// <summary>
		/// テクスチャ
		/// </summary>
		public Texture2D Texture
		{
			get{return texture;}
			set{
				texture = value;
				if(texture!=null)
				{
					if( texture.wrapMode != TextureWrapMode.Clamp )
					{
						texture.wrapMode = TextureWrapMode.Clamp;
					}
				}
			}
		}
		Texture2D texture;

		///パラメーターデータを読み込み
		public AdvParamManager ReadParam(AdvEngine engine)
		{
			AdvParamManager param = new AdvParamManager ();
			param.InitDefaultAll (engine.DataManager.SettingDataManager.DefaultParam);
			param.ReadSaveDataBuffer (paramBuf);
			return param;
		}

		///パラメーターデータ
		byte[] paramBuf;
		//グラフィックデータ
		byte[] graphicManagerBuf;
		//サウンドデータ
		byte[] soundManagerBuf;
		//選択肢データ
		byte[] selectionManagerBuf;

		/// <summary>
		/// 日付
		/// </summary>
		public System.DateTime Date { get { return this.date; } }
		System.DateTime date;

		/// <summary>
		/// タイトル
		/// </summary>
		public string Title { get { return this.title; } }
		string title = "";

		/// <summary>
		/// セーブデータのファイルパス
		/// </summary>
		public string Path { get { return this.path; } }
		string path;

		/// <summary>
		/// セーブされているか
		/// </summary>
		public bool IsSaved{get { return !string.IsNullOrEmpty(currentSenarioLabel); }	}

		//ファイルバージョン
		public int FileVersion { get { return this.fileVersion; } }
		int fileVersion;

		/// <summary>
		/// パラメーター
		/// </summary>
		public AdvParamManager Param { get; private set;}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="path">セーブデータのファイルパス</param>
		public AdvSaveData(SaveDataType type, string path)
		{
			this.type = type;
			this.path = path;
			this.Param = new AdvParamManager ();
		}

		/// <summary>
		/// クリア
		/// </summary>
		public void Clear()
		{
			currentSenarioLabel = "";

			if (Texture != null) UnityEngine.Object.Destroy(Texture);
			Texture = null;

//			if (sprite != null) UnityEngine.Object.Destroy(sprite);
//			sprite = null;

			paramBuf = null;
			graphicManagerBuf = null;
			soundManagerBuf = null;
		}

		/// <summary>
		/// ゲームのデータをセーブ
		/// </summary>
		/// <param name="engine">ADVエンジン</param>
		/// <param name="tex">セーブアイコン</param>
		public void SaveGameData(AdvEngine engine, Texture2D tex, List<IAdvCustomSaveDataIO> customSaveIoList, List<IAdvCustomSaveDataIO> verstionUpSaveIoList)
		{
			Clear();
			currentSenarioLabel = engine.Page.ScenarioLabel;
			currentPage = engine.Page.PageNo;
			currentGallerySceneLabel = engine.ScenarioPlayer.CurrentGallerySceneLabel;
			paramBuf = engine.Param.ToSaveDataBuffer();
			Param.ReadSaveDataBuffer(paramBuf);
			graphicManagerBuf = engine.GraphicManager.ToSaveDataBuffer();
			soundManagerBuf = SoundManager.GetInstance().ToSaveDataBuffer();
			selectionManagerBuf = engine.SelectionManager.ToSaveDataBuffer();
			JumpMangerBuffer = BinaryUtil.BinaryWrite(engine.ScenarioPlayer.JumpManager.Write);
			CustomBuffer = new AdvCustomSaveData();
			CustomBuffer.WriteCustomSaveData(customSaveIoList);

			VersionUpBuffer = new AdvCustomSaveData();
			VersionUpBuffer.WriteCustomSaveData(verstionUpSaveIoList);

			title = engine.Page.SaveDataTitle;

			Texture = tex;
			date = System.DateTime.Now;
		}


		/// <summary>
		/// オートセーブデータからセーブデータを作成
		/// </summary>
		/// <param name="autoSave">オートセーブデータ</param>
		/// <param name="tex">セーブアイコン</param>
		public void SaveGameData(AdvSaveData autoSave, AdvEngine engine, Texture2D tex)
		{
			Clear();
			currentSenarioLabel = autoSave.currentSenarioLabel;
			currentPage = autoSave.currentPage;
			currentGallerySceneLabel = autoSave.currentGallerySceneLabel;
			paramBuf = (byte[])autoSave.paramBuf.Clone();
			Param.ReadSaveDataBuffer(paramBuf);
			graphicManagerBuf = (byte[])autoSave.graphicManagerBuf.Clone();
			soundManagerBuf = (byte[])autoSave.soundManagerBuf.Clone();
			selectionManagerBuf = (byte[])autoSave.selectionManagerBuf.Clone();
			JumpMangerBuffer = (byte[])autoSave.JumpMangerBuffer.Clone();
			CustomBuffer = autoSave.CustomBuffer.Clone();
			VersionUpBuffer = autoSave.VersionUpBuffer.Clone();
			
			title = autoSave.title;

			Texture = tex;
			date = System.DateTime.Now;
		}

		/// <summary>
		/// ゲームのデータをロード
		/// </summary>
		/// <param name="engine">ADVエンジン</param>
		public void LoadGameData(AdvEngine engine)
		{
			engine.Param.ReadSaveDataBuffer(paramBuf);
			engine.GraphicManager.ReadSaveDataBuffer(engine,graphicManagerBuf);
			engine.SelectionManager.ReadSaveDataBuffer(selectionManagerBuf);
			engine.SoundManager.ReadSaveDataBuffer(soundManagerBuf);
		}

		static readonly int MagicID = FileIOManager.ToMagicID('S', 'a', 'v', 'e');	//識別ID
		public const int Version = 6;	//ファイルバージョン
		public const int Version5 = 5;
		public const int Version4 = 4;
		public const int Version3 = 3;
		public const int Version2 = 2;
		public const int Version1 = 1;

		/// <summary>
		/// バイナリ読み込み
		/// </summary>
		/// <param name="reader"></param>
		public void Read(BinaryReader reader)
		{
			Clear();
			int magicID = reader.ReadInt32();
			if (magicID != MagicID)
			{
				throw new System.Exception("Read File Id Error");
			}

			this.fileVersion = reader.ReadInt32();
			if (fileVersion >= Version1)
			{
				title = reader.ReadString();
				date = new System.DateTime(reader.ReadInt64());
				currentSenarioLabel = reader.ReadString();
				currentPage = reader.ReadInt32();
				if (fileVersion > Version1)
				{
					currentGallerySceneLabel = reader.ReadString();
				}

				int captureMemLen = reader.ReadInt32();
				if (captureMemLen > 0)
				{
					byte[] captureMem = reader.ReadBytes(captureMemLen);
					Texture2D tex = new Texture2D(1, 1, TextureFormat.RGB24, false);
					tex.LoadImage(captureMem);
					Texture = tex;
				}
				else
				{
					Texture = null;
				}

				paramBuf = reader.ReadBytes(reader.ReadInt32());
				Param.ReadSaveDataBuffer(paramBuf);

				graphicManagerBuf = reader.ReadBytes(reader.ReadInt32());
				soundManagerBuf = reader.ReadBytes(reader.ReadInt32());
				selectionManagerBuf = reader.ReadBytes(reader.ReadInt32());

				if (fileVersion > Version3)
				{
					JumpMangerBuffer = reader.ReadBytes(reader.ReadInt32());
				}
				else
				{
					JumpMangerBuffer = new byte[0];
				}

				if (fileVersion > Version4)
				{
					CustomBuffer = new AdvCustomSaveData(reader);
				}
				else
				{
					CustomBuffer = new AdvCustomSaveData();
				}

				if (fileVersion > Version5)
				{
					VersionUpBuffer = new AdvCustomSaveData(reader);
				}
				else
				{
					VersionUpBuffer = new AdvCustomSaveData();
				}
			}
			else
			{
				throw new System.Exception(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.UnknownVersion, fileVersion));
			}
		}

		/// <summary>
		/// バイナリ書き込み
		/// </summary>
		/// <param name="writer">バイナリライター</param>
		public void Write(BinaryWriter writer)
		{
			date = System.DateTime.Now;

			writer.Write(MagicID);
			writer.Write(Version);
			writer.Write(title);
			writer.Write(date.Ticks);
			writer.Write(currentSenarioLabel);
			writer.Write(currentPage);
			writer.Write(currentGallerySceneLabel);

			if (Texture != null)
			{
				byte[] captureMem = Texture.EncodeToPNG();
				writer.Write(captureMem.Length);
				writer.Write(captureMem);
			}
			else
			{
				writer.Write(0);
			}
			writer.Write(paramBuf.Length);
			writer.Write(paramBuf);
			writer.Write(graphicManagerBuf.Length);
			writer.Write(graphicManagerBuf);
			writer.Write(soundManagerBuf.Length);
			writer.Write(soundManagerBuf);
			writer.Write(selectionManagerBuf.Length);
			writer.Write(selectionManagerBuf);
			writer.Write(JumpMangerBuffer.Length);
			writer.Write(JumpMangerBuffer);
			CustomBuffer.Write(writer);
			VersionUpBuffer.Write(writer);
		}
	}
}
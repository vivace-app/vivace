/****************************************************************************
 *
 * Copyright (c) 2021 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/
 
using UnityEngine;
using System;

/**
 * \addtogroup CRIWARE_EDITOR_UTILITY
 * @{
 */

/**
 * <summary>
 * CriWare.Editor 名前空間
 * </summary>
 * <remarks>
 * <para header='説明'>
 * UnityEditor 環境で使用できるクラスをまとめた名前空間です。
 * </para>
 * </remarks>
 */
namespace CriWare.Editor {

/**
 * <summary> CRI Atom エディタ専用機能クラス </summary>
 * <remarks>
 * <para header='説明'>
 * CRI Atomのエディタ専用機能をまとめるクラスです。</para>
 * <para header='注意'>
 * AssemblyDefinition版プラグインで上記クラスを使用する場合、<br/>
 * 参照元のアセンブリ定義に以下のアセンブリ定義への参照が必要になります。<br/>
 * - CriMw.CriWare.Editor<br/>
 * - CriMw.CriWare.Runtime<br/>
 * </para>
 * </remarks>
 */
public class CriAtomEditorUtilities
{
	/**
	 * <summary>エディタでCRI Atomライブラリ初期化</summary>
	 * <remarks>
	 * <para header='説明'>
	 * Editモード専用のCRI Atomライブラリを初期化するためのメソッドです。<br/>
	 * 現在開いているシーンにCriWareInitializerコンポーネントがある場合はその設定で、<br/>
	 * ない場合はデフォルト設定で初期化を行います。</para>
	 * </remarks>
	 */
	public static void InitializeLibrary() {
		if (CriAtomPlugin.IsLibraryInitialized()) {
			return;
		}
		
		CriWareInitializer criInitializer = GameObject.FindObjectOfType<CriWareInitializer>();
		if (criInitializer != null) {
			CriWareInitializer.InitializeAtom(criInitializer.atomConfig);
		} else {
			CriWareInitializer.InitializeAtom(new CriAtomConfig());
			Debug.Log("[CRIWARE] Atom: Can't find CriWareInitializer component; Using default parameters in edit mode.");
		}
	}

	/**
	 * <summary>エディタプレビュー専用音声プレイヤー</summary>
	 * <remarks>
	 * <para header='説明'>
	 * EditモードでADX2音声をプレビュー再生するためのクラスです。<br/>
	 * ACBデータのロードは外部で管理する必要があります。</para>
	 * </remarks>
	 */
	public class PreviewPlayer : CriDisposable {
		public CriAtomExPlayer player { get; private set; }
		private bool finalizeSuppressed = false;
		private bool isPlayerReady = false;

		private void Initialize() {
			CriAtomEditorUtilities.InitializeLibrary();
			if (CriAtomPlugin.IsLibraryInitialized() == false) { return; }

			player = new CriAtomExPlayer();
			if (player == null) { return; }

			player.SetPanType(CriAtomEx.PanType.Pan3d);
			player.UpdateAll();

			isPlayerReady = true;

			CriDisposableObjectManager.Register(this, CriDisposableObjectManager.ModuleType.Atom);

			if (finalizeSuppressed) {
				GC.ReRegisterForFinalize(this);
			}
		}

		/**
		 * <summary>プレイヤーの初期化</summary>
		 */
		public PreviewPlayer() {
			Initialize();
		}

		/**
		 * <summary>プレイヤーの破棄</summary>
		 */
		public override void Dispose() {
			this.dispose();
			GC.SuppressFinalize(this);
			finalizeSuppressed = true;
		}

		private void dispose() {
			CriDisposableObjectManager.Unregister(this);
			if (player != null) {
				player.Dispose();
				player = null;
			}
			this.isPlayerReady = false;
		}

		~PreviewPlayer() {
			this.dispose();
		}
		
		/**
		 * <summary>音声データを設定して再生</summary>
		 * <param name='acb'>ACBデータ</param>
		 * <param name='cueName'>キュー名</param>
		 * <remarks>
		 * <para header='説明'>
		 * ACBデータとキュー名を指定して再生します。</para>
		 * </remarks>
		 */
		public void Play(CriAtomExAcb acb, string cueName) {
			if (isPlayerReady == false) {
				this.Initialize();
			}

			if (acb != null) {
				if (player != null) {
					player.SetCue(acb, cueName);
					player.Start();
				} else {
					Debug.LogWarning("[CRIWARE] Player is not ready. Please try reloading the inspector / editor window");
				}
			} else {
				Debug.LogWarning("[CRIWARE] ACB data is not set for previewing playback");
			}
		}

		/**
		 * <summary>再生を停止</summary>
		 * <param name='withoutRelease'>リリース時間なしで停止させるかどうか</param>
		 * <remarks>
		 * <para header='説明'>
		 * 再生中のすべての音声を停止します。</para>
		 * </remarks>
		 */
		public void Stop(bool withoutRelease = false) {
			if (player != null) {
				if (withoutRelease) {
					player.StopWithoutReleaseTime();
				} else {
					player.Stop();
				}
			}
		}

		/**
		 * <summary>プレイヤーのパラメータをリセット</summary>
		 */
		public void ResetPlayer() {
			player.SetVolume(1f);
			player.SetPitch(0);
			player.Loop(false);
		}
	}

} // end of class

} //namespace CriWare.Editor

/**
 * @}
 */

/* end of file */
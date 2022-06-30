/****************************************************************************
 *
 * Copyright (c) 2012 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#pragma warning disable 0618

using UnityEngine;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/**
 * \addtogroup CRIWARE_UNITY_COMPONENT
 * @{
 */
 namespace CriWare {

/**
 * <summary>CRIWAREエラーオブジェクト</summary>
 * <remarks>
 * <para header='説明'>CRIWAREライブラリのエラーメッセージを取得し出力するコンポーネントです。<br/></para>
 * </remarks>
 */
[AddComponentMenu("CRIWARE/Error Handler")]
public class CriWareErrorHandler : CriMonoBehaviour{
	/**
	 * \deprecated
	 * 削除予定の非推奨APIです。本値は使用されていません。
	 * <summary>コンソールデバッグ出力を有効にするかどうか</summary>
	 * <remarks>
	 * <para header='注意'>Unityデバッグウィンドウだけでなく、コンソールデバッグ出力を有効にするかどうか [deprecated]
	 * PCの場合はデバッグウィンドウに出力されます。</para>
	 * </remarks>
	 */
	public bool enableDebugPrintOnTerminal = false;

	/** エラー発生時に強制的にクラッシュさせるかどうか(デバッグ用) */
	public bool enableForceCrashOnError = false;

	/** シーンチェンジ時にエラーハンドラを削除するかどうか */
	public bool dontDestroyOnLoad = true;

	/**
	 * \deprecated
	 * 削除予定の非推奨APIです。本値は使用されていません。
	 */
	public static string errorMessage { get; set; }

	/**
	 * <summary>ログメッセージプレフィックス</summary>
	 * <remarks>
	 * <para header='説明'>CRIWAREによるログメッセージを示すプレフィックスです。</para>
	 * </remarks>
	 */
	public static readonly string logPrefix = "[CRIWARE]";

	/**
	 * <summary>エラーコールバックデリゲート</summary>
	 * <remarks>
	 * <para header='説明'>CRIWAREネイティブライブラリ内でエラーが発生した際に呼び出されるコール
	 * バックデリゲートです。<br/>
	 * 引数の文字列には、"エラーID:エラー内容"のフォーマットでメッセージが
	 * 記載されています。</para>
	 * </remarks>
	 */
	public delegate void Callback(string message);

	/**
	 * <summary>エラーコールバックイベント</summary>
	 * <remarks>
	 * <para header='説明'>CRIWAREネイティブライブラリ内でエラーが発生した際に呼び出されるコール
	 * バックイベントです。<br/>
	 * 未設定時には、本クラス内に定義されているデフォルトのログ出力関数が
	 * 呼び出されます。<br/>
	 * エラーメッセージを元に独自の処理を記述したい場合、デリゲートを登録して
	 * コールバック関数内で処理を行ってください。<br/>
	 * 本イベントは必ずメインスレッドから呼び出されます。</para>
	 * <para header='注意'>登録したコールバックは、CriWareErrorHandlerが生存中は常に呼び出される
	 * 可能性があります。<br/>
	 * 呼び出し先関数の実体が、CriWareErrorHandlerよりも先に解放されないように
	 * ご注意ください。</para>
	 * </remarks>
	 */
	public static event Callback OnCallback = null;

	/**
	 * \deprecated
	 * 削除予定の非推奨APIです。
	 * CriWareErrorHandler.OnCallback event の使用を検討してください。
	 * <summary>エラーコールバック</summary>
	 * <remarks>
	 * <para header='説明'>CRIWAREネイティブライブラリ内でエラーが発生した際に呼び出されるコール
	 * バックです。<br/>
	 * 未設定時には、本クラス内に定義されているデフォルトのログ出力関数が
	 * 呼び出されます。<br/>
	 * エラーメッセージを元に独自の処理を記述したい場合、デリゲートを登録して
	 * コールバック関数内で処理を行ってください。<br/>
	 * 登録を解除する場合は null を設定してください。</para>
	 * <para header='注意'>登録したコールバックは、CriWareErrorHandlerが生存中は常に呼び出される
	 * 可能性があります。<br/>
	 * 呼び出し先関数の実体が、CriWareErrorHandlerよりも先に解放されないように
	 * ご注意ください。</para>
	 * </remarks>
	 */
	[Obsolete("CriWareErrorHandler.callback is deprecated. Use CriWareErrorHandler.OnCallback event", false)]
	public static Callback callback = null;

	/**
	* \deprecated
	* 削除予定の非推奨APIです。
	* 本値によらずエラーメッセージがキューイングできるようになったため、本値は参照されません。
	 */
	public uint messageBufferCounts = 8;

	private ConcurrentQueue<string> unThreadSafeMessages = new ConcurrentQueue<string>();

	/* オブジェクト作成時の処理 */
	void Awake() {
		/* 初期化カウンタの更新 */
		initializationCount++;
		if (initializationCount != 1) {
			/* 多重初期化は許可しない */
			GameObject.Destroy(this);
			return;
		}

		if (!CriErrorNotifier.IsRegistered(HandleMessage)) {
			CriErrorNotifier.OnCallbackThreadUnsafe += HandleMessage;
		}

		/* シーンチェンジ後もオブジェクトを維持するかどうかの設定 */
		if (dontDestroyOnLoad) {
			DontDestroyOnLoad(transform.gameObject);
		}
	}

	/* Execution Order の設定を確実に有効にするために OnEnable をオーバーライド */
	protected override void OnEnable() {
		base.OnEnable();
		if (!CriErrorNotifier.IsRegistered(HandleMessage)) {
			CriErrorNotifier.OnCallbackThreadUnsafe += HandleMessage;
		}
	}

	protected override void OnDisable() {
		base.OnDisable();
		if (CriErrorNotifier.IsRegistered(HandleMessage)) {
			CriErrorNotifier.OnCallbackThreadUnsafe -= HandleMessage;
		}
	}

	public override void CriInternalUpdate() {
		DequeueErrorMessages();
	}

	public override void CriInternalLateUpdate() { }

	void OnDestroy() {
		/* 初期化カウンタの更新 */
		initializationCount--;
		if (initializationCount != 0) {
			return;
		}

		/* エラー処理の終了処理 */
		if (CriErrorNotifier.IsRegistered(HandleMessage)) {
			CriErrorNotifier.OnCallbackThreadUnsafe -= HandleMessage;
		}
	}

	/* エラーメッセージのポーリングと出力 */
	private void DequeueErrorMessages() {
		string dequeuedMessage;
		while (unThreadSafeMessages.Count != 0) {
			if (!unThreadSafeMessages.TryDequeue(out dequeuedMessage)) {
				continue;
			}
			if (OnCallback != null) {
				OnCallback(dequeuedMessage);
			}
			if (callback != null) {
				callback(dequeuedMessage);
			}
		}
	}

	private void HandleMessage(string errmsg) {
		if (errmsg == null) {
			return;
		}

		if (OnCallback == null && callback == null) {
			OutputDefaultLog(errmsg);
		} else {
			unThreadSafeMessages.Enqueue(errmsg);
		}
		if (enableForceCrashOnError) {
			UnityEngine.Diagnostics.Utils.ForceCrash(UnityEngine.Diagnostics.ForcedCrashCategory.Abort);
		}
	}

	/** デフォルトのログ出力 */
	private static void OutputDefaultLog(string errmsg)
	{
		if (errmsg.StartsWith("E")) {
			Debug.LogError(logPrefix + " Error:" + errmsg);
		} else if (errmsg.StartsWith("W")) {
			Debug.LogWarning(logPrefix + " Warning:" + errmsg);
		} else {
			Debug.Log(logPrefix + errmsg);
		}
	}

	/** 初期化カウンタ */
	private static int initializationCount = 0;

} // end of class

	/**
	 * <summary>CRIWAREネイティブライブラリのエラーログ取得</summary>
	 * <remarks>
	 * <para header='説明'>CRIWAREネイティブライブラリ内で発生したエラーログを取得するクラスです。<br/></para>
	 * </remarks>
	 */
	public class CriErrorNotifier {
		/**
		 * <summary>エラーコールバックデリゲート</summary>
		 * <remarks>
		 * <para header='説明'>CRIWAREネイティブライブラリ内でエラーが発生した際に呼び出されるコール
		 * バックデリゲートです。<br/>
		 * 引数の文字列には、"エラーID:エラー内容"のフォーマットでメッセージが
		 * 記載されています。</para>
		 * </remarks>
		 * <seealso cref='CriErrorNotifier::OnCallbackThreadUnsafe'/>
		 */
		public delegate void Callback(string message);
		private static event Callback _onCallbackThreadUnsafe = null;
		private static object objectLock = new System.Object();

		/**
		 * <summary>エラーコールバックイベント</summary>
		 * <remarks>
		 * <para header='説明'>CRIWAREネイティブライブラリ内でエラーが発生した際に呼び出されるコールバックイベントです。<br/>
		 * 未設定時はログが出力されません。<br/></para>
		 * <para header='注意'>本イベントはメインスレッド外から呼ばれることがあります。<br/>
		 * したがって、本イベントには必ずスレッドセーフなAPIを登録してください。<br/></para>
		 * </remarks>
		 * <seealso cref='CriErrorNotifier::IsRegistered'/>
		 */
		public static event Callback OnCallbackThreadUnsafe {
			add {
				lock (objectLock) {
					if (_onCallbackThreadUnsafe == null || _onCallbackThreadUnsafe.GetInvocationList().Length <= 0) {
						NativeMethod.criErr_SetCallback(null);
						NativeMethod.criErr_SetCallback(ErrorCallbackFromNative);
					}
					_onCallbackThreadUnsafe += value;
				}
			}
			remove {
				lock (objectLock) {
					_onCallbackThreadUnsafe -= value;
					if (_onCallbackThreadUnsafe == null || _onCallbackThreadUnsafe.GetInvocationList().Length <= 0) {
						NativeMethod.criErr_SetCallback(null);
					}
				}
			}
		}

		/**
		 * <summary>登録済みエラーコールバックイベントの確認</summary>
		 * <param name='target'>評価したいメソッド</param>
		 * <returns>登録されているかどうか</returns>
		 * <remarks>
		 * <para header='説明'><see cref='CriErrorNotifier.OnCallbackThreadUnsafe'/> に登録されているメソッドかどうか調べます。<br/>
		 * 多重登録や、解放忘れなどを調べたい場合に使用できます。<br/></para>
		 * </remarks>
		 * <seealso cref='CriErrorNotifier::Callback'/>
		 * <seealso cref='CriErrorNotifier::OnCallbackThreadUnsafe'/>
		 */
		public static bool IsRegistered(Callback target) {
			if (_onCallbackThreadUnsafe == null) {
				return false;
			}
			foreach (Callback item in _onCallbackThreadUnsafe.GetInvocationList()) {
				if (item == target) {
					return true;
				}
			}
			return false;
		}

		/**
		 * <summary>プラグイン内部関数</summary>
		 * <para header='注意'>ユーザーが本関数を呼び出すことは想定されていません。<br/></para>
		 */
		public static void CallEvent(string message) {
			// for expansion
			if (_onCallbackThreadUnsafe != null) {
				_onCallbackThreadUnsafe(message);
			}
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void ErrorCallbackFunc(IntPtr errmsgPtr, System.UInt32 p1, System.UInt32 p2, IntPtr parray);

		[AOT.MonoPInvokeCallback(typeof(ErrorCallbackFunc))]
		private static void ErrorCallbackFromNative(IntPtr errmsgPtr, System.UInt32 p1, System.UInt32 p2, IntPtr parray) {
			string errmsg = Marshal.PtrToStringAnsi(NativeMethod.criErr_ConvertIdToMessage(errmsgPtr, p1, p2));
			CallEvent(errmsg);
		}

		protected static class NativeMethod {
#if !CRIWARE_ENABLE_HEADLESS_MODE
			[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
			internal static extern void criErr_SetCallback(ErrorCallbackFunc callback);
			[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
			internal static extern IntPtr criErr_ConvertIdToMessage(IntPtr errmsgPtr, System.UInt32 p1, System.UInt32 p2);
#else
			internal static void criErr_SetCallback(ErrorCallbackFunc callback) { }
			internal static IntPtr criErr_ConvertIdToMessage(IntPtr errmsgPtr, System.UInt32 p1, System.UInt32 p2) { return IntPtr.Zero; }
#endif
		}
	}

} //namespace CriWare
/** @} */

/* --- end of file --- */

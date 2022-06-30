/****************************************************************************
 *
 * Copyright (c) 2011 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;
using System;
using System.Collections;

/**
 * \addtogroup CRIATOM_UNITY_COMPONENT
 * @{
 */

namespace CriWare {

/**
 * <summary>音声再生を行うコンポーネントです。</summary>
 * <remarks>
 * <para header='説明'>任意のGameObjectに付加して使用します。<br/>
 * 再生するキューが3Dポジショニングを行うように設定されている場合、3D再生を行います。
 * この際、 CriWare.CriAtomListener が付加されているGameObjectの位置との間で定位計算を行うため、
 * カメラやメインキャラクタに CriWare.CriAtomListener を付加しておく必要があります。<br/>
 * Public変数は基本的にUnityEditor上で設定します。</para>
 * </remarks>
 */
[AddComponentMenu("CRIWARE/CRI Atom Source")]
public class CriAtomSource : CriAtomSourceBase
{
	#region Variables
	[SerializeField]
	private string _cueName = "";
	[SerializeField]
	private string _cueSheet = "";
	#endregion

	#region Properties
	/**
	 * <summary>再生するキュー名を設定／取得します。</summary>
	 * <remarks>
	 * <para header='説明'>CriAtomSource::Play() 関数を呼び出した場合や、
	 * CriAtomSource::playOnStart プロパティの設定により実行開始時に再生する場合には、
	 * 本プロパティで設定されているキューを再生します。</para>
	 * </remarks>
	 */
	public string cueName {
		get {return this._cueName;}
		set {this._cueName = value;}
	}

	/**
	 * <summary>キューシート名を設定／取得します。</summary>
	 * <remarks>
	 * <para header='説明'>CriAtomSource::Play 関数や CriAtomSource::cueName プロパティで指定したキューは、
	 * 本プロパティで設定されているキューシートから検索されます。</para>
	 * </remarks>
	 */
	public string cueSheet {
		get {return this._cueSheet;}
		set {this._cueSheet = value;}
	}
	#endregion

	#region Functions

	/**
	 * <summary>設定されているキューを再生開始します。</summary>
	 * <returns>再生ID</returns>
	 * <remarks>
	 * <para header='説明'>どのキューを再生するかは、事前に CriAtomSource::cueName
	 * プロパティにより設定しておく必要があります。</para>
	 * </remarks>
	 */
	public override CriAtomExPlayback Play()
	{
		return this.Play(this.cueName);
	}

	protected override CriAtomExAcb GetAcb()
	{
		CriAtomExAcb acb = null;
		if (!String.IsNullOrEmpty(this.cueSheet)) {
			acb = CriAtom.GetAcb(this.cueSheet);
		}
		return acb;
	}

	/**
	 * <summary>設定されているキューを再生開始します。</summary>
	 * <remarks>
	 * <para header='説明'>事前に CriAtomSource::playOnStart, CriAtomSource::cueName
	 * プロパティを設定しておく必要があります。</para>
	 * </remarks>
	 */
	protected override void PlayOnStart()
	{
		if (this.playOnStart && !String.IsNullOrEmpty(this.cueName)) {
			StartCoroutine(PlayAsync(this.cueName));
		}
	}

	/**
	 * <summary>非同期に、指定したキュー名のキューを再生開始します。</summary>
	 * <param name='cueName'>キュー名</param>
	 * <returns>コルーチン</returns>
	 * <remarks>
	 * <para header='説明'>Unityのコルーチン機能を使い、非同期に実行されます。
	 * 本関数は MonoBehaviour::StartCoroutine の引数に指定して呼び出してください。</para>
	 * </remarks>
	 */
	private IEnumerator PlayAsync(string cueName)
	{
		CriAtomExAcb acb = null;
		while (acb == null && !String.IsNullOrEmpty(this.cueSheet)) {
			acb = CriAtom.GetAcb(this.cueSheet);
			if (acb == null) {
				yield return null;
			}
		}
		this.player.SetCue(acb, cueName);
		InternalPlayCue();
	}

	#endregion
}

} //namespace CriWare
/** @} */
/* end of file */

/****************************************************************************
 *
 * Copyright (c) 2020 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;
using System.Collections.Generic;

/**
 * \addtogroup CRIATOM_UNITY_COMPONENT
 * @{
 */

namespace CriWare {

/**
 * <summary>3Dリージョンコンポーネント</summary>
 * <remarks>
 * <para header='説明'>3D音源、3Dリスナー及びトランシーバーに対して空間によるグループ化を行うコンポーネントです。<br/>
 * 任意のGameObjectに付加して使用します。<br/>
 * CriAtomSource 、 CriAtomListener 、 CriAtomTransceiver のregion3dの値、<br/>
 * または初期のリージョン設定として設定できます。<br/>
 * 本コンポーネントは一つのGameObjectに一つのみ取り付けられます。</para>
 * </remarks>
 * <seealso cref='CriAtomSource'/>
 * <seealso cref='CriAtomListener'/>
 * <seealso cref='CriAtomTransceiver'/>
 */
[AddComponentMenu("CRIWARE/CRI Atom Region"), DisallowMultipleComponent]
public class CriAtomRegion : CriMonoBehaviour
{
	#region Fields & Properties
	/**
	 * <summary>内部で使用している CriAtomEx3dRegion です。</summary>
	 * <remarks>
	 * <para header='説明'>CriAtomEx3dRegion を直接制御する場合にはこのプロパティから CriAtomEx3dRegion を取得してください。</para>
	 * </remarks>
	 */
	public CriAtomEx3dRegion region3dHn { get; protected set; }
	#endregion

	#region Private Fields & Properties
	internal List<CriAtomSourceBase> referringSources = new List<CriAtomSourceBase>();
	internal List<CriAtomListener> referringListeners = new List<CriAtomListener>();
	internal List<CriAtomTransceiver> referringTransceivers = new List<CriAtomTransceiver>();
	#endregion

	#region Methods

	private void Awake()
	{
		this.InternalInitialize();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.InitializeParameters();
	}

	private void OnDestroy()
	{
		this.InternalFinalize();
	}

	protected virtual void InternalInitialize()
	{
		CriAtomPlugin.InitializeLibrary();
		this.region3dHn = new CriAtomEx3dRegion();
	}

	protected virtual void InternalFinalize()
	{
		/* Clear references to this region */
		while (referringSources.Count > 0) {
			referringSources[0].region3d = null;
		}
		referringSources.Clear();
		while (referringListeners.Count > 0) {
			referringListeners[0].region3d = null;
		}
		referringListeners.Clear();
		while(referringTransceivers.Count > 0) {
			referringTransceivers[0].region3d = null;
		}
		referringTransceivers.Clear();

		region3dHn.Dispose();
		region3dHn = null;
		CriAtomPlugin.FinalizeLibrary();
	}

	protected virtual void InitializeParameters()
	{
		if (this.region3dHn == null) {
			Debug.LogError("[CRIWARE] Internal: CriAtomEx3dRegion is not created correctly.", this);
			return;
		}
	}

	public override void CriInternalUpdate() { }

	public override void CriInternalLateUpdate() { }

	#endregion
} // end of class

} //namespace CriWare
/** @} */
/* end of file */
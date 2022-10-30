using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace FantomLib
{
    /// <summary>
    /// 長押しを取得してコールバックする [(新)InputSystem / (旧)InputManager 両対応版]
    ///（UI上での判定に向いている。EventSystem と Graphics Raycaster が必要）
    /// 2020/10/18 Fantom (Unity 2019.4, InputSystem 1.0.0 以降)
    /// http://fantom1x.blog130.fc2.com/blog-entry-375.html
    /// http://fantom1x.blog130.fc2.com/blog-entry-251.html
    ///（使い方）
    ///・Image や Text, Button などの UI を持つ GameObject にアタッチして、インスペクタから OnLongClick（引数なし）にコールバックする関数を登録すれば使用可。
    ///・シーンに EventSystem、(ルート)Canvas に Graphics Raycaster がアタッチされている必要がある。
    ///（仕様説明）
    ///・EventSystem からのイベント（OnPointerDown, OnPointerUp, OnPointerExit）を取得し、一定時間（Valid Time）押下され続けていたら長押しと認識する。
    ///・途中で有効領域外（UIから外れる）へ出たり、指を離したりしたときは無効。
    ///・はじめの指のみ認識（複数指の場合、ピンチの可能性があるため無効とする）。
    ///※スマホだとUIを透過にしていると、上手く認識できないようなので注意（なるべく不透明画像が良い）。
    /// </summary>
    public class LongClickEventTrigger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {

#region Inspector settings Section

        //==========================================================
        //Inspector Settings

        #region Inspector settings

        //長押しとして認識する時間 [秒]（これより長い時間で長押しとして認識する）
        public float validTime = 1.0f;

        #endregion Inspector settings

        //==========================================================
        //Callbacks

        #region Callbacks

        //長押しイベント発生コールバック
        public UnityEvent OnLongClick;

        //長押し進捗開始のイベントコールバック
        public UnityEvent OnStart;

        //進捗中のイベントコールバック
        [Serializable] public class ProgressHandler : UnityEvent<float> { }  //進捗 0～1f（0～100%）
        public ProgressHandler OnProgress;

        //進捗中断のイベントコールバック
        public UnityEvent OnCancel;

        #endregion Callbacks

        //==========================================================

#endregion Inspector settings Section


#region Properties and Local values Section

        //==========================================================
        //Property

        #region Properties

        //稼働フラグ（一時的な有効・無効化に利用）
        bool _isActive = true;
        public bool IsActive { 
            get {
                return _isActive;
            }
            set {
                //切り替えた時は、一旦初期化する
                if (value != _isActive)
                {
                    CancelAndInvokeCallback();
                    ResetInternalValue();
                }
                _isActive = value;
            }
        }

        //長押検出プロパティ（フレーム毎取得用：完了した瞬間のみ true）
        public bool IsLongClick { get; private set; }

        #endregion Properties

        //==========================================================
        //Local Values

        #region Local values

        //長押し認識時刻（この時刻を超えたら長押しとして認識する）
        float requiredTime;

        //押下中フラグ（単一指のみの取得としても利用）
        bool pressing = false;

        #endregion Local values

        //==========================================================

#endregion Properties and Local values Section


#region Unity life cycle Section

        //==========================================================

        //アクティブになったら、初期化する（アプリの中断などしたときはリセットする）
        private void OnEnable()
        {
            ResetInternalValue();
        }

        private void OnDisable()
        {
            CancelAndInvokeCallback();
        }

        //==========================================================

        // Update is called once per frame
        private void Update()
        {
            if (IsActive)
            {
                UpdateLongClickOperation();
            }
        }

        //==========================================================

#endregion Unity life cycle Section


#region LongClick operation Section

        //==========================================================

        //長押し操作検出
        void UpdateLongClickOperation()
        {
            //フレーム毎にリセットする
            IsLongClick = false;

            if (pressing)  //はじめに押した指のみとなる
            {
                if (requiredTime < Time.time)  //一定時間過ぎたら認識
                {
                    IsLongClick = true;

                    if (OnLongClick != null)
                        OnLongClick.Invoke();

                    pressing = false;          //長押し完了したら無効にする
                }
                else
                {
                    if (OnProgress != null)
                    {
                        float amount = Mathf.Clamp01(1f - (requiredTime - Time.time) / validTime);  //0～1f
                        OnProgress.Invoke(amount);
                    }
                }
            }
        }


        //UI領域内で押下
        public void OnPointerDown(PointerEventData data)
        {
            if (!IsActive)
                return;

            if (!pressing)  //ユニークにするため
            {
                pressing = true;
                requiredTime = Time.time + validTime;

                if (OnStart != null)
                    OnStart.Invoke();
            }
            else
            {
                //２本以上の指の場合、ピンチの可能性があるため無効にする
                CancelAndInvokeCallback();
            }
        }


        //※スマホだとUIを透過にしていると、指を少し動かしただけでも反応してしまうので注意
        public void OnPointerUp(PointerEventData data)
        {
            CancelAndInvokeCallback();
        }


        //UI領域から外れたら無効にする
        public void OnPointerExit(PointerEventData data)
        {
            CancelAndInvokeCallback();
        }

        //==========================================================

        //キャンセル処理(フラグ)とコールバック発火
        void CancelAndInvokeCallback()
        {
            if (pressing)
            {
                pressing = false;

                if (OnCancel != null)
                    OnCancel.Invoke();
            }
        }

        //==========================================================

        //内部処理の値リセット (※最低限のみ)
        void ResetInternalValue()
        {
            pressing = false;
            IsLongClick = false;
        }

        //==========================================================

#endregion LongClick operation Section

    }
}
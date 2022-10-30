using System;
using UnityEngine;
using UnityEngine.Events;

namespace FantomLib
{
    /// <summary>
    /// スワイプ方向を取得してコールバックする [(新)InputSystem / (旧)InputManager 両対応版]
    ///（画面全体、または一部 [上半分、下半分など] での操作に向いている）
    /// 2020/10/18 Fantom (Unity 2019.4, InputSystem 1.0.0 以降)
    /// http://fantom1x.blog130.fc2.com/blog-entry-374.html
    /// http://fantom1x.blog130.fc2.com/blog-entry-250.html
    ///（使い方）
    ///・適当な GameObject にアタッチして、インスペクタから OnSwipe（Vector2 を１つ引数にとる）にコールバックする関数を登録すれば使用可。
    ///・またはプロパティ SwipeInput.Direction をフレーム毎監視しても良い（こちらの場合は無し（Vector2.zero）も含まれる）。
    ///・プロパティの Direction はデフォで方向のみ（Vector2.left / .right / .up / .down）。returnsRawValue = true で移動量(px)を返す。
    ///・ActiveInputHandling が Both のときのみ useInputSystemIfBothHandling が有効。
    ///（仕様説明）
    ///・タッチの移動量（エディタやスマホ以外の場合はマウス）で判定する。画面幅の Valid Width（％）以上移動したときスワイプとして認識する。
    ///・ただし、移動が制限時間（Timeout）を超えた時は無視する。
    ///・複数の指では認識できない（※２つ以上の指の場合はピンチの可能性もあるため無効とする）。
    ///・タッチデバイスを InputCompatible クラスで UNITY_ANDROID, UNITY_IOS (SUPPORT_TOUTH_AT_RUNTIME) としているので、他のデバイスも加えたい場合は #if の条件文にデバイスを追加する（タッチが取得できるもののみ）。
    /// </summary>
    public class SwipeInput : MonoBehaviour
    {

#region Inspector settings Section

        //==========================================================
        //Inspector Settings

        #region Inspector settings

        //ActiveInputHandling が Both のとき、InputSystem を使う（※Both 以外のときは無効）
        [Tooltip("Use InputSystem when ActiveInputHandling is Both (* Invalid when other than Both)")]
        public bool useInputSystemIfBothHandling = false;  //true = InputSystem / false = InputManager を使う

        //画面幅（Screen.width）サイズを Valid Width 比率の基準にする（false=高さ（Screen.height）を基準）
        public bool widthReference = true;

        //スワイプとして認識する移動量の画面比[画面幅に対する比率]（0.0～1.0：1.0で端から端まで。この値より長い移動量でスワイプとして認識する）
        [Range(0f, 1f)] public float validWidth = 0.25f;

        //スワイプとして認識する時間 [秒]（これより短い時間でスワイプとして認識する）
        public float timeout = 0.5f;

        //スワイプとして認識する画面領域（0.0～1.0）[(0,0):画面左下, (1,1):画面右上]
        public Rect validArea = new Rect(0, 0, 1, 1);

        #endregion Inspector settings

        //==========================================================
        //Callbacks

        #region Callbacks

        //スワイプイベントコールバック
        [Serializable]
        public class SwipeHandler : UnityEvent<Vector2> { }  //スワイプ方向
        public SwipeHandler OnSwipe;     //Vector2.left / .right / .up / .down 
        public SwipeHandler OnSwipeRaw;  //移動量[px]

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
                    ResetInternalValue();
                }
                _isActive = value;
            }
        }

        //スワイプ方向（フレーム毎取得用）
        public Vector2 Direction { get; private set; }  //Vector2.left / .right / .up / .down

        //スワイプ移動量：開始～終了位置差分 (endPos - startPos)
        public Vector2 DirectionRaw { get; private set; }  //実際の px (※画面解像度に依存するので注意)

        #endregion Properties

        //==========================================================
        //Local Values

        #region Local values

        //スワイプ開始座標
        Vector2 startPos;

        //スワイプ終了座標
        Vector2 endPos;

        //スワイプ時間制限（この時刻を超えたらスワイプとして認識しない）
        float limitTime;

        //押下中フラグ（単一指のみの取得にするため）
        bool pressing;

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

        //==========================================================

        // Update is called once per frame
        private void Update()
        {
            if (IsActive)
            {
                UpdateSwipeOperation();
            }
        }

        //==========================================================

#endregion Unity life cycle Section


#region Swipe operation Section

        //==========================================================

        //スワイプ操作検出
        private void UpdateSwipeOperation()
        {
            //フレーム毎にリセットする
            Direction = DirectionRaw = Vector2.zero;

            //複数の指は不可とする（※２つ以上の指の場合はピンチの可能性もあるため）
            if (IsTouchCount1OrNotSupportPlatform)  //※Editor, PC は無条件で true
            {
                //押したとき（タッチ/左クリック）[※タッチ１つのみ]
                if (!pressing && IsPointerDown)
                {
                    //開始位置を記録
                    startPos = CurrentPosition;

                    //認識エリア内か？
                    if (validArea.xMin * Screen.width <= startPos.x && startPos.x <= validArea.xMax * Screen.width &&
                        validArea.yMin * Screen.height <= startPos.y && startPos.y <= validArea.yMax * Screen.height)
                    {
                        pressing = true;
                        limitTime = Time.time + timeout;
                    }
                }
                //離したとき（タッチ/左クリック）[※タッチ１つで、既に押されているときのみ]
                else if (pressing && IsPointerUp)
                {
                    pressing = false;

                    //時間制限前なら認識
                    if (Time.time < limitTime)
                    {
                        //終了位置を取得して判定
                        endPos = CurrentPosition;

                        DirectionRaw = endPos - startPos;
                        float dx = Mathf.Abs(DirectionRaw.x);
                        float dy = Mathf.Abs(DirectionRaw.y);
                        float requiredPx = widthReference ? Screen.width * validWidth : Screen.height * validWidth;

                        //横方向として認識
                        if (dy < dx)
                        {
                            //長さを超えていたら認識
                            if (requiredPx < dx)
                                Direction = Mathf.Sign(DirectionRaw.x) < 0 ? Vector2.left : Vector2.right;
                        }
                        //縦方向として認識
                        else
                        {
                            //長さを超えていたら認識
                            if (requiredPx < dy)
                                Direction = Mathf.Sign(DirectionRaw.y) < 0 ? Vector2.down : Vector2.up;
                        }

                        if (Direction != Vector2.zero)
                        {
                            if (OnSwipe != null)
                                OnSwipe.Invoke(Direction);
                            if (OnSwipeRaw != null)
                                OnSwipeRaw.Invoke(DirectionRaw);
                        }
                    }
                }
            }
            //タッチが１つでないときは無効にする（※２つ以上の指の場合は、ピンチの可能性もあるため）
            else  //※Editor, PC はここには来ない
            {
                pressing = false;
            }
        }

        //==========================================================

        //内部処理の値リセット (※最低限のみ)
        void ResetInternalValue()
        {
            pressing = false;
            Direction = DirectionRaw = Vector2.zero;
        }

        //==========================================================

#endregion Swipe operation Section


#region InputSystem / InputManager wrapper Section

        //==========================================================
        //Touch / Mouse event

        //タッチが１つか？ or タッチ以外のプラットフォーム(Editor, PC 等)か？
        bool IsTouchCount1OrNotSupportPlatform {
            get {
                if (InputCompatible.SupportTouchAtRuntime)  //ランタイムでタッチが使えるプラットフォーム
                {
                    return InputCompatible.TouchCount(useInputSystemIfBothHandling) == 1;
                }
                return true;  //Editor, PC, etc.
            }
        }

        //押したとき（タッチ/左クリック）[※タッチ１つのみ]
        bool IsPointerDown {
            get {
                if (InputCompatible.SupportTouchAtRuntime)  //ランタイムでタッチが使えるプラットフォーム
                {
                    return InputCompatible.IsSingleTouchPhaseBegan(useInputSystemIfBothHandling);
                }
                return InputCompatible.GetMouseButtonDown(0, useInputSystemIfBothHandling);
            }
        }

        //離したとき（タッチ/左クリック）[※タッチ１つのみ]
        bool IsPointerUp {
            get {
                if (InputCompatible.SupportTouchAtRuntime)  //ランタイムでタッチが使えるプラットフォーム
                {
                    return InputCompatible.IsSingleTouchPhaseEndedOrCanceled(useInputSystemIfBothHandling);
                }
                return InputCompatible.GetMouseButtonUp(0, useInputSystemIfBothHandling);
            }
        }

        //==========================================================
        //Position of pointer

        //現在のタッチ/マウス位置 [※タッチ１つのみ]
        //※呼び出す前に IsPointerDown, IsPointerUp 等でタッチを判定済みであること（Touch.activeTouches[0] のチェックはしてないため）
        Vector2 CurrentPosition {
            get {
                if (InputCompatible.SupportTouchAtRuntime)  //ランタイムでタッチが使えるプラットフォーム
                {
                    return InputCompatible.SingleTouchPosition(useInputSystemIfBothHandling);
                }
                return InputCompatible.MousePosition(useInputSystemIfBothHandling);
            }
        }

        //==========================================================

#endregion InputSystem / InputManager wrapper Section


#region Gizmos Section

#if UNITY_EDITOR

        //==========================================================

        public bool gizmo_viewValidArea = false;
        public Color gizmo_validAreaColor = Color.green;

        //==========================================================

        private void OnDrawGizmos()
        {
            if (gizmo_viewValidArea)
            {
                var reso = XEditorUtils.GetGameViewResolution();
                var x = validArea.x * reso.x;
                var y = validArea.y * reso.y;
                var w = validArea.width * reso.x;
                var h = validArea.height * reso.y;
                var p0 = new Vector3(x, y, 0);
                var p1 = new Vector3(x, y + h, 0);
                var p2 = new Vector3(x + w, y + h, 0);
                var p3 = new Vector3(x + w, y, 0);

                Gizmos.color = gizmo_validAreaColor;
                Gizmos.DrawLine(p0, p1);
                Gizmos.DrawLine(p1, p2);
                Gizmos.DrawLine(p2, p3);
                Gizmos.DrawLine(p3, p0);
            }
        }

        //==========================================================

#endif

#endregion Gizmos Section

    }
}
//ランタイムでタッチで取得したいプラットフォームの定義
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
    #define SUPPORT_TOUTH_AT_RUNTIME
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using InputSystemTouchPhase = UnityEngine.InputSystem.TouchPhase;
#endif

namespace FantomLib
{
    /// <summary>
    /// InputSystem / InputManager からの取得をラップする (※どちらかと言うと Swipe, Pinch, LongClick 機能用)
    /// 2020/10/18 Fantom (Unity 2019.4, InputSystem 1.0.0 以降)
    /// http://fantom1x.blog130.fc2.com/blog-entry-373.html
    /// ※InputSystem を使うときは、EnhancedTouch.Enable() しておく必要がある（EnhancedTouchSupportActivator をヒエラルキーに置いておく）。
    /// ・Player Settings の ActiveInputHandling に依存し、取得方法が変わる。
    /// ・引数の useInputSystemIfBothHandling は ActiveInputHandling が Both (InputSystem, InputManager 両方存在) のときのみ有効（それ以外では使われない）。
    /// ・(旧)KeyCode と InputSystem.Key は特に記号・機能キーにおいては一部互換性が無いので、使用するときは注意。
    /// ・タッチデバイスを UNITY_ANDROID, UNITY_IOS (SUPPORT_TOUTH_AT_RUNTIME) としているので、他のデバイスも加えたい場合は #if の条件文にデバイスを追加する（タッチが取得できるもののみ）。
    /// </summary>
    public static class InputCompatible
    {

#region Debug Section

        //==========================================================
        //Debug
        public static void DebugActiveInputHandling()
        {
#if ENABLE_INPUT_SYSTEM
            Debug.Log("InputSystem is enabled.");
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            Debug.Log("InputManager (Legacy) is enabled.");
#endif

#if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
            Debug.Log("ActiveInputHandling is Both.");
#endif
        }

        //==========================================================
        
#endregion Debug Section



#region Property Section

        //==========================================================
        //Property

        /// <summary>
        /// ランタイムでタッチが使えるプラットフォームか？ (※SUPPORT_TOUTH_AT_RUNTIME に依存)
        /// ※プリプロセッサの状態を値として取得
        /// </summary>
        public static bool SupportTouchAtRuntime {
            get {
#if SUPPORT_TOUTH_AT_RUNTIME
                return true;
#else //Editor, PC
                return false;
#endif
            }
        }

        /// <summary>
        /// ランタイムで InputSystem が使える状態か？ (※ENABLE_INPUT_SYSTEM に依存)
        /// ※プリプロセッサの状態を値として取得
        /// </summary>
        public static bool EnableInputSystem {
            get {
#if ENABLE_INPUT_SYSTEM
                return true;
#else
                return false;
#endif
            }
        }
        
        /// <summary>
        /// ランタイムで InputManager が使えるか？ (※ENABLE_LEGACY_INPUT_MANAGER に依存)
        /// ※プリプロセッサの状態を値として取得.
        /// ※Unity2018 以前は定義自体が無いので、常に false になると思う。両方使っているときの判別用。
        /// </summary>
        public static bool EnableLegacyInputManager {
            get {
#if ENABLE_LEGACY_INPUT_MANAGER
                return true;
#else //Unity2018 or earlier
                return false;
#endif
            }
        }

        //==========================================================

        static int _keyCodeLength = -1;
        /// <summary>
        /// Enum.GetNames(typeof(KeyCode)).Length
        /// </summary>
        public static int KeyCodeLength {
            get {
                if (_keyCodeLength < 0)
                {
                    _keyCodeLength = Enum.GetNames(typeof(KeyCode)).Length;
                }
                return _keyCodeLength;
            }
        }

        static int _inputSystemKeyLength = -1;
        /// <summary>
        /// Enum.GetNames(typeof(Key)).Length
        ///※InputSystem が無効のときは、常に 0
        /// </summary>
        public static int InputSystemKeyLength {
            get {
                if (_inputSystemKeyLength < 0)
                {
#if ENABLE_INPUT_SYSTEM
                    _inputSystemKeyLength = Enum.GetNames(typeof(Key)).Length;
#else
                    _inputSystemKeyLength = 0;
#endif
                }
                return _inputSystemKeyLength;
            }
        }

        //==========================================================

#endregion Property Section



#region Touch event Section

        //==========================================================
        // Touch count
        //==========================================================

        /// <summary>
        /// タッチの数 (マウスは不可)
        /// </summary>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns></returns>
        public static int TouchCount(bool useInputSystemIfBothHandling = true)
        {
#if SUPPORT_TOUTH_AT_RUNTIME
    #if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
            return useInputSystemIfBothHandling ? EnhancedTouch.activeTouches.Count : Input.touchCount;
    #elif ENABLE_INPUT_SYSTEM
            return EnhancedTouch.activeTouches.Count;
    #else //Unity2018 or earlier
            return Input.touchCount;
    #endif
#else //Editor, PC
            return 0;  //※マウスは常に無効
#endif
        }


        //==========================================================
        // Touch positions
        //==========================================================

        #region Touch Position

        /// <summary>
        /// 全てのタッチの Position を配列で取得 (指インデクス順)
        /// </summary>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns></returns>
        public static Vector2[] TouchPositions(bool useInputSystemIfBothHandling = true)
        {
#if SUPPORT_TOUTH_AT_RUNTIME
            if (TouchCount(useInputSystemIfBothHandling) == 0)
                return null;
    #if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
            return useInputSystemIfBothHandling ? InputSystemActiveTouchPositions() : InputManagerActiveTouchPositions();
    #elif ENABLE_INPUT_SYSTEM
            return InputSystemActiveTouchPositions();
    #else //Unity2018 or earlier
            return InputManagerActiveTouchPositions();
    #endif
#else //Editor, PC
            return null;
#endif
        }

        /// <summary>
        /// InputSystem の現在のタッチの Position を配列で取得 (指インデクス順)
        /// </summary>
        /// <returns></returns>
        public static Vector2[] InputSystemActiveTouchPositions()
        {
#if SUPPORT_TOUTH_AT_RUNTIME && ENABLE_INPUT_SYSTEM
            int count = EnhancedTouch.activeTouches.Count;
            var positions = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                //finger.index (n番目の指) で並べ替えておく
                int idx = EnhancedTouch.activeTouches[i].finger.index;
                positions[idx] = EnhancedTouch.activeTouches[i].screenPosition;
            }
            return positions;
#else //Editor, PC
            return null;
#endif
        }

        /// <summary>
        /// (旧)InputManager の現在のタッチの Position を配列で取得 (指インデクス順)
        /// </summary>
        /// <returns></returns>
        public static Vector2[] InputManagerActiveTouchPositions()
        {
#if SUPPORT_TOUTH_AT_RUNTIME
            int count = Input.touchCount;
            var positions = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                //※fingerId と touches[] のインデクスは必ずしも一致しないらしいので、並び替えておく
                //https://docs.unity3d.com/ja/current/ScriptReference/Touch-fingerId.html
                int idx = Input.touches[i].fingerId;
                positions[idx] = Input.touches[i].position;
            }
            return positions;
#else //Editor, PC
            return null;
#endif
        }

        //==========================================================

        /// <summary>
        /// 単一のタッチのとき、Position 取得.
        /// ※タッチ１つ以外, Editor, PC 等は正しい値にならないので、注意（複数指のときは使用不可）
        /// </summary>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns></returns>
        public static Vector2 SingleTouchPosition(bool useInputSystemIfBothHandling = true)
        {
            if (TouchCount(useInputSystemIfBothHandling) != 1)
                return Vector2.zero;  

#if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
            if (useInputSystemIfBothHandling)
                return EnhancedTouch.activeTouches[0].screenPosition;
            else
                return Input.touches[0].position;
#elif ENABLE_INPUT_SYSTEM
            return EnhancedTouch.activeTouches[0].screenPosition;
#else //Unity2018 or earlier
            return Input.touches[0].position;
#endif
        }

        #endregion Touch Position

        //==========================================================


        //==========================================================
        // Touch Phase
        //==========================================================

        #region Touch Phase and Finger

#if ENABLE_INPUT_SYSTEM

        //==========================================================

        //指インデクス（押下順）で EnhancedTouch 取得
        //※範囲外チェックをしてないので注意
        //fingerIdx: 指を置いた順 [0]～[n-1]
        //count: TouchCount を入れる
        static bool TryGetInputSystemTouch(int fingerIdx, int count, out EnhancedTouch result)
        {
            if (EnhancedTouch.activeTouches[fingerIdx].finger.index == fingerIdx)
            {
                result = EnhancedTouch.activeTouches[fingerIdx];
                return true;
            }

            for (int i = 0; i < count; i++)
            {
                if (EnhancedTouch.activeTouches[i].finger.index == fingerIdx)
                {
                    result = EnhancedTouch.activeTouches[i];
                    return true;
                }
            }

            result = default;
            return false;  //fingerIdx が無い
        }

        //==========================================================

        /// <summary>
        /// InputSystem のタッチのフェーズを照合
        /// </summary>
        /// <param name="fingerIdx">指を置いた順 [0]～[n-1]</param>
        /// <param name="phase"></param>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns></returns>
        public static bool IsInputSystemTouchPhase(int fingerIdx, InputSystemTouchPhase phase, bool useInputSystemIfBothHandling = true)
        {
            if (fingerIdx < 0)
                return false;

            int count = TouchCount(useInputSystemIfBothHandling);
            if (fingerIdx >= count)
                return false;

            if (TryGetInputSystemTouch(fingerIdx, count, out var touch))
            {
                return touch.phase == phase;
            }
            return false;
        }

        /// <summary>
        /// InputSystem のタッチのフェーズが Began, Moved, Stationary のいずれか？
        /// </summary>
        /// <param name="fingerIdx">指を置いた順 [0]～[n-1]</param>
        /// <param name="useInputSystemIfBothHandling"></param>
        /// <returns></returns>
        public static bool IsInputSystemTouchPhaseActive(int fingerIdx, bool useInputSystemIfBothHandling = true)
        {
            if (fingerIdx < 0)
                return false;

            int count = TouchCount(useInputSystemIfBothHandling);
            if (fingerIdx >= count)
                return false;

            if (TryGetInputSystemTouch(fingerIdx, count, out var touch))
            {
                return touch.phase.IsActive();
            }
            return false;
        }

        /// <summary>
        /// InputSystem のタッチのフェーズが Ended, Canceled のいずれか？
        /// </summary>
        /// <param name="fingerIdx">指を置いた順 [0]～[n-1]</param>
        /// <param name="useInputSystemIfBothHandling"></param>
        /// <returns></returns>
        public static bool IsInputSystemTouchPhaseEndedOrCanceled(int fingerIdx, bool useInputSystemIfBothHandling = true)
        {
            if (fingerIdx < 0)
                return false;

            int count = TouchCount(useInputSystemIfBothHandling);
            if (fingerIdx >= count)
                return false;

            if (TryGetInputSystemTouch(fingerIdx, count, out var touch))
            {
                return touch.phase.IsEndedOrCanceled();
            }
            return false;
        }

#endif

        //==========================================================

        //指インデクス（押下順）で (旧)Touch 取得
        //※範囲外チェックをしてないので注意
        //fingerIdx: 指を置いた順 [0]～[n-1]
        //count: TouchCount を入れる
        static bool TryGetInputManagerTouch(int fingerIdx, int count, out UnityEngine.Touch result)
        {
            if (Input.touches[fingerIdx].fingerId == fingerIdx)
            {
                result = Input.touches[fingerIdx];
                return true;
            }

            //※fingerId と touches[] のインデクスは必ずしも一致しないらしいので fingerId を検出
            //https://docs.unity3d.com/ja/current/ScriptReference/Touch-fingerId.html
            for (int i = 0; i < count; i++)
            {
                if (Input.touches[i].fingerId == fingerIdx)
                {
                    result = Input.touches[i];
                    return true;
                }
            }

            result = default;
            return false;  //fingerIdx が無い
        }

        //==========================================================

        /// <summary>
        /// InputManager のタッチのフェーズを照合
        /// </summary>
        /// <param name="fingerIdx">指を置いた順 [0]～[n-1]</param>
        /// <param name="phase">TouchPhase</param>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns></returns>
        public static bool IsInputManagerTouchPhase(int fingerIdx, UnityEngine.TouchPhase phase, bool useInputSystemIfBothHandling = true)
        {
            if (fingerIdx < 0)
                return false;

            int count = TouchCount(useInputSystemIfBothHandling);
            if (fingerIdx >= count)
                return false;

            if (TryGetInputManagerTouch(fingerIdx, count, out var touch))
            {
                return touch.phase == phase;
            }
            return false;
        }

        /// <summary>
        /// InputManager のタッチのフェーズが Began, Moved, Stationary のいずれか？
        /// </summary>
        /// <param name="fingerIdx">指を置いた順 [0]～[n-1]</param>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns></returns>
        public static bool IsInputManagerTouchPhaseActive(int fingerIdx, bool useInputSystemIfBothHandling = true)
        {
            if (fingerIdx < 0)
                return false;

            int count = TouchCount(useInputSystemIfBothHandling);
            if (fingerIdx >= count)
                return false;

            if (TryGetInputManagerTouch(fingerIdx, count, out var touch))
            {
                return touch.phase.IsActive();
            }
            return false;
        }

        /// <summary>
        /// InputManager のタッチのフェーズが Ended, Canceled のいずれか？
        /// </summary>
        /// <param name="fingerIdx">指を置いた順 [0]～[n-1]</param>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns></returns>
        public static bool IsInputManagerTouchPhaseEndedOrCanceled(int fingerIdx, bool useInputSystemIfBothHandling = true)
        {
            if (fingerIdx < 0)
                return false;

            int count = TouchCount(useInputSystemIfBothHandling);
            if (fingerIdx >= count)
                return false;

            if (TryGetInputManagerTouch(fingerIdx, count, out var touch))
            {
                return touch.phase.IsEndedOrCanceled();
            }
            return false;
        }

        #endregion Touch Phaseand Finger

        //==========================================================


        //==========================================================
        // 固定タッチ数で検出
        //==========================================================

        #region Single Touch

        /// <summary>
        /// 単一タッチで指インデクスが 0 (最初の指) か？
        /// ※指が１つでない、１つでも最初の指でない（2本→1本 のとき）は false
        /// </summary>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns></returns>
        public static bool IsSingleTouch(bool useInputSystemIfBothHandling = true)
        {
            //１本指以外は無効
            if (TouchCount(useInputSystemIfBothHandling) != 1)
                return false;  //Editor, PC 等 (マウスは常に無効)

#if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
            if (useInputSystemIfBothHandling)
                return EnhancedTouch.activeTouches[0].finger.index == 0;
            else
                return Input.touches[0].fingerId == 0;
#elif ENABLE_INPUT_SYSTEM
            return EnhancedTouch.activeTouches[0].finger.index == 0;
#else //Unity2018 or earlier
            return Input.touches[0].fingerId == 0;
#endif
        }

        /// <summary>
        /// 単一タッチで押したときの検出（タッチが１つでないときは、常に false）
        /// </summary>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns></returns>
        public static bool IsSingleTouchPhaseBegan(bool useInputSystemIfBothHandling = true)
        {
            //１本指以外は無効
            if (TouchCount(useInputSystemIfBothHandling) != 1)
                return false;  //Editor, PC 等 (マウスは常に無効)

#if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
            if (useInputSystemIfBothHandling)
                return EnhancedTouch.activeTouches[0].phase == InputSystemTouchPhase.Began;
            else
                return Input.touches[0].phase == UnityEngine.TouchPhase.Began;
#elif ENABLE_INPUT_SYSTEM
            return EnhancedTouch.activeTouches[0].phase == InputSystemTouchPhase.Began;
#else //Unity2018 or earlier
            return Input.touches[0].phase == UnityEngine.TouchPhase.Began;
#endif
        }

        /// <summary>
        /// 単一タッチ押下中か？（継続判定）（タッチが１つでないときは、常に false）
        /// </summary>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns>Began, Moved, Stationary のとき true</returns>
        public static bool IsSingleTouchPhaseActive(bool useInputSystemIfBothHandling = true)
        {
            //１本指以外は無効
            if (TouchCount(useInputSystemIfBothHandling) != 1)
                return false;  //Editor, PC 等 (マウスは常に無効)

#if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
            if (useInputSystemIfBothHandling)
                return EnhancedTouch.activeTouches[0].phase.IsActive();  //IsActive() は Began, Moved, Stationary が true になる
            else
                return Input.touches[0].phase.IsActive();
#elif ENABLE_INPUT_SYSTEM
            return EnhancedTouch.activeTouches[0].phase.IsActive();  //IsActive() は Began, Moved, Stationary が true になる
#else //Unity2018 or earlier
            return Input.touches[0].phase.IsActive();
#endif
        }

        /// <summary>
        /// 単一タッチで 離したとき(Ended) or キャンセル(Canceled) の検出（タッチが１つでないときは、常に false）
        /// </summary>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns>Ended, Canceled のとき true</returns>
        public static bool IsSingleTouchPhaseEndedOrCanceled(bool useInputSystemIfBothHandling = true)
        {
            //１本指以外は無効
            if (TouchCount(useInputSystemIfBothHandling) != 1)
                return false;  //Editor, PC 等 (マウスは常に無効)

#if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
            if (useInputSystemIfBothHandling)
                return EnhancedTouch.activeTouches[0].phase.IsEndedOrCanceled();
            else
                return Input.touches[0].phase.IsEndedOrCanceled();
#elif ENABLE_INPUT_SYSTEM
            return EnhancedTouch.activeTouches[0].phase.IsEndedOrCanceled();
#else //Unity2018 or earlier
            return Input.touches[0].phase.IsEndedOrCanceled();
#endif
        }

        #endregion Single Touch

        //==========================================================

        #region Double Touch

        /// <summary>
        /// ２本タッチで押したときの検出（タッチが２つでないときは、常に false）
        /// </summary>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns></returns>
        public static bool IsDoubleTouchPhaseBegan(bool useInputSystemIfBothHandling = true)
        {
            //２本指以外は無効
            if (TouchCount(useInputSystemIfBothHandling) != 2)
                return false;  //Editor, PC 等 (マウスは常に無効)

            return IsMultiTouchPhaseBegan(1, useInputSystemIfBothHandling);
        }

        /// <summary>
        /// ２本タッチで Began, Moved, Stationary のいずれかの検出（タッチが２つでないときは、常に false）
        /// </summary>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns></returns>
        public static bool IsDoubleTouchPhaseActive(bool useInputSystemIfBothHandling = true)
        {
            //２本指以外は無効
            if (TouchCount(useInputSystemIfBothHandling) != 2)
                return false;  //Editor, PC 等 (マウスは常に無効)

            return IsMultiTouchPhaseActive(1, useInputSystemIfBothHandling);
        }

        /// <summary>
        /// ２本タッチで Ended, Canceled のいずれかの検出（タッチが２つでないときは、常に false）
        /// </summary>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns></returns>
        public static bool IsDoubleTouchPhaseEndedOrCanceled(bool useInputSystemIfBothHandling = true)
        {
            //２本指以外は無効
            if (TouchCount(useInputSystemIfBothHandling) != 2)
                return false;  //Editor, PC 等 (マウスは常に無効)

            return IsMultiTouchPhaseEndedOrCanceled(1, useInputSystemIfBothHandling);
        }

        #endregion Double Touch

        //==========================================================

        #region Triple Touch

        /// <summary>
        /// ３本タッチで押したときの検出（タッチが３つでないときは、常に false）
        /// </summary>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns></returns>
        public static bool IsTripleTouchPhaseBegan(bool useInputSystemIfBothHandling = true)
        {
            //３本指以外は無効
            if (TouchCount(useInputSystemIfBothHandling) != 3)
                return false;  //Editor, PC 等 (マウスは常に無効)

            return IsMultiTouchPhaseBegan(2, useInputSystemIfBothHandling);
        }

        /// <summary>
        /// ３本タッチで Began, Moved, Stationary のいずれかの検出（タッチが３つでないときは、常に false）
        /// </summary>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns></returns>
        public static bool IsTripleTouchPhaseActiven(bool useInputSystemIfBothHandling = true)
        {
            //３本指以外は無効
            if (TouchCount(useInputSystemIfBothHandling) != 3)
                return false;  //Editor, PC 等 (マウスは常に無効)

            return IsMultiTouchPhaseActive(2, useInputSystemIfBothHandling);
        }

        /// <summary>
        /// ３本タッチで Ended, Canceled のいずれかの検出（タッチが３つでないときは、常に false）
        /// </summary>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns></returns>
        public static bool IsTripleTouchPhaseEndedOrCanceled(bool useInputSystemIfBothHandling = true)
        {
            //３本指以外は無効
            if (TouchCount(useInputSystemIfBothHandling) != 3)
                return false;  //Editor, PC 等 (マウスは常に無効)

            return IsMultiTouchPhaseEndedOrCanceled(2, useInputSystemIfBothHandling);
        }

        #endregion Triple Touch

        //==========================================================

        #region Multi Touch

        /// <summary>
        /// n 本タッチで押したときの検出（タッチが n 個でないときは、常に false）
        /// </summary>
        /// <param name="fingerIdx">指インデクス(押下順)：0～n-1</param>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns></returns>
        public static bool IsMultiTouchPhaseBegan(int fingerIdx, bool useInputSystemIfBothHandling = true)
        {
            //n本指以外は無効
            if (TouchCount(useInputSystemIfBothHandling) != fingerIdx + 1)
                return false;  //Editor, PC 等 (マウスは常に無効)

#if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
            if (useInputSystemIfBothHandling)
                return IsInputSystemTouchPhase(fingerIdx, InputSystemTouchPhase.Began, useInputSystemIfBothHandling);
            else
                return IsInputManagerTouchPhase(fingerIdx, UnityEngine.TouchPhase.Began, useInputSystemIfBothHandling);
#elif ENABLE_INPUT_SYSTEM
            return IsInputSystemTouchPhase(fingerIdx, InputSystemTouchPhase.Began, useInputSystemIfBothHandling);
#else //Unity2018 or earlier
            return IsInputManagerTouchPhase(fingerIdx, UnityEngine.TouchPhase.Began, useInputSystemIfBothHandling);
#endif
        }

        /// <summary>
        /// n 本タッチで Began, Moved, Stationary の検出（タッチが n 個でないときは、常に false）
        /// </summary>
        /// <param name="fingerIdx">指インデクス(押下順)：0～n-1</param>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns></returns>
        public static bool IsMultiTouchPhaseActive(int fingerIdx, bool useInputSystemIfBothHandling = true)
        {
            //n本指以外は無効
            if (TouchCount(useInputSystemIfBothHandling) != fingerIdx + 1)
                return false;  //Editor, PC 等 (マウスは常に無効)

#if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
            if (useInputSystemIfBothHandling)
                return IsInputSystemTouchPhaseActive(fingerIdx, useInputSystemIfBothHandling);
            else
                return IsInputManagerTouchPhaseActive(fingerIdx, useInputSystemIfBothHandling);
#elif ENABLE_INPUT_SYSTEM
            return IsInputSystemTouchPhaseActive(fingerIdx, useInputSystemIfBothHandling);
#else //Unity2018 or earlier
            return IsInputManagerTouchPhaseActive(fingerIdx, useInputSystemIfBothHandling);
#endif
        }

        /// <summary>
        /// n 本タッチで Ended, Canceled の検出（タッチが n 個でないときは、常に false）
        /// </summary>
        /// <param name="fingerIdx">指インデクス(押下順)：0～n-1</param>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns></returns>
        public static bool IsMultiTouchPhaseEndedOrCanceled(int fingerIdx, bool useInputSystemIfBothHandling = true)
        {
            //n本指以外は無効
            if (TouchCount(useInputSystemIfBothHandling) != fingerIdx + 1)
                return false;  //Editor, PC 等 (マウスは常に無効)

#if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
            if (useInputSystemIfBothHandling)
                return IsInputSystemTouchPhaseEndedOrCanceled(fingerIdx, useInputSystemIfBothHandling);
            else
                return IsInputManagerTouchPhaseEndedOrCanceled(fingerIdx, useInputSystemIfBothHandling);
#elif ENABLE_INPUT_SYSTEM
            return IsInputSystemTouchPhaseEndedOrCanceled(fingerIdx, useInputSystemIfBothHandling);
#else //Unity2018 or earlier
            return IsInputManagerTouchPhaseEndedOrCanceled(fingerIdx, useInputSystemIfBothHandling);
#endif
        }

        #endregion Multi Touch

        //==========================================================

#endregion Touch event Section



#region Mouse event Section

        //==========================================================

        /// <summary>
        /// 現在のマウスポインタ位置 (px)
        /// Input.mousePosition 互換（※ただし Vector2 [z は無い]）
        /// </summary>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns></returns>
        public static Vector2 MousePosition(bool useInputSystemIfBothHandling = true)
        {
#if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
            return useInputSystemIfBothHandling ? (Mouse.current?.position.ReadValue() ?? Vector2.zero) 
                : new Vector2(Input.mousePosition.x, Input.mousePosition.y);
#elif ENABLE_INPUT_SYSTEM
            return Mouse.current?.position.ReadValue() ?? Vector2.zero;
#else //Unity2018 or earlier
            return new Vector2(Input.mousePosition.x, Input.mousePosition.y);
#endif
        }

        //==========================================================



        //==========================================================
        // Mouse event (InputSystem / InputManager compatible)
        //==========================================================
        /*
        http://fantom1x.blog130.fc2.com/blog-entry-366.html
                             押した  離した  継続
        GetMouseButtonDown | True  | False | False |
        GetMouseButton     | True  | False | True  |
        GetMouseButtonUp   | False | True  | False |

                                                押した  離した  継続
        Mouse.current.～.wasPressedThisFrame  | True  | False | False |
        Mouse.current.～.isPressed            | True  | False | True  |
        Mouse.current.～.wasReleasedThisFrame | False | True  | False |
        */

        #region InputSystem / InputManager compatible Mouse event

        /// <summary>
        /// マウスボタンを押したとき
        /// ・Input.GetMouseButtonDown(n) 互換
        /// </summary>
        /// <param name="button">ボタン番号： 0: 左 / 1:右 / 2:中 / 3: 戻る / 4:進む</param>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns></returns>
        public static bool GetMouseButtonDown(int button, bool useInputSystemIfBothHandling = true)
        {
#if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
            return useInputSystemIfBothHandling ? InputSystemGetMouseButtonDown(button) : Input.GetMouseButtonDown(button);
#elif ENABLE_INPUT_SYSTEM
            return InputSystemGetMouseButtonDown(button);
#else //Unity2018 or earlier
            return Input.GetMouseButtonDown(button);  //※この関数はタッチも取れるがどの指かはわからない
#endif
        }

        /// <summary>
        /// マウスボタンを離したとき
        ///・Input.GetMouseButtonUp(n) 互換
        /// </summary>
        /// <param name="button">ボタン番号： 0: 左 / 1:右 / 2:中 / 3: 戻る / 4:進む</param>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns></returns>
        public static bool GetMouseButtonUp(int button, bool useInputSystemIfBothHandling = true)
        {
#if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
            return useInputSystemIfBothHandling ? InputSystemGetMouseButtonUp(button) : Input.GetMouseButtonUp(button);
#elif ENABLE_INPUT_SYSTEM
            return InputSystemGetMouseButtonUp(button);
#else //Unity2018 or earlier
            return Input.GetMouseButtonUp(button);  //※この関数はタッチも取れるがどの指かはわからない
#endif
        }

        /// <summary>
        /// マウスボタンが押下中（継続中）か？
        /// ・Input.GetMouseButton(n) 互換
        /// ※開始も true になる
        /// </summary>
        /// <param name="button">ボタン番号： 0: 左 / 1:右 / 2:中 / 3: 戻る / 4:進む</param>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns></returns>
        public static bool GetMouseButton(int button, bool useInputSystemIfBothHandling = true)
        {
#if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
            return useInputSystemIfBothHandling ? InputSystemGetMouseButton(button) : Input.GetMouseButton(button);
#elif ENABLE_INPUT_SYSTEM
            return InputSystemGetMouseButton(button);
#else //Unity2018 or earlier
            return Input.GetMouseButton(button);  //※この関数はタッチも取れるがどの指かはわからない
#endif
        }

        #endregion InputSystem / InputManager compatible Mouse event

        //==========================================================

        #region Mouse wheel

        /// <summary>
        /// マウスホイールのスクロールを取得.
        /// ※mouseScrollDelta はホイール１目盛で 1.0, Mouse.current.scroll はピクセル量(120 ? 機器による？)
        /// </summary>
        /// <param name="useInputSystemIfBothHandling">新旧両方有効のとき、InputSystem を使う</param>
        /// <returns></returns>
        public static float GetMouseScroll(bool useInputSystemIfBothHandling = true)
        {
#if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
            if (useInputSystemIfBothHandling)
                return Mouse.current?.scroll?.ReadValue().y ?? 0;
            else
                return Input.mouseScrollDelta.y;
#elif ENABLE_INPUT_SYSTEM
            return Mouse.current?.scroll?.ReadValue().y ?? 0;
#else //Unity2018 or earlier
            return Input.mouseScrollDelta.y;
#endif
        }

        #endregion Mouse wheel

        //==========================================================



        //==========================================================
        // Mouse event (InputSystem only)
        //==========================================================

        #region InputSystem only Mouse event

#if ENABLE_INPUT_SYSTEM

        /// <summary>
        /// InputSystem でマウスのボタンが押されたか？(GetMouseButtonDown() 相当)
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool InputSystemGetMouseButtonDown(int button)
        {
            if (Mouse.current == null)
                return false;

            switch (button)
            {
                case 0:
                    return Mouse.current.leftButton.wasPressedThisFrame;
                case 1:
                    return Mouse.current.rightButton.wasPressedThisFrame;
                case 2:
                    return Mouse.current.middleButton.wasPressedThisFrame;
                case 3:
                    return Mouse.current.backButton.wasPressedThisFrame;
                case 4:
                    return Mouse.current.forwardButton.wasPressedThisFrame;
                default:
                    #if UNITY_EDITOR
                    Debug.LogError("Not implementation : button = " + button);
                    #endif
                    return false;
            }
        }

        /// <summary>
        /// InputSystem でマウスのボタンが離されたか？(GetMouseButtonUp() 相当)
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool InputSystemGetMouseButtonUp(int button)
        {
            if (Mouse.current == null)
                return false;

            switch (button)
            {
                case 0:
                    return Mouse.current.leftButton.wasReleasedThisFrame;
                case 1:
                    return Mouse.current.rightButton.wasReleasedThisFrame;
                case 2:
                    return Mouse.current.middleButton.wasReleasedThisFrame;
                case 3:
                    return Mouse.current.backButton.wasReleasedThisFrame;
                case 4:
                    return Mouse.current.forwardButton.wasReleasedThisFrame;
                default:
                    #if UNITY_EDITOR
                    Debug.LogError("Not implementation : button = " + button);
                    #endif
                    return false;
            }
        }

        /// <summary>
        /// InputSystem でマウスのボタンが押下中か？(GetMouseButton() 相当)
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool InputSystemGetMouseButton(int button)
        {
            if (Mouse.current == null)
                return false;

            switch (button)
            {
                case 0:
                    return Mouse.current.leftButton.isPressed;
                case 1:
                    return Mouse.current.rightButton.isPressed;
                case 2:
                    return Mouse.current.middleButton.isPressed;
                case 3:
                    return Mouse.current.backButton.isPressed;
                case 4:
                    return Mouse.current.forwardButton.isPressed;
                default:
                    #if UNITY_EDITOR
                    Debug.LogError("Not implementation : button = " + button);
                    #endif
                    return false;
            }
        }

#endif
        #endregion InputSystem only Mouse event

        //==========================================================

#endregion Mouse event Section



        //==========================================================
        // Extensions
        //==========================================================

#region InputManager only Extensions Section

        //==========================================================

        // https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.InputExtensions.html
        /// <summary>
        /// (旧)TouchPhase での InputSystemExtensions.IsActive 互換（※(旧)TouchPhase に "None" は無いので注意）
        /// </summary>
        /// <param name="phase">Began, Moved, Stationary のとき true</param>
        /// <returns></returns>
        public static bool IsActive(this UnityEngine.TouchPhase phase)
        {
            return phase == UnityEngine.TouchPhase.Began || phase == UnityEngine.TouchPhase.Moved || phase == UnityEngine.TouchPhase.Stationary;
        }

        // https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.InputExtensions.html
        /// <summary>
        /// (旧)TouchPhase での InputSystemExtensions.IsEndedOrCanceled 互換（※(旧)TouchPhase に "None" は無いので注意）
        /// </summary>
        /// <param name="phase"></param>
        /// <returns>Ended, Canceled のとき true</returns>
        public static bool IsEndedOrCanceled(this UnityEngine.TouchPhase phase)
        {
            return phase == UnityEngine.TouchPhase.Ended || phase == UnityEngine.TouchPhase.Canceled;
        }

        //==========================================================

#endregion InputManager only Extensions Section



#region Compatible Key event Section

        //==========================================================
        // KeyCode や Key の文字列から、入力関数を利用する（文字列変換は内部でキャッシュされる）
        // ・記号系のキーは KeyCode と Key で互換性が無いので注意。
        // ・InputSystem 1.0.0 時点では CapsLock や 日本語変換キー を押すと無効のままになってしまうので注意。
        // ・RefreshSymbolToInputSystemKeyMapWithKeyboardLayout() を実行すると、現在のキーボードレイアウトで
        //   InputSystem の内部マップが更新される。
        //   http://fantom1x.blog130.fc2.com/blog-entry-368.html#InputSystem_ja_keyboard_difference_list
        //==========================================================

        /// <summary>
        /// Input.anyKeyDown 互換
        /// ※InputSystem 1.0.0 時点では CapsLock や 日本語変換キー を押すと無効のままになってしまうので注意
        /// </summary>
        /// <param name="useInputSystemIfBothHandling"></param>
        /// <returns></returns>
        public static bool AnyKeyDown(bool useInputSystemIfBothHandling = true)
        {
#if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
            return useInputSystemIfBothHandling ? Keyboard.current.anyKey.wasPressedThisFrame : Input.anyKeyDown;
#elif ENABLE_INPUT_SYSTEM
            return Keyboard.current.anyKey.wasPressedThisFrame;
#else //Unity2018 or earlier
            return Input.anyKeyDown;
#endif
        }

        /// <summary>
        /// Input.anyKey 互換
        /// ※InputSystem 1.0.0 時点では CapsLock や 日本語変換キー を押すと無効のままになってしまうので注意
        /// </summary>
        /// <param name="useInputSystemIfBothHandling"></param>
        /// <returns></returns>
        public static bool AnyKey(bool useInputSystemIfBothHandling = true)
        {
#if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
            return useInputSystemIfBothHandling ? Keyboard.current.anyKey.isPressed : Input.anyKey;
#elif ENABLE_INPUT_SYSTEM
            return Keyboard.current.anyKey.isPressed;
#else //Unity2018 or earlier
            return Input.anyKey;
#endif
        }

        //==========================================================

        /// <summary>
        /// Input.GetKeyDown() 互換
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="useInputSystemIfBothHandling"></param>
        /// <returns></returns>
        public static bool GetKeyDown(string keyName, bool useInputSystemIfBothHandling = true)
        {
            if (string.IsNullOrEmpty(keyName))
                return false;

#if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
            return useInputSystemIfBothHandling ? GetKeyDown(keyName.ToInputSystemKey()) : Input.GetKeyDown(keyName.ToKeyCode());
#elif ENABLE_INPUT_SYSTEM
            return GetKeyDown(keyName.ToInputSystemKey());
#else //Unity2018 or earlier
            return Input.GetKeyDown(keyName.ToKeyCode());
#endif
        }

        /// <summary>
        /// Input.GetKeyUp() 互換
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="useInputSystemIfBothHandling"></param>
        /// <returns></returns>
        public static bool GetKeyUp(string keyName, bool useInputSystemIfBothHandling = true)
        {
            if (string.IsNullOrEmpty(keyName))
                return false;

#if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
            return useInputSystemIfBothHandling ? GetKeyUp(keyName.ToInputSystemKey()) : Input.GetKeyUp(keyName.ToKeyCode());
#elif ENABLE_INPUT_SYSTEM
            return GetKeyUp(keyName.ToInputSystemKey());
#else //Unity2018 or earlier
            return Input.GetKeyUp(keyName.ToKeyCode());
#endif
        }

        /// <summary>
        /// Input.GetKey() 互換
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="useInputSystemIfBothHandling"></param>
        /// <returns></returns>
        public static bool GetKey(string keyName, bool useInputSystemIfBothHandling = true)
        {
            if (string.IsNullOrEmpty(keyName))
                return false;

#if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
            return useInputSystemIfBothHandling ? GetKey(keyName.ToInputSystemKey()) : Input.GetKey(keyName.ToKeyCode());
#elif ENABLE_INPUT_SYSTEM
            return GetKey(keyName.ToInputSystemKey());
#else //Unity2018 or earlier
            return Input.GetKey(keyName.ToKeyCode());
#endif
        }

        //==========================================================

#endregion Compatible Key event Section



#region InputSystem Key event Section


#if ENABLE_INPUT_SYSTEM

        //==========================================================

        /// <summary>
        /// Input.GetKeyDown() 相当
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool GetKeyDown(Key key)
        {
            if (IsInvalidInputSystemKey(key))
                return false;

            return Keyboard.current[key].wasPressedThisFrame;
        }


        /// <summary>
        /// Input.GetKeyDown() 相当
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool GetKeyUp(Key key)
        {
            if (IsInvalidInputSystemKey(key))
                return false;

            return Keyboard.current[key].wasReleasedThisFrame;
        }

        /// <summary>
        /// Input.GetKey() 相当
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool GetKey(Key key)
        {
            if (IsInvalidInputSystemKey(key))
                return false;

            return Keyboard.current[key].isPressed;  //※Android のバックキーはなぜか１秒くらいで false になってしまう (Input.GetKey ではならない)
        }

        //==========================================================

        //引数として利用できない Key
        static readonly HashSet<Key> _invalidInputSystemKeys 
            = new HashSet<Key>() { Key.None, Key.IMESelected };

        /// <summary>
        /// 引数として利用できない(無効な) Key か？
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsInvalidInputSystemKey(this Key key)
        {
            return _invalidInputSystemKeys.Contains(key);
        }

        /// <summary>
        /// 引数として利用できる(無効でない) Key か？
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsValidInputSystemKey(this Key key)
        {
            return !IsInvalidInputSystemKey(key);
        }

#endif

        //==========================================================

#endregion InputSystem Key event Section



#region Type convert Section

        //==========================================================
        // 型変換 拡張メソッド
        //==========================================================

        /// <summary>
        /// string → KeyCode 変換
        /// </summary>
        /// <param name="keyName">例："Space", "escape" 等 (大文字小文字は区別しない)</param>
        /// <returns></returns>
        public static KeyCode ToKeyCode(this string keyName)
        {
            if (string.IsNullOrEmpty(keyName))
                return KeyCode.None;

            //変換マップにあれば、それを返す
            var lower = keyName.ToLowerOrCached();  //小文字で統一
            if (ToKeyCodeMap.ContainsKey(lower))
                return ToKeyCodeMap[lower];
            return KeyCode.None;
        }

#if ENABLE_INPUT_SYSTEM

        /// <summary>
        /// InputSystem.Key → KeyCode 変換
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static KeyCode ToKeyCode(this Key key)
        {
            return ToKeyCode(key.ToStringOrCached());
        }


        /// <summary>
        /// string → InputSystem.Key 変換
        /// </summary>
        /// <param name="keyName">例："Space", "escape" 等 (大文字小文字は区別しない)</param>
        /// <returns></returns>
        public static Key ToInputSystemKey(this string keyName)
        {
            if (string.IsNullOrEmpty(keyName))
                return Key.None;

            //変換マップにあれば、それを返す
            var lower = keyName.ToLowerOrCached();  //※小文字で統一
            if (ToInputSystemKeyMap.ContainsKey(lower))
                return ToInputSystemKeyMap[lower];
            return Key.None;
        }

        /// <summary>
        /// KeyCode → InputSystem.Key 変換
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        public static Key ToInputSystemKey(this KeyCode keyCode)
        {
            return ToInputSystemKey(keyCode.ToStringOrCached());
        }

#endif

        //==========================================================



        #region Make convert map

        //==========================================================
        // 変換マップの作成
        //※ただし、(旧)KeyCode と InputSystem.Key は仕様の違いから、特に記号系は実際のキーとは異なることがある。
        //https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Keyboard.html
        //==========================================================

#if ENABLE_INPUT_SYSTEM

        //string → InputSystem.Key 変換マップ（※キーは小文字で統一）
        //・単純に Key の小文字キー → Key
        //・"alpha1" → Digit1, "keypad0" → Numpad0 のような先頭文字パターン
        //・"return" → Enter, "menu" → ContextMenu のような別名パターン
        //・"backslash" → OEM2 のように、104, 106 キーボードなどで違うキー
        static Dictionary<string, Key> _toInputSystemKeyMap;
        static Dictionary<string, Key> ToInputSystemKeyMap {
            get {
                if (_toInputSystemKeyMap == null)
                {
                    var names = Enum.GetNames(typeof(Key));
                    var values = Enum.GetValues(typeof(Key)).Cast<Key>();
                    _toInputSystemKeyMap = new Dictionary<string, Key>(names.Length);

                    //※enum は値の重複ができるため、names を基準にする
                    for (int i = 0; i < names.Length; i++)
                    {
                        var key = names[i].ToLowerOrCached(); //小文字で統一
                        var value = values.ElementAt(i);
                        if (!_toInputSystemKeyMap.ContainsKey(key))
                        {
                            _toInputSystemKeyMap[key] = value;

                            //先頭文字列で変換対応するもの (※小文字で統一)
                            var pre = ReplacePrefixIfStartsWith(key, _toInputSystemKeyPrefixMap);
                            if (pre != key)
                            {
                                if (!_toInputSystemKeyMap.ContainsKey(pre))
                                    _toInputSystemKeyMap[pre] = value;
                            }

                            //完全一致で変換対応するもの (※小文字で統一)
                            if (_toInputSystemKeyWholeMap.TryGetValue(key, out var whole))
                            {
                                if (!_toInputSystemKeyMap.ContainsKey(whole))
                                    _toInputSystemKeyMap[whole] = value;
                            }
                        }
                    }

                    //displayName を使って、動的にマップを変更する
                    UpdateToInputSystemKeyMapByDisplayName(_inputSystemKeyChangeOEMMap);
                }
                return _toInputSystemKeyMap;
            }
        }
        #region Note: duplicate value [Unity2019.4.12f1 時点]
        /*
        ※Key に大文字小文字無視での重複は無い前提（Unity2019.4.11f1 時点では確認済み)
        ※values だと 112 個になってしまう（names だと 119 個）
        RightAlt = 54 ┐
        AltGr = 54,   ┘重複(別名)
        LeftMeta = 57,    ┐
        LeftWindows = 57, ┤
        LeftApple = 57,   ┤
        LeftCommand = 57, ┘重複(別名)
        RightMeta = 58,    ┐
        RightWindows = 58, ┤
        RightApple = 58,   ┤
        RightCommand = 58, ┘重複(別名)
        */
        #endregion Note: duplicate value [Unity2019.4.12f1 時点]

        //==========================================================

        /// <summary>
        /// ToInputSystemKey() の変換マップを変更する or 無いときは追加.
        /// ※キーは小文字で統一される
        /// </summary>
        /// <param name="key">Key.ToString() or 変換前の KeyCode.ToString()</param>
        /// <param name="newKey">変換後の Key</param>
        /// <returns>true = 追加成功</returns>
        public static bool UpdateToInputSystemKeyMap(string key, Key newKey)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            var lower = key.ToLowerOrCached();  //小文字で統一
            ToInputSystemKeyMap[lower] = newKey;
            return true;
        }

        //==========================================================

        // Key → string 変換キャッシュ
        static readonly Dictionary<Key, string> _inputSystemKeyToStringMap 
            = new Dictionary<Key, string>(InputSystemKeyLength);

        /// <summary>
        /// Key → string 変換 or キャッシュから取得
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string ToStringOrCached(this Key key)
        {
            if (_inputSystemKeyToStringMap.ContainsKey(key))
                return _inputSystemKeyToStringMap[key];

            var str = key.ToString();
            _inputSystemKeyToStringMap[key] = str;
            return str;
        }

#endif

        //==========================================================

        //string → KeyCode 変換マップ（※キーは小文字で統一）
        static Dictionary<string, KeyCode> _toKeyCodeMap;
        static Dictionary<string, KeyCode> ToKeyCodeMap {
            get {
                if (_toKeyCodeMap == null)
                {
                    var names = Enum.GetNames(typeof(KeyCode));
                    var values = Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>();
                    _toKeyCodeMap = new Dictionary<string, KeyCode>(names.Length);

                    //※enum は値の重複ができるため、names を基準にする
                    for (int i = 0; i < names.Length; i++)
                    {
                        var key = names[i].ToLowerOrCached(); //小文字で統一
                        var value = values.ElementAt(i);
                        if (!_toKeyCodeMap.ContainsKey(key))
                        {
                            _toKeyCodeMap[key] = value;

                            //先頭文字列で変換対応するもの (※小文字で統一)
                            var pre = ReplacePrefixIfStartsWith(key, _toKeyCodePrefixMap);
                            if (pre != key)
                            {
                                if (!_toKeyCodeMap.ContainsKey(pre))
                                    _toKeyCodeMap[pre] = value;
                            }

                            //完全一致で変換対応するもの (※小文字で統一)
                            if (_toKeyCodeWholeMap.TryGetValue(key, out var whole))
                            {
                                if (!_toKeyCodeMap.ContainsKey(whole))
                                    _toKeyCodeMap[whole] = value;
                            }
                        }
                    }
                }
                return _toKeyCodeMap;
            }
        }
        #region Note: duplicate value [Unity2019.4.12f1 時点]
        /*
        ※KeyCode に大文字小文字無視での重複は無い前提（Unity2019.4.11f1 時点では確認済み)
        ※values だと 324 個になってしまう（names だと 326 個）
        RightCommand = 309, ┐
        RightApple = 309,   ┘重複(別名)
        LeftCommand = 310,  ┐
        LeftApple = 310,    ┘重複(別名)
        */
        #endregion Note: duplicate value [Unity2019.4.12f1 時点]

        //==========================================================

        /// <summary>
        /// ToKeyCode() の変換マップを変更する or 無いときは追加.
        /// ※キーは小文字で統一される
        /// </summary>
        /// <param name="key">変換前の Key.ToString()</param>
        /// <param name="newKeyCode">変換後の KeyCode</param>
        /// <returns>true = 追加成功</returns>
        public static bool UpdateToKeyCodeMap(string key, KeyCode newKeyCode)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            var lower = key.ToLowerOrCached();  //小文字で統一
            ToKeyCodeMap[lower] = newKeyCode;
            return true;
        }
        

        //==========================================================

        // KeyCode → string 変換キャッシュ
        static readonly Dictionary<KeyCode, string> _keyCodeToStringMap 
            = new Dictionary<KeyCode, string>(KeyCodeLength);

        /// <summary>
        /// KeyCode → string 変換 or キャッシュから取得
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        public static string ToStringOrCached(this KeyCode keyCode)
        {
            if (_keyCodeToStringMap.ContainsKey(keyCode))
                return _keyCodeToStringMap[keyCode];

            var str = keyCode.ToString();
            _keyCodeToStringMap[keyCode] = str;
            return str;
        }

        //==========================================================

        // 小文字化の変換キャッシュ
        static readonly Dictionary<string, string> _toLowerStringMap 
            = new Dictionary<string, string>(384);

        /// <summary>
        /// 小文字化 or キャッシュから取得
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        static string ToLowerOrCached(this string keyName)
        {
            if (_toLowerStringMap.ContainsKey(keyName))
            {
                return _toLowerStringMap[keyName];
            }

            var lower = keyName.ToLower();
            _toLowerStringMap[keyName] = lower;
            return lower;
        }

        //==========================================================

        #endregion Make convert map

        //==========================================================



        //==========================================================
        // 特殊(固定)変換マップデータ
        //==========================================================

        #region Convert map special data

        //先頭文字列で変換対応するもの (※小文字にしておくこと)
        //※ 17 個対応 (Unity2019.4.11f1 時点)
        static readonly Dictionary<string, string> _toInputSystemKeyPrefixMap
            = new Dictionary<string, string>()
            {
                // Key    ,  KeyCode
                { "digit" , "alpha" },
                { "numpad", "keypad" },
            };


        //value - key を逆転したマップ
        static readonly Dictionary<string, string> _toKeyCodePrefixMap
            = _toInputSystemKeyPrefixMap.ToDictionary(e => e.Value, e => e.Key);  //value - key を逆転したマップ


        //マップにある場合、先頭文字列をすげ替える (※keyName は小文字にしておくこと)
        static string ReplacePrefixIfStartsWith(string keyName, Dictionary<string, string> map)
        {
            foreach (var item in map)
            {
                if (keyName.StartsWith(item.Key))
                    return item.Value + keyName.Substring(item.Key.Length);  //Prefix すげ替え
            }
            return keyName;  //map に無ければ、そのまま
        }

        //==========================================================

        //完全一致で変換対応するもの (※小文字にしておくこと)
        static readonly Dictionary<string, string> _toInputSystemKeyWholeMap
            = new Dictionary<string, string>()
            {
                // Key         ,  KeyCode
                { "enter"      , "return" },
                { "printscreen", "print" },
                { "contextmenu", "menu" },
                { "leftctrl"   , "leftcontrol" },
                { "rightctrl"  , "rightcontrol" },
                { "leftmeta"   , "leftcommand" },   //KeyCode には LeftMeta はない
                { "rightmeta"  , "rightcommand" }, //KeyCode には RightMeta はない
            };


        //value - key を逆転したマップ
        static readonly Dictionary<string, string> _toKeyCodeWholeMap
            = _toInputSystemKeyWholeMap.ToDictionary(e => e.Value, e => e.Key);

        //==========================================================

#if ENABLE_INPUT_SYSTEM

        //displayName を使って、動的にマップを変更するデータ（KeyCode → InputSystem.Key 用）
        //"backslash" など OEM～ となるキーのみ
        static readonly Dictionary<string, string> _inputSystemKeyChangeOEMMap
            = new Dictionary<string, string>()
            {
                 //Key       , displayName
                { "backslash", "\\" },  //日本語キーボードだと OEM2 になる
            };


        //displayName を使って、動的にマップを変更する（KeyCode → InputSystem.Key 用）
        public static void UpdateToInputSystemKeyMapByDisplayName(Dictionary<string, string> map)
        {
            //Control Path を使って、動的にマップを変更するデータ
            foreach (var item in map)
            {
                var newKey = GetKeyFromDisplayName(item.Value);
                if (newKey != Key.None)
                    UpdateToInputSystemKeyMap(item.Key, newKey);
            }
        }

        //==========================================================

        //displayName を使って、動的にマップを変更するデータ（KeyCode → InputSystem.Key 用）
        //※記号キーのみ
        static readonly Dictionary<string, string> _inputSystemKeyChangeSymbolMap
            = new Dictionary<string, string>()
            {
                 //Key          , displayName
                { "minus"       , "-" },
                { "caret"       , "^" },  //日本語キーボードだと Equals になる
                { "at"          , "@" },  //日本語キーボードだと LeftBracket になる
                { "leftbracket" , "[" },  //日本語キーボードだと RightBracket になる
                { "rightbracket", "]" },  //日本語キーボードだと Backslash になる
                { "colon"       , ":" },  //日本語キーボードだと Quote になる
                { "semicolon"   , ";" },
                { "comma"       , "," },
                { "period"      , "." },
                { "slash"       , "/" },
            };


        //記号キーの ToInputSystemKeyMap を KeyboardLayout に基づいて更新する
        //※これを使用すると、現在のキーボードによって、記号キーのマップが更新されるので、
        //  元の KeyCode → InputSystem.Key の記号キー対応も変わるので注意。
        //  例えば、日本語キーボードなら、"colon" ([:]キー) → Quote が返るようになる。
        public static void RefreshSymbolToInputSystemKeyMapWithKeyboardLayout()
        {
            UpdateToInputSystemKeyMapByDisplayName(_inputSystemKeyChangeSymbolMap);
        }


        //==========================================================

        /// <summary>
        /// displayName から Key (enum) を取得する.
        /// https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Controls.html#control-paths
        /// ※失敗は None になる。None 自体は取得できない。
        /// </summary>
        /// <param name="displayName">例: "a", "/", "\\" 等</param>
        /// <returns>成功 = None 以外 / 失敗 = None</returns>
        public static Key GetKeyFromDisplayName(string displayName)
        {
            try
            {
                var control = Keyboard.current.FindKeyOnCurrentKeyboardLayout(displayName);
                return control.keyCode;
            }
            catch (Exception e)
            {
                #if UNITY_EDITOR
                Debug.LogError("displayName = " + displayName + " : " + e.Message);
                #endif
            }
            return Key.None;  //失敗
        }


        /// <summary>
        /// control path から Key (enum) を取得する.
        /// https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Controls.html#control-paths
        /// ※失敗は None になる。None 自体は取得できない。
        /// </summary>
        /// <param name="controlPath">例: "#(a)", "#(\\/)", "#(\\\\)" 等</param>
        /// <returns>成功 = None 以外 / 失敗 = None</returns>
        public static Key GetKeyFromControlPath(string controlPath)
        {
            try
            {
                var control = (KeyControl)Keyboard.current[controlPath];
                return control.keyCode;
            }
            catch (Exception e)
            {
                #if UNITY_EDITOR
                Debug.LogError("controlPath = " + controlPath + " : " + e.Message);
                #endif
            }
            return Key.None;  //失敗
        }

#endif

        //==========================================================

        #endregion Convert map special data

        #region Note: InputSystem.Key → KeyCode で対応してないもの [Unity2019.4.12f1 時点]
        /*
        6 個

        OEM1
        OEM2
        OEM3
        OEM4
        OEM5
        IMESelected  → Keyboard.current[inputSystemKey] すると、ArgumentOutOfRangeException: Specified argument was out of the range of valid values. が出る
        */
        #endregion Note: InputSystem.Key → KeyCode で対応してないもの [Unity2019.4.12f1 時点]

        #region Note: KeyCode → InputSystem.Key で対応してないもの Mouse0 より前 [Unity2019.4.12f1 時点]
        /*
        全：215 個 (= 28 + 187)
        以下のみ：28 個
        ※記号等は、日本語・英語キーボードで配置が違うので、なるべく使わない方が良いかも
        https://blog.tsukumo.co.jp/gaming/2015/04/post_16.html

        Clear     → 反応しない(※日本語キーボードのとき)
        Exclaim
        DoubleQuote
        Hash
        Dollar
        Percent
        Ampersand
        LeftParen
        RightParen
        Asterisk
        Plus
        Colon
        Less
        Greater
        Question
        At
        Caret
        Underscore
        LeftCurlyBracket
        Pipe
        RightCurlyBracket
        Tilde
        F13
        F14
        F15
        Help
        SysReq   → PrtScn (PrintScreen) で反応する(※日本語キーボードのとき), 逆に Print は反応しない
        Break
        */
        #endregion Note: KeyCode → InputSystem.Key で対応してないもの Mouse0 より前 [Unity2019.4.12f1 時点]

        #region Note: KeyCode → InputSystem.Key で対応してないもの Mouse0 以降 [Unity2019.4.12f1 時点]
        /*
        187 個

        Mouse0
        Mouse1
        Mouse2
        Mouse3
        Mouse4
        Mouse5
        Mouse6
        JoystickButton0
        JoystickButton1
        JoystickButton2
        JoystickButton3
        JoystickButton4
        JoystickButton5
        JoystickButton6
        JoystickButton7
        JoystickButton8
        JoystickButton9
        JoystickButton10
        JoystickButton11
        JoystickButton12
        JoystickButton13
        JoystickButton14
        JoystickButton15
        JoystickButton16
        JoystickButton17
        JoystickButton18
        JoystickButton19
        Joystick1Button0
        Joystick1Button1
        Joystick1Button2
        Joystick1Button3
        Joystick1Button4
        Joystick1Button5
        Joystick1Button6
        Joystick1Button7
        Joystick1Button8
        Joystick1Button9
        Joystick1Button10
        Joystick1Button11
        Joystick1Button12
        Joystick1Button13
        Joystick1Button14
        Joystick1Button15
        Joystick1Button16
        Joystick1Button17
        Joystick1Button18
        Joystick1Button19
        Joystick2Button0
        Joystick2Button1
        Joystick2Button2
        Joystick2Button3
        Joystick2Button4
        Joystick2Button5
        Joystick2Button6
        Joystick2Button7
        Joystick2Button8
        Joystick2Button9
        Joystick2Button10
        Joystick2Button11
        Joystick2Button12
        Joystick2Button13
        Joystick2Button14
        Joystick2Button15
        Joystick2Button16
        Joystick2Button17
        Joystick2Button18
        Joystick2Button19
        Joystick3Button0
        Joystick3Button1
        Joystick3Button2
        Joystick3Button3
        Joystick3Button4
        Joystick3Button5
        Joystick3Button6
        Joystick3Button7
        Joystick3Button8
        Joystick3Button9
        Joystick3Button10
        Joystick3Button11
        Joystick3Button12
        Joystick3Button13
        Joystick3Button14
        Joystick3Button15
        Joystick3Button16
        Joystick3Button17
        Joystick3Button18
        Joystick3Button19
        Joystick4Button0
        Joystick4Button1
        Joystick4Button2
        Joystick4Button3
        Joystick4Button4
        Joystick4Button5
        Joystick4Button6
        Joystick4Button7
        Joystick4Button8
        Joystick4Button9
        Joystick4Button10
        Joystick4Button11
        Joystick4Button12
        Joystick4Button13
        Joystick4Button14
        Joystick4Button15
        Joystick4Button16
        Joystick4Button17
        Joystick4Button18
        Joystick4Button19
        Joystick5Button0
        Joystick5Button1
        Joystick5Button2
        Joystick5Button3
        Joystick5Button4
        Joystick5Button5
        Joystick5Button6
        Joystick5Button7
        Joystick5Button8
        Joystick5Button9
        Joystick5Button10
        Joystick5Button11
        Joystick5Button12
        Joystick5Button13
        Joystick5Button14
        Joystick5Button15
        Joystick5Button16
        Joystick5Button17
        Joystick5Button18
        Joystick5Button19
        Joystick6Button0
        Joystick6Button1
        Joystick6Button2
        Joystick6Button3
        Joystick6Button4
        Joystick6Button5
        Joystick6Button6
        Joystick6Button7
        Joystick6Button8
        Joystick6Button9
        Joystick6Button10
        Joystick6Button11
        Joystick6Button12
        Joystick6Button13
        Joystick6Button14
        Joystick6Button15
        Joystick6Button16
        Joystick6Button17
        Joystick6Button18
        Joystick6Button19
        Joystick7Button0
        Joystick7Button1
        Joystick7Button2
        Joystick7Button3
        Joystick7Button4
        Joystick7Button5
        Joystick7Button6
        Joystick7Button7
        Joystick7Button8
        Joystick7Button9
        Joystick7Button10
        Joystick7Button11
        Joystick7Button12
        Joystick7Button13
        Joystick7Button14
        Joystick7Button15
        Joystick7Button16
        Joystick7Button17
        Joystick7Button18
        Joystick7Button19
        Joystick8Button0
        Joystick8Button1
        Joystick8Button2
        Joystick8Button3
        Joystick8Button4
        Joystick8Button5
        Joystick8Button6
        Joystick8Button7
        Joystick8Button8
        Joystick8Button9
        Joystick8Button10
        Joystick8Button11
        Joystick8Button12
        Joystick8Button13
        Joystick8Button14
        Joystick8Button15
        Joystick8Button16
        Joystick8Button17
        Joystick8Button18
        Joystick8Button19
        */
        #endregion Note: KeyCode → InputSystem.Key で対応してないもの Mouse0 以降 [Unity2019.4.12f1 時点]

        //==========================================================

#endregion Type convert Section

    }
}

using UnityEngine;

namespace Daifugo.Data
{
    /// <summary>
    /// ゲームルール設定（どのルールが有効・無効か）
    /// Phase 1: 最小限のフラグのみ（将来の拡張準備）
    /// Phase 2以降: 各種ローカルルールのフラグを追加
    /// </summary>
    [CreateAssetMenu(fileName = "GameRules", menuName = "Daifugo/Data/GameRules")]
    public class GameRulesSO : ScriptableObject
    {
        // ========== Phase 2以降で有効化予定 ==========

        [Header("Basic Rules")]
        [Tooltip("革命ルールを有効にする（4枚出しで強さ逆転）")]
        [SerializeField] private bool enableRevolution = false;

        [Tooltip("8切りルールを有効にする（8で場が流れる）")]
        [SerializeField] private bool enable8Cut = true;  // Phase 1で既に実装済み

        [Tooltip("縛りルールを有効にする（同じスート連続でスート固定）")]
        [SerializeField] private bool enableBind = false;

        [Tooltip("スペ3返しルールを有効にする（♠3でジョーカーを返せる）")]
        [SerializeField] private bool enableSpade3Return = false;

        // ========== Public Properties ==========

        /// <summary>革命ルールが有効か</summary>
        public bool IsRevolutionEnabled => enableRevolution;

        /// <summary>8切りルールが有効か</summary>
        public bool Is8CutEnabled => enable8Cut;

        /// <summary>縛りルールが有効か</summary>
        public bool IsBindEnabled => enableBind;

        /// <summary>スペ3返しルールが有効か</summary>
        public bool IsSpade3ReturnEnabled => enableSpade3Return;

        // ========== Phase 3以降で追加予定 ==========

        // [Header("Advanced Rules")]
        // [Tooltip("都落ちルールを有効にする")]
        // public bool enableMiyakoOchi = false;
        //
        // [Tooltip("下克上ルールを有効にする")]
        // public bool enableGekoJo = false;
        //
        // [Tooltip("イレブンバックルールを有効にする")]
        // public bool enable11Back = false;
        //
        // [Tooltip("禁止上がりルールを有効にする")]
        // public bool enableForbiddenFinish = false;

#if UNITY_EDITOR
        /// <summary>
        /// Validates Inspector settings
        /// </summary>
        private void OnValidate()
        {
            // Phase 1では8切りのみ実装済み
            if (!enable8Cut)
            {
                Debug.LogWarning($"[{GetType().Name}] 8切りルールはPhase 1で実装済みです。無効化すると動作に影響があります。", this);
            }
        }
#endif
    }
}

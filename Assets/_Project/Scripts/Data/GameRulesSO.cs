using UnityEngine;

namespace Daifugo.Data
{
    /// <summary>
    /// ゲームルール設定（どのルールが有効・無効か）
    /// Runtime で変更可能（UI からルール選択時に使用）
    ///
    /// 【設計判断】
    /// - ScriptableObject を直接継承（VariableSO ではない）
    ///   理由: 複合型（4つの関連するbool値のグループ）であり、
    ///         Variables パターンは単一値を想定している
    /// - RuntimeSet パターンと同様のアプローチ
    ///   特殊な責務には ScriptableObject を直接継承
    /// - EventChannel は現時点では不要と判断
    ///   将来必要になったら追加可能（setter に onRulesChanged?.RaiseEvent() を追加）
    /// - シンプルさと責務の明確さを優先
    /// </summary>
    [CreateAssetMenu(fileName = "GameRules", menuName = "Daifugo/Data/GameRules")]
    public class GameRulesSO : ScriptableObject
    {
        // ========== Basic Rules ==========

        [Header("Basic Rules")]
        [Tooltip("8切りルールを有効にする（8で場が流れる）")]
        [SerializeField] private bool enable8Cut = true;

        [Tooltip("縛りルールを有効にする（同じスート連続でスート固定）")]
        [SerializeField] private bool enableBind = true;

        [Header("Advanced Rules (未実装)")]
        [Tooltip("革命ルールを有効にする（4枚出しで強さ逆転）")]
        [SerializeField] private bool enableRevolution = false;

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

        // ========== Public Methods (Runtime Modification) ==========

        /// <summary>
        /// 8切りルールの有効/無効を設定
        /// Runtime でルールを変更する場合に使用
        /// </summary>
        /// <param name="value">有効にする場合は true、無効にする場合は false</param>
        public void Set8Cut(bool value)
        {
            enable8Cut = value;
        }

        /// <summary>
        /// 縛りルールの有効/無効を設定
        /// Runtime でルールを変更する場合に使用
        /// </summary>
        /// <param name="value">有効にする場合は true、無効にする場合は false</param>
        public void SetBind(bool value)
        {
            enableBind = value;
        }

        /// <summary>
        /// 革命ルールの有効/無効を設定
        /// Runtime でルールを変更する場合に使用
        /// </summary>
        /// <param name="value">有効にする場合は true、無効にする場合は false</param>
        public void SetRevolution(bool value)
        {
            enableRevolution = value;
        }

        /// <summary>
        /// スペ3返しルールの有効/無効を設定
        /// Runtime でルールを変更する場合に使用
        /// </summary>
        /// <param name="value">有効にする場合は true、無効にする場合は false</param>
        public void SetSpade3Return(bool value)
        {
            enableSpade3Return = value;
        }

        // ========== 将来追加予定のルール ==========

        // [Header("Additional Rules (未実装)")]
        // [Tooltip("都落ちルールを有効にする")]
        // [SerializeField] private bool enableMiyakoOchi = false;
        //
        // [Tooltip("下克上ルールを有効にする")]
        // [SerializeField] private bool enableGekoJo = false;
        //
        // [Tooltip("イレブンバックルールを有効にする")]
        // [SerializeField] private bool enable11Back = false;
        //
        // [Tooltip("禁止上がりルールを有効にする")]
        // [SerializeField] private bool enableForbiddenFinish = false;
    }
}

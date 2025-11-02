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
        // ========== Basic Rules (日本大富豪連盟 五大公式ルール) ==========

        [Header("Basic Rules (日本大富豪連盟 五大公式ルール)")]
        [Tooltip("8切りルールを有効にする（8で場が流れる）")]
        [SerializeField] private bool enable8Cut = true;

        [Tooltip("縛りルールを有効にする（同じスート連続でスート固定）")]
        [SerializeField] private bool enableBind = true;

        [Tooltip("スペ3返しルールを有効にする（♠3でジョーカーを返せる）")]
        [SerializeField] private bool enableSpade3Return = false;

        // ========== Special Rules (ローカルルール) ==========

        [Header("Special Rules (ローカルルール)")]
        [Tooltip("11バックルールを有効にする（Jで一時的に強さ逆転）")]
        [SerializeField] private bool enable11Back = false;

        [Tooltip("禁止上がりルールを有効にする（ジョーカー、2、8、スペ3で上がると負け）")]
        [SerializeField] private bool enableForbiddenFinish = false;

        // ========== Advanced Rules (複数枚出しルール) ==========

        [Header("Advanced Rules (複数枚出しルール)")]
        [Tooltip("革命ルールを有効にする（4枚出しで強さ逆転）")]
        [SerializeField] private bool enableRevolution = false;

        // ========== Public Properties ==========

        /// <summary>革命ルールが有効か</summary>
        public bool IsRevolutionEnabled => enableRevolution;

        /// <summary>8切りルールが有効か</summary>
        public bool Is8CutEnabled => enable8Cut;

        /// <summary>縛りルールが有効か</summary>
        public bool IsBindEnabled => enableBind;

        /// <summary>11バックルールが有効か</summary>
        public bool Is11BackEnabled => enable11Back;

        /// <summary>スペ3返しルールが有効か</summary>
        public bool IsSpade3ReturnEnabled => enableSpade3Return;

        /// <summary>禁止上がりルールが有効か</summary>
        public bool IsForbiddenFinishEnabled => enableForbiddenFinish;

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
        /// 11バックルールの有効/無効を設定
        /// Runtime でルールを変更する場合に使用
        /// </summary>
        /// <param name="value">有効にする場合は true、無効にする場合は false</param>
        public void Set11Back(bool value)
        {
            enable11Back = value;
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

        /// <summary>
        /// 禁止上がりルールの有効/無効を設定
        /// Runtime でルールを変更する場合に使用
        /// </summary>
        /// <param name="value">有効にする場合は true、無効にする場合は false</param>
        public void SetForbiddenFinish(bool value)
        {
            enableForbiddenFinish = value;
        }

        // ========== 将来追加検討中のルール ==========

        // [Header("Additional Rules (将来追加検討中)")]
        // [Tooltip("都落ちルールを有効にする（大富豪が最下位になったら貧民に転落）")]
        // [SerializeField] private bool enableMiyakoOchi = false;
        //
        // [Tooltip("下克上ルールを有効にする（貧民が1位になったら大富豪に昇格）")]
        // [SerializeField] private bool enableGekoJo = false;
    }
}

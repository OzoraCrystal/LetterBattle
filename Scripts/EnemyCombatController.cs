using UnityEngine;
using System.Collections;
using TMPro;
using System.Text;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyStateMachine))]
public class EnemyCombatController : MonoBehaviour
{
    // 配置参数
    [SerializeField] private float baseAttackInterval = 3f;
    [SerializeField] private TextMeshProUGUI spellDisplay;

    // 依赖组件
    private EnemyStateMachine _stateMachine;
    private List<Skill> _skillData;
    private HealthManager _playerHealth;
    private BattleManager _battleManager;

    // 运行时数据
    public Skill _currentSkill;
    private int _currentSpellIndex;
    private Coroutine _attackCoroutine;

    private void Awake()
    {
        _stateMachine = GetComponent<EnemyStateMachine>();
    }
    public void Initialize(List<Skill> data, HealthManager player, BattleManager manager)
    {
        _skillData = data;
        _playerHealth = player;
        _battleManager = manager;
        StartCombatCycle();
    }

    private void StartCombatCycle()
    {
        StopAllCoroutines();
        if (_attackCoroutine != null) StopCoroutine(_attackCoroutine);
        _attackCoroutine = StartCoroutine(CombatBehavior());
    }

    private IEnumerator CombatBehavior()
    {
        while (_battleManager.currentState != BattleState.Death) // 状态检查
        {
            _stateMachine.TransitionTo(EnemyState.Idle);
            yield return new WaitForSeconds(baseAttackInterval);

            if (_battleManager.currentState == BattleState.Death) yield break;

            _stateMachine.TransitionTo(EnemyState.Selecting);
            yield return StartCoroutine(SelectSkill());

            if (_battleManager.currentState == BattleState.Death) yield break;

            _stateMachine.TransitionTo(EnemyState.Spelling);
            yield return StartCoroutine(ExecuteSpelling());

            if (_battleManager.currentState == BattleState.Death) yield break;

            _stateMachine.TransitionTo(EnemyState.Attacking);
            yield return StartCoroutine(PerformAttack());
        }
    }

    private IEnumerator SelectSkill()
    {
        _currentSkill = _skillData[Random.Range(0, _skillData.Count)];
        yield return null;
    }

    private IEnumerator ExecuteSpelling()
    {
        _currentSpellIndex = 0;
        UpdateSpellDisplay();

        while (_currentSpellIndex < _currentSkill.skillSequence.Length)
        {
            yield return new WaitForSeconds(_currentSkill.spellInterval);
            _currentSpellIndex++;
            UpdateSpellDisplay();
        }
    }

    private void UpdateSpellDisplay()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < _currentSkill.skillSequence.Length; i++)
        {
            string color = i <= _currentSpellIndex ? "green" : "white";
            sb.Append($"<color={color}>{_currentSkill.skillSequence[i]}</color>");
        }
        spellDisplay.text = sb.ToString();
    }
    private IEnumerator PerformAttack()
    {
        _playerHealth.TakeDamage(_currentSkill.skillDamage);

        yield return _battleManager.StartCoroutine(_battleManager.ShowEnemyAttackEffect());

        yield return new WaitForSeconds(0.5f);
    }
    public void StopAllActions()
    {
        StopAllCoroutines();
        _stateMachine.TransitionTo(EnemyState.Idle);
    }
}
public enum EnemyState
{
    Idle,        // 空闲状态
    Selecting,   // 选择技能中
    Spelling,    // 拼写进行中
    Attacking    // 攻击执行中
}
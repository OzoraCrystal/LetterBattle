using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum BattleState
{
    Idle,      // 未拼写
    Spelling,  // 拼写中
    Stop,      // 停止
    Death      // 玩家死亡
}

public class BattleManager : MonoBehaviour
{
    // UI 引用
    public Slider countdownSlider;
    public TextMeshProUGUI PlayerBattleReport;
    public TextMeshProUGUI EnemyBattleReport;
    public TextMeshProUGUI EnemySpellText;

    // Manager 引用
    public PlayerUIManager playerUIManager;
    public EnemyUIManager enemyUIManager;
    public HealthManager playerHealthManager;
    public HealthManager enemyHealthManager;
    [SerializeField] private EnemyCombatController enemyController;

    [Header("Player Animation")]
    [SerializeField] private Animator playerAnimator;

    [Header("Enemy Animation")]
    [SerializeField] private Animator enemyAnimator;

    // 参数：输入等待时长（滑条实时显示）
    public float inputTimeLimit = 2f;
    private float remainingTime;

    // 角色技能数据
    public List<Skill> playerSkillData;
    public List<Skill> enemySkillData;

    // 当前输入的技能链（记录玩家每次按下的正确 WSAD 字母）
    public string currentInputSequence = "";
    private Coroutine timeoutCoroutine;
    private Coroutine skillPromptCoroutine;

    // 状态标识
    private bool isDisplayingMessage = false;
    private bool isSkillPromptActive = false;

    private static BattleManager _instance;
    public static BattleManager Instance => _instance;

    private SkillData skillData;

    // 当前状态，初始为 Idle
    public BattleState currentState = BattleState.Idle;

    private void Awake()
    {
        _instance = this;
        skillData = GetSkillData();
        playerSkillData = GetSkillDataByType(SkillType.Player);
        enemySkillData = GetSkillDataByType(SkillType.Enemy);
    }

    void Start()
    {
        InitializeBattle();
        enemyController.Initialize(
            enemySkillData,
            playerHealthManager,
            this
        );
        skillPromptCoroutine = StartCoroutine(ShowSkillPrompt());
    }

    void Update()
    {
        if (isDisplayingMessage) return;

        switch (currentState)
        {
            case BattleState.Idle:
                HandleIdleInput();
                break;
            case BattleState.Spelling:
                HandleSpellingInput();
                break;
            case BattleState.Stop:
                break;
            case BattleState.Death:
                break;
        }
        UpdateCountdownBar();
    }

    private void InitializeBattle()
    {
        EnemyBattleReport.text = "";
        EnemySpellText.text = "";
        countdownSlider.gameObject.SetActive(false);

        playerUIManager?.BindPlayerHealth(playerHealthManager);
        enemyUIManager?.BindEnemyHealth(enemyHealthManager);

        playerHealthManager.OnDeath += OnPlayerDeath;
        enemyHealthManager.OnDeath += OnEnemyDeath;
        enemyController.GetComponent<EnemyStateMachine>().OnStateChanged += HandleEnemyStateChange;
    }

    #region —— 提示与反馈

    private IEnumerator ShowSkillPrompt()
    {
        isSkillPromptActive = true;
        PlayerBattleReport.color = Color.cyan;
        PlayerBattleReport.text = "";
        string prompt = "请拼写技能链";
        StringBuilder sb = new StringBuilder();
        foreach (char c in prompt)
        {
            if (!isSkillPromptActive) yield break;
            sb.Append(c);
            PlayerBattleReport.text = sb.ToString();
            yield return new WaitForSeconds(1f / 30);
        }
        yield return new WaitForSeconds(0.1f);
        isSkillPromptActive = false;
    }

    // 非阻塞反馈文本（例如“技能拼写中”）
    private IEnumerator ShowFeedback(string message, float seconds)
    {
        isDisplayingMessage = true;
        string original = PlayerBattleReport.text;
        PlayerBattleReport.text = "";
        foreach (char c in message)
        {
            PlayerBattleReport.text += c;
            yield return new WaitForSeconds(1f / 30);
        }
        yield return new WaitForSeconds(seconds);
        PlayerBattleReport.text = original;
        isDisplayingMessage = false;
    }

    // 显示反馈后（例如拼写成功或失败），会进入 Stop 状态后再恢复原状态
    private IEnumerator ShowResult(string message, Color color, float seconds)
    {
        if (isDisplayingMessage) yield break;
        if (isSkillPromptActive)
        {
            StopSkillPrompt();
            PlayerBattleReport.text = "";
        }
        isDisplayingMessage = true;
        BattleState originalState = currentState;
        ChangeState(BattleState.Stop);
        PlayerBattleReport.color = color;
        PlayerBattleReport.text = "";
        foreach (char c in message)
        {
            PlayerBattleReport.text += c;
            yield return new WaitForSeconds(1f / 30);
        }
        yield return new WaitForSeconds(seconds);
        PlayerBattleReport.text = "";
        isDisplayingMessage = false;
        ChangeState(originalState);
    }

    // 关闭当前提示（例如 Idle 状态下的“请拼写技能链”）
    private void StopSkillPrompt()
    {
        isSkillPromptActive = false;
        if (skillPromptCoroutine != null)
        {
            StopCoroutine(skillPromptCoroutine);
            skillPromptCoroutine = null;
        }
    }

    #endregion

    #region —— 输入处理

    // Idle 状态下，检测 WSAD 键——当按下的键能作为某个技能链的首字母时，进入 Spelling 状态
    private void HandleIdleInput()
    {
        if (CheckWSADKeyDown(out char key))
        {
            string keyStr = key.ToString();
            if (IsValidInputSequence(keyStr))
            {
                // 合法输入，进入拼写状态，并显示“技能拼写中”
                currentInputSequence = keyStr;
                ChangeState(BattleState.Spelling);
                StopSkillPrompt();
                StartSpellingTimeout();
                UIManager.Instance.RefreshSkillCells();
            }
            else
            {
                StartCoroutine(ShowFeedback("拼写失败!", 1f));
            }
        }
    }

    // Spelling 状态下，每次按下 WSAD 键都将其追加到当前输入中
    private void HandleSpellingInput()
    {
        if (CheckWSADKeyDown(out char key))
        {
            string newSequence = currentInputSequence + key.ToString();
            if (IsValidInputSequence(newSequence))
            {
                currentInputSequence = newSequence;
                ResetSpellingTimeout();
                UIManager.Instance.RefreshSkillCells();
                // 检查是否已完整匹配到某个技能
                Skill matchedSkill = GetMatchedSkill(currentInputSequence);
                if (matchedSkill != null && currentInputSequence.Length == matchedSkill.skillSequence.Length)
                {
                    StopSpellingTimeout();
                    HandleSpellingSuccess(matchedSkill);
                }
            }
            else
            {
                StopSpellingTimeout();
                HandleSpellingFailure("拼写失败!");
            }
        }
    }

    // 检测 WSAD 键按下，返回对应的字符
    private bool CheckWSADKeyDown(out char key)
    {
        if (Input.GetKeyDown(KeyCode.W)) { key = 'W'; return true; }
        if (Input.GetKeyDown(KeyCode.A)) { key = 'A'; return true; }
        if (Input.GetKeyDown(KeyCode.S)) { key = 'S'; return true; }
        if (Input.GetKeyDown(KeyCode.D)) { key = 'D'; return true; }
        key = '\0';
        return false;
    }

    // 判断给定的序列是否为任一玩家技能的前缀（不区分大小写）
    private bool IsValidInputSequence(string sequence)
    {
        return playerSkillData.Any(skill => skill.skillSequence.StartsWith(sequence, System.StringComparison.OrdinalIgnoreCase));
    }

    // 当前输入序列与某个技能的完整序列相等时，则返回该技能；否则返回 null
    private Skill GetMatchedSkill(string sequence)
    {
        return playerSkillData.FirstOrDefault(skill => string.Equals(skill.skillSequence, sequence, System.StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region —— 超时管理

    private void StartSpellingTimeout()
    {
        remainingTime = inputTimeLimit;
        if (timeoutCoroutine != null) StopCoroutine(timeoutCoroutine);
        timeoutCoroutine = StartCoroutine(SpellingTimeoutCoroutine());
    }

    private void ResetSpellingTimeout()
    {
        remainingTime = inputTimeLimit;
    }

    private void StopSpellingTimeout()
    {
        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = null;
        }
    }

    private IEnumerator SpellingTimeoutCoroutine()
    {
        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            yield return null;
        }
        // 超时后：显示“拼写超时!”并重置状态
        HandleSpellingFailure("拼写超时!");
    }

    // 更新倒计时滑条
    private void UpdateCountdownBar()
    {
        if (currentState != BattleState.Spelling) return;
        countdownSlider.gameObject.SetActive(true);
        countdownSlider.value = Mathf.Clamp01(remainingTime / Mathf.Max(inputTimeLimit, 0.1f));
    }

    #endregion

    #region —— 成功与失败处理

    // 当玩家成功拼写完整匹配到某个技能时调用
    private void HandleSpellingSuccess(Skill skill)
    {
        StartCoroutine(SpellingSuccessSequence(skill));
    }

    private IEnumerator SpellingSuccessSequence(Skill skill)
    {
        // 播放玩家施法、敌人受击动画
        playerAnimator.SetTrigger("spellSuccessTrigger");
        enemyAnimator.SetTrigger("hurtTrigger");
        enemyHealthManager.TakeDamage(skill.skillDamage);

        yield return ShowResult($"拼写成功! 造成{skill.skillDamage}点伤害!", Color.red, 1f);

        // 重置输入，状态返回 Idle，并重新提示“请拼写技能链”
        currentInputSequence = "";
        UIManager.Instance.RefreshSkillCells();
        ChangeState(BattleState.Idle);
        StartCoroutine(ShowSkillPrompt());
    }

    // 当输入错误或超时时调用
    private void HandleSpellingFailure(string message)
    {
        StartCoroutine(SpellingFailureSequence(message));
    }

    private IEnumerator SpellingFailureSequence(string message)
    {
        yield return ShowResult(message, Color.red, 1f);
        currentInputSequence = "";
        UIManager.Instance.RefreshSkillCells();
        ChangeState(BattleState.Idle);
        StartCoroutine(ShowSkillPrompt());
    }

    #endregion

    #region —— 状态管理、重试、工具方法

    public void ChangeState(BattleState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        countdownSlider.gameObject.SetActive(newState == BattleState.Spelling);
        if (newState == BattleState.Idle)
        {
            currentInputSequence = "";
            UIManager.Instance.RefreshSkillCells();
            countdownSlider.gameObject.SetActive(false);
        }
    }

    public void OnPlayerDeath()
    {
        playerAnimator.SetTrigger("deathTrigger");
        ChangeState(BattleState.Death);
        enemyController.StopAllActions();
        StopAllCoroutines();
        StartCoroutine(ShowResultAndWaitForRetry("玩家失败! 按R键重试", Color.red));
    }

    public void OnEnemyDeath()
    {
        enemyAnimator.SetTrigger("deathTrigger");
        ChangeState(BattleState.Death);
        enemyController.StopAllActions();
        StopAllCoroutines();
        StartCoroutine(ShowResultAndWaitForRetry("战斗成功! 按R键重新开始", Color.green));
    }

    private IEnumerator ShowResultAndWaitForRetry(string message, Color color)
    {
        isDisplayingMessage = true;
        PlayerBattleReport.color = color;
        PlayerBattleReport.text = "";
        foreach (char c in message)
        {
            PlayerBattleReport.text += c;
            yield return new WaitForSeconds(1f / 30);
        }
        while (!Input.GetKeyDown(KeyCode.R))
        {
            yield return null;
        }
        PlayerBattleReport.text = "";
        isDisplayingMessage = false;
        RestartBattle();
    }

    private void RestartBattle() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    public SkillData GetSkillData()
    {
        if (skillData == null)
        {
            skillData = Resources.Load<SkillData>("SkillData/LHSkillData");
        }
        return skillData;
    }

    public List<Skill> GetSkillDataByType(SkillType type)
    {
        List<Skill> filterData = new List<Skill>();
        foreach (var skill in skillData.DataList)
        {
            if (skill.skillType == type)
            {
                filterData.Add(skill);
            }
        }
        return filterData;
    }

    private void HandleEnemyStateChange(EnemyState newState)
    {
        switch (newState)
        {
            case EnemyState.Spelling:
                StartCoroutine(ShowReport("敌人正在施法!", Color.white));
                break;
            case EnemyState.Attacking:
                break;
        }
    }

    public IEnumerator ShowEnemyAttackEffect()
    {
        playerAnimator.SetTrigger("hurtTrigger");
        enemyAnimator.SetTrigger("spellSuccessTrigger");
        string message = $"敌人释放了 {enemyController._currentSkill.skillName} ! 造成{enemyController._currentSkill.skillDamage}点伤害! ";
        yield return StartCoroutine(ShowReport(message, Color.red));
        yield return new WaitForSeconds(0.3f);
    }

    private IEnumerator ShowReport(string message, Color color)
    {
        EnemyBattleReport.text = "";
        EnemyBattleReport.color = color;
        foreach (char c in message)
        {
            EnemyBattleReport.text += c;
            yield return new WaitForSeconds(1f / 30);
        }
    }

    #endregion
}

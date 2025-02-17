using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private Transform skillInfo;
    private Transform QuitBtn;
    private GameObject SkillCellPrefab;
    private List<SkillCell> skillCells = new List<SkillCell>();

    private static UIManager _instance;
    public static UIManager Instance => _instance;

    private void Awake()
    {
        _instance = this;
        InitUI();
        InitPrefab();
    }

    private void Start()
    {
        Refresh();
    }

    private void InitUI()
    {
        skillInfo = transform.Find("SkillInfo");
        QuitBtn=transform.Find("Bottom/QuitBtn");
        QuitBtn.GetComponent<Button>().onClick.AddListener(OnQuitGame);
    }

    private void InitPrefab()
    {
        SkillCellPrefab = Resources.Load("Prefabs/Skill") as GameObject;
    }

    // 实例化 SkillCell 并保存引用
    private void Refresh()
    {
        List<Skill> skills = BattleManager.Instance.playerSkillData;
        foreach (Skill skill in skills)
        {
            Transform cellTrans = Instantiate(SkillCellPrefab.transform, skillInfo) as Transform;
            SkillCell skillCell = cellTrans.GetComponent<SkillCell>();
            skillCell.Initialize(skill);
            skillCells.Add(skillCell);
        }
    }
    // 刷新所有 SkillCell 的高亮显示（根据当前输入序列）
    public void RefreshSkillCells()
    {
        foreach (var cell in skillCells)
        {
            cell.UpdateHighlight(BattleManager.Instance.currentInputSequence);
        }
    }
    private void OnQuitGame()
    {
        print(">>>>> OnQuitGame");
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

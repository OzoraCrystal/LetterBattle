using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillCell : MonoBehaviour
{
    private TextMeshProUGUI UISkillName;
    private TextMeshProUGUI UISequence;
    private Skill skill;

    private void Awake()
    {
        UISkillName = transform.Find("SkillName").GetComponent<TextMeshProUGUI>();
        UISequence = transform.Find("Sequence").GetComponent<TextMeshProUGUI>();
    }

    // 初始化时设置技能名称和初始序列（全部显示为灰色）
    public void Initialize(Skill skill)
    {
        this.skill = skill;
        UISkillName.text = skill.skillName;
        UISequence.text = $"<color=grey>{skill.skillSequence}</color>";
    }

    // 根据 BattleManager.currentInputSequence 更新高亮显示
    // 如果当前输入是该技能序列的前缀，则将匹配部分显示为绿色，后续部分为灰色；
    // 否则全部显示为灰色（提示输入错误）
    public void UpdateHighlight(string currentInput)
    {
        if (string.IsNullOrEmpty(currentInput))
        {
            UISequence.text = $"<color=grey>{skill.skillSequence}</color>";
            return;
        }

        if (skill.skillSequence.StartsWith(currentInput, System.StringComparison.OrdinalIgnoreCase))
        {
            string matchedPart = skill.skillSequence.Substring(0, currentInput.Length);
            string restPart = skill.skillSequence.Substring(currentInput.Length);
            UISequence.text = $"<color=green>{matchedPart}</color><color=grey>{restPart}</color>";
        }
        else
        {
            UISequence.text = $"<color=grey>{skill.skillSequence}</color>";
        }
    }
}

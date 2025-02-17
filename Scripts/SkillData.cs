using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "LH/SkillData", fileName = "SkillData")]
public class SkillData : ScriptableObject
{
    public List<Skill> DataList = new List<Skill>();
}

[System.Serializable]
public class Skill
{
    public string skillName;
    public string skillSequence;
    public float skillDamage;
    public float spellInterval;
    public SkillType skillType;
}
public enum SkillType
{
    Player,
    Enemy,
}
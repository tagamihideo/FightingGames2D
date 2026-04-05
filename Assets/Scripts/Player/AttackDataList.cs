using UnityEngine;

[CreateAssetMenu(fileName = "AttadkDataList", menuName = "Scriptable Objects/AttadkDataList")]
public class AttackDataList : ScriptableObject
{
    
    [Tooltip("キャラ名")] public string cName;

    [Header("通常技")]
    [Tooltip("弱パンチ")] public AttackData LP; 
    [Tooltip("中パンチ")] public AttackData MP; 
    [Tooltip("強パンチ")] public AttackData HP;
    [Tooltip("弱パンチ")] public AttackData LK;
    [Tooltip("中パンチ")] public AttackData MK;
    [Tooltip("強パンチ")] public AttackData HK;

    [Header("しゃがみ技")]
    [Tooltip("弱パンチ")] public AttackData CLP;
    [Tooltip("中パンチ")] public AttackData CMP;
    [Tooltip("強パンチ")] public AttackData CHP;
    [Tooltip("弱パンチ")] public AttackData CLK;
    [Tooltip("中パンチ")] public AttackData CMK;
    [Tooltip("強パンチ")] public AttackData CHK;

    [Header("空中技")]
    [Tooltip("弱パンチ")] public AttackData JLP;
    [Tooltip("中パンチ")] public AttackData JMP;
    [Tooltip("強パンチ")] public AttackData JHP;
    [Tooltip("弱パンチ")] public AttackData JLK;
    [Tooltip("中パンチ")] public AttackData JMK;
    [Tooltip("強パンチ")] public AttackData JHK;

    [Header("特殊技")]
    public AttackData[] specialMove;
}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackData", menuName = "Scriptable Objects/AttackData")]
public class AttackData : ScriptableObject
{
    public enum AttackAttribute
    {
        HIGH,
        OVERHEAD,
        LOW,
        THROW
    }

    // [Tooltip("技名")] public string attackName;
    [Tooltip("属性")] public AttackAttribute attribute;

    [Header("フレーム")]
    [Tooltip("全体フレーム")]   public int totalFrame;
    [Tooltip("発生フレーム")]   public int attackStart;
    [Tooltip("攻撃フレーム")]   public int attackDuration;
    [Tooltip("硬直")]           public int attackRecovery;
    [Tooltip("着地硬直")]       public int landingRecovery;
    [Tooltip("ヒットストップ")] public int hitStop;

    [Header("数値")]
    [Tooltip("ダメージ")]             public int damege;
    [Tooltip("シールドへのダメージ")] public float shieldDamage;
    [Tooltip("敵への硬直(ヒット時)")] public int eHitRecovery;
    [Tooltip("敵への硬直(ガード時)")] public int eGuardRecovery;
    [Tooltip("ヒットバック")]         public Vector2 hitBack;
    [Tooltip("ガードバック")]         public Vector2 guardBack;

    [Header("アニメーション")]
    [Tooltip("全体の画像")] 
    public List<Sprite> spriteAll;
    [Tooltip("発生までの画像")]
    public List<Sprite> startUpSprite;
    [Tooltip("判定中の画像")]
    public List<Sprite> activeAttackSprite;
    [Tooltip("硬直中の画像")]
    public List<Sprite> recoverySprite;

    [Header("当たり判定")]
    [Tooltip("初期の判定位置")] public Vector2 startOffset;
    [Tooltip("初期の食らい判定")] public Vector2 startHurtSize;
    [Tooltip("フレームごとの当たり判定")]
    public List<FrameHitBoxes> frameBoxes = new List<FrameHitBoxes>();

    private void OnValidate()
    {
        totalFrame = attackStart + attackDuration + attackRecovery;
        CheckStartSprite();
        CheckDurationSprite();
        CheckRecoverySprite();
        CheckTotalFlame();
        RefreshSpriteAll();
    }

    // 画像リストのリフレッシュ.
    void RefreshSpriteAll()
    {
        spriteAll.Clear();

        // startUp → active → recovery の順に追加.
        spriteAll.AddRange(startUpSprite);
        spriteAll.AddRange(activeAttackSprite);
        spriteAll.AddRange(recoverySprite);
    }

    // 全体フレームと当たり判定のリストの長さを合わせる関数.
    void CheckTotalFlame()
    {
        if (totalFrame <= 0) return;

        // トータルより多かったら消す.
        while (frameBoxes.Count > totalFrame)
        {
            frameBoxes.RemoveAt(frameBoxes.Count - 1);
        }

        // トータルより少なかったら追加.
        while (frameBoxes.Count < totalFrame)
        {
            frameBoxes.Add(new FrameHitBoxes());
        }

        for (int i = 0; i < frameBoxes.Count; i++)
        {
            if (frameBoxes[i] == null || frameBoxes[i].boxes.Count == 0)
            {
                frameBoxes[i].boxes.Add(new BoxData()
                {
                    boxType = BoxData.BoxType.Hurt,
                    offset = startOffset,
                    size = startHurtSize
                });
            }
        }
    }

    // 発生フレームと画像の数を合わせる関数.
    void CheckStartSprite()
    {
        if(startUpSprite.Count != attackStart)
        {
            while (startUpSprite.Count > attackStart)
            {
                startUpSprite.Remove(startUpSprite[startUpSprite.Count - 1]);
            }

            while(startUpSprite.Count < attackStart)
            {
                startUpSprite.Add(startUpSprite[startUpSprite.Count - 1]);
            }
        }
    }

    // 持続フレームと画像の数を合わせる関数.
    void CheckDurationSprite()
    {
        if (activeAttackSprite.Count != attackDuration)
        {
            while (activeAttackSprite.Count > attackDuration)
            {
                activeAttackSprite.Remove(activeAttackSprite[activeAttackSprite.Count - 1]);
            }

            while (activeAttackSprite.Count < attackDuration)
            {
                activeAttackSprite.Add(activeAttackSprite[activeAttackSprite.Count - 1]);
            }
        }
    }

    // 硬直フレームと画像の数を合わせる関数.
    void CheckRecoverySprite()
    {
        if (recoverySprite.Count != attackRecovery)
        {
            while (recoverySprite.Count > attackRecovery)
            {
                recoverySprite.Remove(recoverySprite[recoverySprite.Count - 1]);
            }

            while (recoverySprite.Count < attackRecovery)
            {
                recoverySprite.Add(recoverySprite[recoverySprite.Count - 1]);
            }
        }
    }
}

[System.Serializable]
public class FrameHitBoxes
{
    [Tooltip("このフレームで有効な当たり判定")]
    public List<BoxData> boxes = new List<BoxData>();
}

[System.Serializable]
public class BoxData
{
    public enum BoxType 
    { 
        Hit,    // 攻撃判定. 
        Hurt    // 食らい判定.
    }

    public BoxType boxType;
    [Tooltip("判定の位置 (攻撃側中心からの相対座標)")]
    public Vector2 offset;
    [Tooltip("判定のサイズ")]
    public Vector2 size;
}
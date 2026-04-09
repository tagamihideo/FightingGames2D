using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CharaBase : MonoBehaviour
{
#region 変数
    #region コンポーネント
    Rigidbody2D rb;
    SpriteRenderer sp;
    Animator animator;
    AnimatorStateInfo stateInfo;
    #endregion

    #region 向き
    protected Vector3 RIGHT = new Vector3(1, 1, 1); // 右向き用の座標.
    protected Vector3 LEFT = new Vector3(-1, 1, 1); // 左向き用の座標.
    private Vector3 currentDirection;               // 現在の向き.
    #endregion

    #region hp
    [Header("HP")]
    public int maxHp = 100;                   // 最大hp.
    public int nowHp = 100;                   // 現在のhp.
    #endregion

    #region 移動
    [Header("移動")]
    [SerializeField] float frontSpeed;      // 移動速度.
    [SerializeField] float backSpeed;       // 移動速度.
    [SerializeField] float jumpForce;       // ジャンプ力.

    [SerializeField] LayerMask groundLayer;     // 地面を判定するためのレイヤー.
    [SerializeField] LayerMask wallLayer;       // 壁を判定するためのレイヤー.
    [SerializeField] LayerMask enemyLayer;      // 敵を判定するためのレイヤー.
    [SerializeField] float checkDistance;       // レイヤーを調べる距離.

    private bool wasGroundFlg = false;      // 地面に着いた瞬間のフラグ.
    protected bool groundFlg = false;       // 地面にいるかどうかのフラグ.
    protected bool wallFlg = false;         // 壁に触れたかどうかのフラグ.
    protected bool jumpFlg = false;         // ジャンプできるかどうかのフラグ.
    protected bool crouchFlg = false;       // しゃがんでいるかどうか.
    protected bool canMoveFlg = true;       // うごけるかどうか.
    protected bool enemyFlg = false;        // 敵に触れたかどうかのフラグ.
    #endregion

    #region 攻撃
    public AttackData currentAttack;        // 現在出している技をいれる用の変数.
    protected Coroutine attackCol;          // 攻撃処理を行うコルーチン.
    protected bool attackFlg = false;       // 攻撃中かどうかのフラグ.
    protected bool jumpAttackFlg = false;   // ジャンプ攻撃をしたかどうかのフラグ.
    protected bool hitFlg = false;          // ダメージをもらったかどうか.
    private int attackCnt = 0;              // 攻撃用のカウンタ.
    #endregion

    #region シールド
    [Header("シールド")]
    public GameObject shield;                   // シールドオブジェクト.
    [SerializeField] float chipDamage;          // シールドを展開しているときの削り量.
    [SerializeField] float reviveShieldScale;   // シールドが削り切れた際の復活サイズ.
    protected float shieldStartScale;           // シールドの初期サイズ.
    protected float shieldScale = 3.5f;         // シールドの現在サイズ.
    protected bool shieldFlg = false;           // シールドを展開しているかどうかのフラグ.
    #endregion

    #region 入力
    [Header("入力")]
    protected Vector2 moveInput;                        // 入力した方向を取得する変数.
    private List<string> inputLog = new List<string>(); // 入力を入れるためのリスト.
    protected string inputAction;                       // 攻撃やシールドを行ったときに使う変数.
    protected string inputOld;                          // 一つ前の入力とチェックするための変数.
    private float inputCnt;                             // 入力のカウント用変数.
    #endregion

    #region 判定
    [Header("判定")]
    [SerializeField] protected AttackDataList attackDataList;   // 攻撃情報の入ったスクリプタブルオブジェクト
    [SerializeField] protected List<BoxCollider2D> HurtBox;     // 食らい判定
    [SerializeField] protected List<BoxCollider2D> HitBox;      // 攻撃判定
    [SerializeField] private Vector2 crouchOffset;              // しゃがみ状態の食らい判定のオフセット
    [SerializeField] private Vector2 crouchSize;                // しゃがみ状態の食らい判定のサイズ
    private Vector2 standHurtOffset;                            // 立ち状態の食らい判定のオフセット
    private Vector2 standHurtSize;                              // 立ち状態の食らい判定のサイズ
    #endregion

    [SerializeField] string playerTag = "Player";   // タグを調べるための変数.
    [SerializeField] CharaBase enemy;               // 敵の情報.
    Coroutine recoveryCol;


    #endregion

    #region Unityイベント関数
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sp = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        animator.SetFloat("Hp", maxHp);
        StartCoroutine(Shield());
        enemy = (playerTag == "Player")
            ? GameObject.Find("Enemy").GetComponent<CharaBase>()
            : GameObject.Find("Player").GetComponent<CharaBase>();

        standHurtOffset = HurtBox[0].offset;
        standHurtSize = HurtBox[0].size;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Vector2 boxSize = new Vector2(0.5f, 0.2f);
        Vector2 origin = transform.position;

        // BoxCast の実際の中心座標（キャスト後）.
        Vector2 castCenter = origin + Vector2.down * checkDistance;

        // 色分け（地面判定アリ → 緑 / ナシ → 赤）.
        Gizmos.color = groundFlg ? Color.green : Color.red;

        // BoxCast の範囲をワイヤーで描画.
        Gizmos.DrawWireCube(castCenter, boxSize);

        Vector2 boxSize2 = HurtBox[0].size;

        // BoxCast の実際の中心座標（キャスト後）.
        Vector2 castCenter2 = origin + Vector2.zero * checkDistance;

        // 色分け（敵の上判定アリ → 青 / ナシ → 黄色）.
        Gizmos.color = (wallFlg || enemyFlg) ? Color.blue : Color.yellow;

        // BoxCast の範囲をワイヤーで描画.
        Gizmos.DrawWireCube(castCenter2, boxSize2);

    }
    #endregion

    #region public関数

    // GameManagerに行動を送るためのTick.
    public void Tick()
    {
        CheckGround();

        if (canMoveFlg)
        {
            Move();
            Jump();
            DirectionChange();
        }
        AddLog();
        Animation();
    }

    public bool HitCheck(AttackData attack)
    {
        if(attack == null || shieldFlg) return false;

        if (!attackFlg && !jumpFlg)
        {
            if (IsGuard() && attack.attribute == AttackData.AttackAttribute.HIGH) return false;
            if (IsGuard() && crouchFlg && attack.attribute == AttackData.AttackAttribute.LOW) return false;
            if (IsGuard() && !crouchFlg && attack.attribute == AttackData.AttackAttribute.OVERHEAD) return false;
        }

        TakeDamage(attack);
        return true;
    }

    // 攻撃を受けたときの処理.
    public void TakeDamage(AttackData attack)
    {
        // 攻撃状態なら.
        if (attackFlg)
        {
            AttackStop();  // 攻撃を止める.
        }

        if (!hitFlg)  
        {
            hitFlg = true;
            animator.ResetTrigger("Hit");
            animator.SetTrigger("Hit");
            recoveryCol = StartCoroutine(Recovery(attack.attackRecovery));
        }

        // ダメージを食らってhpが0にならないなら.
        if (nowHp - attack.damege > 0)
        {
            nowHp -= attack.damege;
            HitBack(attack.hitBack);
        }
        else // ゲーム終了.
        {
            nowHp = 0;
            canMoveFlg = false;
            enemy.canMoveFlg = false;
            GameManager.instance.GameEnd();
        }

        hitFlg = false;
    }

    // シールドへのダメージ処理.
    public void ShieldDamage(AttackData attack)
    {
        if (shieldScale - attack.shieldDamage > 0)
        {
            shieldScale -= attack.shieldDamage;
            HitBack(attack.guardBack);
        }
        else
        {
            shieldScale = 0;
        }

    }

    // 攻撃を止める処理.
    public void AttackStop()
    {
        if (attackCol != null)
        {
            StopCoroutine(attackCol);
            DisableAllBoxes();
            attackCnt = 0;
            HurtBox[0].enabled = true;
            animator.enabled = true;
            attackCol = null;
            attackFlg = false;
        }
    }

    // 食らい判定を立ち状態にセットする関数.
    public void SetStandBox()
    {
        HurtBox[0].offset = standHurtOffset; 
        HurtBox[0].size = standHurtSize;
    }

    // 食らい判定をしゃがみ状態にセットする関数.
    public void SetCrouchBox()
    {
        HurtBox[0].offset = crouchOffset;
        HurtBox[0].size = crouchSize;
    }

    #endregion

    #region 移動処理
    // 移動処理.
    protected void Move()
    {
        Vector3 move = Vector3.zero;

        // Debug.Log(moveInput.y);

        crouchFlg = IsCrouch();

        // 向きが左なら.
        if (currentDirection == LEFT)
        {
            if (moveInput.x < 0f)
            {
                move = new Vector3(moveInput.x, 0f, 0f) * Time.deltaTime * frontSpeed;
            }
            else
            {
                move = new Vector3(moveInput.x, 0f, 0f) * Time.deltaTime * backSpeed;
            }
        }
        else
        {
            if (moveInput.x > 0f)
            {
                move = new Vector3(moveInput.x, 0f, 0f) * Time.deltaTime * frontSpeed;
            }
            else
            {
                move = new Vector3(moveInput.x, 0f, 0f) * Time.deltaTime * backSpeed;
            }
        }

        // 移動できる状態なら移動する.
        if (!attackFlg && !shieldFlg && !hitFlg && !crouchFlg && groundFlg)
        {
            transform.Translate(move);

            //Debug.Log(moveInput.x);
        }
    }

    bool IsCrouch()
    {
        return moveInput.y < -0.5f;
    }

    public bool IsGuard()
    {
        int dir = currentDirection == RIGHT ? -1 : 1;
        return moveInput.x * dir > 0.5f;
    }

    // 向きを変える関数.
    void DirectionChange()
    {
        if (jumpFlg) return;
        if (enemy == null) return;

        float x = transform.position.x - enemy.transform.position.x;

        if (x > 0f)
        {
            transform.localScale = LEFT;
            currentDirection = LEFT;
        }
        else if (x < 0f)
        {
            transform.localScale = RIGHT;
            currentDirection = RIGHT;
        }
    }

    // 地面に立っているかチェックする関数.
    private void CheckGround()
    {
        groundFlg = Physics2D.BoxCast(transform.position,
                                    new Vector2(0.5f, 0.2f),
                                    0f, Vector2.down,
                                    checkDistance, groundLayer);

        if (groundFlg)
        {
            jumpFlg = false;
            HurtBox[0].isTrigger = false;
        }

        if(!wasGroundFlg)
        {
            rb.linearVelocity = Vector2.zero;
            wasGroundFlg = true;
        }
    }

    #endregion

    #region ジャンプ処理
    // ジャンプ処理.
    protected void Jump()
    {
        if (!attackFlg && !shieldFlg && !hitFlg && groundFlg && !jumpFlg)
        {
            Vector2 jumpDir = Vector2.zero;

            if (GetArrow(moveInput) == "↖")
            {
                jumpDir = new Vector2(-0.05f, 0.2f).normalized;
            }
            else if (GetArrow(moveInput) == "↑")
            {
                jumpDir = new Vector2(0f, 1f).normalized;
            }
            else if (GetArrow(moveInput) == "↗")
            {
                jumpDir = new Vector2(0.05f, 0.2f).normalized;
            }

            if (jumpDir != Vector2.zero && attackCol == null)
            {
                rb.linearVelocity = jumpDir * jumpForce;
                jumpFlg = true;
                HurtBox[0].isTrigger = true;
                // wasGroundFlg = false;
            }
        }

        if (jumpFlg)
        {
            CheckWall();
            CheckEnemy();
        }
    }

    // ジャンプ中に壁に触れていないかチェックする関数.
    private void CheckWall()
    {
        wallFlg = Physics2D.BoxCast(transform.position,
                                    HurtBox[0].size,
                                    0f, Vector2.zero,
                                    checkDistance, wallLayer);

        if (wallFlg)
        {
            rb.linearVelocityX = 0f;
        }
    }

    // ジャンプ中にジャンプ中の敵と触れていないかチェックする関数.
    private void CheckEnemy()
    {
        enemyFlg = Physics2D.BoxCast(transform.position,
                            HurtBox[0].size,
                            0f, Vector2.zero,
                            checkDistance, enemyLayer);

        if (enemyFlg && enemy.jumpFlg)
        {
            rb.linearVelocityX = 0f;
        }
    }


    #endregion

    #region 攻撃処理
    // 攻撃用のコルーチン.
    protected IEnumerator AttackCol(AttackData attack)
    {
        // Hit判定のリセット.
        for(int i = 0; i < HitBox.Count; i++)
        {
            HitBox[i].GetComponent<Attack>().ResetHitFlg();
        }

        animator.enabled = false;

        // attacCntが攻撃の全体フレームより低いなら.
        while (attackCnt < attack.totalFrame)
        {
            if (hitFlg) break;

            // 空中攻撃を出していて地面についているなら.
            if (jumpAttackFlg && groundFlg)
            {
                DisableAllBoxes();
                HurtBox[0].enabled = true;
                animator.enabled = true;
                break;
            }

            attackFlg = true;
            sp.sprite = attack.spriteAll[attackCnt];
            SetAttack(attack);
            attackCnt++;

            yield return new WaitForSeconds(GameManager.flame);
        }

        while(jumpAttackFlg && !groundFlg)
        {
            yield return new WaitForSeconds(GameManager.flame);
        }


        // 判定のリセット.
        DisableAllBoxes();
        HurtBox[0].enabled = true;

        // アニメーターのリセット.
        animator.enabled = true;
        animator.SetTrigger("Idle");
        animator.ResetTrigger("Idle");

        // 空中攻撃をしているなら.
        if (jumpAttackFlg && groundFlg)
        {
            // 着地硬直.
            yield return StartCoroutine(Recovery(attack.landingRecovery));
        }

        if (crouchFlg)
        {
            SetCrouchBox();
        }
        else
        {
            SetStandBox();
        }

        // 攻撃関連のリセット.
        attackCnt = 0;
        attackFlg = false;
        jumpAttackFlg = false;
        attackCol = null;
    }

    // 攻撃の判定をセットする関数.
    private void SetAttack(AttackData attack)
    {
        if (attackCnt < 0 || attackCnt >= attack.frameBoxes.Count) return;

        int hurtCnt = 0, hitCnt = 0;

        DisableAllBoxes();

        for (int i = 0; i < attack.frameBoxes[attackCnt].boxes.Count; i++)
        {
            if (attack.frameBoxes[attackCnt].boxes[i].boxType == BoxData.BoxType.Hurt)
            {
                if (hurtCnt >= HurtBox.Count) continue;
                HurtBox[hurtCnt].enabled = true;
                HurtBox[hurtCnt].offset = attack.frameBoxes[attackCnt].boxes[i].offset;
                HurtBox[hurtCnt].size = attack.frameBoxes[attackCnt].boxes[i].size;
                hurtCnt++;
            }
            else
            {
                if (hitCnt >= HitBox.Count) continue;
                HitBox[hitCnt].enabled = true;
                HitBox[hitCnt].offset = attack.frameBoxes[attackCnt].boxes[i].offset;
                HitBox[hitCnt].size = attack.frameBoxes[attackCnt].boxes[i].size;
                hitCnt++;
            }
        }

    }

    // ヒットバック関数.
    private void HitBack(Vector2 vector2)
    {
        int direction = (enemy.transform.localScale == RIGHT) ? 1 : -1;
        rb.AddForce(vector2 * direction, ForceMode2D.Impulse);
    }

    // 当たり判定を全て無効化する関数.
    private void DisableAllBoxes()
    {
        foreach(var box in HurtBox) 
        {
            box.enabled = false;
        }

        foreach(var box in HitBox)
        {
            box.enabled = false;
        }
    }

    // 硬直用のコルーチン.
    IEnumerator Recovery(int recovery)
    {
        yield return new WaitForSeconds(GameManager.flame * recovery);
        animator.SetTrigger("Idle");
        animator.ResetTrigger("Idle");
        recoveryCol = null;
    }
    #endregion

    #region シールド処理
    // シールド処理を行うコルーチン.
    private IEnumerator Shield()
    {
        shieldScale = shield.transform.localScale.x;
        shieldStartScale = shieldScale;

        // ゲームが終了していないなら.
        while (canMoveFlg)
        {
            if (!jumpFlg)
            { 
                shield.SetActive(shieldFlg);    // シールドをアクティブにする.
                shield.transform.localScale = new Vector3(shieldScale, shieldScale, 1); // シールドのスケールを設定.
                yield return new WaitForSeconds(GameManager.flame * 5);
                if (shieldFlg)
                {
                    // シールドが1よりも多いなら.
                    if (shieldScale > 1f)
                    {
                        shieldScale -= chipDamage;  // シールドを削る.
                    }
                    else
                    {
                        shieldScale = reviveShieldScale;    // シールドを復活サイズに変更する.
                        shieldFlg = false;
                        // スタン処理.
                        // Recovery(30);
                    }
                }
                else
                {
                    // スタートのスケールより小さいなら.
                    if (shieldScale < shieldStartScale)
                    {
                        shieldScale += chipDamage;
                    }
                    else
                    {
                        shieldScale = shieldStartScale;
                    }
                }
            }
            else
            {
                yield return null;
            }
        }
    }

    protected virtual void ActivShield(InputAction.CallbackContext context)
    {
        if (!attackFlg && !hitFlg && !jumpFlg)
        {
            shieldFlg = true;
            inputAction = "Shield";
        }
    }

    protected virtual void InActiveShield(InputAction.CallbackContext context)
    {
        if(shieldFlg)
        {
            shieldFlg = false;
        }
    }
    #endregion

    #region 入力処理
    // ログ用に入力を矢印に変換する関数.
    protected string GetArrow(Vector2 dir)
    {
        float val = 0.5f;
        int x = 0, y = 0;

        if (dir.x > val) x = 1;
        else if (dir.x < -val) x = -1;

        if (dir.y > val) y = 1;
        else if (dir.y < -val) y = -1;

        if (x == 0 && y == 1) return "↑";
        if (x == 0 && y == -1) return "↓";
        if (x == -1 && y == 0) return "←";
        if (x == 1 && y == 0) return "→";

        if (x == -1 && y == 1) return "↖";
        if (x == 1 && y == 1) return "↗";
        if (x == -1 && y == -1) return "↙";
        if (x == 1 && y == -1) return "↘";

        return "N"; // 方向なし.
    }

    // 入力ログを追加する関数.
    private void AddLog()
    {
        string log = GetArrow(moveInput);

        if (inputAction != null)
        {
            log += $" {inputAction}";
        }

        if (log == inputOld)
        {
            if (inputCnt < 99) inputCnt++;

            if (inputLog.Count > 0)
            {
                inputLog[inputLog.Count - 1] = $"{inputCnt}  {log}";
            }
        }
        else
        {
            inputCnt = 1;
            inputLog.Add($"{inputCnt} {log}");
        }
       
        // 最新5件だけ表示.
        int startIndex = Mathf.Max(0, inputLog.Count - 5);
        string displayText = string.Join("\n", inputLog.GetRange(startIndex, inputLog.Count - startIndex));

        inputOld = log;

        int index = -1;

        if (playerTag == "Player")
        {
            index = 0;
        }
        else if (playerTag == "Enemy")
        {
            index = 1;
        }

        if (index > -1)
        {
            GameManager.instance.UpdateLog(displayText ,index);
        }
    }
    #endregion

    #region Animation
    void Animation()
    {
        animator.SetFloat("Hp", nowHp);
        if (!jumpAttackFlg)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
            animator.SetBool("Crouch", crouchFlg);
        }
        animator.SetBool("Ground", groundFlg);
        animator.SetBool("Protect", shieldFlg);
        animator.SetBool("Jump", jumpFlg);
        animator.SetBool("Down", jumpFlg && rb.linearVelocity.y < 0f);
    }
    #endregion
}

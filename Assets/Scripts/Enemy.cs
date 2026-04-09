using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class Enemy : CharaBase
{
    [SerializeField] GameObject panel;
    [SerializeField] string[] actions;
    private string nowAciton = "Idle";
    [SerializeField] bool aiFlg = false;
    Player player;


    protected override void Awake()
    {
        base.Awake();
        panel.SetActive(false);
        player = GameObject.Find("Player").GetComponent<Player>();

        if(GameModeManager.instance != null)
        {
            if(GameModeManager.instance.battleMode == GameModeManager.BattleMode.VS)
            {
                nowAciton = actions[0];
            }
            else
            {
                nowAciton = actions[1];
            }
        }
        else
        {
            nowAciton = actions[0];
        }
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(AIMove());
        StartCoroutine(AI());
    }

    void Update()
    {
        //float test = 1f / Time.deltaTime;
        //Debug.Log("Flame : " + test.ToString("f4"));
    }

    Vector2 GetRandomDirection()
    {
        Vector2[] dirs = new Vector2[]
        {
            new Vector2(transform.localScale.x,  0),  // 前
            new Vector2(transform.localScale.x,  0),  // 前
            new Vector2(transform.localScale.x,  0),  // 前
            new Vector2(transform.localScale.x,  0),  // 前
            new Vector2( 0,  0),                        // 移動なし
            new Vector2( 0,  0),                        // 移動なし
            new Vector2( 0,  1),                        // 垂直ジャンプ                
            new Vector2(-transform.localScale.x, 0),    // 後
        };

        int r = Random.Range(0, dirs.Length);

        return dirs[r];
    }

    string GetAttackName()
    {
        string[] str = new string[]
        {
            "LP",
            "MP",
            "HP",
            "LK",
            "MK",
            "HK",
        };

        int r = Random.Range(0, str.Length);

        return str[r];
    }


    IEnumerator AI()
    {
        while(true)
        {
            if(aiFlg && groundFlg && !jumpFlg && !attackFlg)
            {
                inputAction = null;
                float dirX = Mathf.Sign(player.transform.position.x - transform.position.x);
                if (dirX < 0)
                {
                    transform.localScale = LEFT;
                }
                else if (dirX > 0)
                {
                    transform.localScale = RIGHT;
                }
                int action = Random.Range(0, 10);
                //Debug.Log(action);

                if(Mathf.Abs(transform.position.x - player.transform.position.x) < 1.5f)
                {
                    if (action < 7)
                    {
                        moveInput = GetRandomDirection();
                        shieldFlg = false;
                    }
                    //else if (action < 7)
                    //{
                    //    moveInput = Vector2.zero;
                    //    inputAction = "Shield";
                    //    //shieldFlg = true;
                    //}
                    else
                    {
                        moveInput = Vector2.zero;
                        shieldFlg = false;
                        DoAttack(GetAttackName());
                    }
                }
                else
                {
                    if (!attackFlg)
                    {
                        moveInput = GetRandomDirection();
                        shieldFlg = false;
                    }
                }

                yield return new WaitForSeconds(GameManager.flame * 10);
            }
            else
            {
                moveInput = Vector2.zero;
                yield return null;
            }

        }
    }

    IEnumerator AIMove()
    {
        while (true)
        {
            // Debug.Log(nowAciton);
            switch (nowAciton)
            {
                case "AI":
                    aiFlg = true;
                    break;
                case "Idle":
                    moveInput = Vector2.zero;
                    shieldFlg = false;
                    aiFlg = false;
                    inputAction = null;
                    break;
                case "Jump":
                    if (groundFlg && !jumpFlg)
                    {
                        moveInput = Vector2.up;
                    }
                    else
                    {
                        moveInput = Vector2.zero;
                    }
                    shieldFlg = false;
                    aiFlg = false;
                    inputAction = null;
                    break;
                case "Attack":
                    if (!attackFlg)
                    {
                        moveInput = Vector2.zero;
                        shieldFlg = false;
                        if (!attackFlg && !shieldFlg)
                        {
                            DoAttack(GetAttackName());
                        }
                        aiFlg = false;
                    }
                        break;
                case "Shield":
                    if (shieldScale == shieldStartScale)
                    {
                        moveInput = Vector2.zero;
                        shieldFlg = true;
                        inputAction = "Shield";
                        aiFlg = false;
                    }
                    break;
            }

            yield return new WaitForSeconds(1f);
        }

    }

    void DoAttack(string attackName)
    {
        AttackData data = null;

        var field = typeof(AttackDataList).GetField(attackName);
        if (field != null)
        {
            data = field.GetValue(attackDataList) as AttackData;
        }

        if (data != null && !attackFlg && !hitFlg && !shieldFlg && canMoveFlg)
        {
            currentAttack = data;
            inputAction = "Attack";
            attackCol = StartCoroutine(AttackCol(currentAttack));
        }
    }

    public void PanelOn()
    {
        panel.SetActive(true);
    }

    public void AIAction()
    {
        nowAciton = actions[0];
        panel.SetActive(false);
    }

    public void IdleAction()
    {
        nowAciton = actions[1];
        panel.SetActive(false);

    }

    public void JumpAction()
    {
        nowAciton = actions[2];
        panel.SetActive(false);
    }

    public void AttackAction()
    {
        nowAciton = actions[3];
        panel.SetActive(false);
    }
    public void ShieldAction()
    {
        nowAciton = actions[4];
        panel.SetActive(false);
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (col.collider.CompareTag("Player"))
        {
            if (col.transform.position.y > transform.position.y + 1f)
            {
                col.transform.position -= new Vector3(0.01f * col.transform.localScale.x, 0, 0);
            }
        }
    }
}

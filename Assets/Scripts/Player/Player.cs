using UnityEngine;
using UnityEngine.InputSystem;

public class Player : CharaBase
{
    PlayerInput input;
    [SerializeField] int playerIndex = 0;

    protected override void Awake()
    {
        base.Awake();
        input = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        // UnityEvent経由で受け取れるように.
        input.notificationBehavior = PlayerNotifications.InvokeUnityEvents;

        if (playerIndex == 0)
        {
            if (Gamepad.all.Count > playerIndex)
            {
                input.SwitchCurrentControlScheme("Gamepad1", Gamepad.all[playerIndex]);
            }
            else
            {
                input.SwitchCurrentControlScheme("Keyboard1", Keyboard.current);
            }
        }
        else if (playerIndex == 1)
        {
            if (Gamepad.all.Count > playerIndex)
            {
                input.SwitchCurrentControlScheme("Gamepad2", Gamepad.all[playerIndex]);
            }
            //else if(Gamepad.all.Count != 0)
            else
            {
                input.SwitchCurrentControlScheme("Keyboard2", Keyboard.current);
            }
        }

        // イベント登録.
        input.actions["Move"].performed += OnMovePerformrd;
        input.actions["Move"].canceled += OnMoveCanceled;
        input.actions["Shield"].performed += ActivShield;
        input.actions["Shield"].canceled += InActiveShield;

        input.actions["LP"].performed += Attack;
        input.actions["MP"].performed += Attack;
        input.actions["HP"].performed += Attack;
        input.actions["LK"].performed += Attack;
        input.actions["MK"].performed += Attack;
        input.actions["HK"].performed += Attack;

        input.actions["LP"].canceled += AttackCanceled;
        input.actions["MP"].canceled += AttackCanceled;
        input.actions["HP"].canceled += AttackCanceled;
        input.actions["LK"].canceled += AttackCanceled;
        input.actions["MK"].canceled += AttackCanceled;
        input.actions["HK"].canceled += AttackCanceled;
    }

    private void OnDestroy()
    {
        input.actions["Move"].performed -= OnMovePerformrd;
        input.actions["Move"].canceled -= OnMoveCanceled;
        input.actions["Shield"].performed -= ActivShield;
        input.actions["Shield"].canceled -= InActiveShield;

        input.actions["LP"].performed -= Attack;
        input.actions["MP"].performed -= Attack;
        input.actions["HP"].performed -= Attack;
        input.actions["LK"].performed -= Attack;
        input.actions["MK"].performed -= Attack;
        input.actions["HK"].performed -= Attack;

        input.actions["LP"].canceled -= AttackCanceled;
        input.actions["MP"].canceled -= AttackCanceled;
        input.actions["HP"].canceled -= AttackCanceled;
        input.actions["LK"].canceled -= AttackCanceled;
        input.actions["MK"].canceled -= AttackCanceled;
        input.actions["HK"].canceled -= AttackCanceled;
    }
    
    void OnMovePerformrd(InputAction.CallbackContext context)
    {
        // 入力時の動作.
        moveInput = context.ReadValue<Vector2>();
    }

    void OnMoveCanceled(InputAction.CallbackContext context)
    {
        // 離した時の動作.
        moveInput = Vector2.zero;
    }

    void Attack(InputAction.CallbackContext context)
    {
        if (playerIndex == 1) return;

        string acitonName = context.action.name;
        AttackData data = null;

        if(jumpFlg)
        {
            acitonName = "J" + acitonName;
        }

        if(crouchFlg)
        {
            acitonName = "C" + acitonName;
        }

        // Debug.Log(acitonName);

        // AttackDataListからactionNameに一致するフィールドを探す.
        var field = typeof(AttackDataList).GetField(acitonName);
        if(field != null)
        {
            data = field.GetValue(attackDataList) as AttackData;
        }
        else
        {
            Debug.LogWarning($"AttackDataList に 'actionName' というフィールドが見つかりません。");
            return;
        }

        if(data != null )
        {
            // 攻撃ログ追加.
            inputAction = "Attack";

            if (!attackFlg && !hitFlg && !shieldFlg && canMoveFlg)
            {
                //animator.enabled = false;

                if (jumpFlg)
                {
                    jumpAttackFlg = true;
                    // Debug.Log("空中攻撃");
                }

                currentAttack = data;
                attackCol = StartCoroutine(AttackCol(currentAttack));

            }
        }
    }

    void AttackCanceled(InputAction.CallbackContext context)
    {
        inputAction = null;
    }
}

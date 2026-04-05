using UnityEngine;

public class Attack : MonoBehaviour
{
    [SerializeField] string attackerTag = "Player";
    CharaBase player;
    bool hitFlg = false;

    private void Awake()
    {
        player = GetComponentInParent<CharaBase>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Shield"))
        {
            Debug.Log("Shield");
            if(!hitFlg)
            {
                col.GetComponentInParent<CharaBase>().ShieldDamage(player.currentAttack);
                hitFlg = true;
            }
            return;
        }

        if (attackerTag == "Player" && col.CompareTag("Enemy") ||
        attackerTag == "Enemy" && col.CompareTag("Player"))
        {
            if(!hitFlg)
            {
                // col.GetComponentInParent<CharaBase>().TakeDamage(player.currentAttack);
                //col.GetComponent<CharaBase>().TakeDamage(player.currentAttack);
                hitFlg = col.GetComponentInParent<CharaBase>().HitCheck(player.currentAttack);
            }

        }
    }

    public void ResetHitFlg()
    {
        hitFlg = false;
    }
}

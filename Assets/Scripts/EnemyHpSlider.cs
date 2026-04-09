using UnityEngine;
using UnityEngine.UI;

public class EnemyHpSlider : MonoBehaviour
{
    Slider slider;
    Player p;
    Enemy e;

    bool enemyFlg;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameModeManager.instance != null)
        {
            bool useEnemy = GameModeManager.instance.UseSecondPlayer();

            if (useEnemy)
            {
                e = GameObject.Find("Enemy").GetComponent<Enemy>();   
                enemyFlg = true;
            }
            else
            {
                p = GameObject.Find("Enemy").GetComponent<Player>();
                enemyFlg= false;
            }
        }
        else
        {
            e = GameObject.Find("Enemy").GetComponent<Enemy>();
            enemyFlg = true ;
        }

        slider = GetComponent<Slider>();

        if(enemyFlg)
        {
            slider.maxValue = e.maxHp;
            slider.value = e.nowHp;
        }
        else
        {
            slider.maxValue = p.maxHp;
            slider.value = p.nowHp;
        }
    }

    private void Update()
    {
        if(enemyFlg) 
        {
            slider.value = e.nowHp;
        }
        else
        {
            slider.value = p.nowHp;
        }
    }
}

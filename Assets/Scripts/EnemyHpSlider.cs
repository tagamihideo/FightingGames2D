using UnityEngine;
using UnityEngine.UI;

public class EnemyHpSlider : MonoBehaviour
{
    Slider slider;
    Player p;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        slider = GetComponent<Slider>();
        p = GameObject.Find("Enemy").GetComponent<Player>();

        slider.maxValue = p.maxHp;
        slider.value = p.nowHp;
    }

    private void Update()
    {
        slider.value = p.nowHp;
    }
}

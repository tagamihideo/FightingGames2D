using UnityEngine;

public class CharacterRoleSetter : MonoBehaviour
{
    [SerializeField] Player playerScript;
    [SerializeField] Enemy enemyScript;

    void Awake()
    {
        if(GameModeManager.instance != null)
        {
            bool useEnemy = GameModeManager.instance.UseSecondPlayer();

            if (useEnemy)
            {
                playerScript.enabled = false;
                enemyScript.enabled = true;
            }
            else
            {
                playerScript.enabled = true;
                enemyScript.enabled = false;
            }
        }
    }
}
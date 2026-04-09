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
                //Destroy(GetComponent<Player>());
                playerScript.enabled = false;
                enemyScript.enabled = true;
            }
            else
            {
                // Destroy(GetComponent<Enemy>());
                playerScript.enabled = true;
                enemyScript.enabled = false;
            }
        }
        else
        {
            //Destroy(GetComponent<Player>());
            playerScript.enabled = false;
            enemyScript.enabled = true;
        }
    }
    
}
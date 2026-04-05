using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickVS()
    {
        GameModeManager.instance.battleMode = GameModeManager.BattleMode.VS;
        SceneManager.LoadScene("GameScene");
    }

    public void OnClickTraining()
    {
        GameModeManager.instance.battleMode = GameModeManager.BattleMode.Training;
        SceneManager.LoadScene("GameScene");
    }
}

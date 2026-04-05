using UnityEngine;
using UnityEngine.InputSystem;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager instance;

    public enum BattleMode
    {
        VS,
        Training
    }

    public BattleMode battleMode;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsControllerConnected()
    {
        return Gamepad.all.Count > 0;
    }

    public bool UseSecondPlayer()
    {
        if(battleMode == BattleMode.VS)
        {
            return !IsControllerConnected();
        }

        if(battleMode == BattleMode.Training)
        { 
            return true; 
        }

        return true;
    }
}

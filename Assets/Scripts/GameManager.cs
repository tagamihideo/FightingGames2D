using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    public const float flame = 0.017f;   // ÉtÉåÅ[ÉÄ

    [SerializeField] GameObject[] player;
    [SerializeField] Text[] inputLogText;
    [SerializeField] Text winText;
    [SerializeField] GameObject canvasButton;

    private CharaBase[] charaBases;

    private bool VSFlg;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        if(GameModeManager.instance != null)
        {
            if (GameModeManager.instance.battleMode == GameModeManager.BattleMode.Training)
            {
                canvasButton.SetActive(true);
                VSFlg = false;
            }
            else
            {
                canvasButton.SetActive(false);
                VSFlg = true;
            }
        }
        else
        {
            canvasButton.SetActive(false);
        }

        charaBases = new CharaBase[player.Length];

        for (int i = 0; i < player.Length; i++)
        {
            if (player[i] == null) continue;

            charaBases[i] = player[i].GetComponent<CharaBase>();
        }

        Application.targetFrameRate = 120;

        StartCoroutine(GameLoop());
        winText.text = "";
    }

    IEnumerator GameLoop()
    {
        while(true)
        {
            for(int i = 0; i < player.Length; i++)
            {
                if (charaBases[i] != null)
                {
                    charaBases[i].Tick();
                }
            }

            yield return new WaitForSeconds(flame);
        }
    }

    public void GameEnd()
    {
        if(VSFlg)
        {
            if (charaBases[0].nowHp <= 0 && charaBases[1].nowHp <= 0)
            {
                winText.text = "DROW";
            }
            else if (charaBases[0].nowHp <= 0)
            {
                winText.text = "2P WIN";
            }
            else if (charaBases[1].nowHp <= 0)
            {
                winText.text = "1P WIN";
            }

            Invoke("ChangeScene", 5);
        }
        else
        {
            RestartScene();
        }
    }

    public void ChangeScene()
    {
        SceneManager.LoadScene("TitleScene");
    }

    public void RestartScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void UpdateLog(string text, int index)
    {
        inputLogText[index].text = text;
    }
}

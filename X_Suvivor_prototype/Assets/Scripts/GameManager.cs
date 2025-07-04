using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("# GameControl")]
    public bool isLive;
    public float gameTime;
    public float maxGameTime = 2 * 10f;

    [Header("# Player Info")]
    public int playerId;
    public int level;
    public int killCount;
    public int nowExp;
    public int[] needExp;

    [Header("# GameObject")]
    public Player player;
    public PoolManager pool;
    public LevelUp uiLevelUo;
    public Result uiResult;
    public GameObject enemyCleaner;

    void Awake()
    // 초기화(선언)
    {
        instance = this;
    }

    public void GameVictory()
    {
        StartCoroutine(GameVictoryRoutine());
    }

    IEnumerator GameVictoryRoutine()
    {
        isLive = false;
        enemyCleaner.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        uiResult.gameObject.SetActive(true);
        uiResult.Win();
        Stop();

        AudioManager.instance.PlayBgm(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Win);
    }

    public void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        isLive = false;

        yield return new WaitForSeconds(0.5f);

        uiResult.gameObject.SetActive(true);
        uiResult.Lose();
        Stop();

        AudioManager.instance.PlayBgm(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Lose);
    }

    public void GameStart(int id)
    {
        playerId = id;

        player.gameObject.SetActive(true);
        uiLevelUo.Select(playerId % 2);
        Resume();
        AudioManager.instance.PlayBgm(true);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    }

    public void GameRetry()
    {
        SceneManager.LoadScene(0);
    }

    void Update()
    {
        if (!isLive) {
            return;
        }
        gameTime += Time.deltaTime;

        if (gameTime > maxGameTime) {
            gameTime = maxGameTime;
            GameVictory();
        }
    }

    public void GetExp()
    {
        if (!isLive)
            return;
        nowExp++;

        if (nowExp == needExp[Mathf.Min(level, needExp.Length-1)]) {
            level++;
            nowExp = 0;
            uiLevelUo.show();
        }
    }

    public void Stop()
    {
        isLive = false;
        Time.timeScale = 0;     //유니티의 시간 흐름 배율
    }

    public void Resume()
    {
        isLive = true;
        Time.timeScale = 1;
    }
}

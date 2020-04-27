using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public float levelStartDelay = 2f;
    public float turnDelay = 0.1f; 
    public static GameManager instance = null;
    public BoardManager boardScript;
    public int playerFoodPoints = 100;
    [HideInInspector] public bool playersTurn = true;

    private GameObject restartButton;
    private GameObject exitButton;
    private Text levelText;
    private GameObject highScore;
    private GameObject levelImage;
    public int level = 1; //change back later
    private int shownLevel;
    private List<Enemy> enemies = new List<Enemy>();
    private List<Fire> fires = new List<Fire>();
    private bool enemiesMoving;
    private bool firesMoving;
    private bool doingSetup;

    private GameObject firstTime;
    private GameObject enemy1;
    private GameObject walls;
    private GameObject enemy2;
    private GameObject enemy3;
    private GameObject fire;

    // Start is called before the first frame update
    void Awake()
    {
       level = PlayerPrefs.GetInt("Level");

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);


        enemies = new List<Enemy>();
        boardScript = GetComponent<BoardManager>();
        InitGame();
    }

    private void OnLevelWasLoaded(int index)
    {
        PlayerPrefs.SetInt("Level", level);

        firstTime = GameObject.Find("FirstTime");
        enemy1 = GameObject.Find("Enemy1");
        enemy2 = GameObject.Find("Enemy2");
        enemy3 = GameObject.Find("Enemy3");
        fire = GameObject.Find("Fire");
        walls = GameObject.Find("Walls");

        firstTime.SetActive(false);
        enemy1.SetActive(false);
        enemy2.SetActive(false);
        enemy3.SetActive(false);
        fire.SetActive(false);
        walls.SetActive(false);

        InitGame();
	}

    public void InitGame()
	{
        try
        {
            doingSetup = true;

            shownLevel = level;


            if (level > PlayerPrefs.GetInt("HighScore"))
                PlayerPrefs.SetInt("HighScore", level);

            restartButton = GameObject.Find("RestartButton");
            exitButton = GameObject.Find("ExitButton");
            levelImage = GameObject.Find("LevelImage");
            levelText = GameObject.Find("LevelText").GetComponent<Text>();
            highScore = GameObject.Find("HighScore");

            highScore.SetActive(false);
            restartButton.SetActive(false);
            exitButton.SetActive(false);
            levelText.text = "level " + level;
            levelImage.SetActive(true);
            Invoke("HideLevelImage", levelStartDelay);

            enemies.Clear();
            fires.Clear();
            boardScript.SetupScene(level);
        }
        catch (Exception) { }
	}

    private void HideLevelImage()
	{
        levelImage.SetActive(false);

        doingSetup = false;

        enabled = true;
        if (level == 1)
            firstTime.SetActive(true);
        if (level == 3)
            enemy1.SetActive(true);
        if (level == 10)
            walls.SetActive(true);
        if (level == 13)
            enemy2.SetActive(true);
        if (level == 34)
            enemy3.SetActive(true);
        if (level == 55)
            fire.SetActive(true);
	}

    public void GameOver()
	{
        firstTime.SetActive(false);
        enemy1.SetActive(false);
        enemy2.SetActive(false);
        enemy3.SetActive(false);
        fire.SetActive(false);
        walls.SetActive(false);
        PlayerPrefs.SetInt("Level",1);
        level = 1;
        restartButton.SetActive(true);
        exitButton.SetActive(true);
        levelText.text = "after " + shownLevel + " levels, you ran out of energy";
        highScore.GetComponent<Text>().text = "highscore: " + PlayerPrefs.GetInt("HighScore");
        highScore.SetActive(true);
        boardScript.ResetFib();
        levelImage.SetActive(true);
        enabled = false;
	}

    void Update()
    {
        if (playersTurn || enemiesMoving || firesMoving|| doingSetup)
            return;
        StartCoroutine(MoveEnemies());
        StartCoroutine(MoveFires());
    }

    public void AddEnemyToList(Enemy script)
    {
        enemies.Add(script);
	}
    public void RemoveEnemyFromList(Enemy script)
    {
        enemies.Remove(script);
    }

    public void AddFireToList(Fire script)
    {
        fires.Add(script);
    }

    IEnumerator MoveFires()
    {
        playersTurn = false;
        firesMoving = true;
        yield return new WaitForSeconds(turnDelay+0.25f);
        if (fires.Count == 0 && !(enemies.Count==0))
        {
            yield return new WaitForSeconds(turnDelay);
        }
        for (int i = 0; i < fires.Count; i++)
        {
            fires[i].MoveFire();
        }
        firesMoving = false;
        if (!enemiesMoving && !firesMoving)
            playersTurn = true;
    }

    IEnumerator MoveEnemies()
	{
        enemiesMoving = true;
        playersTurn = false;
        yield return new WaitForSeconds(turnDelay+0.1f);
        if (!(fires.Count == 0) && enemies.Count == 0)
		{
            yield return new WaitForSeconds(turnDelay);
		}
        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].MoveEnemy();
            yield return new WaitForSeconds(enemies[i].moveTime);
        }
        enemiesMoving = false;
        if (!enemiesMoving && !firesMoving)
            playersTurn = true;
	}

    public int GetColumns()
    {
        return boardScript.GetColumns();
    }

    public int GetRows()
    {
        return boardScript.GetRows();
    }
    public void RestartGame()
    {
        Player.instance.food = playerFoodPoints;
        enabled = true;
        SoundManager.instance.musicSource.Play();
        SceneManager.LoadScene("Main");
    }
    public void BackToTitle()
    {
        Player.instance.food = playerFoodPoints;
        SceneManager.LoadScene("TitleScreen");
    }
}

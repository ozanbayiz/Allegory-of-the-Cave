using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    private GameObject levelImage;
    public int level = 1; //change back later
    private int shownLevel;
    private List<Enemy> enemies = new List<Enemy>();
    private List<Fire> fires = new List<Fire>();
    private bool enemiesMoving;
    private bool firesMoving;
    private bool doingSetup;

    // Start is called before the first frame update
    void Awake()
    { 
       level = PlayerPrefs.GetInt("Level");

        Debug.Log("Loading level: " + level);

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
        InitGame();
	}

    public void InitGame()
	{
        shownLevel = level;
        //PlayerPrefs.SetInt("Level", level);
        doingSetup = true;

        restartButton = GameObject.Find("RestartButton");
        exitButton = GameObject.Find("ExitButton");
        levelImage = GameObject.Find("LevelImage");
        levelText = GameObject.Find("LevelText").GetComponent<Text>();

        exitButton.SetActive(false);
        restartButton.SetActive(false);
        levelText.text = "Day " + level;
        levelImage.SetActive(true);
        Invoke("HideLevelImage", levelStartDelay);

        enemies.Clear();
        fires.Clear();
        boardScript.SetupScene(level);
	}

    private void HideLevelImage()
	{
        levelImage.SetActive(false);
        doingSetup = false;
        enabled = true;

	}

    public void GameOver()
	{
        PlayerPrefs.SetInt("Level",1);
        level = 1;
        levelText.text = "After " + shownLevel + " days, you starved.";
        boardScript.ResetFib();
        levelImage.SetActive(true);
        restartButton.SetActive(true);
        exitButton.SetActive(true);
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
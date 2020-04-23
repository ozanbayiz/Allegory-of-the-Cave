using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGame : MonoBehaviour
{
    public int columns = 16;
    public int rows = 9;
    public GameObject[] outerWallTiles;
    public GUISkin buttonSkin;


    public GameObject newGameButton;
    public GameObject continueButton;
    void Awake()
    {
        newGameButton = GameObject.FindGameObjectWithTag("NewGameButton");
        continueButton = GameObject.FindGameObjectWithTag("ContinueButton");

        if (PlayerPrefs.GetInt("Level")<=1)
            continueButton.gameObject.SetActive(false);
        else
            continueButton.gameObject.SetActive(true);
        for (int x = -1; x < columns; x++)
        { 
            for (int y = -1; y < rows; y++)
            {
                GameObject toInstantiate = outerWallTiles[(int)Random.Range(0, outerWallTiles.Length-1)];
                GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
            }
        }

    }

    public void NewGame()
    {
        PlayerPrefs.SetInt("Level", 1);
        Application.LoadLevel("Main");
    }

    public void ContinueGame()
    {
        Application.LoadLevel("Main");
    }

    public void CloseGame()
    {
        Application.Quit();
    }
}

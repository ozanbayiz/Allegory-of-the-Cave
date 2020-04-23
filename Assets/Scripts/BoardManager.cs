using System.Collections;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Serializable]
    public class Count
    {
        public int minimum;
        public int maximum;

        public Count(int min, int max)
        {
            minimum = min;
            maximum = max;
        }
    }

    public int columns = 8;
    public int rows = 8;
    public Count wallCount = new Count(12, 18);
    public Count foodCount = new Count(1, 5);
    public GameObject exit;
    public List<GameObject> floortiles;
    public List<GameObject> wallTiles;
    public List<GameObject> moveableWallTiles;
    public List<GameObject> foodTiles;
    public List<GameObject> enemyTiles = new List<GameObject>();
    public List<GameObject> outerWallTiles;
    public List<int> storeFib = new List<int> { 1, 1 };

    private Transform boardHolder;
    private List<Vector3> possibleDirections = new List<Vector3> { new Vector3(0, 1,0f), new Vector3(1, 0, 0f), new Vector3(0, -1, 0f), new Vector3(-1, 0, 0f) };
    private List<Vector3> gridPositions = new List<Vector3>();
    private List<Vector3> exitPositions = new List<Vector3>();

    void InitialiseList()
    {
        gridPositions.Clear();
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                gridPositions.Add(new Vector3(x, y, 0f));
                if (x > 3 && y > 3)
                    exitPositions.Add(new Vector3(x, y, 0f));
            }
        }
        gridPositions.Remove(new Vector3(0,0,0f));
        Vector3 randomDirection = possibleDirections.GetRange(0,2)[Random.Range(0, 2)];
        gridPositions.Remove(randomDirection);
        gridPositions.Remove(2*randomDirection);
    }

    public int GetColumns()
    {
        return columns;
    }

    public int GetRows()
    {
        return rows;
    }

    void BoardSetup()
	{
        boardHolder = new GameObject("Board").transform;

        for (int x = -1; x < columns + 1; x++)
        {
            for (int y = -1; y < rows + 1; y++)
            {
                GameObject toInstantiate = floortiles[Random.Range(0, floortiles.Count)];
                if (x == -1 || x == columns || y == -1 || y == rows)
                    toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Count)];

                GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;

                instance.transform.SetParent(boardHolder);
            }
        }

    }

    Vector3 RandomPosition()
	{
        int randomIndex = Random.Range(0, gridPositions.Count);
        Vector3 randomPosition = gridPositions[randomIndex];
        gridPositions.RemoveAt(randomIndex);
        return randomPosition;
	}

    Vector3 ExitPosition()
    {
        int randomIndex = Random.Range(0, exitPositions.Count);
        Vector3 randomPosition = exitPositions[randomIndex];
        gridPositions.Remove(randomPosition);
        Vector3 randomDirection = possibleDirections[Random.Range(0, possibleDirections.Count)];
        gridPositions.Remove(randomDirection);
        return randomPosition;
    }

    void LayoutObjectAtRandom(List<GameObject> tileArray, int minimum, int maximum)
	{
        int objectCount = Random.Range(minimum, maximum + 1);
        for (int i = 0; i < objectCount; i++)
        {
            Vector3 randomPosition = RandomPosition();
            GameObject tileChoice = tileArray[Random.Range(0, tileArray.Count)];
            Instantiate(tileChoice, randomPosition, Quaternion.identity);
        }
    }

    private int FibEnemyCount(int level)
    {
        int greatestFib = storeFib[storeFib.Count - 1];
        while(greatestFib < level)
        {
            storeFib.Add(storeFib[storeFib.Count - 2] + storeFib[storeFib.Count - 1]);
            greatestFib = storeFib[storeFib.Count - 1];
        }
        if(greatestFib == level)
            return (int)(storeFib.Count-2)/2;
        return (int)(storeFib.Count - 3)/2;
    }

    private int FibEnemyRange(int level)
    {
        int ret = 1;
        if (level >= 13)
            ret++;
        if (level >= 34)
            ret++;
        if (level >= 55)
            ret++;
        return ret;
    }

    public void ResetFib()
    {
        storeFib = new List<int> { 1, 1 };
    }

    public void SetupScene(int level)
    {
        BoardSetup();
        InitialiseList();
        Instantiate(exit, ExitPosition(), Quaternion.identity);

        if(level >= 13)
            LayoutObjectAtRandom(wallTiles, 8, 12);
        else
            LayoutObjectAtRandom(wallTiles.GetRange(0,1), 10,14);

        GameObject player = GameObject.Find("Player");
        Transform exitLocation = GameObject.FindGameObjectWithTag("Exit").GetComponent<Transform>().transform;

        if (!player.GetComponent<Player>().CanProducePath(player.GetComponent<Transform>().transform, exitLocation))
            SetupScene(level);

        if(level >= 13)
            LayoutObjectAtRandom(moveableWallTiles, 4, 6);

        LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum);
        int enemyCount = FibEnemyCount(level);
        int enemyRange = FibEnemyRange(level); 
        LayoutObjectAtRandom(enemyTiles.GetRange(0,enemyRange), enemyCount, enemyCount);
    }
}
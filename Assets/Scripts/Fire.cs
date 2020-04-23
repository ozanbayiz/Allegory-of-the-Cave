using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using static System.Collections.IEnumerable;
using Random = UnityEngine.Random;

public class Fire : MonoBehaviour
{
    public LayerMask defaultLayer;
    public LayerMask blockingLayer;
    public int playerTurnsPerMove = 8;
    public GameObject fire;

    private bool[] turnArray;
    private int turnIndex = 0;
    private Vector2 fireLocation;
    private BoxCollider2D boxCollider;
    private List<Vector2> possibleDirections = new List<Vector2> { new Vector2(0, 1), new Vector2(0, -1), new Vector2(-1, 0), new Vector2(1, 0)};
    private List<Vector2> validDirections = new List<Vector2>();

    protected void Start()
    {
        GameManager.instance.AddFireToList(this);
        boxCollider = this.GetComponent<BoxCollider2D>();
        fireLocation = transform.position;
        turnArray = CreateTurnArray();
    }

    public void MoveFire()
    {
        validDirections.Clear();
        if (turnArray[turnIndex])
        {
            ValidDirections();
            if (validDirections.Count > 0)
            {
                Vector2 direction = RandomDirection();
                NewFire(direction);
            }
        }
        turnIndex++;
        if (turnIndex > playerTurnsPerMove - 1)
            turnIndex -= playerTurnsPerMove;
    }
    bool[] CreateTurnArray()
    {
        bool[] ret = new bool[playerTurnsPerMove];
        ret[playerTurnsPerMove - 1] = true;
        for (int i = 0; i < playerTurnsPerMove - 1; i++)
        {
            ret[i] = false;
        }
        return ret;
    }
    Vector2 RandomDirection()
    {
        int index = (Random.Range(0, validDirections.Count));
        return validDirections[index];
    }

    void ValidDirections()
    {
        RaycastHit2D hitBlocking;
        RaycastHit2D hitDefault;
        foreach(Vector2 direction in possibleDirections)
        {
            if (CanMove(direction, out hitBlocking, out hitDefault))
                validDirections.Add(direction);
        }

    }

    private bool CanMove(Vector2 direction, out RaycastHit2D hitBlocking, out RaycastHit2D hitDefault)
    {
        Vector2 start = transform.position;

        boxCollider.enabled = false;
        hitBlocking = Physics2D.Linecast(start, start+direction, blockingLayer);
        hitDefault = Physics2D.Linecast(start, start + direction, defaultLayer);
        boxCollider.enabled = true;

        if (hitBlocking.transform == null && hitDefault.transform == null)
        {
            return true;
        }
        return false;
    }

    private void NewFire(Vector2 direction)
    { 
        Instantiate(fire, fireLocation + direction, Quaternion.identity);
    }

    
}

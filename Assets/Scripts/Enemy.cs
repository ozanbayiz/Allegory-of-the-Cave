using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject
{
    public int playerDamage;

    private Animator animator;
    public Transform target;
    private List<Path> bestPath;
    private int turnIndex = 0;
    private bool[] turnArray;

    public int enemyHealth = 4;
    public int playerTurnsPerMove = 2;
    public AudioClip enemyAttack1;
    public AudioClip enemyAttack2;
    public AudioClip enemyDeath;


    int xDir;
    int yDir;

    // Start is called before the first frame update
    protected override void Start()
    {
        turnArray = CreateTurnArray();
        GameManager.instance.AddEnemyToList(this);
        animator = GetComponent<Animator>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
        base.Start();
        bestPath = HappyPath();
    }

    bool[] CreateTurnArray()
    {
        bool[] ret = new bool[playerTurnsPerMove];
        ret[playerTurnsPerMove - 1] = true;
        for(int i = 0; i<playerTurnsPerMove-1; i++)
        {
            ret[i] = false;
        }
        return ret;
    }
    public void DamageEnemy()
    {
        animator.SetTrigger("EnemyDamage");
        if (enemyHealth <= 1)
        {
            GameManager.instance.RemoveEnemyFromList(this);
            animator.runtimeAnimatorController = null;
        }
        if (enemyHealth <= 0)
        {
            SoundManager.instance.RandomizeSfx(enemyDeath);
            gameObject.active = false;
        }
        enemyHealth--;
        Debug.Log(enemyHealth);
    }

    protected override void AttemptMove<T> (int xDir, int yDir)
	{
        if (turnArray[turnIndex])
            base.AttemptMove<T>(xDir, yDir);
        turnIndex++;
        if (turnIndex > playerTurnsPerMove - 1)
            turnIndex -= playerTurnsPerMove;
	}

    public void MoveEnemy()
    {
        GetComponent<BoxCollider2D>().enabled = true;
        bestPath = HappyPath();

        Path nextMove = bestPath[0];
        bestPath.RemoveAt(0);
        xDir = nextMove.x - (int)transform.position.x;
        yDir = nextMove.y - (int)transform.position.y;

        AttemptMove<Player>(xDir, yDir);
	}

    protected override void OnCantMove <T> (T component)
    {
        Player hitPlayer = component as Player;
        SoundManager.instance.RandomizeSfx(enemyAttack1, enemyAttack2);
        animator.SetTrigger("EnemyAttack");

        hitPlayer.LoseFood(playerDamage);
    }

    public int BlocksToTarget(Vector2 start, Vector2 end)
    {
        return (int)(Mathf.Abs(start.x - end.x) + Mathf.Abs(start.y - end.y));
    }

    private List<Path> GetAdjacentSquares(Path p)
    {
        List<Path> ret = new List<Path>();

        int _x = p.x;
        int _y = p.y;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                int __x = _x + x;
                int __y = _y + y;
                if ((x == 0 && y == 0) || (x != 0 && y != 0))
                    continue;
                else if (__x < GameManager.instance.GetColumns() && __y < GameManager.instance.GetRows() && __x >= 0 && __y >= 0 && !CheckForCollision(new Vector2(_x, _y), new Vector2(__x, __y)))
                    ret.Add(new Path(p.g + 1, BlocksToTarget(new Vector2(__x, __y), target.position), p, __x, __y));
            }
        }
        return ret;
    }

    private bool CheckForCollision(Vector2 start, Vector2 end)
    {
        this.GetComponent<BoxCollider2D>().enabled = false;
        RaycastHit2D hit = Physics2D.Linecast(start, end, blockingLayer);

        this.GetComponent<BoxCollider2D>().enabled = true;

        if (hit.transform != null && !hit.collider.tag.Equals("Player"))
            return true;

        return false;
    }

    private List<Path> HappyPath()
    {
        List<Path> coolPaths =  new List<Path>();
        List<Path> evalPaths = new List<Path>();

        Path destinationSquare = new Path(BlocksToTarget(transform.position, target.position),0,null,(int)target.position.x,(int)target.position.y);

        evalPaths.Add(new Path(0, BlocksToTarget(transform.position,target.position), null, (int)transform.position.x, (int)transform.position.y));
        Path currentSquare = null;

        string shoowoop = "";
        while (evalPaths.Count > 0)
        {
            currentSquare = LowestFPath(evalPaths);

            coolPaths.Add(currentSquare);
            evalPaths.Remove(currentSquare);

            if(ContainsPath(coolPaths, destinationSquare))
            {
                return BuildPath(currentSquare);
                break;
            }

            List<Path> adjacentSquares = GetAdjacentSquares(currentSquare);
            foreach (Path p in adjacentSquares)
            {
                if (ContainsPath(coolPaths, p))
                    continue;
                if (!ContainsPath(evalPaths, p))
                {
                    p.g = currentSquare.g + 1;
                    p.h = BlocksToTarget(new Vector2(p.x, p.y), target.position);
                    evalPaths.Add(p);
                }
                else if (p.h + currentSquare.g + 1 < p.f)
                    p.parent = currentSquare;
            }
        }
        return BuildPath(currentSquare);
    }
    public bool CanMakeList()
    {
        return (HappyPath() != null);
    }

    private List<Path> BuildPath(Path p)
    {
        List<Path> bestPath = new List<Path>();
        Path currentLoc = p;
        bestPath.Insert(0, currentLoc);
        Path lastMove;
        while(currentLoc.parent != null)
        {
            currentLoc = currentLoc.parent;
            if (currentLoc.parent != null)
                bestPath.Insert(0, currentLoc);
            else
                lastMove = currentLoc;
        }
        return bestPath;
    }


    public Path LowestFPath(List<Path> evalList)
    {
        Path ret = evalList[0];
        for (int i = 1; i < evalList.Count; i++) if (ret.f > evalList[i].f) ret = evalList[i];
        return ret;
    }

    public bool ContainsPath(List<Path> l, Path p)
    {
        foreach (Path e in l) if (e.Equals(p)) return true;
        return false;
    }

}


public class Path: object
{

    public int g;
    public int h;
    public Path parent;
    public int x;
    public int y;

    public Path(int _g, int _h, Path _parent, int _x, int _y)
    {
        g = _g;
        h = _h;
        parent = _parent;
        x = _x;
        y = _y;
    }
    public int f
    {
        get
        {
            return g + h;
        }
    }

    public override bool Equals(System.Object obj)
    {
        if (obj == null) return false;
        Path c = obj as Path;

        return (this.x==c.x && this.y==c.y);
    }
}
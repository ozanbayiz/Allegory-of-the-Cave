using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Player : MovingObject
{
    public int wallDamage = 1;
    public int fireDamage = 30;
    public int pointsPerFood = 20;
    public int pointsPerSoda = 10;
    public float restartLevelDelay = 1f;
    public Text foodText;
    public int food;
    public static Player instance;

    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip eatSound1;
    public AudioClip eatSound2;
    public AudioClip drinkSound1;
    public AudioClip drinkSound2;
    public AudioClip gameOverSound;
    public AudioClip chopSound1;
    public AudioClip chopSound2;


    private Animator animator;
    private Vector2 touchOrigin = -Vector2.one;

    private int horizontal = 0;
    private int vertical = 0;

    protected override void Start()
    {
        animator = GetComponent<Animator>();
        food = GameManager.instance.playerFoodPoints;
        foodText.text = "energy: " + food;

        instance = this;

        base.Start();
    }

    private void OnDisable()
	{
        GameManager.instance.playerFoodPoints = food;
	}

    void Update()
    {
        if (!GameManager.instance.playersTurn) return;

        horizontal = 0;
        vertical = 0;

    #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");

        if (horizontal != 0)
            vertical = 0;
#else
        if (Input.touchCount > 0)
		{
            Touch myTouch = Input.touches[0];

            if (myTouch.phase == TouchPhase.Began)
			{
                touchOrigin = myTouch.position;
			}
            else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
			{
                Vector2 touchEnd = myTouch.position;
                float x = touchEnd.x - touchOrigin.x;
                float y = touchEnd.y - touchOrigin.y;
                touchOrigin.x = -1;
                if (Mathf.Abs(x) > Mathf.Abs(y))
                    horizontal = x > 0 ? 1 : -1;
                else
                    vertical = y > 0 ? 1 : -1;
			}
		}
#endif

        if (horizontal != 0 || vertical != 0)
            AttemptMove<Wall>(horizontal, vertical);
    }

	protected override void AttemptMove<T>(int xDir, int yDir)
	{
        GameManager.instance.playersTurn = true;

        base.AttemptMove<Enemy>(xDir, yDir);
        base.AttemptMove<T>(xDir, yDir);

        RaycastHit2D hit;

        if(Move(xDir, yDir, out hit))
		{
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
            food--;
            foodText.text = "energy: " + food;
            GameManager.instance.playersTurn = false;
        }

        CheckIfGameOver();

        GameManager.instance.playersTurn = false;
	}

    private void OnTriggerEnter2D (Collider2D other)
	{
        if (other.tag == "Exit")
        {
            Invoke("Restart", restartLevelDelay);

            enabled = false;
        }
        else if (other.tag == "Food")
        {
            food += pointsPerFood;
            foodText.text = "+  " + pointsPerFood + " energy: " + food;
            SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
        }
        else if (other.tag == "Soda")
        {
            food += pointsPerSoda;
            foodText.text = "+  " + pointsPerSoda + " energy: " + food;
            SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
        }
        else if (other.tag == "Fire")
        {
            food -= fireDamage;
            foodText.text = "-  " + fireDamage + " energy: " + food;
        }
        other.gameObject.SetActive(false);
    }

    protected override void OnCantMove<T>(T component)
	{
        try
        {
            MoveWall disWall = component as MoveWall;
            disWall.AttemptMoveWall(horizontal, vertical);
            GameManager.instance.playersTurn = false;
        }
        catch (Exception){}

        try
        {
            BreakWall hitWall = component as BreakWall;
            hitWall.DamageWall();
            GameManager.instance.playersTurn = false;
        }
        catch (Exception) { }
        try
        {
            Enemy enemy = component as Enemy;
            enemy.DamageEnemy();
            GameManager.instance.playersTurn = false;
        }
        catch (Exception) { }
        food--;

        SoundManager.instance.RandomizeSfx(chopSound1, chopSound2);
        foodText.text = "energy: " + food;
        animator.SetTrigger("PlatoAttack");
    }

    private void Restart()
	{
        GameManager.instance.level++;
        Application.LoadLevel(Application.loadedLevel);
	}

    public void LoseFood (int loss)
	{
        animator.SetTrigger("PlatoDamage");
        food -= loss;
        foodText.text = "-  " + loss + " energy: " + food;
        CheckIfGameOver();
	}

	private void CheckIfGameOver()
	{
        if (food <= 0)
        {
            SoundManager.instance.PlaySingle(gameOverSound);
            SoundManager.instance.musicSource.Stop();

            GameManager.instance.GameOver();
        }
	}
    public int GetFood() { return food; }
    public void SetFood(int i) { food = i; }
    public int BlocksToTarget(Vector2 start, Vector2 end)
    {
        return (int)(Mathf.Abs(start.x - end.x) + Mathf.Abs(start.y - end.y));
    }

    private List<Path> GetAdjacentSquares(Path p, Transform target)
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
    public bool CanProducePath(Transform start, Transform target)
    {
        List<Path> coolPaths = new List<Path>();
        List<Path> evalPaths = new List<Path>();

        Path destinationSquare = new Path(BlocksToTarget(start.position, target.position), 0, null, (int)target.position.x, (int)target.position.y);

        evalPaths.Add(new Path(0, BlocksToTarget(start.position, target.position), null, (int)start.position.x, (int)start.position.y));
        Path currentSquare;

        while (evalPaths.Count > 0)
        {
            currentSquare = LowestFPath(evalPaths);

            coolPaths.Add(currentSquare);
            evalPaths.Remove(currentSquare);

            if (ContainsPath(coolPaths, destinationSquare))
            {
                return true;
            }

            List<Path> adjacentSquares = GetAdjacentSquares(currentSquare, target);
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
        return false;
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

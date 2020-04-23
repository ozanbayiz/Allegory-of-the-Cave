using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakWall : Wall
{
    private int hp = 5;
    public Sprite[] damageSprites;

    private SpriteRenderer spriteRenderer;


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void DamageWall()
    {
        hp--;

        if (hp <= 0)
            gameObject.SetActive(false);

        spriteRenderer.sprite = damageSprites[4 - hp];
    }

}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public List<Bullet> activeBullets = new List<Bullet>();
    public List<Bullet> markedForDeathBullets = new List<Bullet>();
    public List<Bullet> inactiveBullets = new List<Bullet>();
    public Player player;
    public GameObject mainEnemy;
    public float xbounds;
    public float ybounds;

    public Boss currentBoss;

    public static GameManager instance { get; private set; }

    public void FixedUpdate()
    {
        player.PlayerUpdate();
        WaveFixedUpdate();
        Trash();
    }

    private void WipeAllEnemiesAndBullets()
    {

        foreach (Bullet b in activeBullets)
        {
            b.Die();
        }
        Trash();
    }

    void Awake()
    {
        instance = this;
        //player = GameObject.Find("Player").GetComponent<Player>();
    }


    void DisposeAllBullets()
    {
        foreach (Bullet b in activeBullets)
        {
            markedForDeathBullets.Add(b);
        }
    }



    void Trash()
    {

        foreach (Bullet ded in markedForDeathBullets)
        {
            activeBullets.Remove(ded);
            inactiveBullets.Add(ded);
            ded.thisCollider.enabled = false;
            ded.gameObject.SetActive(false);
        }

        markedForDeathBullets.Clear();
    }


    void WaveFixedUpdate()
    {
        foreach (Bullet bullet in activeBullets)
        {
            bullet.BulletUpdate();
        }
        currentBoss.BossUpdate();
    }

    //Returns angle from current to toLookAt in degrees
    public float LookAtPos(Vector3 current, Vector3 toLookAt)
    {
        float ang = ((180 / Mathf.PI) * Mathf.Atan2(toLookAt.y - current.y, toLookAt.x - current.x)) - 90;
        return ang < 0 ? 360 + ang : ang;
    }

}
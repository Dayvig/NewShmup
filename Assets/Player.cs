using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
    // Update is called once per frame

    public float rotationTarget;
    public Vector3 positionTarget;
    [Range(0.05f, 0.25f)]
    public float baseMovementSpeed;
    [Range(1f, 30f)]
    public float smoothingFactor;
    [Range(0.01f, 0.4f)]
    public float cylinderOffset;

    [Range(1.25f, 3f)]
    public float focusedSpeedFactor;

    [Range(2f, 8f)]
    public float boostFactor;

    [Range(0.05f, 0.5f)]
    public float movementRampupInterval;

    [Range(0.05f, 0.5f)]
    public float boostInterval;

    float shotTimer;
    public float fireRate;
    public float bulletSpeed;
    public GameObject baseBullet;
    public float currentMovementSpeed;

    public bool fireLeftCylinder = true;

    public float movementRampUpTimer = 0.0f;
    private float movementSpeedPriorToRampup;
    private bool justBoosted;
    public int currentLives;

    float deathTimer = 99f;
    bool invincible = false;
    float deathInterval = 2f;
    public SpriteRenderer PlayerSprite;
    public Animator explosion;

    public enum MOVEMENTSTATE
    {
        IDLE,
        FOCUSED,
        MOVEMENTRAMPUP,
        STANDARD,
        BOOST
    }

    public MOVEMENTSTATE currentMovementState;

    public static Player instance { get; private set; }
    private void Awake()
    {
        instance = this;
    }

    public void SetupPlayer()
    {
        for (int i = 0; i < GameManager.instance.playerLives; i++)
        {
            Instantiate(GameManager.instance.lifeIcon, GameManager.instance.livesLayout.transform);
        }
        currentLives = GameManager.instance.playerLives;
    }

    public void TakeHit()
    {
        if (!invincible)
        {
            currentLives--;
            if (currentLives >= 0)
            {
                GameManager.instance.livesLayout.transform.GetChild(currentLives).gameObject.SetActive(false);
            }
            else
            {
                GameManager.instance.Lose();
            }
            deathTimer = 0.0f;
            invincible = true;
            PlayerSprite.enabled = false;
            explosion.gameObject.SetActive(true);
            explosion.Play("stockexplosion");
            StartCoroutine("hideExplosion");
        }
    }
    public IEnumerator hideExplosion()
    {
        yield return new WaitForSeconds(1f);
        explosion.gameObject.SetActive(false);
        transform.position = new Vector3(0f, -3.4f, 0f);
    }

    public void PlayerUpdate()
    {
        if (deathTimer < deathInterval * 2)
        {
            deathTimer += Time.deltaTime;
            if (deathTimer > deathInterval / 2)
            {
                PlayerSprite.enabled = true;
                takeInputs();
            }
            if (deathTimer > deathInterval)
            {
                invincible = false;
            }
        }
        else
        {
            takeInputs();
        }
    }

    public void PlayerFixedUpdate()
    {
        Vector3 currentPos = gameObject.transform.position;
        Quaternion rot = gameObject.transform.rotation;

        rotationTarget = GameManager.instance.LookAtPos(currentPos, GameManager.instance.mainEnemy.transform.position);

        gameObject.transform.rotation = Quaternion.Euler(new Vector3(rot.x, rot.y, rotationTarget));
        gameObject.transform.position = Vector3.Lerp(currentPos, positionTarget, Time.deltaTime * smoothingFactor);

        shotTimer += Time.deltaTime;
        if (shotTimer > fireRate)
        {
            Fire();
            shotTimer = 0.0f;
        }
    }

    void playerValCalc()
    {
        switch (currentMovementState)
        {
            case MOVEMENTSTATE.IDLE:
                currentMovementSpeed = 0f; 
                break;
            case MOVEMENTSTATE.FOCUSED:
                currentMovementSpeed = baseMovementSpeed / focusedSpeedFactor; 
                break;
            case MOVEMENTSTATE.MOVEMENTRAMPUP:
                if (movementRampUpTimer > movementRampupInterval)
                {
                    SetState(MOVEMENTSTATE.STANDARD);
                }
                else
                {
                    movementRampUpTimer += Time.deltaTime;
                    currentMovementSpeed = Mathf.Lerp(movementSpeedPriorToRampup, baseMovementSpeed, movementRampUpTimer / movementRampupInterval);
                }
                break;
            case MOVEMENTSTATE.STANDARD:
                currentMovementSpeed = baseMovementSpeed;
                break;
            case MOVEMENTSTATE.BOOST:
                if (movementRampUpTimer > boostInterval)
                {
                    SetState(MOVEMENTSTATE.STANDARD);
                }
                else
                {
                    movementRampUpTimer += Time.deltaTime;
                    currentMovementSpeed = Mathf.Lerp(baseMovementSpeed * boostFactor, baseMovementSpeed, movementRampUpTimer / boostInterval);
                }
                break;
        }
    }

    void takeInputs()
    {
        bool movementKeyPressed = false;
        bool focused = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool boosting = Input.GetKey(KeyCode.Space);
        if (!Input.GetKey(KeyCode.Space)) { 
            justBoosted = false;
        }
        Vector3 newMovementVector = new Vector3(0f, 0f, 0f);
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            newMovementVector += (Vector3.down) * 0.8f;
            movementKeyPressed = true;
        }
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            newMovementVector += (Vector3.up) * 0.8f;
            movementKeyPressed = true;
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            newMovementVector += (Vector3.left);
            movementKeyPressed = true;
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            newMovementVector += (Vector3.right);
            movementKeyPressed = true;
        }

        handleMovementStateTransitions(movementKeyPressed, focused, boosting);
        playerValCalc();
        positionTarget = transform.position + (newMovementVector * currentMovementSpeed);
    }

    void handleMovementStateTransitions(bool moveKeyPressed, bool focusHeld, bool boost)
    {
        if (boost && moveKeyPressed && !focusHeld && !justBoosted)
        {
            StartBoost();
        }
        if (moveKeyPressed && !focusHeld)
        {
            if (currentMovementState.Equals(MOVEMENTSTATE.IDLE) || currentMovementState.Equals(MOVEMENTSTATE.FOCUSED))
            StartMovementRampup();
        }
        else if (moveKeyPressed && focusHeld)
        {
            SetState(MOVEMENTSTATE.FOCUSED);
        }
        else if (!moveKeyPressed)
        {
            SetState(MOVEMENTSTATE.IDLE);
        }
    }

    void StartMovementRampup()
    {
        movementSpeedPriorToRampup = currentMovementSpeed;
        movementRampUpTimer = 0.0f;
        SetState(MOVEMENTSTATE.MOVEMENTRAMPUP);
    }

    void StartBoost()
    {
        movementRampUpTimer = 0.0f;
        justBoosted = true;
        SetState(MOVEMENTSTATE.BOOST);
    }

    void SetState(MOVEMENTSTATE next)
    {
        currentMovementState = next;
    }

    void Fire()
    {
        GameObject newBulletObject = null;
        foreach (Bullet b in GameManager.instance.inactiveBullets)
        {
            newBulletObject = b.gameObject;
            break;
        }
        if (newBulletObject == null)
        {
            newBulletObject = Instantiate(baseBullet, transform.position, Quaternion.identity);
        }

        //create and instantiate new bullet

        Bullet bulletScript = newBulletObject.GetComponent<Bullet>();
        bulletScript.init(transform.position, bulletSpeed, fireLeftCylinder);
        if (!GameManager.instance.activeBullets.Contains(bulletScript))
        {
            GameManager.instance.activeBullets.Add(bulletScript);
        }
        GameManager.instance.inactiveBullets.Remove(bulletScript);

        fireLeftCylinder = !fireLeftCylinder;
    }

}

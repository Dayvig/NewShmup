using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : Bullet
{
    public int bulletID;
    public WeaponBehavior sharpnelData;
    public void init(WeaponData weaponData, WeaponBehavior weapon, float firingAngle, float lifetime)
    {
        this.gameObject.SetActive(true);
        thisCollider.enabled = true;

        float rotationTarget = firingAngle;
        float rotation = -(rotationTarget * (Mathf.PI / 180));

        Quaternion rot = gameObject.transform.rotation;
        float angle = -rotation * Mathf.Rad2Deg;
        gameObject.transform.rotation = Quaternion.Euler(new Vector3(rot.x, rot.y, angle));

        xSpeed = Mathf.Sin(rotation);
        ySpeed = Mathf.Cos(rotation);

        flight = new Vector3(xSpeed, ySpeed, 0).normalized * weaponData.bulletSpeed;
        positionCurrent = weapon.transform.position;
        positionTarget = positionCurrent;

        gameObject.transform.position = positionCurrent;
        bulletID = weaponData.weaponID;
        lifeTimer = Random.Range(-weaponData.lifetimeVariance, weaponData.lifetimeVariance);
        timeBeforeDestruction = lifetime;
        thisData = weaponData;
        SetupSpawnerWeapon();
    }

    public void init(WeaponData weaponData, WeaponBehavior weapon, float firingAngle, float posOffset, float lifetime)
    {
        this.gameObject.SetActive(true);
        thisCollider.enabled = true;

        float rotationTarget = firingAngle;
        float rotation = -(rotationTarget * (Mathf.PI / 180));

        Quaternion rot = gameObject.transform.rotation;
        float angle = -rotation * Mathf.Rad2Deg;
        gameObject.transform.rotation = Quaternion.Euler(new Vector3(rot.x, rot.y, angle));

        xSpeed = Mathf.Sin(rotation);
        ySpeed = Mathf.Cos(rotation);

        flight = new Vector3(xSpeed, ySpeed, 0).normalized * weaponData.bulletSpeed;
        Vector3 position = weapon.transform.position;
        positionCurrent = position + (new Vector3(-flight.y, flight.x, flight.z).normalized) * posOffset;
        positionTarget = positionCurrent;

        positionTarget = positionCurrent;

        gameObject.transform.position = positionCurrent;
        bulletID = weaponData.weaponID;
        lifeTimer = 0.0f;
        timeBeforeDestruction = lifetime;
        thisData = weaponData;
        SetupSpawnerWeapon();
    }

    void SetupSpawnerWeapon()
    {
        if (thisData is SpawnerWeaponData)
        {
            SpawnerWeaponData spawnerData = (SpawnerWeaponData)thisData;
            if (this.transform.childCount > 1)
            {
                List<Transform> toMove = new List<Transform>();
                for (int i = 1; i < this.transform.childCount; i++)
                {
                    toMove.Add(transform.GetChild(i));
                }
                foreach (Transform t in toMove)
                {
                    t.SetParent(GameManager.instance.transform);
                }
            }
            if (transform.childCount == 1 && transform.GetChild(0).GetComponent<WeaponBehavior>() != null)
            {
                WeaponBehavior weaponBehavior = transform.GetChild(0).GetComponent<WeaponBehavior>();
                weaponBehavior.weaponMountData = spawnerData.sharpnel;
                weaponBehavior.init();
            }
            if (transform.childCount == 0)
            {
                GameObject newWeapon = Instantiate(GameManager.instance.invisibleWeapon, this.transform);
                GameManager.instance.invisibleWeaponPool.Add(newWeapon);
                WeaponBehavior weaponBehavior = newWeapon.GetComponent<WeaponBehavior>();
                weaponBehavior.weaponMountData = spawnerData.sharpnel;
                weaponBehavior.init();
                sharpnelData = weaponBehavior;
            }
        }
    }

    public void Die(bool withShrapnel) {
        if (thisData is SpawnerWeaponData && withShrapnel)
        {
            GameManager.instance.fireQueue.Add(sharpnelData);
        }
        GameManager.instance.currentBoss.markedForDeathEnemyBullets.Add(this);
        thisCollider.enabled = false;
    }

    public override void CheckCollisions()
    {
        RaycastHit2D[] hits = Physics2D.LinecastAll(transform.position, positionCurrent, 7);
        if (hits.Length != 0)
        {
            foreach (RaycastHit2D h in hits)
            {
                if (h.transform.gameObject.CompareTag("Player"))
                {
                    Collide(h.collider);
                    break;
                }
            }
        }
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(positionTarget, 0.1f);
        foreach (Collider2D c in hitColliders)
        {
            Collide(c);
        }
        transform.position = positionCurrent;
        if (positionCurrent.x > GameManager.instance.xbounds || positionCurrent.x < -GameManager.instance.xbounds ||
            positionCurrent.y > GameManager.instance.ybounds || positionCurrent.y < -GameManager.instance.ybounds)
        {
            Die(false);
        }
    }

    public override void BulletUpdate()
    {
        if (timeBeforeDestruction > 0.0f)
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer > timeBeforeDestruction)
            {
                Die(true);
            }
        }
        positionTarget += flight;
        positionCurrent = Vector3.Lerp(
            positionCurrent,
            positionTarget,
            Time.deltaTime * 30.0f);
        if (thisCollider.enabled)
        {
            CheckCollisions();
        }
    }

    public override void Collide(Collider2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            Die(true);
            GameManager.instance.player.TakeHit();
        }
    }

}

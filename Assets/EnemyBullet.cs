using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.Table;
using static UnityEngine.UI.Image;

public class EnemyBullet : Bullet
{
    public int bulletID;

    public void init(WeaponData weaponData, WeaponBehavior weapon, float firingAngle)
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
    }

    public override void Die() {
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
            Die();
        }
    }

    public override void Collide(Collider2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            Die();
            //Enemy take damage
        }
    }

}

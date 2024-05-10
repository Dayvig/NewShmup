using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;
using static UnityEngine.UI.Image;

public class Bullet : MonoBehaviour
{
    public BoxCollider2D thisCollider;
    public Vector3 flight;
    public Vector3 positionCurrent;
    public Vector3 positionTarget;
    public float xSpeed;
    public float ySpeed;
    private float offset;
    public float lifeTimer;
    public float timeBeforeDestruction;
    public WeaponData thisData;
    public virtual void Die() {
        GameManager.instance.markedForDeathBullets.Add(this);
        thisCollider.enabled = false;
    }

    public virtual void init(Vector3 position, float speed, bool leftCylinder)
    {
        offset = Player.instance.cylinderOffset;
        this.gameObject.SetActive(true);
        thisCollider.enabled = true;

        float rotationTarget = GameManager.instance.LookAtPos(position, GameManager.instance.mainEnemy.transform.position);
        float rotation = -(rotationTarget * (Mathf.PI / 180));

        Quaternion rot = gameObject.transform.rotation;
        float angle = -rotation * Mathf.Rad2Deg;
        gameObject.transform.rotation = Quaternion.Euler(new Vector3(rot.x, rot.y, angle));

        xSpeed = Mathf.Sin(rotation);
        ySpeed = Mathf.Cos(rotation);

        flight = new Vector3(xSpeed, ySpeed, 0).normalized * speed;

        positionCurrent = position + (new Vector3((leftCylinder ? 1 : -1) * -position.y, (leftCylinder ? 1 : -1) * position.x, position.z).normalized * offset);
        positionTarget = positionCurrent;

        gameObject.transform.position = positionCurrent;
        lifeTimer = 0.0f;
        timeBeforeDestruction = 0.0f;
    }
    public virtual void BulletUpdate()
    {
        if (timeBeforeDestruction > 0.0f)
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer > timeBeforeDestruction)
            {
                Die();
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

    public virtual void CheckCollisions()
    {
        RaycastHit2D[] hits = Physics2D.LinecastAll(transform.position, positionCurrent, 7);
        if (hits.Length != 0)
        {
            foreach (RaycastHit2D h in hits)
            {
                if (h.transform.gameObject.CompareTag("Enemy"))
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

    public virtual void Collide(Collider2D col)
    {
        if (col.gameObject.CompareTag("Enemy"))
        {
            Die();
            GameManager.instance.bossHP--;
            if (GameManager.instance.bossHP < 0)
            {
                GameManager.instance.Win();
            }
        }
    }

}

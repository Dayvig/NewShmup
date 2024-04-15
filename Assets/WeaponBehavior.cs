using System.Collections.Generic;
using UnityEngine;

public class WeaponBehavior : MonoBehaviour
{
    public List<WeaponData> firingPattern;
    public int firingPatternIndex = 0;
    public WeaponData weaponData;
    public WeaponMountData weaponMountData;

    public float fireTimer;
    public float currentAngle;
    public float targetAngle;

    public Vector3 previousPosition;
    public Vector3 targetPosition;

    public float aimTimer;
    public float moveTimer;
    public int moveIndex;
    public float rotationTimer;
    public int rotationIndex;
    public bool targetAcquired;
    public bool atMovementDestination;

    public int currentAmmo;

    public enum WEAPON_STATE
    {
        STANDARDFIRE,
        AMMOFIRE,
        RELOADING,
    }

    public WEAPON_STATE state;
    private void Start()
    {
        weaponData = firingPattern[firingPatternIndex];

        Quaternion rot = gameObject.transform.rotation;
        currentAngle = targetAngle = weaponMountData.initialFiringAngle;
        gameObject.transform.rotation = Quaternion.Euler(new Vector3(rot.x, rot.y, currentAngle));
        rotationIndex = 0;
        moveIndex = 0;
        targetAcquired = true;
        atMovementDestination = true;
        currentAmmo = firingPattern[0].ammo;
        previousPosition = transform.localPosition;
        firingPattern = weaponMountData.weaponsOnMount;

    }


    void FixedUpdate()
    {
        MountMovementUpdate();
        if (state == WEAPON_STATE.STANDARDFIRE)
        {
            standardFireUpdate();
        }
        if (state == WEAPON_STATE.AMMOFIRE)
        {
            ammoFireUpdate();
        }
        if (state == WEAPON_STATE.RELOADING)
        {
            reloadUpdate();
        }
    }

    void MountMovementUpdate()
    {
        Quaternion rot = transform.rotation;

        if (weaponMountData.movementSpeed > 0 && weaponMountData.movementWayPoints.Count > 0)
        {
            if (atMovementDestination)
            {
                moveTimer += Time.deltaTime;
                if (moveTimer >= weaponMountData.movementInterval)
                {
                    targetPosition = weaponMountData.movementWayPoints[moveIndex];
                    atMovementDestination = false;
                    moveTimer = 0f;
                }
            }
            else
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, weaponMountData.movementSpeed / 1000);
                if (Vector3.Distance(transform.localPosition, targetPosition) <= 0.01f)
                {
                    transform.localPosition = targetPosition;
                    previousPosition = transform.localPosition;
                    atMovementDestination = true;
                    moveIndex++;
                    if (moveIndex >= weaponMountData.movementWayPoints.Count)
                    {
                        moveIndex = 0;
                    }
                }
            }
        }

        if (weaponMountData.mountType.Equals(WeaponMountData.WeaponMountType.PLAYERAIMEDMOUNT))
        {
            aimTimer += Time.deltaTime;
            if (aimTimer > weaponMountData.targetAcquisitionInterval)
            {
                targetAngle = GameManager.instance.LookAtPos(transform.position, GameManager.instance.player.transform.position);
                targetAcquired = false;
                aimTimer = 0.0f;
            }
            if (!targetAcquired)
            {
                float nextAngleDelta = currentAngle > targetAngle ? weaponMountData.targetAcquisitionSpeed * -1: weaponMountData.targetAcquisitionSpeed;
                if (nextAngleDelta > 0)
                {
                    currentAngle = (currentAngle + nextAngleDelta) > targetAngle ? targetAngle : currentAngle + nextAngleDelta;
                }
                else
                {
                    currentAngle = (currentAngle + nextAngleDelta) < targetAngle ? targetAngle : currentAngle + nextAngleDelta;
                }
                if (Mathf.Abs(currentAngle - targetAngle) <= 1f)
                {
                    currentAngle = targetAngle;
                    targetAcquired = true;
                }

            }

            gameObject.transform.rotation = Quaternion.Euler(new Vector3(rot.x, rot.y, currentAngle));
        }
        else if (weaponMountData.mountType.Equals(WeaponMountData.WeaponMountType.ROTATINGMOUNT))
        {
            if (targetAcquired)
            {
                rotationTimer += Time.deltaTime;
                if (rotationTimer > weaponMountData.rotationIntervals[rotationIndex])
                {
                    targetAngle = weaponMountData.rotationWayPoints[rotationIndex];
                    if (weaponMountData.clockwise[rotationIndex])
                    {
                        targetAngle = targetAngle > currentAngle ? targetAngle - 360 : targetAngle;
                    }
                    else
                    {
                        targetAngle = targetAngle < currentAngle ? targetAngle + 360 : targetAngle;
                    }
                    rotationTimer = 0.0f;
                    targetAcquired = false;
                }
            }
            else 
            {
                float nextAngleDelta = weaponMountData.clockwise[rotationIndex] ? weaponMountData.rotationSpeed * -1: weaponMountData.rotationSpeed;
                if (nextAngleDelta > 0)
                {
                    currentAngle = (currentAngle + nextAngleDelta) > targetAngle ? targetAngle : currentAngle + nextAngleDelta;
                }
                else
                {
                    currentAngle = (currentAngle + nextAngleDelta) < targetAngle ? targetAngle : currentAngle + nextAngleDelta;
                }
                if (Mathf.Abs(currentAngle - targetAngle) <= 1f)
                {
                    currentAngle = targetAngle;
                    targetAcquired = true;
                    if (currentAngle >= 360)
                    {
                        currentAngle -= 360;
                    }
                    if (currentAngle <= 0)
                    {
                        currentAngle += 360;
                    }
                    rotationIndex++;
                    if (rotationIndex >= weaponMountData.rotationIntervals.Count)
                    {
                        rotationIndex = 0;
                    }
                }

            }
            gameObject.transform.rotation = Quaternion.Euler(new Vector3(rot.x, rot.y, currentAngle));
        }
    }

    void standardFireUpdate()
    {
        if (fireTimer <= weaponData.fireRate)
        {
            fireTimer += Time.deltaTime;
        }
        else
        {
            fireTimer = 0f;
            Fire(weaponData);
            CycleWeaponPattern();
        }
    }

    void reloadUpdate()
    {
        if (fireTimer <= weaponData.reloadInterval)
        {
            fireTimer += Time.deltaTime;
        }
        else
        {
            fireTimer = 0f;
            if (!weaponMountData.fireEntirePatternWithAmmo)
            {
                CycleWeaponPattern();
            }
            else
            {
                state = WEAPON_STATE.AMMOFIRE;
                currentAmmo = weaponData.ammo;
            }
        }
    }

    void ammoFireUpdate()
    {
        if (weaponData.ammo == 0)
        {
            standardFireUpdate();
            return;
        }
        if (fireTimer <= weaponData.fireRate)
        {
            fireTimer += Time.deltaTime;
        }
        else
        {
            fireTimer = 0f;
            Fire(weaponData);
            if (weaponMountData.fireEntirePatternWithAmmo)
            {
                CycleWeaponPattern(false);
            }
            currentAmmo--;
            if (currentAmmo <= 0)
            {
                state = WEAPON_STATE.RELOADING;
            }
        }
    }

    private void CycleWeaponPattern()
    {
        CycleWeaponPattern(true);
    }

    private void CycleWeaponPattern(bool reload)
    {
        firingPatternIndex++;
        if (firingPatternIndex >= firingPattern.Count)
        {
            firingPatternIndex = 0;
        }
        weaponData = firingPattern[firingPatternIndex];
        if (reload)
        {
            if (weaponData.ammo != 0)
            {
                state = WEAPON_STATE.AMMOFIRE;
                currentAmmo = weaponData.ammo;
            }
            else
            {
                state = WEAPON_STATE.STANDARDFIRE;
                currentAmmo = 0;
            }
        }
    }

    private void Fire(WeaponData weaponData)
    {
        if (weaponData.numBulletsPerShot == 1)
        {
            GameObject newBulletObject = null;
            foreach (Bullet b in GameManager.instance.inactiveBullets)
            {
                newBulletObject = b.gameObject;
                break;
            }
            if (newBulletObject == null)
            {
                newBulletObject = Instantiate(weaponData.bulletPrefab, transform.position, Quaternion.identity);
            }

            //create and instantiate new bullet

            EnemyBullet bulletScript = newBulletObject.GetComponent<EnemyBullet>();
            bulletScript.init(weaponData, this, currentAngle);

            RemoveBulletFromLists(bulletScript);
            GameManager.instance.currentBoss.activeEnemyBullets.Add(bulletScript);


        }
        else
        {
            //Sets initial to 0 if odd and 1 if even
            int initial = 1 - (weaponData.numBulletsPerShot % 2);
            for (int s = initial; s < weaponData.numBulletsPerShot + initial; s++)
            {
                float angleOffSet =
                    ((((float)s / (weaponData.numBulletsPerShot + (1 - (2 * (weaponData.numBulletsPerShot % 2))))) * weaponData.bulletSpread) -
                     (weaponData.bulletSpread / 2));
                GameObject newBulletObject = null;
                foreach (EnemyBullet b in GameManager.instance.currentBoss.inactiveEnemyBullets)
                {
                    if (b.bulletID == weaponData.weaponID)
                    {
                        newBulletObject = b.gameObject;
                        break;
                    }
                }
                if (newBulletObject == null)
                {
                    newBulletObject = Instantiate(weaponData.bulletPrefab, transform.position, Quaternion.identity);
                }
                EnemyBullet bulletScript = newBulletObject.GetComponent<EnemyBullet>();
                bulletScript.init(weaponData, this, currentAngle + angleOffSet);

                RemoveBulletFromLists(bulletScript);
                GameManager.instance.currentBoss.activeEnemyBullets.Add(bulletScript);

            }
        }
    }

    public void CloseWeapon()
    {
        Debug.Log("Closing " + gameObject.name);
        gameObject.SetActive(false);
    }

    public void OpenWeapon()
    {
        Debug.Log("Opening " + gameObject.name);
        gameObject.SetActive(true);
    }

    void RemoveBulletFromLists(EnemyBullet b)
    {
        if (GameManager.instance.currentBoss.inactiveEnemyBullets.Contains(b))
        {
            GameManager.instance.currentBoss.inactiveEnemyBullets.Remove(b);
        }
        if (GameManager.instance.currentBoss.activeEnemyBullets.Contains(b))
        {
            GameManager.instance.currentBoss.activeEnemyBullets.Remove(b);
        }
    }
}

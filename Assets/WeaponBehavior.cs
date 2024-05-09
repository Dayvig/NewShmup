using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
    public float independantRotationIntervalTimer;
    public int rotationIndex;
    public int independantRotationIndex;
    public bool rotationDestReached;
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
        init();
    }

    public void init()
    {
        firingPatternIndex = 0;
        firingPattern = weaponMountData.weaponsOnMount;

        weaponData = firingPattern[firingPatternIndex];

        Quaternion rot = gameObject.transform.rotation;
        currentAngle = targetAngle = weaponMountData.initialFiringAngle;
        gameObject.transform.rotation = Quaternion.Euler(new Vector3(rot.x, rot.y, currentAngle));
        rotationIndex = 0;
        independantRotationIndex = 0;
        moveIndex = 0;
        rotationDestReached = true;
        atMovementDestination = false;
        currentAmmo = firingPattern[0].ammo;
        previousPosition = transform.localPosition;
        if (weaponMountData.movementWayPoints.Count > 0) {
            targetPosition = weaponMountData.movementWayPoints[moveIndex];
        }
        spawnDelayTimer = Random.Range(-weaponMountData.randomDelayOffset, weaponMountData.randomDelayOffset);
        fireTimer = Random.Range(-weaponData.fireRateVariance, weaponData.fireRateVariance);
    }

    public int fireIndex = 0;
    public float specialAmmoFireTimer = 0.0f;
    public float spawnDelayTimer = 0.0f;
    void FixedUpdate()
    {
        if (spawnDelayTimer <= weaponMountData.delayOnSpawn)
        {
            spawnDelayTimer += Time.deltaTime;
            return;
        }
        MountMovementUpdate();
        if (weaponMountData.fireAtSpecificIntervals)
        {
            fireTimer += Time.deltaTime;

            if (fireTimer >= weaponMountData.fireIntervals[fireIndex])
            {
                state = WEAPON_STATE.STANDARDFIRE;
                fireTimer = 0.0f;
            }
            if (state == WEAPON_STATE.STANDARDFIRE)
            {
                currentAmmo = weaponData.ammo;
                if (currentAmmo > 0)
                {
                    state = WEAPON_STATE.AMMOFIRE;
                    specialAmmoFireTimer = 0f;
                }
                else
                {
                    Fire(weaponData);
                    CycleWeaponPattern();
                    fireIndex++;
                    if (fireIndex > weaponMountData.fireIntervals.Count-1)
                    {
                        fireIndex = 0;
                    }
                    state = WEAPON_STATE.RELOADING;
                }
            }
            if (state == WEAPON_STATE.AMMOFIRE)
            {
                specialAmmoFireUpdate();
            }
        }
        else
        {
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
    }

    void MountMovementUpdate()
    {
        Quaternion rot = transform.rotation;

        if (weaponMountData.movementSpeed > 0 && weaponMountData.movementWayPoints.Count > 0)
        {
            if (!weaponMountData.moveToWayPointBeforeTicking)
            {
                //Increses rotation Timer
                moveTimer += Time.deltaTime;


                if (Vector3.Distance(transform.localPosition, targetPosition) <= 0.01f)
                {
                    if (moveTimer > weaponMountData.movementInterval)
                    {
                        moveTimer = 0;
                        transform.localPosition = targetPosition;
                        previousPosition = transform.localPosition;
                        atMovementDestination = false;
                        moveIndex++;
                        if (moveIndex >= weaponMountData.movementWayPoints.Count)
                        {
                            moveIndex = 0;
                        }
                        targetPosition = weaponMountData.movementWayPoints[moveIndex];
                    }
                }
                else
                {
                    transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, weaponMountData.movementSpeed / 1000);
                }
            }
            else
            {
                if (moveTimer > weaponMountData.movementInterval)
                {
                    atMovementDestination = false;
                    targetPosition = weaponMountData.movementWayPoints[moveIndex];
                }

                if (!atMovementDestination)
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
                else
                {
                    moveTimer += Time.deltaTime;
                }
            }
        }

        if (weaponMountData.mountType.Equals(WeaponMountData.WeaponMountType.PLAYERAIMEDMOUNT))
        {

            if (!rotationDestReached)
            {
                float nextAngleDelta = currentAngle > targetAngle ? weaponMountData.targetAcquisitionSpeed * -1 : weaponMountData.targetAcquisitionSpeed;
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
                    rotationDestReached = true;
                }

            }
            else
            {
                aimTimer += Time.deltaTime;
            }
            if (aimTimer > weaponMountData.targetAcquisitionInterval)
            {
                targetAngle = GameManager.instance.LookAtPos(transform.position, GameManager.instance.player.transform.position);
                rotationDestReached = false;
                aimTimer = 0.0f;
            }

            gameObject.transform.rotation = Quaternion.Euler(new Vector3(rot.x, rot.y, currentAngle));
        }
        else if (weaponMountData.mountType.Equals(WeaponMountData.WeaponMountType.ROTATINGMOUNT))
        {
            if (!weaponMountData.rotateToWayPointBeforeTicking)
            {
                //Increses rotation Timer
                rotationTimer += Time.deltaTime;

                //checks if the rotation timer is more than the sum of the intervals of the current index. If so, continously rotates to new waypoints.
                if (rotationDestReached)
                {
                    AcquireTarget(rotationIndex);
                }

                if (rotationTimer > Sum(weaponMountData.rotationIntervals, rotationIndex))
                {
                    if (RotateToTarget())
                    {
                        rotationIndex++;
                        if (rotationIndex >= weaponMountData.rotationIntervals.Count)
                        {
                            rotationIndex = 0;
                            rotationTimer = 0.0f;
                        }
                    }
                }
            }
            else
            {
                if (rotationDestReached)
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
                        rotationDestReached = false;
                    }
                }
                else
                {
                    RotateToTarget();
                }
            }
        }
    }

    float Sum(List<float> intervals, int index)
    {
        float sum = 0;
        for (int i = 0; i <= index; i++)
        {
            sum += intervals[i];
        }
        return sum;
    }

    void AcquireTarget(int index)
    {
        targetAngle = weaponMountData.rotationWayPoints[index];
        if (weaponMountData.clockwise[index])
        {
            targetAngle = targetAngle > currentAngle ? targetAngle - 360 : targetAngle;
        }
        else
        {
            targetAngle = targetAngle < currentAngle ? targetAngle + 360 : targetAngle;
        }
        rotationDestReached = false;

    }
    bool RotateToTarget()
    {
        Quaternion rot = transform.rotation;

        float nextAngleDelta = weaponMountData.clockwise[rotationIndex] ? weaponMountData.rotationSpeed * -1 : weaponMountData.rotationSpeed;
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
            rotationDestReached = true;
            if (currentAngle >= 360)
            {
                currentAngle -= 360;
            }
            if (currentAngle <= 0)
            {
                currentAngle += 360;
            }
            return true;
        }

        gameObject.transform.rotation = Quaternion.Euler(new Vector3(rot.x, rot.y, currentAngle));
        return false;
    }

    void standardFireUpdate()
    {
        if (fireTimer <= weaponData.fireRate)
        {
            fireTimer += Time.deltaTime;
        }
        else
        {
            fireTimer -= weaponData.fireRate + Random.Range(-weaponData.fireRateVariance, weaponData.fireRateVariance);
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
            fireTimer -= weaponData.fireRate + Random.Range(-weaponData.fireRateVariance, weaponData.fireRateVariance);
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

    void specialAmmoFireUpdate()
    {
        specialAmmoFireTimer += Time.deltaTime;

        if (specialAmmoFireTimer > weaponData.fireRate)
        {
            specialAmmoFireTimer -= weaponData.fireRate + Random.Range(-weaponData.fireRateVariance, weaponData.fireRateVariance);
            Fire(weaponData);
            if (weaponMountData.fireEntirePatternWithAmmo)
            {
                CycleWeaponPattern(false);
            }
            currentAmmo--;
            if (currentAmmo <= 0)
            {
                fireIndex++;
                if (fireIndex > weaponMountData.fireIntervals.Count - 1)
                {
                    fireIndex = 0;
                }
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

    public void Fire(WeaponData weaponData)
    {
        switch (weaponData.weaponClass)
        {
            case WEAPON_TYPE.SPREADSHOT:
                SpreadFire(weaponData);
                break;
            case WEAPON_TYPE.PARALLELSHOT:
                if (weaponData is ParallelWeaponData)
                {
                    ParallelFire((ParallelWeaponData)weaponData);
                }
                break;

        }
    }

    void SpawnNewBullet()
    {

        GameObject newBulletObject = null;
        foreach (EnemyBullet b in GameManager.instance.currentBoss.inactiveEnemyBullets)
        {
            if (b.name.Equals(weaponData.name))
            {
                newBulletObject = b.gameObject;
                break;
            }
        }
        if (newBulletObject == null)
        {
            newBulletObject = Instantiate(weaponData.bulletPrefab, transform.position, Quaternion.identity);
        }

        newBulletObject.transform.localScale = weaponData.bulletSize == 0 ? new Vector3(0.1f, 0.1f, 0f) : new Vector3(weaponData.bulletSize, weaponData.bulletSize, 0);

        EnemyBullet bulletScript = newBulletObject.GetComponent<EnemyBullet>();
        bulletScript.init(weaponData, this, currentAngle, weaponData.bulletLifetime);

        RemoveBulletFromLists(bulletScript);
        GameManager.instance.currentBoss.activeEnemyBullets.Add(bulletScript);
    }

    void SpawnNewBullet(float pos, float angle)
    {
        GameObject newBulletObject = null;
        foreach (EnemyBullet b in GameManager.instance.currentBoss.inactiveEnemyBullets)
        {
            if (b.name.Equals(weaponData.name))
            {
                newBulletObject = b.gameObject;
                break;
            }
        }
        if (newBulletObject == null)
        {
            newBulletObject = Instantiate(weaponData.bulletPrefab, transform.position, Quaternion.identity);
        }

        //create and instantiate new bullet
        newBulletObject.transform.localScale = weaponData.bulletSize == 0 ? new Vector3(0.1f, 0.1f, 0f) : new Vector3(weaponData.bulletSize, weaponData.bulletSize, 0);

        EnemyBullet bulletScript = newBulletObject.GetComponent<EnemyBullet>();
        bulletScript.init(weaponData, this, angle, pos, weaponData.bulletLifetime);

        RemoveBulletFromLists(bulletScript);
        GameManager.instance.currentBoss.activeEnemyBullets.Add(bulletScript);
    }

    private void SpreadFire(WeaponData weaponData)
    {

        if (weaponData.numBulletsPerShot == 1)
        {
            SpawnNewBullet(0, currentAngle + Random.Range(-weaponData.bulletSpreadVariance, weaponData.bulletSpreadVariance));
        }
        else if (weaponData.customBulletSpread.Count == 0)
        {
            //Sets initial to 0 if odd and 1 if even
            int initial = 1 - (weaponData.numBulletsPerShot % 2);
            for (int s = initial; s < weaponData.numBulletsPerShot + initial; s++)
            {
                float angleOffSet =
                    ((((float)s / (weaponData.numBulletsPerShot + (1 - (2 * (weaponData.numBulletsPerShot % 2))))) * weaponData.bulletSpread) -
                     (weaponData.bulletSpread / 2));
                SpawnNewBullet(0, currentAngle + angleOffSet + Random.Range(-weaponData.bulletSpreadVariance, weaponData.bulletSpreadVariance));
            }
        }
        else
        {
            for (int s = 0; s < weaponData.numBulletsPerShot; s++)
            {
                SpawnNewBullet(0, weaponData.dontRespectTrajectory ? currentAngle + weaponData.customBulletSpread[s] + Random.Range(-weaponData.bulletSpreadVariance, weaponData.bulletSpreadVariance) : weaponData.customBulletSpread[s] + Random.Range(-weaponData.bulletSpreadVariance, weaponData.bulletSpreadVariance));
            }
        }
    }

    private void ParallelFire(ParallelWeaponData weaponData)
    {
        if (weaponData.numBulletsPerShot == 1)
        {
            SpawnNewBullet();
        }
        else
        {
            //Sets initial to 0 if odd and 1 if even
            int initial = 1 - (weaponData.numBulletsPerShot % 2);
            for (int s = initial; s < weaponData.numBulletsPerShot + initial; s++)
            {
                float posOffset =
                    ((((float)s / (weaponData.numBulletsPerShot + (1 - (2 * (weaponData.numBulletsPerShot % 2))))) * weaponData.bulletSpacing) -
                     (weaponData.bulletSpacing / 2));
                SpawnNewBullet(posOffset, currentAngle);
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

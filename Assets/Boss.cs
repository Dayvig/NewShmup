using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Boss : MonoBehaviour
{

    public List<EnemyBullet> activeEnemyBullets = new List<EnemyBullet>();
    public List<EnemyBullet> inactiveEnemyBullets = new List<EnemyBullet>();
    public List<EnemyBullet> markedForDeathEnemyBullets = new List<EnemyBullet>();



    public List<WeaponRack> weaponRacks = new List<WeaponRack>();

    public float bossTimer = 0.0f;
    public float marker = 0.0f;
    public int bossAdvancementStage = 0;
    public BossAdvancementData bossAdvancementData;

    [Serializable]public class WeaponRack
    {
        public List<WeaponMountData> mountedWeapons = new List<WeaponMountData>();
        public List<Vector3> mountedWeaponPositions = new List<Vector3>();
        public List<GameObject> weaponPrefabs = new List<GameObject>();
        public List<GameObject> weaponObjects = new List<GameObject>();
        public bool active;

        public WeaponRack(List<WeaponMountData> mountedWeapons, List<Vector3> mountedWeaponPositions)
        {
            this.mountedWeapons = mountedWeapons;
            this.mountedWeaponPositions = mountedWeaponPositions;
        }

        public void Close()
        {
            for (int i = 0; i < weaponObjects.Count; i++)
            {
                weaponObjects[i].SetActive(false);
                weaponObjects[i].GetComponent<WeaponBehavior>().CloseWeapon();
            }
        }

        public void Open()
        {
            for (int i = 0; i < weaponObjects.Count; i++)
            {
                weaponObjects[i].SetActive(true);
                weaponObjects[i].transform.localPosition = mountedWeaponPositions[i];
                weaponObjects[i].GetComponent<WeaponBehavior>().OpenWeapon();
            }
        }
    }

    private void Start()
    {
        InstantiateWeapons();
        bossTimer = 0.0f;
        marker = 0.0f;
    }

    void InstantiateWeapons()
    {
        foreach (WeaponRack rack in weaponRacks)
        {
            for (int i = 0; i < rack.mountedWeapons.Count; i++)
            {
                rack.mountedWeapons[i].initialPosition = rack.mountedWeaponPositions[i];
                GameObject newWeaponObject = Instantiate(rack.weaponPrefabs[i], gameObject.transform);
                rack.weaponObjects.Add(newWeaponObject);
                newWeaponObject.transform.localPosition = rack.mountedWeaponPositions[i];
                WeaponBehavior wb = newWeaponObject.GetComponent<WeaponBehavior>();
                wb.weaponMountData = rack.mountedWeapons[i];
                wb.firingPattern = rack.mountedWeapons[i].weaponsOnMount;
            }
            rack.Close();
        }
    }

    private void FixedUpdate()
    {
        bossTimer += Time.deltaTime;
        if (bossTimer > bossAdvancementData.timeIntervals[bossAdvancementStage] && marker < bossTimer)
        {
            foreach (int s in bossAdvancementData.weaponParams[bossAdvancementStage].openWeapons)
            {
                if (s <= weaponRacks.Count)
                {
                    weaponRacks[s].Open();
                }
            }
            foreach (int s in bossAdvancementData.weaponParams[bossAdvancementStage].closedWeapons)
            {
                if (s <= weaponRacks.Count)
                {
                    weaponRacks[s].Close();
                }
            }
            marker = bossTimer;
            if (bossAdvancementStage < bossAdvancementData.timeIntervals.Count)
            {
                bossAdvancementStage++;
            }
        }
    }

    private void WipeAllEnemiesAndBullets()
    {

        foreach (EnemyBullet b in activeEnemyBullets)
        {
            b.Die();
        }
        Trash();
    }


    void DisposeAllBullets()
    {
        foreach (EnemyBullet b in activeEnemyBullets)
        {
            markedForDeathEnemyBullets.Add(b);
        }
    }



    void Trash()
    {
        foreach (EnemyBullet ded in markedForDeathEnemyBullets)
        {
            activeEnemyBullets.Remove(ded);
            inactiveEnemyBullets.Add(ded);
            ded.thisCollider.enabled = false;
            ded.gameObject.SetActive(false);
        }

        markedForDeathEnemyBullets.Clear();
    }


    public void BossUpdate()
    {
        foreach (EnemyBullet bullet in activeEnemyBullets)
        {
            bullet.BulletUpdate();
        }
        Trash();
    }

}

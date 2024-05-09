using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Boss : MonoBehaviour
{

    [HideInInspector]
    public List<EnemyBullet> activeEnemyBullets = new List<EnemyBullet>();
    [HideInInspector]
    public List<EnemyBullet> inactiveEnemyBullets = new List<EnemyBullet>();
    [HideInInspector] 
    public List<EnemyBullet> markedForDeathEnemyBullets = new List<EnemyBullet>();


    public List<WeaponRack> weaponRacks = new List<WeaponRack>();
    public List<WeaponRack> activeRacks = new List<WeaponRack>();
    public List<WeaponRack> inactiveRacks = new List<WeaponRack>();

    public float bossTimer = 0.0f;
    public float marker = 0.0f;
    public int bossAdvancementStage = 0;
    public BossAdvancementData scriptedAdvancementData;
    public BossAdvancementData randomAdvancementData;

    public bool randomRackAdvancement = false;

    [Serializable]public class WeaponRack
    {
        public List<WeaponMountData> mountedWeapons = new List<WeaponMountData>();
        public List<Vector3> mountedWeaponPositions = new List<Vector3>();
        public List<GameObject> weaponPrefabs = new List<GameObject>();
        public List<GameObject> weaponObjects = new List<GameObject>();
        public List<WeaponBehavior> weaponBehaviors = new List<WeaponBehavior>();

        public bool active;
        public float lifeTime;

        public WeaponRack(List<WeaponMountData> mountedWeapons, List<Vector3> mountedWeaponPositions)
        {
            this.mountedWeapons = mountedWeapons;
            this.mountedWeaponPositions = mountedWeaponPositions;
            active = false;
        }

        public void Close()
        {
            SetWeapons(false);
            AddToRackCollection(GameManager.instance.currentBoss.inactiveRacks);
        }

        public void SetWeapons(bool state)
        {
            for (int i = 0; i < weaponObjects.Count; i++)
            {
                weaponObjects[i].SetActive(state);
                if (state)
                {
                    weaponBehaviors[i].OpenWeapon();
                }
                else
                {
                    weaponBehaviors[i].CloseWeapon();
                }
                active = state;
            }
        }

        public void AddToRackCollection(List<WeaponRack> target)
        {
            List<WeaponRack> opposite = (target.Equals(GameManager.instance.currentBoss.activeRacks) ? GameManager.instance.currentBoss.inactiveRacks : GameManager.instance.currentBoss.activeRacks);
            if (!target.Contains(this))
            {
                target.Add(this);
            }
            if (opposite.Contains(this))
            {
                opposite.Remove(this);
            }

        }

        public void Open()
        {
            SetWeapons(true);
            AddToRackCollection(GameManager.instance.currentBoss.activeRacks);
            lifeTime = 0.0f;
        }
    }

    private void Start()
    {
        InstantiateWeapons();
        GameManager.instance.bossHP = 8000;
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
                rack.weaponBehaviors.Add(wb);
                wb.weaponMountData = rack.mountedWeapons[i];
                wb.firingPattern = rack.mountedWeapons[i].weaponsOnMount;
            }
            rack.Close();
            inactiveRacks.Add(rack);
        }
    }

    public void BossFixedUpdate()
    {
        if (randomRackAdvancement)
        {
            bossTimer += Time.deltaTime;

            switch (bossAdvancementStage)
            {
                case 0:
                    if (bossTimer > randomAdvancementData.timeIntervals[0])
                    {
                        bossAdvancementStage = 1;
                        WeaponRack newRack = inactiveRacks[UnityEngine.Random.Range(0, inactiveRacks.Count)];
                        newRack.Open();
                    }
                    break;
                case 1:
                    if (bossTimer > randomAdvancementData.timeIntervals[1])
                    {
                        bossAdvancementStage = 2;
                        WeaponRack newRack = inactiveRacks[UnityEngine.Random.Range(0, inactiveRacks.Count)];
                        newRack.Open();
                    }
                    break;
                case 2:
                    if (bossTimer > randomAdvancementData.timeIntervals[2])
                    {
                        bossAdvancementStage = 3;
                    }
                    break;
                case 3:
                    if (bossTimer > randomAdvancementData.timeIntervals[3])
                    {
                        bossAdvancementStage = 4;
                        if (inactiveRacks.Count != 0)
                        {
                            WeaponRack newRack = inactiveRacks[UnityEngine.Random.Range(0, inactiveRacks.Count)];
                            newRack.Open();
                        }
                    }
                    break;
                case 4:
                    if (bossTimer > randomAdvancementData.timeIntervals[4])
                    {
                        bossAdvancementStage = 5;
                        if (inactiveRacks.Count != 0)
                        {
                            WeaponRack newRack = inactiveRacks[UnityEngine.Random.Range(0, inactiveRacks.Count)];
                            newRack.Open();
                        }
                    }

                    break;
            }
            for (int i = 0; i < activeRacks.Count; i++)
            {
                activeRacks[i].lifeTime += Time.deltaTime;
                //add variable
                if (activeRacks[i].lifeTime > randomAdvancementData.weaponUptime && activeRacks[i].active)
                {
                    activeRacks[i].SetWeapons(false);
                }
                if (activeRacks[i].lifeTime > (randomAdvancementData.weaponUptime + randomAdvancementData.weaponGracePeriod))
                {
                    activeRacks[i].AddToRackCollection(inactiveRacks);
                    if (inactiveRacks.Count != 0)
                    {
                        WeaponRack newRack = inactiveRacks[UnityEngine.Random.Range(0, inactiveRacks.Count)];
                        newRack.Open();
                    }
                }
            }
        }
        else
        {
            if (bossAdvancementStage < scriptedAdvancementData.timeIntervals.Count)
            {
                bossTimer += Time.deltaTime;
                if (bossTimer > scriptedAdvancementData.timeIntervals[bossAdvancementStage] && marker < bossTimer)
                {
                    foreach (int s in scriptedAdvancementData.weaponParams[bossAdvancementStage].openWeapons)
                    {
                        if (s <= weaponRacks.Count)
                        {
                            weaponRacks[s].Open();
                        }
                    }
                    foreach (int s in scriptedAdvancementData.weaponParams[bossAdvancementStage].closedWeapons)
                    {
                        if (s <= weaponRacks.Count)
                        {
                            weaponRacks[s].Close();
                        }
                    }
                    marker = bossTimer;
                    if (bossAdvancementStage < scriptedAdvancementData.timeIntervals.Count)
                    {
                        bossAdvancementStage++;
                    }
                }
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
        GameManager.instance.BossHPBar.sizeDelta = new Vector2((425 * (GameManager.instance.bossHP / 8000)), GameManager.instance.BossHPBar.sizeDelta.y);
        Trash();
    }

}

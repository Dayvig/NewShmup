using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/WeaponData", order = 1)]
public class WeaponData : ScriptableObject
{
    [SerializeField] public GameObject bulletPrefab;

    [SerializeField] public string weaponName;
    [SerializeField] public string weaponDescription;
    [SerializeField] public int weaponID;

    [SerializeField] public WEAPON_TYPE weaponClass;

    [SerializeField] public float bulletSpeed;
    [SerializeField] public float fireRate;
    [SerializeField] public float fireRateVariance;
    [SerializeField] public int ammo;
    [SerializeField] public float reloadInterval;
    [SerializeField] public float bulletSpread;
    [SerializeField] public float bulletSpreadVariance;
    [SerializeField] public List<float> customBulletSpread;
    [SerializeField] public int numBulletsPerShot;
    [SerializeField] public float bulletSize;
    [SerializeField] public float bulletLifetime;
    [SerializeField] public float lifetimeVariance;
    [SerializeField] public bool dontRespectTrajectory;

}

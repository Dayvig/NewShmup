using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "WeaponData/ParallelWeaponData", order = 1)]
public class ParallelWeaponData : WeaponData
{
    [SerializeField] public float bulletSpacing;
    [SerializeField] public List<float> customBulletSpacing;


}

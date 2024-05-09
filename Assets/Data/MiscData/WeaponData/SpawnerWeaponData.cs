using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "WeaponData/SpawnerWeaponData", order = 1)]
public class SpawnerWeaponData : WeaponData
{
    [SerializeField] public WeaponMountData sharpnel;
    [SerializeField] public bool spawnOnDestruction;



}

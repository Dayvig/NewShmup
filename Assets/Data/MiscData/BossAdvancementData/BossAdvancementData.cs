using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/BossAdvancementData", order = 1)]
public class BossAdvancementData : ScriptableObject
{
    [SerializeField] public List<float> timeIntervals;
    [SerializeField] public float weaponUptime;
    [SerializeField] public float weaponGracePeriod;

    [SerializeField] public List<WeaponOpenParams> weaponParams;

}


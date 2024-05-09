using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/WeaponOpenParams", order = 1)]
[Serializable]public class WeaponOpenParams : ScriptableObject
{
    public List<int> openWeapons = new List<int>();
    public List<int> closedWeapons = new List<int>();
}

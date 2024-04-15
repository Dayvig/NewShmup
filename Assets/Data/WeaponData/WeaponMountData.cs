using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/WeaponMountData", order = 1)]
public class WeaponMountData : ScriptableObject
{
    public enum WeaponMountType
    {
        PLAYERAIMEDMOUNT,
        ROTATINGMOUNT,
        STATIONARYMOUNT
    }


    [SerializeField] public WeaponMountType mountType;

    [TextArea(5, 10)] public string d1 = "Values for rotating mount. Rotates between each rotationWayPoint at rotationSpeed, moving to the next waypoint at that current rotationInterval";
    [SerializeField] public List<bool> clockwise;
    [SerializeField] public List<float> rotationWayPoints;
    [SerializeField] public float rotationSpeed;
    [SerializeField] public List<float> rotationIntervals;

    [TextArea(5, 10)] public string d2 = "Values for player aimed mount. Aims at the player. Reads the player's position every targetAcquisitionInterval, moving at targetAcquisitionSpeed to aim at it.";
    [SerializeField] public float targetAcquisitionSpeed;
    [SerializeField] public float targetAcquisitionInterval;

    [TextArea(5, 10)] public string d3 = "Movement values for moving mount. Moves to movementwaypoint at movementspeed, pausing for movementInterval after every movement.";
    [SerializeField] public float movementSpeed;
    [SerializeField] public float movementInterval;
    [SerializeField] public List<Vector3> movementWayPoints;

    [SerializeField] public Vector3 initialPosition;
    [SerializeField] public float initialFiringAngle;

    [SerializeField] public List<WeaponData> weaponsOnMount;
    [SerializeField] public bool fireEntirePatternWithAmmo;

    //add weapon upgrade module
}


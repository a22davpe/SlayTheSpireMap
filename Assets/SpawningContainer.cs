using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;

[CreateAssetMenu]
[Serializable]
public class SpawningContainer : ScriptableObject
{
    public Slot[] Slots;

    public FloorInfo[] constantFloors;

    public FloorInfo[] notPremitted;

    public MapSlot GetMapSlot(int2 index){

        if(TryGetConstantFloor(constantFloors, index.y, out MapSlot mapSlot))
            return mapSlot;

        return RandomSlot(index.y);
    }


    public MapSlot RandomSlot(int floorLevel){

        Slot[] availableSlots = GetAvailableMapSlots(floorLevel);
        float randomValue = UnityEngine.Random.Range(0, TotalOdds(availableSlots));

        for (int i = 0; i < availableSlots.Length; i++)
        {
            randomValue -= availableSlots[i].spawnOdds;

            if(randomValue <= 0)
                return availableSlots[i].slot;
        }

        Debug.LogError("No mapslot found!");

        return new MapSlot();
    }

    float TotalOdds(Slot[] slots) => slots.Sum(t => t.spawnOdds);

    Slot[] GetAvailableMapSlots(int floorLevel){

        List<Slot> temp = new List<Slot>();

        for (int i = 0; i < Slots.Length; i++)
        {
            if(Slots[i].unlockLevel <= floorLevel && MapSlotIsPremiited(Slots[i].slot, notPremitted, floorLevel))
                temp.Add(Slots[i]);
        }

        if(temp.Count == 0)
            Debug.LogError("No available slots");

        return temp.ToArray();
    }

        bool TryGetConstantFloor(FloorInfo[] floorInfo,int floorLevel,out MapSlot mapSlot){

        for (int i = 0; i < floorInfo.Length; i++)
        {
            if(floorInfo[i].floorLevel == floorLevel)
            {
                mapSlot = floorInfo[i].mapSlot;
                return true;
            }
        }

        mapSlot = null;
        return false;

    }

    bool MapSlotIsPremiited( MapSlot mapSlot,FloorInfo[] nonPremittedList, int floorLevel){
        for (int i = 0; i < nonPremittedList.Length; i++)
        {
            if(nonPremittedList[i].mapSlot == mapSlot && nonPremittedList[i].floorLevel == floorLevel)
            return false;
        }

        return true;
    }
}

[System.Serializable]

public class FloorInfo{
    public int floorLevel = 0;
    public MapSlot mapSlot;
}

[System.Serializable]
public class Slot{

public float spawnOdds = 1;

public int unlockLevel = 0;

public MapSlot slot;

}

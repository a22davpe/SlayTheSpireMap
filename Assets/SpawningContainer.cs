using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu]
[Serializable]
public class SpawningContainer : ScriptableObject
{
    public Slot[] Slots;

    public MapSlot GetMapSlot(){
        return RandomSlot();
    }



    public MapSlot RandomSlot(){

        //Get slots
        float tempOdds =  Slots.Sum(t => t.spawnOdds);

        for (int i = 0; i < Slots.Length; i++)
        {
            tempOdds -= Slots[i].spawnOdds;

            if(tempOdds >= 0)
                return Slots[i].slots;
        }

        Debug.LogError("No mapslot found!");

        return new MapSlot();
    }

    float GetTotalOdds() => Slots.Sum(t => t.spawnOdds);
}

[System.Serializable]
public class Slot{

public float spawnOdds = 1;
public MapSlot slots;

}

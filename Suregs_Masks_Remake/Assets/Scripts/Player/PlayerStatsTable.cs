using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerStatsTable
{
    public static int GetWeaponDamage(int level)
    {
        return level switch
        {
            1 => 100,
            2 => 120,
            3 => 140,
            4 => 170,
            5 => 200,
            6 => 240,
            7 => 280,
            8 => 350,
            9 => 420,
            10 => 500,
            _ => 100
        };
    }

    public static int GetArmorHealth(int level)
    {
        return level switch
        {
            1 => 100,
            2 => 120,
            3 => 150,
            4 => 190,
            5 => 250,
            6 => 280,
            7 => 350,
            8 => 400,
            9 => 480,
            10 => 600,
            _ => 100
        };
    }
}

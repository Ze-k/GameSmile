using System;

public class Stats 
{
    public int Health;
    public int Mana;

    public void _Ready(int MaxHealth, int MaxMana)
    {
        Health = MaxHealth;
        Mana = MaxMana;
    }
}
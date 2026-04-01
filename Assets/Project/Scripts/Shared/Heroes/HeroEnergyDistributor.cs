using System;
using Project.Scripts.Shared.Tiles;

namespace Project.Scripts.Shared.Heroes
{
    public static class HeroEnergyDistributor
    {
        public static void Distribute(HeroSlotState[] slots, TileKind kind, int amount)
        {
            if (amount <= 0)
                return;

            for (var i = 0; i < slots.Length; i++)
            {
                ref var slot = ref slots[i];

                if (false == slot.IsAssigned || slot.Kind != kind)
                    continue;

                slot.CurrentEnergy = Math.Min(slot.MaxEnergy, slot.CurrentEnergy + amount);
            }
        }
    }
}
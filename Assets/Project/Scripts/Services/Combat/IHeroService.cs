using System.Collections.Generic;
using Project.Scripts.Configs;
using Project.Scripts.Shared.Heroes;

namespace Project.Scripts.Services.Combat
{
    public interface IHeroService
    {
        IReadOnlyList<HeroSlotState> GetSlots(BattleSide side);
        void TryActivate(int slotIndex);
        void TryActivate(BattleSide side, int slotIndex);
        void AssignEnemyHeroes(HeroConfig[] heroes);
        void AddEnemyHeroEnergy(int slotIndex, int amount);
    }
}
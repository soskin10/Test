using System.Collections.Generic;
using Project.Scripts.Configs;
using Project.Scripts.Configs.Battle;
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
        bool TryDischargeHero(BattleSide side, int slotIndex, out HeroActionType actionType, out int actionValue);
        void ApplyDamageToHero(BattleSide side, int slotIndex, int amount);
        void ApplyHealToHero(BattleSide side, int slotIndex, int amount);
    }
}
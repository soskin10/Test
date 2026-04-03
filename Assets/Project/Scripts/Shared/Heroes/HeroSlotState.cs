using Project.Scripts.Shared.Tiles;

namespace Project.Scripts.Shared.Heroes
{
    public struct HeroSlotState
    {
        public bool IsAssigned;
        public TileKind Kind;
        public int CurrentEnergy;
        public int MaxEnergy;
        public HeroActionType ActionType;
        public int ActionValue;
        public int CurrentHP;
        public int MaxHP;

        //MaxHP == 0 - hero is immortal.
        public bool IsAlive => MaxHP <= 0 || CurrentHP > 0;

        public bool IsReady => IsAssigned && IsAlive && MaxEnergy > 0 && CurrentEnergy >= MaxEnergy;
    }
}
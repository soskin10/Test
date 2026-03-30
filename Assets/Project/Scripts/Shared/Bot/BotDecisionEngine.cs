using System.Collections.Generic;
using Project.Scripts.Shared.Heroes;

namespace Project.Scripts.Shared.Bot
{
    public sealed class BotDecisionEngine
    {
        private readonly BotSettings _settings;
        private readonly System.Random _rng;


        public BotDecisionEngine(BotSettings settings, int seed)
        {
            _settings = settings;
            _rng = new System.Random(seed);
        }


        public float NextAttackDelay()
        {
            var range = _settings.MaxAttackInterval - _settings.MinAttackInterval;
            return (float)(_rng.NextDouble() * range + _settings.MinAttackInterval);
        }

        public int GenerateAttackDamage()
        {
            return _rng.Next(_settings.MinAttackDamage, _settings.MaxAttackDamage + 1);
        }

        public float GenerateHeroActivationDelay(float min, float max)
        {
            return (float)(_rng.NextDouble() * (max - min) + min);
        }

        public int PickRandomAssignedSlot(IReadOnlyList<HeroSlotState> slots)
        {
            var assignedCount = 0;
            for (var i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsAssigned)
                    assignedCount++;
            }

            if (assignedCount == 0)
                return -1;

            var pick = _rng.Next(assignedCount);
            var count = 0;
            for (var i = 0; i < slots.Count; i++)
            {
                if (false == slots[i].IsAssigned)
                    continue;
                if (count == pick)
                    return i;
                count++;
            }

            return -1;
        }
    }
}
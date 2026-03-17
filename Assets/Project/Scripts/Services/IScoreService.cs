using System.Collections.Generic;
using R3;

namespace Project.Scripts.Services
{
    public interface IScoreService
    {
        ReadOnlyReactiveProperty<int> Score { get; }

        void AddMatchScore(List<MatchResult> matches, int cascadeLevel);
        void AddBombScore(int tilesDestroyed);
        void Reset();
    }
}

using Project.Scripts.Shared.Heroes;
using UnityEngine;

namespace Project.Scripts.Gameplay.UI
{
    public interface ITargetableView
    {
        UnitDescriptor Descriptor { get; }
        RectTransform HitArea { get; }
        bool IsReadySource { get; }
        bool IsValidTarget(UnitDescriptor source);
        void SetSourceHighlight(bool active);
        void SetTargetHighlight(bool active, HeroActionType actionType);
    }
}
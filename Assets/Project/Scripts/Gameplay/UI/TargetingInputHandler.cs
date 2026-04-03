using Project.Scripts.Services.Combat;
using Project.Scripts.Shared.Heroes;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Project.Scripts.Gameplay.UI
{
    public class TargetingInputHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private const float OffsetPx = 20f;

        
        private TargetingRegistry _registry;
        private IAbilityExecutionService _abilityExecution;
        private ITargetableView _source;
        private ITargetableView _target;


        public void Init(TargetingRegistry registry, IAbilityExecutionService abilityExecution)
        {
            _registry = registry;
            _abilityExecution = abilityExecution;
        }

        public void OnPointerDown(PointerEventData e)
        {
            if (_registry == null)
                return;

            var unit = _registry.FindAtPosition(e.position, OffsetPx);
            if (unit == null || !unit.IsReadySource || unit.Descriptor.Side != BattleSide.Player)
                return;

            _source = unit;
            _source.SetSourceHighlight(true);
        }

        public void OnDrag(PointerEventData e)
        {
            if (_source == null)
                return;

            var candidate = _registry.FindAtPosition(e.position, OffsetPx);
            var valid = candidate != null && candidate != _source && candidate.IsValidTarget(_source.Descriptor);

            if (_target != null && _target != candidate)
            {
                _target.SetTargetHighlight(false, default);
                _target = null;
            }

            if (valid)
            {
                _target = candidate;
                _target.SetTargetHighlight(true, _source.Descriptor.ActionType);
            }
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (_source != null && _target != null)
                _abilityExecution.Execute(_source.Descriptor, _target.Descriptor);

            _source?.SetSourceHighlight(false);
            _target?.SetTargetHighlight(false, default);
            _source = null;
            _target = null;
        }
    }
}
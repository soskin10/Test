using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Gameplay.UI
{
    public class TargetingRegistry
    {
        private readonly List<ITargetableView> _units = new();
        private readonly Camera _camera;


        public TargetingRegistry(Camera camera = null)
        {
            _camera = camera;
        }


        public void Register(ITargetableView view)
        {
            _units.Add(view);
        }

        public void Unregister(ITargetableView view)
        {
            _units.Remove(view);
        }

        public ITargetableView FindAtPosition(Vector2 screenPos, float offsetPx)
        {
            for (var i = 0; i < _units.Count; i++)
            {
                var unit = _units[i];
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    unit.HitArea, screenPos, _camera, out var local);

                var r = unit.HitArea.rect;
                var expanded = new Rect(r.x - offsetPx, r.y - offsetPx,
                    r.width + offsetPx * 2, r.height + offsetPx * 2);

                if (expanded.Contains(local))
                    return unit;
            }

            return null;
        }

        public void ClearAll()
        {
            for (var i = 0; i < _units.Count; i++)
            {
                var u = _units[i];
                u.SetSourceHighlight(false);
                u.SetTargetHighlight(false, default);
            }
        }
    }
}
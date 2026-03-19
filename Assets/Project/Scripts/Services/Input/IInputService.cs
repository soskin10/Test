using System;
using UnityEngine;

namespace Project.Scripts.Services.Input
{
    public interface IInputService
    {
        event Action<Vector2> OnDragStarted;
        event Action<Vector2> OnDragDelta;
        event Action OnDragCanceled;
    }
}
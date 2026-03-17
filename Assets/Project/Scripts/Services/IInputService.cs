using System;
using Project.Scripts.Services.ServiceLocatorSystem;
using UnityEngine;

namespace Project.Scripts.Services
{
    public interface IInputService : IService
    {
        event Action<Vector2> OnDragStarted;
        event Action<Vector2> OnDragDelta;
        event Action OnDragCanceled;
    }
}
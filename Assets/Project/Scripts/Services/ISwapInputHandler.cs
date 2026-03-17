using System;
using Project.Scripts.Services.ServiceLocatorSystem;

namespace Project.Scripts.Services
{
    public interface ISwapInputHandler : IService
    {
        event Action<SwapRequest> OnSwapRequested;
    }
}
using System;

namespace Project.Scripts.Services.Input
{
    public interface ISwapInputHandler
    {
        event Action<SwapRequest> OnSwapRequested;
    }
}
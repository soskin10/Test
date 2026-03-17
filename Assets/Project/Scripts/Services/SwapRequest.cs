using UnityEngine;

namespace Project.Scripts.Services
{
    public readonly struct SwapRequest
    {
        public readonly Vector2Int From;
        public readonly Vector2Int To;
        

        public SwapRequest(Vector2Int from, Vector2Int to)
        {
            From = from;
            To = to;
        }
    }
}
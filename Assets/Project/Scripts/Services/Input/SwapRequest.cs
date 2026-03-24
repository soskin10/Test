using Project.Scripts.Shared;

namespace Project.Scripts.Services.Input
{
    public readonly struct SwapRequest
    {
        public readonly GridPoint From;
        public readonly GridPoint To;
        public readonly GridPoint PivotPosition;


        public SwapRequest(GridPoint from, GridPoint to)
        {
            From = from;
            To = to;
            PivotPosition = to;
        }
    }
}
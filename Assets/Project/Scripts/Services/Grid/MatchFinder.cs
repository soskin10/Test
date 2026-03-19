using System.Collections.Generic;
using Project.Scripts.Tiles;
using UnityEngine;

namespace Project.Scripts.Services.Grid
{
    public class MatchFinder : IMatchFinder
    {
        private readonly int _minLength;

        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.up, Vector2Int.down,
            Vector2Int.left, Vector2Int.right
        };


        public MatchFinder(int minLength)
        {
            _minLength = minLength;
        }

        public List<MatchResult> FindMatches(TileType[,] grid)
        {
            var width = grid.GetLength(0);
            var height = grid.GetLength(1);
            var runs = new List<Run>();

            for (var y = 0; y < height; y++)
            {
                var x = 0;
                while (x < width)
                {
                    var type = grid[x, y];
                    if (type == TileType.None)
                    {
                        x++; 
                        continue;
                    }

                    var start = x;
                    while (x < width && grid[x, y] == type) 
                        x++;
                    
                    var len = x - start;
                    if (len < _minLength) 
                        continue;

                    var set = new HashSet<Vector2Int>(len);
                    for (var i = start; i < x; i++)
                        set.Add(new Vector2Int(i, y));

                    runs.Add(new Run(set, len, hasH: true, hasV: false, type));
                }
            }

            for (var x = 0; x < width; x++)
            {
                var y = 0;
                while (y < height)
                {
                    var type = grid[x, y];
                    if (type == TileType.None) 
                    { 
                        y++; 
                        continue; 
                    }

                    var start = y;
                    while (y < height && grid[x, y] == type) 
                        y++;
                    
                    var len = y - start;
                    if (len < _minLength) 
                        continue;

                    var set = new HashSet<Vector2Int>(len);
                    for (var i = start; i < y; i++)
                        set.Add(new Vector2Int(x, i));

                    runs.Add(new Run(set, len, hasH: false, hasV: true, type));
                }
            }

            Merge(runs);
            
            return BuildResults(runs);
        }


        private static void Merge(List<Run> runs)
        {
            bool anyMerge;
            do
            {
                anyMerge = false;
                for (var i = 0; i < runs.Count && false == anyMerge; i++)
                {
                    for (var j = i + 1; j < runs.Count && false == anyMerge; j++)
                    {
                        if (false == runs[i].Pos.Overlaps(runs[j].Pos))
                            continue;

                        var merged = new HashSet<Vector2Int>(runs[i].Pos);
                        merged.UnionWith(runs[j].Pos);
                        runs[i] = new Run(
                            merged,
                            Mathf.Max(runs[i].Len, runs[j].Len),
                            runs[i].HasH || runs[j].HasH,
                            runs[i].HasV || runs[j].HasV,
                            runs[i].Type
                        );
                        runs.RemoveAt(j);
                        anyMerge = true;
                    }
                }
            }
            while (anyMerge);
        }

        private static List<MatchResult> BuildResults(List<Run> runs)
        {
            var results = new List<MatchResult>(runs.Count);
            for (var i = 0; i < runs.Count; i++)
            {
                var run = runs[i];
                results.Add(new MatchResult
                {
                    Positions = new List<Vector2Int>(run.Pos),
                    MaxLineLength = run.Len,
                    IsComplex = run.HasH && run.HasV,
                    TileType = run.Type,
                    Shape = ComputeShape(run.Pos, run.HasH, run.HasV),
                    Center = ComputeCenter(run.Pos)
                });
            }
            
            return results;
        }

        private static MatchShape ComputeShape(HashSet<Vector2Int> positions, bool hasH, bool hasV)
        {
            if (false == hasV) 
                return MatchShape.Horizontal;
            
            if (false == hasH) 
                return MatchShape.Vertical;

            foreach (var pos in positions)
            {
                var neighborCount = 0;
                for (var d = 0; d < Directions.Length; d++)
                    if (positions.Contains(pos + Directions[d]))
                        neighborCount++;

                if (neighborCount >= 3)
                    return MatchShape.TShape;
            }

            return MatchShape.LShape;
        }

        private static Vector2Int ComputeCenter(HashSet<Vector2Int> positions)
        {
            int sumX = 0, sumY = 0;
            foreach (var pos in positions)
            {
                sumX += pos.x;
                sumY += pos.y;
            }
            
            return new Vector2Int(
                Mathf.RoundToInt((float)sumX / positions.Count),
                Mathf.RoundToInt((float)sumY / positions.Count)
            );
        }


        private readonly struct Run
        {
            public readonly HashSet<Vector2Int> Pos;
            public readonly int Len;
            public readonly bool HasH;
            public readonly bool HasV;
            public readonly TileType Type;

            
            public Run(HashSet<Vector2Int> pos, int len, bool hasH, bool hasV, TileType type)
            {
                Pos = pos;
                Len = len;
                HasH = hasH;
                HasV = hasV;
                Type = type;
            }
        }
    }
}
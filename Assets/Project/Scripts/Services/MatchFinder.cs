using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Project.Scripts.Tiles;
using UnityEngine;

namespace Project.Scripts.Services
{
    public class MatchFinder : IMatchFinder
    {
        private readonly int _minLength;


        public MatchFinder(int minLength)
        {
            _minLength = minLength;
        }

        public UniTask InitAsync() => UniTask.CompletedTask;

        public List<MatchResult> FindMatches(TileType[,] grid)
        {
            var width = grid.GetLength(0);
            var height = grid.GetLength(1);
            var runs = new List<(HashSet<Vector2Int> pos, int len, bool hasH, bool hasV)>();

            for (int y = 0; y < height; y++)
            {
                int x = 0;
                while (x < width)
                {
                    var type = grid[x, y];
                    if (type == TileType.None)
                    {
                        x++; 
                        continue;
                    }
                    
                    int start = x;
                    while (x < width && grid[x, y] == type) x++;
                    int len = x - start;
                    if (len < _minLength) 
                        continue;
                    
                    var set = new HashSet<Vector2Int>(len);
                    for (int i = start; i < x; i++)
                        set.Add(new Vector2Int(i, y));
                    
                    runs.Add((set, len, true, false));
                }
            }

            for (int x = 0; x < width; x++)
            {
                int y = 0;
                while (y < height)
                {
                    var type = grid[x, y];
                    if (type == TileType.None) 
                    {
                        y++; 
                        continue; 
                    }
                    
                    int start = y;
                    while (y < height && grid[x, y] == type) 
                        y++;
                    
                    int len = y - start;
                    if (len < _minLength) 
                        continue;
                    
                    var set = new HashSet<Vector2Int>(len);
                    for (int i = start; i < y; i++)
                        set.Add(new Vector2Int(x, i));
                    
                    runs.Add((set, len, false, true));
                }
            }

            return Merge(runs);
        }


        private static List<MatchResult> Merge(List<(HashSet<Vector2Int> pos, int len, bool hasH, bool hasV)> runs)
        {
            bool anyMerge;
            do
            {
                anyMerge = false;
                for (int i = 0; i < runs.Count && false == anyMerge; i++)
                {
                    for (int j = i + 1; j < runs.Count && false == anyMerge; j++)
                    {
                        if (false == runs[i].pos.Overlaps(runs[j].pos))
                            continue;
                        
                        var merged = new HashSet<Vector2Int>(runs[i].pos);
                        merged.UnionWith(runs[j].pos);
                        runs[i] = (
                            merged,
                            Mathf.Max(runs[i].len, runs[j].len),
                            runs[i].hasH || runs[j].hasH,
                            runs[i].hasV || runs[j].hasV
                        );
                        runs.RemoveAt(j);
                        anyMerge = true;
                    }
                }
            }
            while (anyMerge);

            var results = new List<MatchResult>(runs.Count);
            for (int i = 0; i < runs.Count; i++)
                results.Add(new MatchResult
                {
                    Positions = new List<Vector2Int>(runs[i].pos),
                    MaxLineLength = runs[i].len,
                    IsComplex = runs[i].hasH && runs[i].hasV
                });
            
            return results;
        }
    }
}
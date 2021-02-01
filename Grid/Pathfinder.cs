using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace S333
{
    public interface IGridPathfinding
    {
        bool Walkable { get; }
    }

    public class Pathfinder<TCell> where TCell : IGridPathfinding
    {
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;


        private Grid<TCell> grid;

        private List<PathNode> openList;
        private List<PathNode> closedList;
        private List<PathNode> allNodesList;

        public Pathfinder(Grid<TCell> grid)
        {
            this.grid = grid;
        }

        public bool PathExists(int startX, int startY, int endX, int endY)
        {
            return FindPath(startX, startY, endX, endY) != null;
            // TODO: optimise this
        }

        public List <TCell> FindPath(int startX, int startY, int endX, int endY) 
        {
            allNodesList = new List<PathNode>();

            // wrap all cells in nodes
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    TCell c = grid.GetValue(x, y);
                    if (!c.Walkable) continue;
                    PathNode pn = new PathNode(x, y, c);
                    allNodesList.Add(pn);
                }
            }

            // Setup
            PathNode startNode = allNodesList.First((n) => n.X == startX && n.Y == startY);
            PathNode endNode = allNodesList.First((n) => n.X == endX && n.Y == endY);

            openList = new List<PathNode> { startNode };
            closedList = new List<PathNode>();

            startNode.G = 0;
            startNode.H = CalculateDistance(startNode, endNode);

            // Cycle
            while(openList.Count > 0)
            {
                PathNode currentNode = GetLowestFCostNode(openList);
                if(currentNode == endNode)
                {
                    // reached final goal
                    return CalculatePath(currentNode);
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                foreach (var neighbour in GetUsefullNeighbours(currentNode))
                {
                    int tentativeGCost = currentNode.G + CalculateDistance(currentNode, neighbour);
                    if(tentativeGCost < neighbour.G)
                    {
                        neighbour.PreviousNode = currentNode;
                        neighbour.G = tentativeGCost;
                        neighbour.H = CalculateDistance(neighbour, endNode);

                        if(!openList.Contains(neighbour))
                        {
                            openList.Add(neighbour);
                        }
                    }
                }
            }

            // out of nodes on open list
            return null;
        }

        private List<TCell> CalculatePath(PathNode end)
        {
            List<TCell> path = new List<TCell>();
            end.CalcPath(path);
            path.Reverse();
            return path;

        }

        private int CalculateDistance(PathNode a, PathNode b)
        {
            int xDistance = Mathf.Abs(a.X - b.X);
            int yDistance = Mathf.Abs(a.Y - b.Y);
            bool canMoveDiagonal = false;
            if(canMoveDiagonal)
            {
                int remaining = Mathf.Abs(xDistance - yDistance);
                return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
            }
            else
            {
                return (xDistance + yDistance) * MOVE_STRAIGHT_COST;
            }
        }

        private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
        {
            PathNode lowestFCostNode = pathNodeList[0];
            for (int i = 0; i < pathNodeList.Count; i++)
            {
                if(pathNodeList[i].F < lowestFCostNode.F)
                {
                    lowestFCostNode = pathNodeList[i];
                }
            }
            return lowestFCostNode;
        }

        private List<PathNode> GetUsefullNeighbours(PathNode currentNode)
        {
            var neighbours = allNodesList.Where((x) => x.IsNeighbour(currentNode) && !closedList.Contains(x)).ToList();
            return neighbours;
        }

        /// <summary>
        /// Wrapper for generic tile grid cells to be used as pathfinding nodes
        /// </summary>
        class PathNode
        {
            public TCell CellObject;
            
            public PathNode(int x, int y, TCell cellObject)
            {
                X = x;
                Y = y;
                CellObject = cellObject;
                G = int.MaxValue;
            }
            public int X;
            public int Y;
            public int G { get; set; }
            public int H { get; set; }
            public int F { get => G + H; }

            public PathNode PreviousNode;

            public void CalcPath(List<TCell> path)
            {
                path.Add(CellObject);

                if(PreviousNode != null)
                {
                    PreviousNode.CalcPath(path); 
                }
            }

            public bool IsNeighbour(PathNode other)
            {
                return
                    (X == other.X + 1 && Y == other.Y)      // right
                    || (X == other.X - 1 && Y == other.Y)   // left
                    || (X == other.X && Y == other.Y - 1)   // down
                    || (X == other.X && Y == other.Y + 1);  // up

            }
        }
    }
}


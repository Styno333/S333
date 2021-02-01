using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace S333
{
    public enum GridPivot
    {
        Center,
        LowerLeft
    }
    public enum GridDrawPlane
    {
        XY,
        XZ
    }

    public class Grid<TGridObject>
    {
        public class GridCellChangedEventArgs : EventArgs
        {
            public int X;
            public int Y;
            public TGridObject Value;
        }
        public static event UnityAction<GridCellChangedEventArgs> OnCellChanged;

        private Vector2Int _size;
        private float _cellSize;

        private TGridObject[,] _cells;

        public int Width => _size.x;
        public int Height => _size.y;

        private Vector3 _origin;

        private GridPivot _pivot;


        private GridDrawPlane _drawPlane;

        // temp
        Transform _drawContainer;


        public Grid (int sizeX, int sizeY, float cellSize, Vector3 origin, GridPivot pivot, GridDrawPlane drawPlane, bool debug = false)
        {
            _size = new Vector2Int(sizeX, sizeY);
            _cells = new TGridObject[sizeX, sizeY];
            _cellSize = cellSize;
            _origin = origin;
            _pivot = pivot;
            _drawPlane = drawPlane;

            if(debug)
                DrawDebug();
        }

        public Grid(int sizeX, int sizeY, float cellSize, Vector3 origin, GridPivot pivot, GridDrawPlane drawPlane, Func<int, int, TGridObject> creator, bool debug = false)
        {
            _size = new Vector2Int(sizeX, sizeY);
            _cells = new TGridObject[sizeX, sizeY];
            _cellSize = cellSize;
            _origin = origin;
            _pivot = pivot;
            _drawPlane = drawPlane;

            _drawContainer = new GameObject("Grid Container").transform;


            //// create objects
            for (int x = 0; x < _size.x; x++)
            {
                for (int y = 0; y < _size.y; y++)
                {
                    var go = creator(x, y);
                    _cells[x, y] = go;

                    if (go is IGridCellNeighbour goGCN)
                    {
                        ApplyNeighbour(goGCN, x, y);
                    }

                    if (go is IGridCellDrawer drawer)
                    {
                        var TileObject = GameObject.Instantiate(drawer.Prefab).transform;
                        TileObject.SetParent(_drawContainer);
                        TileObject.localPosition = Grid2World(x, y);
                    }

                    if(go is IGridCellWorldPos wp)
                    {
                        wp.SetWorldPos(Grid2World(x, y));
                    }
                }
            }

            if (debug)
                DrawDebug();
        }

        public void Populate(Func<int, int, TGridObject> creator)
        {
            _drawContainer = new GameObject("Grid Container").transform;

            // create objects
            for (int x = 0; x < _size.x; x++)
            {
                for (int y = 0; y < _size.y; y++)
                {
                    var go = creator(x, y);
                    _cells[x, y] = go;

                    if (go is IGridCellNeighbour goGCN)
                    {
                        ApplyNeighbour(goGCN, x, y);
                    }

                    if (go is IGridCellDrawer drawer)
                    {
                        var TileObject = GameObject.Instantiate(drawer.Prefab).transform;
                        TileObject.SetParent(_drawContainer);
                        TileObject.localPosition = Grid2World(x, y);
                    }
                }
            }
        }


        /// <summary>
        /// Neighbour logic
        /// </summary>
        /// <param name="goGCN"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void ApplyNeighbour(IGridCellNeighbour goGCN, int x, int y)
        {
            // neighbour check
            if (x > 0) // left
            {
                var ln = GetValue(x - 1, y) as IGridCellNeighbour;
                if (ln != null)
                {
                    ln.SetNeighbour(GridDirection.Right, goGCN);
                    goGCN.SetNeighbour(GridDirection.Left, ln);
                }
            }
            if (y > 0) // down
            {
                var dn = GetValue(x, y - 1) as IGridCellNeighbour;
                if (dn != null)
                {
                    dn.SetNeighbour(GridDirection.Up, goGCN);
                    goGCN.SetNeighbour(GridDirection.Down, dn);
                }
            }
        }


        /// <summary>
        /// Sets the value of a gridcell
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        public void SetValue(int x, int y, TGridObject value)
        {
            if(IsInBounds(x,y))
            {
                _cells[x, y] = value;
                OnCellChanged?.Invoke(new GridCellChangedEventArgs() { X = x, Y = y, Value = value });
            }
        }

        /// <summary>
        /// Sets the value of a gridcell
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        public void SetValue(Vector3 worldPos, TGridObject value)
        {
            var p = World2Grid(worldPos);

            if (IsInBounds(p.x, p.y))
            {
                _cells[p.x, p.y] = value;
                OnCellChanged?.Invoke(new GridCellChangedEventArgs() { X = p.x, Y = p.y, Value = value });
            }
        }

        /// <summary>
        /// Get grid value based on coordinate
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public TGridObject GetValue(int x, int y)
        {
            if (IsInBounds(x,y) && _cells[x, y] != null)
                return _cells[x, y];
            else
                return default;
        }

        /// <summary>
        /// Get grid value based on world position
        /// </summary>
        /// <param name="worldPos"></param>
        /// <returns></returns>
        public TGridObject GetValue(Vector3 worldPos)
        {
            var p = World2Grid(worldPos);
            return GetValue(p.x, p.y);
        }



        // --- Utilities ---

        public Vector2Int World2Grid(Vector3 worldPos)
        {
            float wX = worldPos.x;
            float wY = 0;

            // set to correct plane
            switch (_drawPlane)
            {
                case GridDrawPlane.XY:
                    wY = worldPos.y;
                        break;
                case GridDrawPlane.XZ:
                    wY = worldPos.z;
                    break;
                default:
                    break;
            }
            // apply offset
            if (_pivot == GridPivot.Center)
            {
                wX += _size.x * _cellSize * 0.5f;
                wY += _size.y * _cellSize * 0.5f;
            }

            wX = wX / _cellSize;
            wY = wY / _cellSize;

            return new Vector2Int(Mathf.FloorToInt(wX), Mathf.FloorToInt(wY));
        }

        public Vector3 Grid2World(int x, int y, bool center = true)
        {
            float pX = x * _cellSize;
            float pY = y * _cellSize;
            // apply offset
            if (_pivot == GridPivot.Center)
            {
                pX -= _size.x * _cellSize * 0.5f;
                pY -= _size.y * _cellSize * 0.5f;
            }

            if(center)
            {
                pX += _cellSize * 0.5f;
                pY += _cellSize * 0.5f;
            }

            // set to correct plane
            switch (_drawPlane)
            {
                case GridDrawPlane.XY:
                    return new Vector3(pX, pY) + _origin;
                    break;
                case GridDrawPlane.XZ:
                    return new Vector3(pX, 0, pY) + _origin;
                    break;
                default:
                    return _origin;
                    break;
            }
        }

        private bool IsInBounds(int x, int y)
        {
            return x < _size.x && y < _size.y;
        }

        public void Destroy()
        {
            GameObject.Destroy(_drawContainer.gameObject);
        }


        private void DrawDebug()
        {
            for (int x = 0; x < _size.x; x++)
            {
                for (int y = 0; y < _size.y; y++)
                {
                    var start = Grid2World(x, y, false);
                    var endX = Grid2World(x+1, y, false);
                    var endY = Grid2World(x, y + 1, false);
                    Debug.DrawLine(start, endY, Color.white, 100);
                    Debug.DrawLine(start, endX, Color.white, 100);
                }
            }

            var upperLeft = Grid2World(0, _size.y, false);
            var upperRight = Grid2World(_size.x, _size.y, false);
            var lowerRight = Grid2World(_size.x, 0, false);
            Debug.DrawLine(upperLeft, upperRight, Color.white, 100);
            Debug.DrawLine(upperRight, lowerRight, Color.white, 100);
        }
    }

    public enum GridDirection
    {
        Up,
        Right,
        Down,
        Left
    }

    public interface IGridCellNeighbour
    {
        void SetNeighbour(GridDirection dir, IGridCellNeighbour neighbour);
        IGridCellNeighbour GetNeighbour(GridDirection dir);
    }

    public interface IGridCellDrawer
    {
        GameObject Prefab { get; }
    }

    public interface IGridCellWorldPos
    {
        void SetWorldPos(Vector3 worldPos);
        Vector3 WorldPos { get; }
    }
}

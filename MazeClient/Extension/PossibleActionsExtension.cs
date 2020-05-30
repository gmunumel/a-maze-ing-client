using System;
using System.Collections.Generic;
using MazeClient.Enum;
using MazeClient.Model;
using Action = MazeClient.Model.Action;

namespace MazeClient.Extension
{
    public static class PossibleActionsExtension
    {
        public static Dictionary<string, bool?> GetInfoInOrder(
            this List<Action> possibleMoveActions)
        {
            Dictionary<string, bool?> result = new Dictionary<string, bool?>();
            string[] tiles = new string[4] { null, null, null, null };
            bool?[] visited = new bool?[4] { null, null, null, null };

            string tile, direction;
            bool isVisited;

            foreach (Action action in possibleMoveActions)
            {
                if (action.IsStart)
                {
                    tile = TileEnum.S.ToString();
                    direction = action.Direction;
                    isVisited = action.HasBeenVisited;
                }
                else if (action.AllowsScoreCollection)
                {
                    tile = TileEnum.C.ToString();
                    direction = action.Direction;
                    isVisited = action.HasBeenVisited;
                }
                else if (action.AllowsExit)
                {
                    tile = TileEnum.E.ToString();
                    direction = action.Direction;
                    isVisited = action.HasBeenVisited;
                }
                else if (action.RewardOnDestination > 0)
                {
                    tile = TileEnum.o.ToString();
                    direction = action.Direction;
                    isVisited = action.HasBeenVisited;
                }
                else
                {
                    tile = TileEnum.x.ToString();
                    direction = action.Direction;
                    isVisited = action.HasBeenVisited;
                }

                if (string.Compare(direction, MoveEnum.Right.ToString()) == 0)
                {
                    tiles[0] = tile;
                    visited[0] = isVisited;
                }

                if (string.Compare(direction, MoveEnum.Up.ToString()) == 0)
                {
                    tiles[1] = tile;
                    visited[1] = isVisited;
                }

                if (string.Compare(direction, MoveEnum.Left.ToString()) == 0)
                {
                    tiles[2] = tile;
                    visited[2] = isVisited;
                }

                if (string.Compare(direction, MoveEnum.Down.ToString()) == 0)
                {
                    tiles[3] = tile;
                    visited[3] = isVisited;
                }
            }

            for (int i = 0; i < tiles.Length; i++)
            {
                result.Add(tiles[i], visited[i]);
            }

            return result;
        }

        public static void PrintTileDirections(this PossibleActions action, string tile)
        {
            bool isRightSelected = false;
            bool isUpSelected = false;
            bool isLeftSelected = false;
            bool isDownSelected = false;

            string result = string.Empty;

            foreach (Action nextAction in action.PossibleMoveActions)
            {
                if (string.Compare(nextAction.Direction,
                    MoveEnum.Right.ToString()) == 0)
                {
                    isRightSelected = true;
                }

                if (string.Compare(nextAction.Direction,
                    MoveEnum.Up.ToString()) == 0)
                {
                    isUpSelected = true;
                }

                if (string.Compare(nextAction.Direction,
                    MoveEnum.Left.ToString()) == 0)
                {
                    isLeftSelected = true;
                }

                if (string.Compare(nextAction.Direction,
                    MoveEnum.Down.ToString()) == 0)
                {
                    isDownSelected = true;
                }
            }

            if (isUpSelected)
            {
                result += " |\n";
            }

            if (isLeftSelected && isRightSelected)
            {
                result += $"-{tile}-\n";
            }

            if (isLeftSelected && !isRightSelected)
            {
                result += $"-{tile}\n";
            }

            if (!isLeftSelected && isRightSelected)
            {
                result += $" {tile}-\n";
            }

            if (isDownSelected)
            {
                result += " |\n";
            }

            Console.WriteLine(result);
        }
    }
}

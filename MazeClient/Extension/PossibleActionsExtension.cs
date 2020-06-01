using System;
using System.Collections.Generic;
using MazeClient.Enum;
using MazeClient.Model;
using Action = MazeClient.Model.Action;

namespace MazeClient.Extension
{
    public static class PossibleActionsExtension
    {
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

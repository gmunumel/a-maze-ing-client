using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MazeClient.Enum;
using MazeClient.Model;
using Action = MazeClient.Model.Action;
using System.Linq;
using static MazeClient.MazeTree;
using MazeClient.Extension;

namespace MazeClient
{
    class MainClass
    {
        private static APIEnpoints apiCall;

        public static void Main(string[] args)
        {
            try
            {
                apiCall = new APIEnpoints("https://maze.hightechict.nl/api/");

                ExecuteBot().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: fatal error to connect to API {e.Message}");
            }
        }

        private static async Task ExecuteBot()
        {
            bool isForgotten = await apiCall.ForgetPlayer();
            if (!isForgotten)
            {
                Console.WriteLine("Error: not forgotten player");
                return;
            }

            bool isRegisted = await apiCall.RegisterPlayer("gabriel");
            if (!isRegisted)
            {
                Console.WriteLine("Error: not registered player");
                return;
            }

            List<Maze> mazes = await apiCall.GetAllMazes();
            if (mazes.Count == 0)
            {
                Console.WriteLine("Error: not mazes found");
                return;
            }

            foreach(Maze newMaze in mazes)
            {
                Console.WriteLine($"Maze name: {newMaze.Name}, " +
                    $"Total Tiles: {newMaze.TotalTiles}, " +
                    $"Potential Reward: {newMaze.PotentialReward}");

                await PlayMaze(newMaze);
            }

            // No more mazes to play with

            Player player = await apiCall.PlayerInfo();
            Console.WriteLine($"Player {player.Name} - Score: {player.PlayerScore}");

            return;
        }

        #region Private methods
        private static async Task PlayMaze(Maze maze)
        {
            int scoreInHand = 0;
            string tile, move = string.Empty;

            Node root = null;
            MazeTree tree = new MazeTree();

            PossibleActions action = await apiCall.EnterMaze(maze.Name); ;
            if (action != null && action.PossibleMoveActions.Count == 0)
            {
                Console.WriteLine($"Error: cannot enter maze: {maze.Name}");
                return;
            }

            tile = "S";
            action.PrintTile(tile);

            root = tree.Insert(root, new string[] { "S" }, new bool?[] { false });
            Dictionary<string, bool?> infoTiles = action.PossibleMoveActions.GetInfoInOrder();
            root = tree.Insert(root, infoTiles.Keys.ToArray(), infoTiles.Values.ToArray());

            while (true)
            {
                if (action.CanCollectScoreHere &&
                    action.CurrentScoreInHand > 0 &&
                    action.CurrentScoreInHand == maze.PotentialReward)
                {
                    bool isCollected = await apiCall.CollectScore();
                    if (!isCollected)
                    {
                        Console.WriteLine($"Error: cannot collect score in maze {maze.Name}");
                        break;
                    }
                }

                if (action.CanExitMazeHere &&
                    action.CurrentScoreInBag == maze.PotentialReward)
                {
                    bool isExit = await apiCall.ExitMaze();
                    if (!isExit)
                    {
                        Console.WriteLine($"Error: cannot exit maze {maze.Name}");
                    }
                    break;
                }

                move = GetNextMove(action, move);

                UpdateNavigation(nav, move);

                action = await apiCall.NextMove(move);
                if (action.PossibleMoveActions.Count == 0)
                {
                    Console.WriteLine($"Error: cannot move in maze");
                    break;
                }

                scoreInHand += action.CurrentScoreInHand;

                tile = action.GetTile(action, scoreInHand);
                action.PrintTile(tile);


                
                //PrintAll(nav);

                Thread.Sleep(2000);

            }
        }

        //private static void UpdateNavigation(List<Navigation> nav, string move)
        //{
        //    foreach(Navigation n in nav)
        //    {
        //        if (n.HereNow)
        //        {
        //            n.HereNow = false;
        //            n.Actions.FirstOrDefault(x => string.Compare(x.Direction, move) == 0)
        //                .HasBeenVisited = true;
        //            break;
        //        }
        //    }
        //}

        private static string GetNextMove(PossibleActions action, string move)
        {
            string result = move;

            bool isRightAvailable = false, isUpAvailable = false,
                isLeftAvailable = false, isDownAvailable = false,
                isRightRepeated = false, isUpRepeated = false,
                isLeftRepeated = false, isDownRepeated = false;

            foreach (Action nextAction in action.PossibleMoveActions)
            {
                if (string.Compare(nextAction.Direction, MoveEnum.Right.ToString()) == 0)
                {
                    if (!nextAction.HasBeenVisited)
                        isRightAvailable = true;
                    else
                        isRightRepeated = true;
                }

                if (string.Compare(nextAction.Direction, MoveEnum.Up.ToString()) == 0)
                {
                    if (!nextAction.HasBeenVisited)
                        isUpAvailable = true;
                    else
                        isUpRepeated = true;
                }

                if (string.Compare(nextAction.Direction, MoveEnum.Left.ToString()) == 0)
                {
                    if (!nextAction.HasBeenVisited)
                        isLeftAvailable = true;
                    else
                        isLeftRepeated = true;
                }

                if (string.Compare(nextAction.Direction, MoveEnum.Down.ToString()) == 0)
                {
                    if (!nextAction.HasBeenVisited)
                        isDownAvailable = true;
                    else
                        isDownRepeated = true;
                }
            }

            if (isRightAvailable)
                result = MoveEnum.Right.ToString();
            else if (isUpAvailable)
                result = MoveEnum.Up.ToString();
            else if (isLeftAvailable)
                result = MoveEnum.Left.ToString();
            else if (isDownAvailable)
                result = MoveEnum.Down.ToString();
            else
            {
                if (action.PossibleMoveActions.Count > 1)
                {
                    Random rnd = new Random();
                    result = action.PossibleMoveActions[
                        rnd.Next(action.PossibleMoveActions.Count - 1)].Direction;
                }
                else
                {
                    if (isDownRepeated)
                        result = MoveEnum.Down.ToString();
                    else if (isLeftRepeated)
                        result = MoveEnum.Left.ToString();
                    else if (isUpRepeated)
                        result = MoveEnum.Up.ToString();
                    else if (isRightRepeated)
                        result = MoveEnum.Right.ToString();
                }
            }

            return result;
        }

        

        

        //private static void PrintAll(List<Navigation> nav)
        //{
        //    foreach(Navigation n in nav)
        //    {
        //        Console.WriteLine(n);
        //    }
        //}
        #endregion
    }
}

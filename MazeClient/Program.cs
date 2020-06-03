using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MazeClient.Enum;
using MazeClient.Extension;
using MazeClient.Model;
using Action = MazeClient.Model.Action;

namespace MazeClient
{
    class MainClass
    {
        private static APIEnpoints apiCall;

        public static void Main()
        {
            try
            {
                apiCall = new APIEnpoints("https://maze.hightechict.nl/api/");

                ExecuteBot().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: fatal error to connect " +
                    $"to API {e.Message}");
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

            foreach (Maze newMaze in mazes)
            {
                Console.WriteLine($"Maze name: {newMaze.Name}, " +
                    $"Total Tiles: {newMaze.TotalTiles}, " +
                    $"Potential Reward: {newMaze.PotentialReward}");

                //Maze m = new Maze()
                //{
                //    Name = "test",
                //    PotentialReward = 100,
                //    TotalTiles = 4,
                //};

                await PlayMaze(/*m*/newMaze);
            }

            // No more mazes to play with

            Player player = await apiCall.PlayerInfo();
            Console.WriteLine($"Player {player.Name} - Score: {player.PlayerScore}");

            return;
        }

        #region Private methods
        private static async Task PlayMaze(Maze maze)
        {
            string tile, move, moveTree;
            bool isCollected = false;

            MazeTree tree = new MazeTree();

            List<string> pathToExit = new List<string>();
            List<string> pathToCollect = new List<string>();

            PossibleActions action = await apiCall.EnterMaze(maze.Name); ;
            if (action != null && action.PossibleMoveActions.Count == 0)
            {
                Console.WriteLine($"Error: cannot enter maze: {maze.Name}");
                return;
            }

            //PossibleActions action = new PossibleActions
            //{
            //    PossibleMoveActions = new List<Action>() {
            //        {
            //            new Action () {
            //            Direction = MoveEnum.Right.ToString(),
            //            IsStart = true, // S
            //            AllowsExit = false, // E
            //            AllowsScoreCollection = false,// C
            //            HasBeenVisited = false,
            //            RewardOnDestination = 0,
            //            }
            //        },
            //    },
            //    CanCollectScoreHere = false,
            //    CanExitMazeHere = false,
            //    CurrentScoreInHand = 0,
            //    CurrentScoreInBag = 0
            //};

            tile = TileEnum.S.ToString();
            action.PrintTileDirections(tile);

            tree.Add(null, MoveEnum.NONE, TileEnum.S, true);

            //AddingActionsToTree(action.PossibleMoveActions, root, tree);
            //tree.SetCurrent(root);
            //tree.SetVisited(true);

            //List<PossibleActions> actions = new List<PossibleActions>();

            //PossibleActions a = new PossibleActions() //x
            //{
            //    PossibleMoveActions = new List<Action>() {
            //        {
            //            new Action () {
            //                Direction = MoveEnum.Right.ToString(),
            //                IsStart = false, 
            //                AllowsExit = false, 
            //                AllowsScoreCollection = false,
            //                HasBeenVisited = false,
            //                RewardOnDestination = 100,
            //            }
            //        },
            //    },
            //    CanCollectScoreHere = false,
            //    CanExitMazeHere = false,
            //    CurrentScoreInHand = 0,
            //    CurrentScoreInBag = 0
            //};
            //PossibleActions b = new PossibleActions() //o
            //{
            //    PossibleMoveActions = new List<Action>() {
            //        {
            //            new Action () {
            //                Direction = MoveEnum.Right.ToString(),
            //                IsStart = false,
            //                AllowsExit = false,
            //                AllowsScoreCollection = true,
            //                HasBeenVisited = false,
            //                RewardOnDestination = 0,
            //            }
            //        },
            //    },
            //    CanCollectScoreHere = false,
            //    CanExitMazeHere = false,
            //    CurrentScoreInHand = 100,
            //    CurrentScoreInBag = 0
            //};
            //PossibleActions c = new PossibleActions() // C
            //{
            //    PossibleMoveActions = new List<Action>() {
            //        {
            //            new Action () {
            //                Direction = MoveEnum.Right.ToString(),
            //                IsStart = false, 
            //                AllowsExit = true, 
            //                AllowsScoreCollection = false,
            //                HasBeenVisited = false,
            //                RewardOnDestination = 100,
            //            }
            //        },
            //    },
            //    CanCollectScoreHere = true,
            //    CanExitMazeHere = false,
            //    CurrentScoreInHand = 100,
            //    CurrentScoreInBag = 0
            //};
            //PossibleActions d = new PossibleActions() //E
            //{
            //    PossibleMoveActions = new List<Action>() {
            //    },
            //    CanCollectScoreHere = false,
            //    CanExitMazeHere = true,
            //    CurrentScoreInHand = 100,
            //    CurrentScoreInBag = 100
            //};

            //actions.Add(a);
            //actions.Add(b);
            //actions.Add(c);
            //actions.Add(d);

            while (true)
            //foreach (PossibleActions actionX in actions)
            {
                if (action.CanCollectScoreHere &&
                    action.CurrentScoreInHand > 0 &&
                    action.CurrentScoreInHand == maze.PotentialReward)
                {
                    isCollected = true;
                    bool isCollectedInCall = await apiCall.CollectScore();
                    if (!isCollectedInCall)
                    {
                        Console.WriteLine($"Error: cannot collect score " +
                            $"in maze {maze.Name}");
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

                move = GetNextMove(maze, action/*actionX*/, tree, isCollected,
                    out tile, ref pathToExit, ref pathToCollect);
                Console.WriteLine($"\nmove: {move}\n");

                action.PrintTileDirections(tile);
                //actionX.PrintTileDirections(tile);
                //

                moveTree = move;
                move = string.Compare(move, MoveEnum.Parent.ToString()) != 0
                    ? move : ChangeDirection(tree);

                AddingActionsToTree(action.PossibleMoveActions,
                    /*actionX.PossibleMoveActions,*/ tree);
                //tree.SetVisited(true);
                tree.Move(moveTree);

                action = await apiCall.NextMove(move);
                if (action.PossibleMoveActions.Count == 0)
                {
                    Console.WriteLine($"Error: cannot move in maze");
                    break;
                }

                Thread.Sleep(2000);
            }
        }

        private static string GetNextMove(Maze maze, PossibleActions action,
            MazeTree tree, bool isCollected,
            out string tile,
            ref List<string> pathToExit,
            ref List<string> pathToCollect)
        {
            string result = string.Empty,
                tileRight = string.Empty, tileUp = string.Empty,
                tileLeft = string.Empty, tileDown = string.Empty;

            bool isRightAvailable = false, isUpAvailable = false,
                isLeftAvailable = false, isDownAvailable = false;

            tile = string.Empty;

            if (maze.PotentialReward == action.CurrentScoreInHand)
            { 
                if (!isCollected && action.CurrentScoreInBag == 0)
                {
                    if (pathToCollect.Count == 0)
                    {
                        pathToCollect = tree.FindShortestPath(tree.GetRoot(),
                            tree.GetCurrent(),
                            TileEnum.C.ToString());
                    }

                    if (pathToCollect.Count > 0)
                    {
                        result = pathToCollect.First();
                        pathToCollect.RemoveAt(0);
                    }
                }
                else if (action.CurrentScoreInHand == action.CurrentScoreInBag)
                {
                    if (pathToExit.Count == 0)
                    {
                        pathToExit = tree.FindShortestPath(tree.GetRoot(),
                            tree.GetCurrent(),
                            TileEnum.E.ToString());
                    }

                    if (pathToExit.Count > 0)
                    {
                        result = pathToExit.First();
                        pathToExit.RemoveAt(0);
                    }
                }

                if (!string.IsNullOrEmpty(result))
                {
                    Node parent = tree.GetParent();

                    if (parent != null)
                    {
                        tile = parent.Tile.ToString();
                        return result;
                    }
                }
            }

            foreach (Action nextAction in action.PossibleMoveActions)
            {
                if (string.Compare(nextAction.Direction,
                    MoveEnum.Right.ToString()) == 0 &&
                    !nextAction.HasBeenVisited)
                {
                    isRightAvailable = true;
                    tileRight = GetTile(nextAction);
                }

                if (string.Compare(nextAction.Direction,
                    MoveEnum.Up.ToString()) == 0 &&
                    !nextAction.HasBeenVisited)
                {
                    isUpAvailable = true;
                    tileUp = GetTile(nextAction);
                }

                if (string.Compare(nextAction.Direction,
                    MoveEnum.Left.ToString()) == 0 &&
                    !nextAction.HasBeenVisited)
                {
                    isLeftAvailable = true;
                    tileLeft = GetTile(nextAction);
                }

                if (string.Compare(nextAction.Direction,
                    MoveEnum.Down.ToString()) == 0 &&
                    !nextAction.HasBeenVisited)
                {
                    isDownAvailable = true;
                    tileDown = GetTile(nextAction);
                }
            }

            if (isRightAvailable)
            {
                result = MoveEnum.Right.ToString();
                tile = tileRight;
            }
            else if (isUpAvailable)
            {
                result = MoveEnum.Up.ToString();
                tile = tileUp;
            }
            else if (isLeftAvailable)
            {
                result = MoveEnum.Left.ToString();
                tile = tileLeft;
            }
            else if (isDownAvailable)
            {
                result = MoveEnum.Down.ToString();
                tile = tileDown;
            }
            else
            {
                Node parent = tree.GetParent();

                if (parent != null)
                {
                    result = MoveEnum.Parent.ToString();
                    tile = parent.Tile.ToString();
                }
            }

            return result;
        }

        private static void AddingActionsToTree(List<Action> possibleMoveActions,
            MazeTree tree)
        {
            TileEnum tile;

            foreach (Action action in possibleMoveActions)
            {
                if (action.IsStart)
                {
                    tile = TileEnum.S;
                }
                else if (action.AllowsScoreCollection)
                {
                    tile = TileEnum.C;
                }
                else if (action.AllowsExit)
                {
                    tile = TileEnum.E;
                }
                else if (action.RewardOnDestination > 0)
                {
                    tile = TileEnum.o;
                }
                else
                {
                    tile = TileEnum.x;
                }

                Node node = tree.GetCurrent();
                if (string.Compare(action.Direction, MoveEnum.Right.ToString()) == 0)
                {
                    tree.Add(node, MoveEnum.Right, tile, false);
                }

                if (string.Compare(action.Direction, MoveEnum.Up.ToString()) == 0)
                {
                    tree.Add(node, MoveEnum.Up, tile, false);
                }

                if (string.Compare(action.Direction, MoveEnum.Left.ToString()) == 0)
                {
                    tree.Add(node, MoveEnum.Left, tile, false);
                }

                if (string.Compare(action.Direction, MoveEnum.Down.ToString()) == 0)
                {
                    tree.Add(node, MoveEnum.Down, tile, false);
                }
            }
        }

        private static string ChangeDirection(MazeTree tree)
        {
            string result = string.Empty;
            MoveEnum moveEnum = tree.GetCurrent().ParentMove;

            if (moveEnum.CompareTo(MoveEnum.Right) == 0)
            {
                result = MoveEnum.Left.ToString();
            }

            if (moveEnum.CompareTo(MoveEnum.Up) == 0)
            {
                result = MoveEnum.Down.ToString();
            }

            if (moveEnum.CompareTo(MoveEnum.Left) == 0)
            {
                result = MoveEnum.Right.ToString();
            }

            if (moveEnum.CompareTo(MoveEnum.Down) == 0)
            {
                result = MoveEnum.Up.ToString();
            }

            return result;
        }

        /*
        private static string ChangeDirection(string move)
        {
            string result = move;

            if (string.Compare(move, MoveEnum.Down.ToString()) == 0)
            {
                result = MoveEnum.Up.ToString();
            }

            if (string.Compare(move, MoveEnum.Up.ToString()) == 0)
            {
                result = MoveEnum.Down.ToString();
            }

            if (string.Compare(move, MoveEnum.Left.ToString()) == 0)
            {
                result = MoveEnum.Right.ToString();
            }

            if (string.Compare(move, MoveEnum.Right.ToString()) == 0)
            {
                result = MoveEnum.Left.ToString();
            }

            return result;
        }
        */

        private static string GetTile(Action action)
        {
            string result;

            if (action.IsStart)
                result = TileEnum.S.ToString();
            else if (action.AllowsExit)
                result = TileEnum.E.ToString();
            else if (action.AllowsScoreCollection)
                result = TileEnum.C.ToString();
            else if (action.RewardOnDestination > 0)
                result = TileEnum.o.ToString();
            else
                result = TileEnum.x.ToString();

            return result;
        }
        #endregion
    }
}

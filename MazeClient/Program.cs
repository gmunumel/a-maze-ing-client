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

        public static void Main(string[] args)
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
            //bool isForgotten = await apiCall.ForgetPlayer();
            //if (!isForgotten)
            //{
            //    Console.WriteLine("Error: not forgotten player");
            //    return;
            //}

            //bool isRegisted = await apiCall.RegisterPlayer("gabriel");
            //if (!isRegisted)
            //{
            //    Console.WriteLine("Error: not registered player");
            //    return;
            //}

            //List<Maze> mazes = await apiCall.GetAllMazes();
            //if (mazes.Count == 0)
            //{
            //    Console.WriteLine("Error: not mazes found");
            //    return;
            //}

            //foreach(Maze newMaze in mazes)
            //{
            //Console.WriteLine($"Maze name: {newMaze.Name}, " +
            //    $"Total Tiles: {newMaze.TotalTiles}, " +
            //    $"Potential Reward: {newMaze.PotentialReward}");

            Maze m = new Maze()
            {
                Name = "test",
                PotentialReward = 100,
                TotalTiles = 4,
            };

                await PlayMaze(m/*newMaze*/);
            //}

            // No more mazes to play with

            Player player = await apiCall.PlayerInfo();
            Console.WriteLine($"Player {player.Name} - Score: {player.PlayerScore}");

            return;
        }

        #region Private methods
        private static async Task PlayMaze(Maze maze)
        {
            string tile, move;
            List<InfoActionOrder> infoTiles;

            Node root, current;
            MazeTree tree = new MazeTree();

            //PossibleActions action = await apiCall.EnterMaze(maze.Name); ;
            //if (action != null && action.PossibleMoveActions.Count == 0)
            //{
            //    Console.WriteLine($"Error: cannot enter maze: {maze.Name}");
            //    return;
            //}

            PossibleActions action = new PossibleActions
            {
                PossibleMoveActions = new List<Action>() {
                    {
                        new Action () {
                        Direction = MoveEnum.Right.ToString(),
                        IsStart = true, // S
                        AllowsExit = false, // E
                        AllowsScoreCollection = false,// C
                        HasBeenVisited = false,
                        RewardOnDestination = 0,
                        }
                    },
                },
                CanCollectScoreHere = false,
                CanExitMazeHere = false,
                CurrentScoreInHand = 0,
                CurrentScoreInBag = 0
            };

            tile = TileEnum.S.ToString();
            action.PrintTileDirections(tile);

            tree.Add(null, MoveEnum.NONE, TileEnum.S, true);
            root = tree.GetRoot();

            AddingActionsToTree(action.PossibleMoveActions, root, ref tree);

            current = root;

            List<PossibleActions> actions = new List<PossibleActions>();
            PossibleActions a = new PossibleActions()
            {
                PossibleMoveActions = new List<Action>() {
                    {
                        new Action () {
                        Direction = MoveEnum.Right.ToString(),
                        IsStart = false, // S
                        AllowsExit = false, // E
                        AllowsScoreCollection = false,// C
                        HasBeenVisited = false,
                        RewardOnDestination = 100,
                        }
                    },
                },
                CanCollectScoreHere = false,
                CanExitMazeHere = false,
                CurrentScoreInHand = 0,
                CurrentScoreInBag = 0
            };
            PossibleActions b = new PossibleActions()
            {
                PossibleMoveActions = new List<Action>() {
                    {
                        new Action () {
                        Direction = MoveEnum.Right.ToString(),
                        IsStart = false, // S
                        AllowsExit = false, // E
                        AllowsScoreCollection = false,// C
                        HasBeenVisited = false,
                        RewardOnDestination = 0,
                        }
                    },
                },
                CanCollectScoreHere = true,
                CanExitMazeHere = false,
                CurrentScoreInHand = 100,
                CurrentScoreInBag = 0
            };
            PossibleActions c = new PossibleActions()
            {
                PossibleMoveActions = new List<Action>() {
                    {
                        new Action () {
                        Direction = MoveEnum.Right.ToString(),
                        IsStart = false, // S
                        AllowsExit = false, // E
                        AllowsScoreCollection = false,// C
                        HasBeenVisited = false,
                        RewardOnDestination = 0,
                        }
                    },
                },
                CanCollectScoreHere = false,
                CanExitMazeHere = true,
                CurrentScoreInHand = 0,
                CurrentScoreInBag = 100
            };

            actions.Add(a);
            actions.Add(b);
            actions.Add(c);

            //while (true)
            foreach(PossibleActions actionX in actions)
            {
                if (action.CanCollectScoreHere &&
                    action.CurrentScoreInHand > 0 &&
                    action.CurrentScoreInHand == maze.PotentialReward)
                {
                    int aa = 2;
                    //bool isCollected = await apiCall.CollectScore();
                    //if (!isCollected)
                    //{
                    //    Console.WriteLine($"Error: cannot collect score " +
                    //        $"in maze {maze.Name}");
                    //    break;
                    //}
                }

                if (action.CanExitMazeHere &&
                    action.CurrentScoreInBag == maze.PotentialReward)
                {
                    int bb = 2;
                    //bool isExit = await apiCall.ExitMaze();
                    //if (!isExit)
                    //{
                    //    Console.WriteLine($"Error: cannot exit maze {maze.Name}");
                    //}
                    //break;
                }

                move = GetNextMove(/*action*/actionX, current, out tile, out MoveEnum direction);
                Console.WriteLine($"\nmove: {move}\n");

                //action = await apiCall.NextMove(move);
                //if (action.PossibleMoveActions.Count == 0)
                //{
                //    Console.WriteLine($"Error: cannot move in maze");
                //    break;
                //}

                actionX.PrintTileDirections(tile);
                current = tree.Move(root, move, direction);
                current.Visited = true;

                if (direction.CompareTo(MoveEnum.Down) == 0)
                {
                    infoTiles = GetInfoInOrder(actionX.PossibleMoveActions);
                    if (string.Compare(move, MoveEnum.Right.ToString()) == 0)
                        root.Right = tree.Insert(root.Right, infoTiles.Select(x => x.Tile).ToArray(),
                            infoTiles.Select(x => x.Visited).ToArray());

                    if (string.Compare(move, MoveEnum.Up.ToString()) == 0)
                        root.Up = tree.Insert(root.Up, infoTiles.Select(x => x.Tile).ToArray(),
                            infoTiles.Select(x => x.Visited).ToArray());

                    if (string.Compare(move, MoveEnum.Left.ToString()) == 0)
                        root.Left = tree.Insert(root.Left, infoTiles.Select(x => x.Tile).ToArray(),
                            infoTiles.Select(x => x.Visited).ToArray());

                    if (string.Compare(move, MoveEnum.Down.ToString()) == 0)
                        root.Down = tree.Insert(root.Down, infoTiles.Select(x => x.Tile).ToArray(),
                            infoTiles.Select(x => x.Visited).ToArray());
                }

                Thread.Sleep(2000);

            }
        }

        private static string GetNextMove(PossibleActions action, Node current,
            out string tile, out MoveEnum direction)
        {
            string result = string.Empty,
                tileRight = string.Empty, tileUp = string.Empty,
                tileLeft = string.Empty, tileDown = string.Empty;

            bool isRightAvailable = false, isUpAvailable = false,
                isLeftAvailable = false, isDownAvailable = false;
                //isRightRepeated = false, isUpRepeated = false,
                //isLeftRepeated = false, isDownRepeated = false;

            tile = string.Empty;
            direction = MoveEnum.Down;

            foreach (Action nextAction in action.PossibleMoveActions)
            {
                if (string.Compare(nextAction.Direction, MoveEnum.Right.ToString()) == 0 &&
                    !nextAction.HasBeenVisited)
                {
                    isRightAvailable = true;
                    //else
                    //    isRightRepeated = true;

                    tileRight = GetTile(nextAction);
                }

                if (string.Compare(nextAction.Direction, MoveEnum.Up.ToString()) == 0 &&
                    !nextAction.HasBeenVisited)
                {
                    isUpAvailable = true;
                    //else
                    //    isUpRepeated = true;

                    tileUp = GetTile(nextAction);
                }

                if (string.Compare(nextAction.Direction, MoveEnum.Left.ToString()) == 0 &&
                    !nextAction.HasBeenVisited)
                {
                    isLeftAvailable = true;
                    //else
                    //    isLeftRepeated = true;

                    tileLeft = GetTile(nextAction);
                }

                if (string.Compare(nextAction.Direction, MoveEnum.Down.ToString()) == 0 &&
                    !nextAction.HasBeenVisited)
                {
                    isDownAvailable = true;
                    //else
                    //    isDownRepeated = true;

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
                direction = MoveEnum.Up;

                Node parent = current.Parent;

                if (parent.ParentMove.CompareTo(MoveEnum.Right) == 0)
                {
                   result = MoveEnum.Left.ToString();
                }
                else if (parent.ParentMove.CompareTo(MoveEnum.Left) == 0)
                {
                    result = MoveEnum.Right.ToString();
                }
                else if (parent.ParentMove.CompareTo(MoveEnum.Down) == 0)
                {
                    result = MoveEnum.Up.ToString();
                }
                else if (parent.ParentMove.CompareTo(MoveEnum.Up) == 0)
                {
                    result = MoveEnum.Down.ToString();
                }

            }

            return result;
        }

        private static void AddingActionsToTree(List<Action> possibleMoveActions,
            Node node, ref MazeTree tree)
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

        /*
        private static List<InfoActionOrder> GetInfoInOrder(
            List<Action> possibleMoveActions)
        {
            List<InfoActionOrder> result = new List<InfoActionOrder>();
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
                if (tiles[i] != null)
                    result.Add(new InfoActionOrder { Tile = tiles[i], Visited = visited[i] });
                else
                    result.Add(new InfoActionOrder { Tile = null, Visited = null });
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

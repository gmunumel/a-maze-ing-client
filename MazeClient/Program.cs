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

            bool isRegisted = await apiCall.RegisterPlayer("Gabriel");
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

            int countCalls, countTotalCalls = 0;

            //foreach (Maze newMaze in mazes)
            //{


                //Console.WriteLine($"Maze name: {newMaze.Name}, " +
                //    $"Total Tiles: {newMaze.TotalTiles}, " +
                //    $"Potential Reward: {newMaze.PotentialReward}");

                //Maze m = new Maze()
                //{
                //    Name = "test",
                //    PotentialReward = 100,
                //    TotalTiles = 4,
                //};

                Maze maze = mazes.First(m => string.Compare(m.Name, "Loops") == 0);

                countCalls = 0;

                countCalls += await PlayMaze(maze/*newMaze*/);

                countTotalCalls += countCalls;


        //}


        // No more mazes to play with

        Player player = await apiCall.PlayerInfo();
            Console.WriteLine($"Player {player.Name} - Score: {player.PlayerScore}");
            Console.WriteLine($"Number of totals calls: {countTotalCalls}");

            return;
        }

        #region Private methods
        private static async Task<int> PlayMaze(Maze maze)
        {
            int countCalls = 0;
            string tile, move/*, moveTree*/;
            bool isCollected = false, isCollecting = false,
                isExiting = false;

            DirectionEnum direction;

            //MazeTree tree = new MazeTree();
            //Node current, parent = null;

            List<string> paths = new List<string>(),
                /*tiles = new List<string>(), */roadMap = new List<string>();

            List<List<string>> pathsToExit = new List<List<string>>();
            List<List<string>> pathsToCollect = new List<List<string>>();

            PossibleActions action = await apiCall.EnterMaze(maze.Name); ;
            if (action != null && action.PossibleMoveActions.Count == 0)
            {
                Console.WriteLine($"Error: cannot enter maze: {maze.Name}");
                return -1;
            }
            countCalls++;
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

            paths.Add(tile);
            //tiles.Add(tile);

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
                    countCalls++;
                }

                if (action.CanExitMazeHere &&
                    action.CurrentScoreInBag == maze.PotentialReward)
                {
                    bool isExit = await apiCall.ExitMaze();
                    if (!isExit)
                    {
                        Console.WriteLine($"Error: cannot exit maze {maze.Name}");
                    }
                    countCalls++;
                    break;
                }

                List<string> temp = CheckCollectOrExit(maze, action, //roadMap,
                    pathsToCollect, pathsToExit, isCollected, ref paths,
                    ref isCollecting, ref isExiting);

                if ((isCollecting || isExiting) && temp.Count > 0)
                {
                    roadMap = temp;
                    //paths = temp;
                    //paths.Clear();
                    //index = 0;
                    //tiles.Clear();
                }

                GetNextMove(/*index,*/ action, /*actionX*/ /*tree, current,*/
                    /*isCollected,*/ /*out tile, */paths, /*tiles,*/ roadMap,
                    /*pathsToExit, pathsToCollect,*/ out move,
                    out tile, out direction);
                //move = paths[paths.Count - 2];
                //tile = tiles.Last();
                Console.WriteLine($"\nmove: {move}\n");
                Console.WriteLine(string.Join(",", paths));

                action.PrintTileDirections(tile);

                if (direction.CompareTo(DirectionEnum.Down) == 0
                   && !isCollected)
                {
                    AddCollectOrExit(paths, tile, pathsToCollect, pathsToExit);
                }

                if (direction.CompareTo(DirectionEnum.Up) == 0
                    /*|| direction.CompareTo(DirectionEnum.Collect) == 0*/)
                {
                    paths.RemoveAt(paths.Count - 1);
                    paths.RemoveAt(paths.Count - 1);
                    //tiles.RemoveAt(tiles.Count - 1);
                }

                //actionX.PrintTileDirections(tile);
                //

                //moveTree = move;
                //move = string.Compare(move, MoveEnum.Parent.ToString()) != 0
                //    ? move : ChangeDirection(current);

                //AddingActionsToTree(root, parent, 
                //    action.PossibleMoveActions,
                //    /*actionX.PossibleMoveActions,*/ tree);
                //tree.SetVisited(true);
                //current = tree.Move(moveTree, root, parent);

                action = await apiCall.NextMove(move);
                if (action.PossibleMoveActions.Count == 0)
                {
                    Console.WriteLine($"Error: cannot move in maze");
                    break;
                }
                countCalls++;
                //index += 2;

                //else if (direction.CompareTo(DirectionEnum.Collect) == 0)
                //{

                //}

                //Thread.Sleep(2000);
            }

            return countCalls;
        }

        private static void GetNextMove(/*int index,*//*Maze maze, */PossibleActions action,

            /*MazeTree tree, Node node, bool isCollected,*/ //out string tile,
            List<string> paths,
            //List<string> tiles,
            List<string> roadMap,
            //List<List<string>> pathsToExit,
            //List<List<string>> pathsToCollect,
            out string move,
            out string tile,
            out DirectionEnum direction)
        {
            //string result = string.Empty,
            string tileRight = string.Empty, tileUp = string.Empty,
                   tileLeft = string.Empty, tileDown = string.Empty;

            bool isRightAvailable = false, isUpAvailable = false,
                isLeftAvailable = false, isDownAvailable = false;

            //string tile = string.Empty;

            move = string.Empty;
            tile = string.Empty;

            if (roadMap.Count > 2 /*||
                direction.CompareTo(DirectionEnum.Collect) == 0*/)
            {
                direction = DirectionEnum.Collect;

                //if (roadMap.Count == 0)
                //{
                //    tiles.Clear();
                //    tiles.Add(paths[paths.Count - 3]);
                //}
                //else
                //{
                //    paths.Clear();
                //    tiles.Clear();

                //    roadMap.Reverse();
                //    paths = roadMap;
                //    tiles.Add(paths[paths.Count - 3]);

                //    roadMap.Clear();
                //}

                //paths.Clear();
                //tiles.Clear();

                move = roadMap[1];
                tile = roadMap[2];

                if (string.Compare(tile, TileEnum.S.ToString()) == 0)
                {
                    //paths.Clear();
                    //paths.Add(TileEnum.S.ToString());
                }
                else
                {
                    //paths.Add(roadMap[1]);
                    //paths.Add(roadMap[2]);
                    //paths.Add(roadMap[2]);
                }

                roadMap.RemoveAt(0);
                roadMap.RemoveAt(0);
                //roadMap.RemoveAt(0);


                //tiles.Add(roadMap[2]);

                //roadMap.RemoveAt(0);
                //roadMap.RemoveAt(0);



                return;
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

            if (isRightAvailable || isUpAvailable ||
                isLeftAvailable || isDownAvailable)
            {
                direction = DirectionEnum.Down;

                if (isRightAvailable)
                {
                    move = MoveEnum.Right.ToString();
                    tile = tileRight;
                }
                else if (isUpAvailable)
                {
                    move = MoveEnum.Up.ToString();
                    tile = tileUp;
                }
                else if (isLeftAvailable)
                {
                    move = MoveEnum.Left.ToString();
                    tile = tileLeft;
                }
                else if (isDownAvailable)
                {
                    move = MoveEnum.Down.ToString();
                    tile = tileDown;
                }

                paths.Add(move);
                paths.Add(tile);
                //tiles.Add(tile);
            }
            else
            {
                direction = DirectionEnum.Up;

                string tileTemp = paths[paths.Count - 1];
                string moveTemp = paths[paths.Count - 2];

                paths.RemoveAt(paths.Count - 1);
                paths.RemoveAt(paths.Count - 1);
                //tiles.RemoveAt(tiles.Count - 1);

                string changedPath = ChangeDirection(moveTemp);

                paths.Add(changedPath);
                paths.Add(tileTemp);

                tile = paths[paths.Count - 1];
                move = paths[paths.Count - 2];

                //Node parent = node.Parent;

                //if (parent != null)
                //{
                //    result = MoveEnum.Parent.ToString();
                //    tile = parent.Tile.ToString();
                //}
            }

            //return result;
        }

        private static void AddCollectOrExit(List<string> paths,
            string tile,
            List<List<string>> pathsToCollect,
            List<List<string>> pathsToExit)
        {
            //string tile = tiles.Last();
            bool isEqual = false;

            if (string.Compare(tile, TileEnum.C.ToString()) == 0)
            {
                foreach (List<string> pathsCollect in pathsToCollect)
                {
                    if (pathsCollect.SequenceEqual(paths))
                    {
                        isEqual = true;
                        break;
                    }
                }

                if (!isEqual)
                {
                    List<string> pathTemp = paths.ToList();
                    pathsToCollect.Add(pathTemp);
                }
            }
            else if (string.Compare(tile, TileEnum.E.ToString()) == 0)
            {
                foreach (List<string> pathsExit in pathsToExit)
                {
                    if (pathsExit.SequenceEqual(paths))
                    {
                        isEqual = true;
                        break;
                    }
                }

                if (!isEqual)
                {
                    List<string> pathTemp = paths.ToList();
                    pathsToExit.Add(pathTemp);
                }
            }
        }

        private static List<string> CheckCollectOrExit(Maze maze,
            PossibleActions action,
            //List<string> roadMap,
            List<List<string>> pathsToCollect,
            List<List<string>> pathsToExit,
            bool isCollected,
            ref List<string> paths,
            ref bool isCollecting,
            ref bool isExiting)
        {
            List<string> result = new List<string>();

            //isCollecting = false;
            //isExiting = false;

            if (maze.PotentialReward == action.CurrentScoreInHand
                || maze.PotentialReward == action.CurrentScoreInBag)
            {
                if (!isCollected && !isCollecting && action.CurrentScoreInBag == 0)
                {
                    if (pathsToCollect.Count > 0)
                    {
                        result = FindShortestPath(pathsToCollect,
                            ref paths, TileEnum.C.ToString());

                        isExiting = false;
                        isCollecting = true;
                    }

                    //if (pathToCollect.Count > 0)
                    //{
                    //    result = pathToCollect.First();
                    //    pathToCollect.RemoveAt(0);
                    //}
                }
                else if (/*(!isCollected && isCollecting && roadMap.Count == 3) ||*/
                    action.CurrentScoreInHand == 0)
                {
                    if (!isExiting && pathsToExit.Count > 0)
                    {
                        result = FindShortestPath(pathsToExit,
                            ref paths, TileEnum.E.ToString());



                        //paths.AddRange(result);

                        //result = paths;

                        isCollecting = false;
                        isExiting = true;
                    }

                    //if (pathToExit.Count > 0)
                    //{
                    //    result = pathToExit.First();
                    //    pathToExit.RemoveAt(0);
                    //}
                }

                //if (!string.IsNullOrEmpty(result))
                //{
                //    Node parent = node.Parent;

                //    if (parent != null)
                //    {
                //        tile = parent.Tile.ToString();
                //        return result;
                //    }
                //}
            }

            return result;
        }

        /*
        private static void AddingActionsToTree(Node root,
            Node parent, 
            List<Action> possibleMoveActions,
            MazeTree tree)
        {
            TileEnum tile;
            //Node nodeTemp = null;

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
                    //if (node.Right != null)
                    //{
                    //    Node temp = tree.Add(node, MoveEnum.Right, tile);

                    //}
                    //else
                    //{
                    //    tree.Add(node, MoveEnum.Right, tile);
                    //}
                    root = tree.Add(root, MoveEnum.Right, tile);                    
                }

                if (string.Compare(action.Direction, MoveEnum.Up.ToString()) == 0)
                {
                    root = tree.Add(root, MoveEnum.Up, tile);
                }

                if (string.Compare(action.Direction, MoveEnum.Left.ToString()) == 0)
                {
                    root = tree.Add(root, MoveEnum.Left, tile);
                }

                if (string.Compare(action.Direction, MoveEnum.Down.ToString()) == 0)
                {
                    root = tree.Add(root, MoveEnum.Down, tile);
                }

                if (root != null)
                {
                    parent = root.Parent;
                }
            }

            //return node;
        }
        */

        /*
        private static string ChangeDirection(Node node)
        {
            string result = string.Empty;
            MoveEnum moveEnum = node.ParentMove;

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
        */

        private static string ChangeDirection(string path)
        {
            string result = string.Empty;
            string[] joinedString = new string[] { };

            if (string.IsNullOrEmpty(path))
                return string.Empty;

            if (path.Contains(","))
                joinedString = path.Split(',');

            foreach (string sValue in joinedString)
            {
                string toAdd = ChangeDirection(sValue);

                result += toAdd;
                result += ",";
            }

            if (string.Compare(path, MoveEnum.Right.ToString()) == 0)
            {
                result = MoveEnum.Left.ToString();
            }

            if (string.Compare(path, MoveEnum.Up.ToString()) == 0)
            {
                result = MoveEnum.Down.ToString();
            }

            if (string.Compare(path, MoveEnum.Left.ToString()) == 0)
            {
                result = MoveEnum.Right.ToString();
            }

            if (string.Compare(path, MoveEnum.Down.ToString()) == 0)
            {
                result = MoveEnum.Up.ToString();
            }

            if (string.IsNullOrEmpty(result))
                result = path;

            if (string.Compare(result.Substring(result.Length - 1, 1), ",") == 0)
                result = result.Remove(result.Length - 1, 1);

            return result;
        }

        public static List<string> FindShortestPath(List<List<string>> pathAcc,
            ref List<string> currentPath, string tile)
        {
            List<string> result = new List<string>(),
                path = new List<string>();

            //List<string> currentPath = new List<string>();
            //Traverse(Root);
            //TraserveInverse(node, ref currentPath);

            //currentPath.Reverse();

            if (currentPath.Count > 2)
            {
                //PrintPaths(pathAcc);

                pathAcc.RemoveAll(x => !x.Contains(tile));

                Dictionary<string, int> counts = GetPathsCounts(pathAcc, currentPath, tile);

                List<KeyValuePair<string, int>> listSorted = counts.ToList();
                listSorted.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

                path = listSorted.First().Key.Split(',').ToList();
                //string currentPathStr = String.Join(",", currentPath);

                //Console.WriteLine("path: " + string.Join(",", path));
                //Console.WriteLine("currentPath: " + string.Join(",", currentPath));

                result = RemoveSimilar(currentPath, path, tile);

                result = string.Join(",", result).Split(',').ToList();

                //List<string> r = string.Join(",", result).Split(',').ToList();

                //r.RemoveAll(x => string.Compare(x, TileEnum.S.ToString()) == 0
                //        || string.Compare(x, TileEnum.C.ToString()) == 0
                //        || string.Compare(x, TileEnum.E.ToString()) == 0
                //        || string.Compare(x, TileEnum.o.ToString()) == 0
                //        || string.Compare(x, TileEnum.x.ToString()) == 0);

                //result = r;
            }
            else
            {
                int min = int.MaxValue;
                foreach(List<string> ppath in pathAcc)
                {
                    if (ppath.Count < min)
                    {
                        min = ppath.Count;
                        result = ppath;
                        path = ppath;
                    }
                       
                }
            }

            currentPath = path;

            return result;
        }

        private static List<string> RemoveSimilar(List<string> currentPath, List<string> path,
        string tile)
        {
            List<string> result = new List<string>();

            char[] splitCurrentPath = string.Join(",", currentPath).ToCharArray();
            char[] splitPath = string.Join(",", path).ToCharArray();

            string currentPathResult = string.Empty;
            string pathResult = string.Empty;

            //Console.WriteLine("string.Join currentPath): " + string.Join(",", currentPath));
            //Console.WriteLine("string.Join splitPath): " + string.Join(",", path));

            if (string.Compare(string.Join(",", currentPath), string.Join(",", path)) == 0)
            {
                //currentPath.Reverse();
                List<string> reversedCurrentPath = ReverseDeepCopy(currentPath);
                splitCurrentPath = string.Join(",", reversedCurrentPath).ToCharArray();
                for (int i = 0; i < splitCurrentPath.Length; i++)
                {
                    currentPathResult += splitCurrentPath[i];
                    if (splitCurrentPath[i] == Convert.ToChar(tile))
                        break;
                }

                currentPathResult = ChangeDirection(currentPathResult);
            }
            else
            {
                int index = 0;
                //Console.WriteLine("splitCurrentPath[index]: " + splitCurrentPath[index] + " splitPath[index]: " + splitPath[index]);
                while (index < splitCurrentPath.Length
                        && index < splitPath.Length
                        && splitCurrentPath[index] == splitPath[index])
                {
                    index++;
                }

                if (index < splitCurrentPath.Length)
                {
                    //currentPath.Reverse();
                    List<string> reversedCurrentPath = ReverseDeepCopy(currentPath);
                    splitCurrentPath = string.Join(",", reversedCurrentPath).ToCharArray();
                    for (int i = 0; i < splitCurrentPath.Length - index; i++)
                    {
                        currentPathResult += splitCurrentPath[i];
                        if (i + 1 < splitCurrentPath.Length
                            && splitCurrentPath[i + 1] == Convert.ToChar(tile))
                        {
                            currentPathResult += splitCurrentPath[i + 1];
                        }
                    }

                    currentPathResult = ChangeDirection(currentPathResult);
                }

                //Console.WriteLine("index: " + index + " splitPath.Length: " + splitPath.Length);
                if (index < splitPath.Length)
                {
                    pathResult += splitPath[index - 2] == ','
                                    ? char.MinValue
                                    : splitPath[index - 2];
                    pathResult += splitPath[index - 1];
                    for (int i = index; i < splitPath.Length; i++)
                    {
                        pathResult += splitPath[i];
                        if (splitPath[i] == Convert.ToChar(tile))
                            break;
                    }

                    pathResult = pathResult.Replace("\0", string.Empty);
                }
            }

            if (!string.IsNullOrEmpty(currentPathResult))
                result.Add(currentPathResult);

            if (!string.IsNullOrEmpty(pathResult))
                result.Add(pathResult);

            return result;
        }

        private static List<string> ReverseDeepCopy(List<string> list)
        {
            List<string> result = new List<string>();

            foreach (string str in list.AsEnumerable().Reverse())
            {
                result.Add(str);
            }

            return result;
        }

        /*
        private static string ConvertToParent(string path)
        {
            string result = string.Empty;
            string[] joinedString = path.Split(',');

            foreach (string sValue in joinedString)
            {
                string toAdd = sValue;

                if (string.Compare(sValue, MoveEnum.Right.ToString()) == 0
                   || string.Compare(sValue, MoveEnum.Up.ToString()) == 0
                   || string.Compare(sValue, MoveEnum.Left.ToString()) == 0
                   || string.Compare(sValue, MoveEnum.Down.ToString()) == 0)
                    toAdd = MoveEnum.Parent.ToString();

                result += toAdd;
                result += ",";
            }

            if (string.Compare(result.Substring(result.Length - 1, 1), ",") == 0)
                result = result.Remove(result.Length - 1, 1);

            return result;
        }
        */

        private static Dictionary<string, int> GetPathsCounts(List<List<string>> pathAcc,
            List<string> currentPath, string tile)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            foreach (List<string> pathSmallAcc in pathAcc)
            {
                int count = 0, index = 0;
                bool found = false;

                int currentPathLevel = currentPath.Count == 3 ? 1 : (currentPath.Count - 1) / 2;

                for (int i = 0; i < pathSmallAcc.Count - 2 && i < currentPath.Count - 2; i += 2)
                {
                    string smallPath = string.Format("{0}{1}{2}", pathSmallAcc[i], pathSmallAcc[i + 1], pathSmallAcc[i + 2]);
                    string smallCurrentPath = string.Format("{0}{1}{2}", currentPath[i], currentPath[i + 1], currentPath[i + 2]);

                    //Console.WriteLine(smallPath);
                    //Console.WriteLine(smallCurrentPath);

                    if (string.Compare(smallPath, smallCurrentPath) != 0)
                    {
                        count++;
                    }
                    else
                    {
                        currentPathLevel--;
                    }

                    if (string.Compare(pathSmallAcc[i + 2], tile) == 0)
                    {
                        found = true;
                        break;
                    }

                    index = i;
                }

                if (!found)
                {
                    for (int i = index; i < pathSmallAcc.Count - 2; i += 2)
                    {
                        if (string.Compare(pathSmallAcc[i + 2], tile) == 0)
                        {
                            break;
                        }
                        count++;
                    }
                }

                result.Add(string.Join(",", pathSmallAcc), count + currentPathLevel);
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

        public static void PrintPaths(List<List<string>> paths)
        {
            foreach (List<string> path in paths)
                Console.WriteLine(string.Join(",", path));
        }

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

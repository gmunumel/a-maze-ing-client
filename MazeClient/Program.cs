using System;
using System.Collections.Generic;
using System.Linq;
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
            int countCalls, countTotalCalls = 0;

            bool isForgotten = await apiCall.ForgetPlayer();
            if (!isForgotten)
            {
                Console.WriteLine("Error: not forgotten player");
                return;
            }
            countTotalCalls++;

            bool isRegisted = await apiCall.RegisterPlayer("Gabriel");
            if (!isRegisted)
            {
                Console.WriteLine("Error: not registered player");
                return;
            }
            countTotalCalls++;

            List<Maze> mazes = await apiCall.GetAllMazes();
            if (mazes.Count == 0)
            {
                Console.WriteLine("Error: not mazes found");
                return;
            }
            countTotalCalls++;

            foreach (Maze newMaze in mazes)
            {
                //Maze maze = mazes.First(m => string.Compare(m.Name, "Loops") == 0);

                countCalls = await PlayMaze(/*maze*/newMaze);

                Console.WriteLine($"Maze name: {newMaze.Name}, " +
                    $"Total Tiles: {newMaze.TotalTiles}, " +
                    $"Potential Reward: {newMaze.PotentialReward}, " +
                    $"Number of calls: {countCalls}");

                countTotalCalls += countCalls;
            }

            // No more mazes to play with
            Player player = await apiCall.PlayerInfo();
                Console.WriteLine($"Player {player.Name} - Score: {player.PlayerScore}");
                Console.WriteLine($"Number of totals calls: {++countTotalCalls}");

            return;
        }

        #region Private methods
        private static async Task<int> PlayMaze(Maze maze)
        {
            int countCalls = 0;
            string tile;
            bool isCollected = false, isCollecting = false, isExiting = false;

            List<string> paths = new List<string>(), roadMap = new List<string>();

            List<List<string>> pathsToExit = new List<List<string>>();
            List<List<string>> pathsToCollect = new List<List<string>>();

            PossibleActions action = await apiCall.EnterMaze(maze.Name); ;
            if (action != null && action.PossibleMoveActions.Count == 0)
            {
                Console.WriteLine($"Error: cannot enter maze: {maze.Name}");
                return -1;
            }
            countCalls++;
            
            tile = TileEnum.S.ToString();
            //action.PrintTileDirections(tile);

            paths.Add(tile);

            while (true)
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

                List<string> temp = CheckCollectOrExit(maze, action,
                    pathsToCollect, pathsToExit, isCollected, ref paths,
                    ref isCollecting, ref isExiting);

                if ((isCollecting || isExiting) && temp.Count > 0)
                {
                    roadMap = temp;
                }

                GetNextMove(action, paths, roadMap, out string move, out tile,
                    out DirectionEnum direction);

                //Console.WriteLine($"\nmove: {move}\n");
                //Console.WriteLine(string.Join(",", paths));

                //action.PrintTileDirections(tile);

                if (direction.CompareTo(DirectionEnum.Down) == 0
                   && !isCollected)
                {
                    AddCollectOrExit(paths, tile, pathsToCollect, pathsToExit);
                }

                if (direction.CompareTo(DirectionEnum.Up) == 0)
                {
                    paths.RemoveAt(paths.Count - 1);
                    paths.RemoveAt(paths.Count - 1);
                }

                action = await apiCall.NextMove(move);
                if (action.PossibleMoveActions.Count == 0)
                {
                    Console.WriteLine($"Error: cannot move in maze");
                    break;
                }
                countCalls++;

                //Thread.Sleep(2000);
            }

            return countCalls;
        }

        private static void GetNextMove(PossibleActions action,
            List<string> paths,
            List<string> roadMap,
            out string move,
            out string tile,
            out DirectionEnum direction)
        {
            string tileRight = string.Empty, tileUp = string.Empty,
                   tileLeft = string.Empty, tileDown = string.Empty;

            bool isRightAvailable = false, isUpAvailable = false,
                isLeftAvailable = false, isDownAvailable = false;

            bool hasRightReward = false, hasUpReward = false,
                hasLeftReward = false, hasDownReward = false;

            move = string.Empty;
            tile = string.Empty;

            if (roadMap.Count > 2)
            {
                direction = DirectionEnum.Collect;

                move = roadMap[1];
                tile = roadMap[2];

                roadMap.RemoveAt(0);
                roadMap.RemoveAt(0);

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
                    hasRightReward = nextAction.RewardOnDestination > 0;
                }

                if (string.Compare(nextAction.Direction,
                    MoveEnum.Up.ToString()) == 0 &&
                    !nextAction.HasBeenVisited)
                {
                    isUpAvailable = true;
                    tileUp = GetTile(nextAction);
                    hasUpReward = nextAction.RewardOnDestination > 0;
                }

                if (string.Compare(nextAction.Direction,
                    MoveEnum.Left.ToString()) == 0 &&
                    !nextAction.HasBeenVisited)
                {
                    isLeftAvailable = true;
                    tileLeft = GetTile(nextAction);
                    hasLeftReward = nextAction.RewardOnDestination > 0;
                }

                if (string.Compare(nextAction.Direction,
                    MoveEnum.Down.ToString()) == 0 &&
                    !nextAction.HasBeenVisited)
                {
                    isDownAvailable = true;
                    tileDown = GetTile(nextAction);
                    hasDownReward = nextAction.RewardOnDestination > 0;
                }
            }

            if (isRightAvailable || isUpAvailable ||
                isLeftAvailable || isDownAvailable)
            {
                direction = DirectionEnum.Down;

                if (isRightAvailable && hasRightReward)
                {
                    move = MoveEnum.Right.ToString();
                    tile = tileRight;
                }
                else if (isUpAvailable && hasUpReward)
                {
                    move = MoveEnum.Up.ToString();
                    tile = tileUp;
                }
                else if (isLeftAvailable && hasLeftReward)
                {
                    move = MoveEnum.Left.ToString();
                    tile = tileLeft;
                }
                else if (isDownAvailable && hasDownReward)
                {
                    move = MoveEnum.Down.ToString();
                    tile = tileDown;
                }
                else if (isRightAvailable)
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
            }
            else
            {
                direction = DirectionEnum.Up;

                string tileTemp = paths[paths.Count - 1];
                string moveTemp = paths[paths.Count - 2];

                paths.RemoveAt(paths.Count - 1);
                paths.RemoveAt(paths.Count - 1);

                string changedPath = ChangeDirection(moveTemp);

                paths.Add(changedPath);
                paths.Add(tileTemp);

                tile = paths[paths.Count - 1];
                move = paths[paths.Count - 2];
            }
        }

        private static void AddCollectOrExit(List<string> paths,
            string tile,
            List<List<string>> pathsToCollect,
            List<List<string>> pathsToExit)
        {
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
            List<List<string>> pathsToCollect,
            List<List<string>> pathsToExit,
            bool isCollected,
            ref List<string> paths,
            ref bool isCollecting,
            ref bool isExiting)
        {
            List<string> result = new List<string>();

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
                }
                else if (action.CurrentScoreInHand == 0)
                {
                    if (!isExiting && pathsToExit.Count > 0)
                    {
                        result = FindShortestPath(pathsToExit,
                            ref paths, TileEnum.E.ToString());

                        isCollecting = false;
                        isExiting = true;
                    }
                }
            }

            return result;
        }

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

            Dictionary<string, int> counts;
            List<KeyValuePair<string, int>> listSorted;

            List<List<string>> pathAccTemp = new List<List<string>>(pathAcc);

            if (currentPath.Count > 2)
            {
                if (string.Compare(tile, TileEnum.C.ToString()) == 0)
                {
                    tile = TileEnum.E.ToString();

                    pathAccTemp.RemoveAll(x => !x.Contains(tile));

                    counts = GetPathsCounts(pathAccTemp, currentPath, tile);

                    listSorted = counts.ToList();
                    listSorted.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

                    foreach (KeyValuePair<string, int> entry in listSorted)
                    {
                        if (entry.Key.Contains(TileEnum.C.ToString()))
                        {
                            path = entry.Key.Split(',').ToList();
                            break;
                        }
                    }

                    if (path.Count == 0)
                    {
                        tile = TileEnum.C.ToString();
                    }
                }

                if (path.Count == 0)
                {
                    pathAcc.RemoveAll(x => !x.Contains(tile));

                    counts = GetPathsCounts(pathAcc, currentPath, tile);

                    listSorted = counts.ToList();
                    listSorted.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

                    path = listSorted.First().Key.Split(',').ToList();
                }

                result = RemoveSimilar(currentPath, path, tile);

                result = string.Join(",", result).Split(',').ToList();
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

            if (string.Compare(string.Join(",", currentPath), string.Join(",", path)) == 0)
            {
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
                while (index < splitCurrentPath.Length
                        && index < splitPath.Length
                        && splitCurrentPath[index] == splitPath[index])
                {
                    index++;
                }

                if (index < splitCurrentPath.Length)
                {
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

        private static void PrintPaths(List<List<string>> paths)
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

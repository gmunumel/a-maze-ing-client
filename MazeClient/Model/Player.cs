namespace MazeClient.Model
{
    public class Player
    {
        public string PlayerId { get; set; }
        public string Name { get; set; }
        public bool IsInMaze { get; set; }
        public string Maze { get; set; }
        public bool HasFoundEasterEgg { get; set; }
        public int MazeScoreInHand { get; set; }
        public int MazeScoreInBag { get; set; }
        public int PlayerScore { get; set; }
    }
}

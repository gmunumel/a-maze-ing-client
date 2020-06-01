namespace MazeClient.Model
{
    public class Action
    {
        public string Direction { get; set; }
        public bool IsStart { get; set; } // S
        public bool AllowsExit { get; set; } // E
        public bool AllowsScoreCollection { get; set; } // C
        public bool HasBeenVisited { get; set; }
        public int RewardOnDestination { get; set; }

        public override string ToString()
        {
            return $"{Direction.Substring(0, 1)}: {(HasBeenVisited ? "T" : "F")}";
        }

        public string ToStr()
        {
            return $"D:{Direction}, IS:{IsStart}, AE:{AllowsExit}, " +
                $"ASC:{AllowsScoreCollection}, HBV:{HasBeenVisited}, " +
                $"ROD:{RewardOnDestination}";
        }
    }

    public class InfoActionOrder
    {
        public string Tile { get; set; }
        public bool? Visited { get; set; }
    }
}

using System.Collections.Generic;

namespace MazeClient.Model
{
    public class PossibleActions
    {
        public List<Action> PossibleMoveActions { get; set; }
        public bool CanCollectScoreHere { get; set; } // C
        public bool CanExitMazeHere { get; set; } // E
        public int CurrentScoreInHand { get; set; } // 
        public int CurrentScoreInBag { get; set; } //

        public override string ToString()
        {
            return $"PMA:{PossibleMoveActions}, CCSH:{CanCollectScoreHere}, CEMH:{CanExitMazeHere}, CSIH:{CurrentScoreInHand}, CSIB: {CurrentScoreInBag}";
        }
    }
}

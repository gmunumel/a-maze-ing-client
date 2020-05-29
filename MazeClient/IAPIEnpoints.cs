using System.Collections.Generic;
using System.Threading.Tasks;
using MazeClient.Model;

namespace MazeClient
{
    public interface IAPIEnpoints
    {
        Task<bool> ForgetPlayer();
        Task<bool> RegisterPlayer(string name);
        Task<List<Maze>> GetAllMazes();
        Task<PossibleActions> EnterMaze(string nameMaze);
        Task<PossibleActions> NextMove(string move);
        Task<bool> CollectScore();
        Task<bool> ExitMaze();
        Task<Player> PlayerInfo();
    }
}

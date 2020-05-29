using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MazeClient.Model;
using Newtonsoft.Json;

namespace MazeClient
{
    public class APIEnpoints : IAPIEnpoints
    {
        private static readonly HttpClient client = new HttpClient();

        public APIEnpoints(string api_url)
        {
            client.BaseAddress = new Uri(api_url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("accept", "application/json");
            client.DefaultRequestHeaders.Add("Authorization", "HTI Thanks You [5596]");
        }

        public async Task<bool> ForgetPlayer()
        {
            string path = "player/forget";
            bool result = false;

            try
            {
                HttpResponseMessage response = await client.DeleteAsync(path);
                if (response.IsSuccessStatusCode)
                {
                    result = true;
                }
                else
                {
                    //error handle 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: ForgetPlayer - {e.Message}");
            }

            return result;
        }

        public async Task<bool> RegisterPlayer(string name)
        {
            string path = $"player/register?name={name}";
            bool result = false;

            try
            {
                HttpResponseMessage response = await client.PostAsync(path, null);
                if (response.IsSuccessStatusCode)
                {
                    result = true;
                }
                else
                {
                    //error handle 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: RegisterPlayer - {e.Message}");
            }

            return result;
        }

        public async Task<List<Maze>> GetAllMazes()
        {
            string path = "mazes/all";
            List<Maze> mazes = new List<Maze>();

            try
            {
                HttpResponseMessage response = await client.GetAsync(path);
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    //result = CleanString(result);
                    mazes = JsonConvert.DeserializeObject<List<Maze>>(result);
                    //Console.WriteLine(response);
                }
                else
                {
                    //error handle 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: GetAllMazes - {e.Message}");
            }

            return mazes;
        }

        public async Task<PossibleActions> EnterMaze(string nameMaze)
        {
            string path = $"mazes/enter?mazeName={nameMaze}";
            PossibleActions posActions = new PossibleActions();

            try
            {
                HttpResponseMessage response = await client.PostAsync(path, null);
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    //result = CleanString(result);
                    posActions = JsonConvert.DeserializeObject<PossibleActions>(result);
                    //Console.WriteLine(response);
                }
                else
                {
                    //error handle
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: EnterMaze - {e.Message}");
            }

            return posActions;
        }

        public async Task<PossibleActions> NextMove(string move)
        {
            string path = $"maze/move?direction={move}";
            PossibleActions posActions = new PossibleActions();

            try
            {
                HttpResponseMessage response = await client.PostAsync(path, null);
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    //result = CleanString(result);
                    posActions = JsonConvert.DeserializeObject<PossibleActions>(result);
                    //Console.WriteLine(response);
                }
                else
                {
                    //error handle
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: NextMove - {e.Message}");
            }

            return posActions;
        }

        public async Task<bool> CollectScore()
        {
            string path = "maze/collectScore";
            bool result = false;

            try
            {
                HttpResponseMessage response = await client.PostAsync(path, null);
                if (response.IsSuccessStatusCode)
                {
                    result = true;
                }
                else
                {
                    //error handle
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: CollectScore - {e.Message}");
            }

            return result;
        }

        public async Task<bool> ExitMaze()
        {
            string path = "maze/exit";
            bool result = false;

            try
            {
                HttpResponseMessage response = await client.PostAsync(path, null);
                if (response.IsSuccessStatusCode)
                {
                    result = true;
                }
                else
                {
                    //error handle
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: ExitMaze - {e.Message}");
            }

            return result;
        }

        public async Task<Player> PlayerInfo()
        {
            string path = "player";
            Player player = new Player();

            try
            {
                HttpResponseMessage response = await client.GetAsync(path);
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    //result = CleanString(result);
                    player = JsonConvert.DeserializeObject<Player>(result);
                    //Console.WriteLine(response);
                }
                else
                {
                    //error handle
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: PlayerInfo - {e.Message}");
            }

            return player;
        }
    }
}

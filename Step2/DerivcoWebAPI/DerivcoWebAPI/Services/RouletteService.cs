using Dapper;
using DerivcoWebAPI.Database;
using DerivcoWebAPI.Models;
using SqliteDapper.Demo.Database;
using System.Data.SQLite;

namespace DerivcoWebAPI.Services
{
    public interface IRouletteService
    {
        Task<ResponseResult> PlaceBet(Bet bet);
        Task<IEnumerable<Bet>> GetAllBets();
        int Spin();
        ResponseResult Payout(Bet bet);
        List<int> ShowPreviousSpins();
        (bool isValid, string message) ValidateBet(Bet bet);
    }
    public class RouletteService : IRouletteService
    {
        private readonly IDatabaseBootstrap _databaseBootstrap;
        private static List<int> previousSpins = new();

        public RouletteService(IDatabaseBootstrap databaseBootstrap)
        {
            _databaseBootstrap = databaseBootstrap;
        }
        public async Task<ResponseResult> PlaceBet(Bet bet)
        {
            try
            {
                using var connection = new SQLiteConnection(_databaseBootstrap.GetConnectionString());

                await connection.ExecuteAsync("INSERT INTO Bet (UserID, BetID, BetNumber, BetAmount)" +
                    "VALUES (@UserID, @BetID, @BetNumber, @BetAmount);", bet);

                connection.Close();

                return new ResponseResult
                {
                    Success = true,
                    Data = bet
                };
            }
            catch (SQLiteException ex)
            {
                return new ResponseResult
                {
                    Success = false,
                    Message = ex.Message,
                    Data = bet
                };
            }
        }
        public async Task<IEnumerable<Bet>> GetAllBets()
        {
            using var connection = new SQLiteConnection(_databaseBootstrap.GetConnectionString());

            return await connection.QueryAsync<Bet>("SELECT * from bet;");
        }

        public int Spin()
        {
            Random random = new Random();
            int rouletteNumber = random.Next(0, 37);

            previousSpins.Add(rouletteNumber);

            return rouletteNumber;
        }
        public ResponseResult Payout(Bet bet)
        {
            try
            {
                //If there is not a previous spin
                if (previousSpins.Count < 1)
                {
                    return new ResponseResult
                    {
                        Success = false,
                        Data = bet,
                        Message = "There are no previous spins so bet is not winnable."
                    };
                }
                var previousSpin = previousSpins.LastOrDefault();

                //Win
                if (previousSpin == bet.BetNumber)
                {
                    return new ResponseResult
                    {
                        Success = true,
                        Data = new Payout
                        {
                            Win = true,
                            WinningAmount = bet.BetAmount * 36,
                            WinningNumber = bet.BetNumber
                        }
                    };
                }
                //Lose
                else
                {
                    return new ResponseResult
                    {
                        Success = true,
                        Data = new Payout
                        {
                            Win = false,
                            WinningAmount = bet.BetAmount * 36 * 0,
                            WinningNumber = previousSpin
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    Success = false,
                    Data = bet,
                    Message = ex.Message
                };
            }
        }
        public List<int> ShowPreviousSpins()
        {
            return previousSpins;
        }

        (bool isValid, string message) IRouletteService.ValidateBet(Bet bet)
        {
            if (bet.BetNumber > 36 || bet.BetNumber < 0)
            {
                return (false, "The selected bet number is not between or equal to 0 and 36");
            }
            if (bet.BetAmount <= 0)
            {
                return (false, "Bet amount must be more than 0!");
            }

            return (true, "");
        }
    }
}

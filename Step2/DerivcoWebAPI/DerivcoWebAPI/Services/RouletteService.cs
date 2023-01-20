using Dapper;
using DerivcoWebAPI.Database;
using DerivcoWebAPI.Models;
using SqliteDapper.Demo.Database;
using System.Data.SQLite;

namespace DerivcoWebAPI.Services
{
    public interface IRouletteService
    {
        Task<ResponseResult> PlaceBet(List<Bet> bets);
        Task<IEnumerable<Bet>> GetAllBets();
        int Spin();
        ResponseResult Payout(List<Bet> bets);
        List<int> ShowPreviousSpins();
        (bool isValid, string message) ValidateBet(Bet bet);
        (bool isValid, int odds) GetOdds(BetType betType);
    }
    public class RouletteService : IRouletteService
    {
        private readonly IDatabaseBootstrap _databaseBootstrap;
        private static List<int> previousSpins = new();

        public RouletteService(IDatabaseBootstrap databaseBootstrap)
        {
            _databaseBootstrap = databaseBootstrap;
        }
        //Method to insert a new bet into the SQLite database using dapper
        public async Task<ResponseResult> PlaceBet(List<Bet> bets)
        {
            try
            {
                using var connection = new SQLiteConnection(_databaseBootstrap.GetConnectionString());

                foreach (Bet bet in bets)
                {
                    await connection.ExecuteAsync
                    ("INSERT INTO Bet (UserID, BetID, BetNumber, BetAmount, BetType)" +
                     "VALUES (@UserID, @BetID, @BetNumber, @BetAmount, @BetType);", bet);
                }

                connection.Close();

                return new ResponseResult
                {
                    Success = true,
                    Data = bets
                };
            }
            catch (SQLiteException ex)
            {
                return new ResponseResult
                {
                    Success = false,
                    Message = ex.Message,
                    Data = bets
                };
            }
        }
        //Method to return entire bet table using dapper
        public async Task<IEnumerable<Bet>> GetAllBets()
        {
            using var connection = new SQLiteConnection(_databaseBootstrap.GetConnectionString());

            return await connection.QueryAsync<Bet>("SELECT * from bet;");
        }
        //Method to return a random number between or equal to 0 and 36
        //add spin number to static previousSpins list<int>
        public int Spin()
        {
            Random random = new Random();
            int rouletteNumber = random.Next(0, 37);

            previousSpins.Add(rouletteNumber);

            return rouletteNumber;
        }
        //Method to check if the user has won the bet as well as how much money they won
        public ResponseResult Payout(List<Bet> bets)
        {
            try
            {
                List<Payout> payouts = new();
                //If there is not a previous spin
                if (previousSpins.Count < 1)
                {
                    return new ResponseResult
                    {
                        Success = false,
                        Data = bets,
                        Message = "There are no previous spins so bet is not winnable."
                    };
                }

                foreach (Bet bet in bets)
                {
                    //Get latest spin number
                    var previousSpin = previousSpins.LastOrDefault();

                    bool winBet = ValidateBetWin(previousSpin, bet);

                    if (winBet)
                    {
                        //Calculate winning odds based off bet type
                        var payout = GetOdds(bet.BetType);
                        if (!payout.isValid)
                        {
                            return new ResponseResult
                            {
                                Success = false,
                                Data = bet,
                                Message = "Invalid Bet Type"
                            };
                        }

                        //Win
                        payouts.Add(new Payout
                        {
                            Win = true,
                            WinningAmount = bet.BetAmount * payout.odds,
                            WinningNumber = bet.BetNumber,
                            BetNumber = bet.BetNumber,
                            BetAmount = bet.BetAmount,
                            BetID = bet.BetID,
                            BetType = bet.BetType,
                            UserID = bet.UserID,
                            TotalWinningAmount = bet.BetAmount + (bet.BetAmount * payout.odds)
                        });
                    }
                    else
                    {
                        //Lose
                        payouts.Add(new Payout
                        {
                            Win = false,
                            WinningAmount = 0,
                            WinningNumber = previousSpin,
                            BetNumber = bet.BetNumber,
                            BetAmount = bet.BetAmount,
                            BetID = bet.BetID,
                            BetType = bet.BetType,
                            UserID = bet.UserID
                        });
                    }
                }
                return new ResponseResult
                {
                    Success = true,
                    Data = payouts
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    Success = false,
                    Data = bets,
                    Message = ex.Message
                };
            }
        }
        //Determine is user has won the bet depending on whcih bet they chose
        private bool ValidateBetWin(int previousSpin, Bet bet)
        {
            switch (bet.BetType)
            {
                case BetType.Even:
                case BetType.Red:
                    return IsEvenOdd(previousSpin, 0);
                case BetType.Odd:
                case BetType.Black:
                    return IsEvenOdd(previousSpin, 1);
                case BetType.OneToTwelve:
                    return IsBetweenOrEqual(previousSpin, 1, 12);
                case BetType.ThirteenTo24:
                    return IsBetweenOrEqual(previousSpin, 13, 24);
                case BetType.TwentyfiveToThirtysix:
                    return IsBetweenOrEqual(previousSpin, 25, 36);
                case BetType.OneToEighteen:
                    return IsBetweenOrEqual(previousSpin, 1, 18);
                case BetType.NineteenToThirtysix:
                    return IsBetweenOrEqual(previousSpin, 19, 36);
                case BetType.FirstColumn:
                     return ColumnNumber(previousSpin, 1);
                case BetType.SecondColumn:
                    return ColumnNumber(previousSpin, 2);
                case BetType.ThirdColumn:
                    return ColumnNumber(previousSpin, 3);
                case BetType.Number:
                    return bet.BetNumber == previousSpin;
                default: 
                    return false;
            }
        }
        //Determine which of the 3 columns the numbers falls into
        private bool ColumnNumber(int previousSpin, int columnNumber)
        {
            //Create multi dimensional array for roulette
            //I know I could just get the column number and add 3 to search if number is here haha
            int[,] rouletteArray = { 
                { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 }, { 10, 11, 12 }, { 13, 14, 15 }, { 16, 17, 18 }, 
                { 19, 20, 21 }, { 22, 23, 24 }, { 25, 26, 27 }, { 28, 29, 30 }, { 31, 32, 33 }, { 34, 33, 36 }
            };

            for (int i = 0; i < 12; i++)
            {
                if (rouletteArray[i, columnNumber - 1] == previousSpin)
                {
                    return true;
                }
            }

            return false;
        }
        //Determine if number is between two values
        private bool IsBetweenOrEqual(int previousSpin, int min, int max)
        {
            if (previousSpin >= min && previousSpin <= max)
            {
                return true;
            }
            return false;
        }
        //determine is number is even or odd by using its remainder when divided by 2
        private bool IsEvenOdd(int previousSpin, int remainder)
        {
            if (previousSpin % 2 == remainder)
            {
                return true;
            }
            return false;
        }

        //return static list of previous spins
        public List<int> ShowPreviousSpins()
        {
            return previousSpins;
        }
        //Tuple method to ensure bet number is between or equal to 0 and 36 as well as bet amount is bigger than 0
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
        //Get the odds of winning depending on chosen bet type
        public (bool isValid, int odds) GetOdds(BetType betType)
        {
            switch (betType)
            {
                case BetType.Red:
                case BetType.Black:
                case BetType.Odd:
                case BetType.Even:
                case BetType.OneToEighteen:
                case BetType.NineteenToThirtysix:
                    return (true, 1);
                case BetType.FirstColumn:
                case BetType.SecondColumn:
                case BetType.ThirdColumn:
                case BetType.OneToTwelve:
                case BetType.ThirteenTo24:
                case BetType.TwentyfiveToThirtysix:
                    return (true, 2);
                case BetType.Number:
                    return (true, 35);
                default:
                    return (false, 0);
            }
        }
    }
}

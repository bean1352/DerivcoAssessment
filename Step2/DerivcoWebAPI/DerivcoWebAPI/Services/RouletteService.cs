using Dapper;
using DerivcoWebAPI.Database;
using DerivcoWebAPI.Models;
using SqliteDapper.Demo.Database;
using System.Data.SQLite;
using Z.Dapper.Plus;

namespace DerivcoWebAPI.Services
{
    public interface IRouletteService
    {
        Task<ResponseResult> PlaceBet(List<Bet> bets, int? customSpinNumber);
        Task<IEnumerable<Bet>> GetAllBets();
        Task<int> Spin();
        Task<ResponseResult> Payout(List<Bet> bets, int? customSpinNumber);
        Task<IEnumerable<Spin>> ShowPreviousSpins();
        (bool isValid, string message) ValidateBet(Bet bet);
        (bool isValid, int odds) GetOdds(BetType betType);
        Task<IEnumerable<Payout>> GetAllPayouts();
    }
    public class RouletteService : IRouletteService
    {
        private readonly IDatabaseBootstrap _databaseBootstrap;
        private static int[,]? rouletteArray = null;
        public RouletteService(IDatabaseBootstrap databaseBootstrap)
        {
            _databaseBootstrap = databaseBootstrap;
            CreateRouletteMatrix();
        }

        private static void CreateRouletteMatrix()
        {
            //Create multi dimensional array for roulette
            //I know I could just get the column number and add 3 to search if number is here haha
            //12x3 matrix populated in a single for loop not nested 
            if (rouletteArray == null)
            {
                rouletteArray = new int[12, 3];
                int row = rouletteArray.GetLength(0);
                int col = rouletteArray.GetLength(1);

                for (int i = 0; i < row * col; i++)
                {
                    rouletteArray[i / col, i % col] = i + 1;
                }
            }
        }

        //Method to insert a new bet into the SQLite database using dapper
        //This method allows a user to place multiple bets, the roulette spins then the payout is calclated for all bets
        public async Task<ResponseResult> PlaceBet(List<Bet> bets, int? customSpinNumber)
        {
            try
            {
                using var connection = new SQLiteConnection(_databaseBootstrap.GetConnectionString());

                foreach (Bet bet in bets)
                {
                    bet.BetID = Guid.NewGuid();
                }

                await connection.ExecuteAsync
                ("INSERT INTO Bet (BetID, BetNumber, BetAmount, BetType)" +
                    "VALUES (@BetID, @BetNumber, @BetAmount, @BetType);", bets);

                connection.Close();

                if (customSpinNumber == null)
                {
                    _ = await Spin();

                    return await Payout(bets, customSpinNumber);
                }
                else
                {
                    return await Payout(bets, customSpinNumber);
                }
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
        public async Task<int> Spin()
        {
            Random random = new Random();
            int rouletteNumber = random.Next(0, 37);

            Spin spin = new Spin()
            {
                SpinID = Guid.NewGuid(),
                SpinValue = rouletteNumber,
                Timestamp = DateTime.Now
            };

            using var connection = new SQLiteConnection(_databaseBootstrap.GetConnectionString());

            await connection.ExecuteAsync
            ("INSERT INTO Spin (SpinID, SpinValue, Timestamp)" +
                "VALUES (@SpinID, @SpinValue, @Timestamp);", spin);
            

            connection.Close();

            return rouletteNumber;
        }
        //Method to check if the user has won the bet as well as how much money they won
        public async Task<ResponseResult> Payout(List<Bet> bets, int? customSpinNumber)
        {
            try
            {
                List<Payout> payouts = new();
                //If there is not a previous spin
                List<Spin> previousSpins = (List<Spin>)await Task.Run(() => ShowPreviousSpins());
                if (previousSpins is null || previousSpins.Count < 1)
                {
                    return new ResponseResult
                    {
                        Success = false,
                        Data = bets,
                        Message = "There are no previous spins so bet is not winnable."
                    };
                }

                //Get latest spin number
                var previousSpin = previousSpins.FirstOrDefault().SpinValue;
                foreach (Bet bet in bets)
                {

                    if (customSpinNumber != null)
                    {
                        previousSpin = (int)customSpinNumber; 
                    }

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
                            PayoutID = Guid.NewGuid(),
                            Win = true,
                            WinningAmount = bet.BetAmount * payout.odds,
                            WinningNumber = previousSpin,
                            BetNumber = bet.BetNumber,
                            BetAmount = bet.BetAmount,
                            BetID = bet.BetID,
                            BetType = bet.BetType,
                            TotalWinningAmount = bet.BetAmount + (bet.BetAmount * payout.odds)
                        });
                    }
                    else
                    {
                        //Lose
                        payouts.Add(new Payout
                        {
                            PayoutID = Guid.NewGuid(),
                            Win = false,
                            WinningAmount = 0,
                            WinningNumber = previousSpin,
                            BetNumber = bet.BetNumber,
                            BetAmount = bet.BetAmount,
                            BetID = bet.BetID,
                            BetType = bet.BetType,
                        });
                    }
                }

                bool saved = await SavePayout(payouts);

                if (!saved)
                {
                    throw new Exception("Cannot save payout to the database.");
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

        private async Task<bool> SavePayout(List<Payout> payouts)
        {
            try
            {
                using var connection = new SQLiteConnection(_databaseBootstrap.GetConnectionString());


                await connection.ExecuteAsync
                ("INSERT INTO Payout (PayoutID, Win, WinningNumber, WinningAmount, TotalWinningAmount, BetID)" +
                 "VALUES (@PayoutID, @Win, @WinningNumber, @WinningAmount, @TotalWinningAmount, @BetID);", payouts);


                connection.Close();

                return true;
            }
            catch
            {
                return false;
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
        private static bool ColumnNumber(int previousSpin, int columnNumber)
        {
            if (rouletteArray is null)
            {
                CreateRouletteMatrix();
                throw new ArgumentNullException(nameof(rouletteArray));
            }
            else
            {
                //use static roulette matrix
                for (int i = 0; i < 12; i++)
                {
                    if (rouletteArray[i, columnNumber - 1] == previousSpin)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        //Determine if number is between two values
        private static bool IsBetweenOrEqual(int previousSpin, int min, int max)
        {
            if (previousSpin >= min && previousSpin <= max)
            {
                return true;
            }
            return false;
        }
        //determine is number is even or odd by using its remainder when divided by 2
        private static bool IsEvenOdd(int previousSpin, int remainder)
        {
            //0 is neither red,black,even or odd
            if (previousSpin == 0)
            {
                return false;
            }

            if (previousSpin % 2 == remainder)
            {
                return true;
            }
            return false;
        }

        //return static list of previous spins
        public async Task<IEnumerable<Spin>> ShowPreviousSpins()
        {
            using var connection = new SQLiteConnection(_databaseBootstrap.GetConnectionString());

            return await connection.QueryAsync<Spin>("SELECT * from Spin order by timestamp desc;");
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

        public async Task<IEnumerable<Payout>> GetAllPayouts()
        {
            using var connection = new SQLiteConnection(_databaseBootstrap.GetConnectionString());

            //Join the bet table on the query table on BetID with dapper
            var sql = @"SELECT p.*, b.BetAmount, b.BetNumber, b.BetType
                from Payout p
                INNER JOIN Bet b ON p.BetID = b.BetID";

            var products = await connection.QueryAsync<Payout, Bet, Payout>(sql, (payout, bet) => {
                payout.BetID = bet.BetID;
                payout.BetAmount = bet.BetAmount;
                payout.BetType = bet.BetType;
                return payout;
            },
            splitOn: "BetID");

            return products;
        }
    }
}

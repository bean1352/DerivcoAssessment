using DerivcoWebAPI.Controllers;
using DerivcoWebAPI.Database;
using DerivcoWebAPI.Models;
using DerivcoWebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        private readonly DatabaseBootstrap databaseBootstrap;
        private readonly RouletteService rouletteService;
        private readonly RouletteController controller;
        public UnitTest1()
        {
            databaseBootstrap = new DatabaseBootstrap(new DatabaseConfig());
            rouletteService = new(databaseBootstrap);
            controller = new(rouletteService);
        }
        [TestMethod]
        //Test Spin endpoint by ensuring success and making sure spin number is between or equal to 0 and 36
        public async Task SpinTest()
        {
            var actionResult = await controller.Spin(); ;
            var result = actionResult as OkObjectResult;
            ResponseResult? response = result?.Value as ResponseResult;
            Assert.IsTrue(response?.Success);
            Assert.IsTrue((int)response?.Data >= 0 && (int)response.Data < 37);
        }
        [TestMethod]
        //Test previousSpins endpoint by ensuring success and data is not null
        public void PreviousSpinTest()
        {
            var actionResult = controller.ShowPreviousSpins();
            var result = actionResult as OkObjectResult;
            ResponseResult? response = result?.Value as ResponseResult;
            Assert.IsTrue(response?.Success);
            Assert.IsNotNull(response?.Data);
        }
        [TestMethod]
        //Test GetAllBetsEndpoint by ensuring success and data is not null
        public async Task GetAllBetsTest()
        {
            var actionResult = await controller.GetAllBets();
            var result = actionResult as OkObjectResult;
            ResponseResult? response = result?.Value as ResponseResult;
            Assert.IsTrue(response?.Success);
            Assert.IsNotNull(response?.Data);
        }
        [TestMethod]
        //Test PlaceBet endpoint by posting a bet object and making sure it returns success and the same bet object in the data property
        public async Task PlaceBetTest()
        {
            List<Bet> bet = GetBet();
            var actionResult = await controller.PlaceBet(bet);
            var result = actionResult as OkObjectResult;
            ResponseResult? response = result?.Value as ResponseResult;
            Assert.IsNotNull(response?.Data);
        }
        [TestMethod]
        //Test Payout endpoint by posting a bet object and checking if result is success and data is not null
        public async Task PayoutTest()
        {
            //Spin  first to test if payout was success
            await controller.Spin();

            List<Bet> bet = GetBet();
            var actionResult = await controller.Payout(bet);
            var result = actionResult as OkObjectResult;
            ResponseResult? response = result?.Value as ResponseResult;
            Assert.IsTrue(response?.Success);
            Assert.IsNotNull(response?.Data);
        }
        [TestMethod]
        public async Task RouletteEvenTest()
        {
            List<Bet> bet = CreateBet(100, 0, BetType.Even);
            var actionResult = await controller.PlaceBet(bet, 2);
            var result = actionResult as OkObjectResult;
            ResponseResult? response = result?.Value as ResponseResult;
            var payout = response.Data as List<Payout>;
            Assert.IsTrue(payout[0].Win);
        }
        [TestMethod]
        public async Task RouletteOddTest()
        {
            List<Bet> bet = CreateBet(100, 0, BetType.Odd);
            var actionResult = await controller.PlaceBet(bet, 3);
            var result = actionResult as OkObjectResult;
            ResponseResult? response = result?.Value as ResponseResult;
            var payout = response.Data as List<Payout>;
            Assert.IsTrue(payout[0].Win);
        }
        [TestMethod]
        public async Task RouletteRedTest()
        {
            List<Bet> bet = CreateBet(100, 0, BetType.Red);
            var actionResult = await controller.PlaceBet(bet, 2);
            var result = actionResult as OkObjectResult;
            ResponseResult? response = result?.Value as ResponseResult;
            var payout = response.Data as List<Payout>;
            Assert.IsTrue(payout[0].Win);
        }
        [TestMethod]
        public async Task RouletteBlackTest()
        {
            List<Bet> bet = CreateBet(100, 0, BetType.Black);
            var actionResult = await controller.PlaceBet(bet, 3);
            var result = actionResult as OkObjectResult;
            ResponseResult? response = result?.Value as ResponseResult;
            var payout = response.Data as List<Payout>;
            Assert.IsTrue(payout[0].Win);
        }
        [TestMethod]
        public async Task RouletteNumberTest()
        {
            List<Bet> bet = CreateBet(100, 22, BetType.Number);
            var actionResult = await controller.PlaceBet(bet, 22);
            var result = actionResult as OkObjectResult;
            ResponseResult? response = result?.Value as ResponseResult;
            var payout = response.Data as List<Payout>;
            Assert.IsTrue(payout[0].Win);
        }
        [TestMethod]
        public async Task RouletteOneToTwelveTest()
        {
            List<Bet> bet = CreateBet(100, 0, BetType.OneToTwelve);
            var actionResult = await controller.PlaceBet(bet, 3);
            var result = actionResult as OkObjectResult;
            ResponseResult? response = result?.Value as ResponseResult;
            var payout = response.Data as List<Payout>;
            Assert.IsTrue(payout[0].Win);
        }
        [TestMethod]
        public async Task RouletteThirteenTo24Test()
        {
            List<Bet> bet = CreateBet(100, 0, BetType.ThirteenTo24);
            var actionResult = await controller.PlaceBet(bet, 20);
            var result = actionResult as OkObjectResult;
            ResponseResult? response = result?.Value as ResponseResult;
            var payout = response.Data as List<Payout>;
            Assert.IsTrue(payout[0].Win);
        }
        [TestMethod]
        public async Task RouletteTwentyFiveTo36Test()
        {
            List<Bet> bet = CreateBet(100, 0, BetType.TwentyfiveToThirtysix);
            var actionResult = await controller.PlaceBet(bet, 30);
            var result = actionResult as OkObjectResult;
            ResponseResult? response = result?.Value as ResponseResult;
            var payout = response.Data as List<Payout>;
            Assert.IsTrue(payout[0].Win);
        }
        [TestMethod]
        public async Task RouletteOneTo18Test()
        {
            List<Bet> bet = CreateBet(100, 0, BetType.OneToEighteen);
            var actionResult = await controller.PlaceBet(bet, 16);
            var result = actionResult as OkObjectResult;
            ResponseResult? response = result?.Value as ResponseResult;
            var payout = response.Data as List<Payout>;
            Assert.IsTrue(payout[0].Win);
        }
        [TestMethod]
        public async Task RouletteTwentyNineTeenTo36Test()
        {
            List<Bet> bet = CreateBet(100, 0, BetType.NineteenToThirtysix);
            var actionResult = await controller.PlaceBet(bet, 30);
            var result = actionResult as OkObjectResult;
            ResponseResult? response = result?.Value as ResponseResult;
            var payout = response.Data as List<Payout>;
            Assert.IsTrue(payout[0].Win);
        }
        [TestMethod]
        public async Task RouletteFirstColumnTest()
        {
            List<Bet> bet = CreateBet(100, 0, BetType.FirstColumn);
            var actionResult = await controller.PlaceBet(bet, 1);
            var result = actionResult as OkObjectResult;
            ResponseResult? response = result?.Value as ResponseResult;
            var payout = response.Data as List<Payout>;
            Assert.IsTrue(payout[0].Win);
        }
        [TestMethod]
        public async Task RouletteSecondColumnTest()
        {
            List<Bet> bet = CreateBet(100, 0, BetType.SecondColumn);
            var actionResult = await controller.PlaceBet(bet, 2);
            var result = actionResult as OkObjectResult;
            ResponseResult? response = result?.Value as ResponseResult;
            var payout = response.Data as List<Payout>;
            Assert.IsTrue(payout[0].Win);
        }
        [TestMethod]
        public async Task RouletteThirdColumnTest()
        {
            List<Bet> bet = CreateBet(100, 0, BetType.ThirdColumn);
            var actionResult = await controller.PlaceBet(bet, 3);
            var result = actionResult as OkObjectResult;
            ResponseResult? response = result?.Value as ResponseResult;
            var payout = response.Data as List<Payout>;
            Assert.IsTrue(payout[0].Win);
        }
        //Method to create Bet object
        private static List<Bet> GetBet()
        {
            Random random = new Random();

            List<Bet> bets = new List<Bet>();

            for (int i = 0; i < 4; i++)
            {
                bets.Add(new Bet()
                {
                    BetID = Guid.NewGuid(),
                    BetAmount = random.Next(0, 1000),
                    BetNumber = random.Next(0, 37),
                    BetType = BetType.Number
                });
            }

            return bets;
        }
        private static List<Bet> CreateBet(double betAmount, int betNumber, BetType betType)
        {
            List<Bet> bets = new List<Bet>();
            bets.Add(new Bet()
            {
                BetAmount = betAmount,
                BetID = Guid.NewGuid(),
                BetNumber = betNumber,
                BetType = betType
            });

            return bets;
        }
    }
}
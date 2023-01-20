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
        public void SpinTest()
        {
            var actionResult = controller.Spin();;
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
            Assert.IsTrue(response?.Success);
            Assert.AreEqual(response?.Data, bet);
        }
        [TestMethod]
        //Test Payout endpoint by posting a bet object and checking if result is success and data is not null
        public void PayoutTest()
        {
            //Spin  first to test if payout was success
            controller.Spin();

            List<Bet> bet = GetBet();
            var actionResult = controller.Payout(bet);
            var result = actionResult as OkObjectResult;
            ResponseResult? response = result?.Value as ResponseResult;
            Assert.IsTrue(response?.Success);
            Assert.IsNotNull(response?.Data);
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
                    UserID = Guid.NewGuid(),
                    BetID = Guid.NewGuid(),
                    BetAmount = random.Next(0, 1000),
                    BetNumber = random.Next(0, 37)
                });
            }

            return bets;
        }
    }
}
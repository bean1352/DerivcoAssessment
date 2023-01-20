using DerivcoWebAPI.Controllers;
using DerivcoWebAPI.Database;
using DerivcoWebAPI.Models;
using DerivcoWebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public void SpinTest()
        {
            var actionResult = controller.Spin();;
            var result = actionResult?.Result as OkObjectResult;
            ResponseResult? response = result?.Value as ResponseResult;

            Assert.IsTrue((int)response?.Data >= 0 && (int)response.Data < 37);
        }
        [TestMethod]
        public void PreviousSpinTest()
        {
            var actionResult = controller.ShowPreviousSpins();
            var result = actionResult?.Result as OkObjectResult;
            ResponseResult? response = result?.Value as ResponseResult;
            Assert.IsNotNull(response?.Data);
        }
        [TestMethod]
        public async Task GetAllBetsTest()
        {
            var actionResult = await controller.GetAllBets();
            var result = actionResult?.Result as OkObjectResult;
            ResponseResult? response = result?.Value as ResponseResult;
            Assert.IsNotNull(response?.Data);
        }
    }
}
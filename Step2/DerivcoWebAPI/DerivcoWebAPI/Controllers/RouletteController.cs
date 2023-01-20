using DerivcoWebAPI.Models;
using DerivcoWebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DerivcoWebAPI.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("[controller]")]
    public class RouletteController : ControllerBase
    {
        private readonly IRouletteService _rouletteService;
        public RouletteController(IRouletteService rouletteService)
        {
            _rouletteService = rouletteService;
        }

        //Custom spin number added for testing
        [HttpPost("PlaceBet")]
        public async Task<IActionResult> PlaceBet([FromBody] List<Bet> bets, int? spinNumberCustom = null)
        {
            try
            {
                foreach (Bet bet in bets)
                {
                    var tup = _rouletteService.ValidateBet(bet);
                    if (!tup.isValid)
                    {
                        return BadRequest(new ResponseResult

                        {
                            Success = false,
                            Message = tup.message
                        });
                    }
                }
                var result = await _rouletteService.PlaceBet(bets, spinNumberCustom);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GetAllBets")]
        public async Task<IActionResult> GetAllBets()
        {
            try
            {
                var allBets = await _rouletteService.GetAllBets();
                return Ok(new ResponseResult
                {
                    Success = true,
                    Data = allBets
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("Spin")]
        public async Task<IActionResult> Spin()
        {
            try
            {
                var result = await _rouletteService.Spin();
                return Ok(new ResponseResult
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Payout")]
        public async Task<IActionResult> Payout([FromBody] List<Bet> bets)
        {
            try
            {
                foreach (Bet bet in bets)
                {
                    var tup = _rouletteService.ValidateBet(bet);
                    if (!tup.isValid)
                    {
                        return BadRequest(new ResponseResult
                        {
                            Success = false,
                            Message = tup.message
                        });
                    }
                }
                var result = await _rouletteService.Payout(bets, null);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GetAllPayouts")]
        public async Task<IActionResult> GetAllPayouts()
        {
            try
            {
                var allPayouts = await _rouletteService.GetAllPayouts();
                return Ok(new ResponseResult
                {
                    Success = true,
                    Data = allPayouts
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("PreviousSpins")]
        public IActionResult ShowPreviousSpins()
        {
            try
            {
                var result = _rouletteService.ShowPreviousSpins();
                return Ok(new ResponseResult
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GlobalError")]
        public IActionResult GlobalError()
        {
            throw new Exception();
        }
    }
}

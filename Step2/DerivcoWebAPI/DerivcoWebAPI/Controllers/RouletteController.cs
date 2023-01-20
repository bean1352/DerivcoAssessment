using DerivcoWebAPI.Models;
using DerivcoWebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DerivcoWebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RouletteController : ControllerBase
    {
        private readonly IRouletteService _rouletteService;
        public RouletteController(IRouletteService rouletteService)
        {
            _rouletteService = rouletteService;
        }

        [HttpPost("PlaceBet")]
        public async Task<IActionResult> PlaceBet([FromBody] List<Bet> bets)
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
                var result = await _rouletteService.PlaceBet(bets);
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
        public IActionResult Spin()
        {
            try
            {
                var result = _rouletteService.Spin();
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
        public IActionResult Payout([FromBody] List<Bet> bets)
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
                var result = _rouletteService.Payout(bets);
                return Ok(result);
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

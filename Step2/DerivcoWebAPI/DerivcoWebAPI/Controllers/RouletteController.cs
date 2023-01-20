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
        public async Task<ActionResult<ResponseResult>> PlaceBet([FromBody] Bet bet)
        {
            try
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
                var result = await _rouletteService.PlaceBet(bet);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GetAllBets")]
        public async Task<ActionResult<IEnumerable<Bet>>> GetAllBets()
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
        public ActionResult<ResponseResult> Spin()
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
        public ActionResult<ResponseResult> Payout([FromBody] Bet bet)
        {
            try
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
                var result = _rouletteService.Payout(bet);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("PreviousSpins")]
        public ActionResult<List<int>> ShowPreviousSpins()
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
    }
}

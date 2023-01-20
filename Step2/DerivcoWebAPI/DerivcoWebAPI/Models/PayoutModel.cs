using System.Text.Json.Serialization;

namespace DerivcoWebAPI.Models
{
    public class Payout: Bet
    {
        [JsonIgnore]
        public Guid PayoutID { get; set; }
        public bool? Win { get; set; } = false;
        public int? WinningNumber { get; set; }
        public double? WinningAmount { get; set; } = 0;
        public double? TotalWinningAmount { get; set; } = 0;
    }
}

namespace DerivcoWebAPI.Models
{
    public class Payout: Bet
    {
        public bool? Win { get; set; } = false;
        public int? WinningNumber { get; set; }
        public double? WinningAmount { get; set; } = 0;
        public double? TotalWinningAmount { get; set; } = 0;
    }
}

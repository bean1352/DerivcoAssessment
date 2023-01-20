namespace DerivcoWebAPI.Models
{
    public class ResponseResult
    {
        public bool Success { get; set; } = false;
        public string? Message { get; set; }
        public object? Data { get; set; }
    }
    public class Bet
    {
        public Guid UserID { get; set; }
        public Guid BetID { get; set; }
        public int BetNumber { get; set; }
        public double BetAmount { get; set; }
    }
    public class Payout
    {
        public bool Win { get; set; }
        public int? WinningNumber { get; set; }
        public double WinningAmount { get; set; }
    }
}

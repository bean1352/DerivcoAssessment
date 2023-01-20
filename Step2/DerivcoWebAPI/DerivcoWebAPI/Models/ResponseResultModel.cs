namespace DerivcoWebAPI.Models
{
    public class ResponseResult
    {
        public bool Success { get; set; } = false;
        public string? Message { get; set; }
        public object? Data { get; set; }
    }
}

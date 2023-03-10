using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DerivcoWebAPI.Models
{
    public class Bet 
    {
        [JsonIgnore]
        public Guid BetID { get; set; }
        [Range(0, 36)]
        public int? BetNumber { get; set; }
        [Range(1, double.PositiveInfinity)]
        public double BetAmount { get; set; }
        [Required]
        public BetType BetType { get; set; }
    }
    [Flags]
    public enum BetType
    {
        Red = 1,
        Black = 2,
        Number = 3,
        Even = 4,
        Odd = 5,
        OneToTwelve = 6,
        ThirteenTo24 = 7,
        TwentyfiveToThirtysix = 8,
        OneToEighteen = 9,
        NineteenToThirtysix = 10,
        FirstColumn = 11,
        SecondColumn = 12,
        ThirdColumn = 13
    }
}

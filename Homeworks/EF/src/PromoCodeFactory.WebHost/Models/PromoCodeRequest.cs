namespace PromoCodeFactory.WebHost.Models
{
    public class PromoCodeRequest
    {
        /// <summary>
        /// Код промокода.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Продолжительность действия промокода в днях.
        /// </summary>
        public int DurationInDays { get; set; }
    }
}

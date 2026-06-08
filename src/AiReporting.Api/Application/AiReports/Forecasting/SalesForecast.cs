namespace AiReporting.Api.Application.AiReports.Forecasting;

public sealed class SalesForecast
{
    public int ForecastYear { get; set; }
    public decimal PredictedTotal { get; set; }
    public decimal GrowthRate { get; set; }
    public string Method { get; set; } = string.Empty;
}

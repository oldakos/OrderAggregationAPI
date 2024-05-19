namespace OrderAggregationAPI.BackgroundWorker
{
    /// <summary>
    /// A simple configuration container required by the class `BackgroundForwarderService`
    /// </summary>
    public class BackgroundForwarderServiceConfig
    {
        public int PeriodSeconds { get; set; } = 20; //default value as per project specification
    }
}
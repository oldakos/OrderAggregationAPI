namespace OrderAggregationAPI.BackgroundWorker
{
    public class BackgroundForwarderServiceConfig
    {
        public int PeriodSeconds { get; set; } = 20; //default value as per project specification
    }
}
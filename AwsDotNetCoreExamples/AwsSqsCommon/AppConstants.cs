namespace AwsSqsCommon
{
    public class AppConstants
    {
        public string AccessKey { get; set; } = "";
        public string Secret { get; set; } = "";
        public string QueueUrl { get; set; }
        public string QueueName { get; set; }
        public string AwsRegion { get; set; } = "";
    }
}

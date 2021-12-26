using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace AwsSqsCommon
{
    public class SqsHelper : IDisposable
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly AppConstants _appConstants;

        public SqsHelper(AppConstants appConstants)
        {
            _appConstants = appConstants;
            BasicAWSCredentials basicCredentials = new BasicAWSCredentials(_appConstants.AccessKey, _appConstants.Secret);
            RegionEndpoint region = RegionEndpoint.GetBySystemName(_appConstants.AwsRegion);

            _sqsClient = new AmazonSQSClient(basicCredentials, region);
        }

        /// <summary>
        /// Return list of all queues on the account
        /// </summary>
        /// <returns></returns>
        public async Task<ListQueuesResponse> ShowQueues()
        {
            ListQueuesResponse response = await _sqsClient.ListQueuesAsync("");
            return response;
        }

        /// <summary>
        /// Create the queue if not exists
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public async Task CreateQueue(string queueName)
        {
            var queues = await ShowQueues();
            bool queueExists = false;

            foreach (var queue in queues.QueueUrls)
            {
                if (queue.Contains(queueName))
                {
                    Console.WriteLine($"{queueName} already exists, no need to create a new queue");
                    queueExists = true;
                    _appConstants.QueueUrl = queue;
                    _appConstants.QueueName = queueName;
                    break;
                }
            }

            if (!queueExists)
            {
                var attrs = new Dictionary<string, string>();

                CreateQueueResponse responseCreate = await _sqsClient.CreateQueueAsync(
                    new CreateQueueRequest { QueueName = queueName, Attributes = attrs });

                _appConstants.QueueUrl = responseCreate.QueueUrl;
                _appConstants.QueueName = queueName;
            }
        }

        /// <summary>
        /// Delete the queue
        /// </summary>
        /// <returns></returns>
        public async Task DeleteQueue()
        {
            try
            {
                Console.WriteLine($"Deleting queue {_appConstants.QueueName}...");
                await _sqsClient.DeleteQueueAsync(_appConstants.QueueUrl);
                Console.WriteLine($"Queue {_appConstants.QueueName} has been deleted.");
            }
            catch
            {

            }
        }

        /// <summary>
        /// Send message on the queue
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessageHandler(string message)
        {
            Console.WriteLine($"Add email '{message}' to the queue");
            _ = await _sqsClient.SendMessageAsync(_appConstants.QueueUrl, message);
            Console.WriteLine($"email added");
        }

        /// <summary>
        /// Receive, process and then delete the message
        /// </summary>
        /// <returns></returns>
        public async Task ReceiveMessageHandler()
        {
            var response = await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = _appConstants.QueueUrl,
                MaxNumberOfMessages = 1,
                WaitTimeSeconds = 0
                // (Could also request attributes, set visibility timeout, etc.)
            });

            foreach (var message in response.Messages)
            {
                //***
                //*** Process the message
                //***
                ProcessMessage(message);
                //***
                //*** Delete the message after processing
                //***
                await DeleteMessage(message);
            }
        }

        /// <summary>
        /// Process the message if it is a complex object
        /// </summary>
        /// <param name="message"></param>
        private void ProcessMessage(Message message)
        {
            Console.WriteLine($"sending email to: {message.Body}");
            Console.WriteLine($"email sent");
        }


        /// <summary>
        /// Delete message after it has been processed
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task DeleteMessage(Message message)
        {
            Console.WriteLine($"Deleting message {message.Body} from queue...");
            await _sqsClient.DeleteMessageAsync(_appConstants.QueueUrl, message.ReceiptHandle);
            Console.WriteLine($"Message deleted");
        }

        /// <summary>
        /// Dispose the sqsclient
        /// </summary>
        public void Dispose()
        {
            _sqsClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
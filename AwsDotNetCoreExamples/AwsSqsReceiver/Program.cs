// See https://aka.ms/new-console-template for more information
Console.WriteLine("AWS SQS Example");
Console.WriteLine("This app will act as a receiver to receive email addresses, process/prepare email and send it");

using (var sqsHelper = new AwsSqsCommon.SqsHelper(new AwsSqsCommon.AppConstants()))
{
    string txt = string.Empty;
    Console.WriteLine("type the name of the queue. If the queue do not exists, program will create it");
    txt = Console.ReadLine();
    await sqsHelper.CreateQueue(txt?.Trim());
    Console.WriteLine("queue created/configured. Program switching to receiving mode");

    do
    {
        await sqsHelper.ReceiveMessageHandler();
    } while (!Console.KeyAvailable);
    await sqsHelper.DeleteQueue();
}
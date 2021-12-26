// See https://aka.ms/new-console-template for more information
Console.WriteLine("AWS SQS Example");
Console.WriteLine("This app will act as a sender to queue email addresses into sqs queue");


using (var sqsHelper = new AwsSqsCommon.SqsHelper(new AwsSqsCommon.AppConstants()))
{
    string txt = string.Empty;
    Console.WriteLine("type the name of the queue. If the queue do not exists, program will create it");
    txt = Console.ReadLine();
    await sqsHelper.CreateQueue(txt?.Trim());

    Console.WriteLine("type email address to whom email needs to be sent and hit enter");
    Console.WriteLine("to quit, type 'q:'");

    do
    {
        txt = Console.ReadLine();
        if (string.IsNullOrEmpty(txt))
        {
            Console.WriteLine("No email received. try again");
        }
        else if (txt == "q:")
        {
            Console.WriteLine("Quitting program");
        }
        else
        {
            Console.WriteLine($"Queueing email: {txt}");
            await sqsHelper.SendMessageHandler(txt);
        }
    } while (txt != "q:");

    await sqsHelper.DeleteQueue();
}
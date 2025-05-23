using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(">>> Booking Messaging Test Started...");
        var service = new TestConsumerService();

        Console.WriteLine(">>> Press [enter] to exit.");
        Console.ReadLine();

        service.Close();
    }
}

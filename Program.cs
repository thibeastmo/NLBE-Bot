using System;

namespace NLBE_Bot
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Bot TheBot = new Bot();
            TheBot.RunAsync().GetAwaiter().GetResult();
        }
    }
}

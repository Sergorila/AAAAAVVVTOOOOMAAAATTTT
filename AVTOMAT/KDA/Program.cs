using System;

namespace KDA
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter input sequence: ");
            string input = Console.ReadLine();
            Console.WriteLine();

            var stateMachine = new StateMachine(@"C:\Users\Sergey\Desktop\Languages-main\KDA\test.json");
            var result = stateMachine.ValidateWord(input.ToCharArray());

            Console.WriteLine();
            Console.WriteLine($"Result: {result}");
        }
    }
}

using System;
using System.Collections.Generic;

namespace NDA
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Исходный НКА");
            var automaton = new StateMachine(@"C:\Users\Sergey\source\repos\AVTOMAT\NDA\test.json");
            Console.WriteLine("Проверка слова для исходного НКА");
            Console.WriteLine(automaton.ValidateWord(new List<String>() { automaton.BeginState }, "1", new List<string>()));
            Console.WriteLine();

            Console.WriteLine("Полученный КДА");
            automaton.ValidateKDA();
        }
    }
}

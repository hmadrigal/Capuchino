using System;

namespace Capuchino.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var expression = new RegDecoder.Expression(
                literal:"\\s", offset:0, WS:false,isECMA: false
                );
            Console.WriteLine("Hello World!");
        }
    }
}

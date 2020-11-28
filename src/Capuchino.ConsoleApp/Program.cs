using System;
using System.Collections.Generic;
using System.Linq;
using Expresso.Ultrapico.Expresso;
using RegDecoder;

namespace Capuchino.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var regLibrary = new RegLibrary();
            var libraryItems = new List<RegexLibraryItem>();
            regLibrary.SetDefaults(libraryItems);

            var parsedExpressions =
                     (  from libraryItem in libraryItems
                      select new Expression( libraryItem.Expression, 0, false, false)
                      ).ToList();

            Console.ReadKey();
        }
    }
}

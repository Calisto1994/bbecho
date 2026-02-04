using System.Globalization;

namespace Calisto
{
    public class Test
    { 
        static void Main(string[] args)
        {
            string message = string.Join(" ", args);
            ColorUtil.TryParse(ref message);
            Console.WriteLine(message);
        }
    }
}
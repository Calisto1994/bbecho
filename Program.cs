using System;

namespace Calisto
{
    public class Test
    { 
        static void Main(string[] args)
        {
            string message;
            string? exceptionMessage;
            if (Console.IsInputRedirected)
            {
                message = Console.In.ReadToEnd().TrimEnd('\r', '\n');
            }
            else
            {
                message = string.Join(" ",args);
            }

            if (!string.IsNullOrEmpty(message))
            {
                if (ColorUtil.TryParse(ref message, out exceptionMessage))
                {
                    Console.Out.WriteLine(message);
                    Environment.Exit(0);
                }
                else
                {
                    Console.Error.WriteLine(exceptionMessage);
                    Environment.Exit(1);
                } 
            }
        }
    }
}
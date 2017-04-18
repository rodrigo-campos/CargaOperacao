using System;
using System.Diagnostics;

namespace CargaOperacao
{
    public class Util
    {
        public static void WithWatch(Func<Stopwatch, string> messageFn, Action action)
        {
            var watch = new Stopwatch();
            watch.Start();

            action();

            watch.Stop();
            Console.WriteLine(messageFn(watch) ?? string.Empty);
        }
    }
}
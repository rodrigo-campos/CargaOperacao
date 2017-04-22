using System;
using System.Diagnostics;

namespace CargaOperacao
{
    public class Util
    {
        public static T WithWatch<T>(Func<Stopwatch, string> messageFn, Func<T> func)
        {
            var watch = new Stopwatch();
            watch.Start();

            var @return = func();

            watch.Stop();
            Console.WriteLine(messageFn(watch) ?? string.Empty);
            return @return;
        }
    }
}
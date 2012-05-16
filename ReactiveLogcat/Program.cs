using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using XSpect.Yacq;

namespace Azyobuzi.ReactiveLogcat
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "RectiveLogcat";

            Expression<Func<LogOutput, bool>> expr = null;

            if (args.Any())
            {
                try
                {
                    expr = YacqServices.ParseFunc<LogOutput, bool>(String.Join(" ", args));
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine(ex.ToString());
                    Console.ForegroundColor = ConsoleColor.Gray;
                    return;
                }
            }

            expr = expr ?? YacqServices.ParseFunc<LogOutput, bool>("true");

            var disp = Adb.Logcat().AsQbservable()
                .Where(expr)
                .Subscribe(
                    log =>
                    {
                        Console.ForegroundColor = log.Priority == LogPriority.Debug
                            ? ConsoleColor.Green
                            : log.Priority == LogPriority.Info
                                ? ConsoleColor.Cyan
                                : log.Priority == LogPriority.Warning
                                    ? ConsoleColor.Yellow
                                    : log.Priority == LogPriority.Error
                                        ? ConsoleColor.Red
                                        : log.Priority == LogPriority.Fatal
                                            ? ConsoleColor.DarkRed
                                            : ConsoleColor.Gray;

                        Console.WriteLine(log.Time.ToLongTimeString() + " " + log.Tag);
                        Console.WriteLine(log.Message);
                        Console.WriteLine();
                    },
                    () =>
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Environment.Exit(0);
                    }
                );

            Console.ReadKey();
            Console.ForegroundColor = ConsoleColor.Gray;
            disp.Dispose();
        }
    }
}

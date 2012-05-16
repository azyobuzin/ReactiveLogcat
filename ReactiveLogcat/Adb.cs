using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;

namespace Azyobuzi.ReactiveLogcat
{
    public static class Adb
    {
        public static IObservable<LogOutput> Logcat()
        {
            return Observable.Create<LogOutput>(observer =>
            {
                var p = new Process()
                {
                    StartInfo = new ProcessStartInfo("adb.exe", "logcat -v long")
                    {
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    },
                    EnableRaisingEvents = true
                };

                p.Exited += (sender, e) => observer.OnCompleted();

                Scheduler.TaskPool.Schedule(() =>
                {
                    p.Start();

                    var sr = p.StandardOutput;
                    string line = sr.ReadLine();
                    while (!sr.EndOfStream)
                    {
                        if (line.StartsWith("["))
                        {
                            try
                            {
                                var output = new LogOutput();

                                var split = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                var splitDate = split[1].Split('-');
                                var splitTime = split[2].Split(':');
                                var splitMillisecond = splitTime.Last().Split('.');
                                output.Time = new DateTime(
                                    DateTime.Now.Year,
                                    int.Parse(splitDate[0]),
                                    int.Parse(splitDate[1]),
                                    int.Parse(splitTime[0]),
                                    int.Parse(splitTime[1]),
                                    int.Parse(splitMillisecond[0]),
                                    int.Parse(splitMillisecond[1])
                                );

                                var tag = split[4];
                                var priority = tag[0];
                                output.Tag = tag.Substring(2);
                                output.Priority = priority == 'V'
                                    ? LogPriority.Verbose
                                    : priority == 'D'
                                        ? LogPriority.Debug
                                        : priority == 'I'
                                            ? LogPriority.Info
                                            : priority == 'W'
                                                ? LogPriority.Warning
                                                : priority == 'E'
                                                    ? LogPriority.Error
                                                    : priority == 'F'
                                                        ? LogPriority.Fatal
                                                        : LogPriority.Silent;

                                var sb = new StringBuilder();
                                line = sr.ReadLine();
                                while (!sr.EndOfStream && !line.StartsWith("["))
                                {
                                    sb.AppendLine(line);
                                    line = sr.ReadLine();
                                }
                                output.Message = sb.ToString().Trim();

                                observer.OnNext(output);
                            }
                            catch
                            {
                                line = sr.ReadLine();
                            }
                        }
                        else
                        {
                            line = sr.ReadLine();
                        }
                    }
                });

                return () => p.Kill();
            });
        }
    }
}

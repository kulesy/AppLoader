using System;

namespace AppLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter The App You Want To Run:");
            var appToStart = Console.ReadLine();
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = $"/C start {appToStart}";
            startInfo.WorkingDirectory = "C:\\Users\\The Little Monkey\\source\\repos\\AppLoader\\Apps";
            process.StartInfo = startInfo;
            process.Start();
        }
    }
}

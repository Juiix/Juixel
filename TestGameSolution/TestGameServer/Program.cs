using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestGameServer.Networking.Clients;
using TestGameServer.Worlds;
using Utilities.Logging;

namespace TestGameServer
{
    public class Program
    {
        public static ClientManager ClientManager;
        public static WorldManager WorldManager;

        private static bool StopLog = false;

        public static void Main(string[] args)
        {
            ClientManager = new ClientManager();

            WorldManager = new WorldManager();

            var LogThread = new Thread(() =>
            {
                while (!StopLog)
                    Logger.StepLog();
            });
            LogThread.Priority = ThreadPriority.Lowest;
            LogThread.Start();

            while (Console.ReadKey().Key != ConsoleKey.Escape)
            {

            }

            Stop();
        }

        /// <summary>
        /// Stops the server
        /// </summary>
        public static void Stop()
        {
            WorldManager.Stop();
            StopLog = true;
        }
    }
}

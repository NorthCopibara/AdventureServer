using System;

namespace CSharpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerHandleNetworkData.InitializeNetworkPackages();
            ServerTCP.SetupServer();
            Console.ReadLine();
        }
    }
}
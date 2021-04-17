using System;
using System.Collections.Generic;

namespace CSharpServer
{
    public enum ServerPackets
    {
        SConnectionOk = 1,
        SMessage = 2,
        SInteger = 3,
        SFloat = 4,
        SPosition = 4,
    }

    //get send from client to server
    //server has to listen to client packets
    public enum ClientPackets
    {
        CThankYou = 1,
        CMessage = 2,
        CInteger = 3,
        CFloat = 4,
    }

    public class ServerHandleNetworkData
    {
        private delegate void Packet(int index, byte[] data);

        private static Dictionary<int, Packet> _packets;

        public static void InitializeNetworkPackages()
        {
            Console.WriteLine("Initialize network packages");
            _packets = new Dictionary<int, Packet>
            {
                {
                    (int) ClientPackets.CThankYou,
                    HandleThankYou
                },
                {
                    (int) ClientPackets.CMessage,
                    HandleString
                },
                {
                    (int) ClientPackets.CInteger,
                    HandleString
                }
            };
        }

        private static void HandleThankYou(int index, byte[] data)
        {
            var buffer = new PacketBuffer();
            buffer.WriteBytes(data);
            buffer.ReadInteger();
            var msg = buffer.ReadString();
            buffer.Dispose();

            //Add your code want to execute hear:
            Console.WriteLine(msg);
        }

        private static void HandleString(int index, byte[] data)
        {
            var buffer = new PacketBuffer();
            buffer.WriteBytes(data);
            buffer.ReadInteger();
            var msg = buffer.ReadString();
            buffer.Dispose();

            //Add your code want to execute hear:
            Console.WriteLine("Message from {1}: {0}", msg, index);
            //ServerTCP.SendMessage(index,"Poshol naxui " + index);
            ServerTCP.SendMessageAll("Poshol naxui " + index);
        }

        public static void HandleNetworkInformation(int index, byte[] data)
        {
            int packetNum;
            var buffer = new PacketBuffer();
            buffer.WriteBytes(data);
            packetNum = buffer.ReadInteger();
            buffer.Dispose();
            if (_packets.TryGetValue(packetNum, out Packet packet))
            {
                packet.Invoke(index, data);
            }
        }
    }
}
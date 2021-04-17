using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;

namespace CSharpServer
{
    public class ServerTCP
    {
        private const int PORT = 5555;
        
        private static Socket _serverSocket =
            new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static byte[] _buffer = new byte[1024];

        public static readonly Client[] Clients = new Client[Constants.MAX_PLAYER];
        
        public static void SetupServer()
        {
            for (var i = 0; i < Constants.MAX_PLAYER; i++)
            {
                Clients[i] = new Client();
            }
            
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            _serverSocket.Listen(10);
            _serverSocket.BeginAccept(AcceptCallback, null);
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            var socket = _serverSocket.EndAccept(ar);
            _serverSocket.BeginAccept(AcceptCallback, null);

            for (var i = 0; i < Constants.MAX_PLAYER; i++)
            {
                if (Clients[i].ClientSocket == null)
                {
                    Clients[i].ClientSocket = socket;
                    Clients[i].Index = i;
                    Clients[i].Ip = socket.RemoteEndPoint.ToString();
                    Clients[i].StartClient();
                    
                    Console.WriteLine("Connection from '{0}' received", Clients[i].Ip);
                    SendConnectionOK(i);
                    return;
                }
            }
        }

        public static void SendDataTo(int index, byte[] data)
        {
            byte[] sizeInfo = new byte[4];
            sizeInfo[0] = (byte) data.Length;
            sizeInfo[1] = (byte) (data.Length >> 8);
            sizeInfo[2] = (byte) (data.Length >> 16);
            sizeInfo[3] = (byte) (data.Length >> 24);

            Clients[index].ClientSocket.Send(sizeInfo);
            Clients[index].ClientSocket.Send(data);
        }

        public static void SendMessage(int index, string message)
        {
            PacketBuffer buffer = new PacketBuffer();
            buffer.WriteInteger((int)ServerPackets.SMessage);
            buffer.WriteString(message);
            SendDataTo(index, buffer.ToArray);
            buffer.Dispose();
        }

        public static void SendConnectionOK(int index)
        {
            PacketBuffer buffer = new PacketBuffer();
            buffer.WriteInteger((int)ServerPackets.SConnectionOk);
            buffer.WriteString("You are successfully  connected to the server");
            SendDataTo(index, buffer.ToArray);
            buffer.Dispose();
        }
    }

    public class Client
    {
        public int Index;
        public string Ip;
        public Socket ClientSocket;
        public bool closing = false;
        private byte[] _buffer = new byte[1024];

        public void StartClient()
        {
            ClientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, ClientSocket);
            closing = false;
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var socket = (Socket) ar.AsyncState;

            try
            {
                var received = socket.EndReceive(ar);
                if (received <= 0)
                {
                    CloseClient(Index);
                }
                else
                {
                    var dataBuffer = new byte[received];
                    Array.Copy(_buffer, dataBuffer, received);
                    ServerHandleNetworkData.HandleNetworkInformation(Index, dataBuffer);
                    socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, ClientSocket);
                }
            }
            catch (Exception)
            {
                CloseClient(Index);
            }
        }

        private void CloseClient(int index)
        {
            closing = true;
            Console.WriteLine("Connection from {0} has ben terminated.", Ip);
            //PlayerLeftGame
            ClientSocket.Close();
        }
    }

    public class Room
    {
        private List<Client> _clients;
        
        public Room(Client[] clients)
        {
            _clients = clients.ToList();
        }
        
        
    }
}
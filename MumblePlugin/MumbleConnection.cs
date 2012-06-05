using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;

using ProtoBuf;
using MumbleProto;
using System.Threading;

namespace Plugin
{


    class MumbleConnection
    {
        private int port;
        private string host;
        private bool connected;
        
        private uint mumbleVersion;
        private string username;

        private TcpClient tcpClient;
        private Stream sslStream;

        private Thread listenThread;
        private Thread pingerThread;

        public event EventHandler<MumblePacketEventArgs> PacketReceivedEvent;


        public MumbleConnection()
        {
            port = 64738;

            host = "apophis.ch";
            username = "LeChuck";

            connected = false;

            mumbleVersion = (1 << 16) + (2 << 8) + 3;
        }


        public void Connect()
        {
            tcpClient = new TcpClient(host, port);
            sslStream = new System.Net.Security.SslStream(tcpClient.GetStream(), true);

            connected = true;

            PacketReceivedEvent += OnPacketEvent;

            listenThread = new Thread(Listen);
            listenThread.Start();

            pingerThread = new Thread(Pinger);
            pingerThread.Start();
        }

        public void SendVersion(string clientVersion)
        {
            var message = new MumbleProto.Version();
            
            message.release = clientVersion;
            message.version = mumbleVersion;
            message.os = Environment.OSVersion.Platform.ToString();
            message.os_version = Environment.OSVersion.VersionString;

            MumbleWrite(message);
        }


        public void SendAuthenticate()
        {
            var message = new MumbleProto.Authenticate();

            message.username = username;
            message.celt_versions.Add(-2147483637);

            MumbleWrite(message);
        }



        private void Listen()
        {
            while (connected)
            {
                var message = MumbleRead();
                if (message == null)
                {
                    break;
                }

                var temp = PacketReceivedEvent;
                if (temp != null)
                {
                    temp(this, new MumblePacketEventArgs(message));
                }
            }
            connected = false;
        }

        private void OnPacketEvent(object sender, MumblePacketEventArgs args)
        {
            System.Console.WriteLine("Received" + args.Message.GetType().ToString());
        }


        private void Pinger()
        {
        }

        private void MumbleWrite(IExtensible message)
        {
            var sslStreamWriter = new BinaryWriter(sslStream);
            if (message is UDPTunnel)
            {
                var audioMessage = message as UDPTunnel;

                Int16 messageType = (Int16)MumbleProtocolFactory.MessageType(message);
                Int32 messageSize = (Int32)audioMessage.packet.Length;

                sslStreamWriter.Write(messageType);
                sslStreamWriter.Write(messageSize);
                sslStreamWriter.Write(audioMessage.packet);
            }
            else
            {
                MemoryStream messageStream = new MemoryStream();
                Serializer.Serialize(messageStream, message);

                Int16 messageType = (Int16)MumbleProtocolFactory.MessageType(message);
                Int32 messageSize = (Int32)messageStream.Length;

                sslStreamWriter.Write(messageType);
                sslStreamWriter.Write(messageSize);
                messageStream.Position = 0;
                messageStream.CopyTo(sslStream);
            }
        }

        private IExtensible MumbleRead()
        {
            var sslStreamReader = new BinaryReader(sslStream);

            var header = sslStreamReader.ReadBytes(6);
            IExtensible result = null;

            Int16 type = BitConverter.ToInt16(header, 0);
            Int32 size = BitConverter.ToInt32(header, 2);

            if (type == (int)MessageTypes.UDPTunnel)
            {
                result = new UDPTunnel() { packet = sslStreamReader.ReadBytes(size) };
            }
            else
            {
                result = MumbleProtocolFactory.Deserialize((MessageTypes)type, size, sslStreamReader);
            }
            return result;
        }
    }
}

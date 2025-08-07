using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace work.ctrl3d
{
    public class UDP : IDisposable
    {
        private readonly UdpClient _udp;
        private readonly Encoding _encoding;
        private IPEndPoint _remoteEndPoint;
    
        public event Action<byte[], IPEndPoint> OnBytesReceived;
        public event Action<string, IPEndPoint> OnStringReceived;

        public UDP(int port, Encoding encoding = null)
        {
            _udp = new UdpClient(port);
            _encoding = encoding ?? Encoding.UTF8;
            _remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
            _udp.BeginReceive(ReceiveData, null);
        }

        private void ReceiveData(IAsyncResult result)
        {
            var receivedBytes = _udp.EndReceive(result, ref _remoteEndPoint);
            OnBytesReceived?.Invoke(receivedBytes, _remoteEndPoint);
        
            var receivedMessage = _encoding.GetString(receivedBytes);
            OnStringReceived?.Invoke(receivedMessage, _remoteEndPoint);
        
            _udp.BeginReceive(ReceiveData, null);
        }

        public void SendBytes(byte[] sendBytes, IPEndPoint endPoint)
        {
            _udp.Send(sendBytes, sendBytes.Length, endPoint);
        }

        public void SendString(string message, IPEndPoint endPoint)
        {
            var sendBytes = _encoding.GetBytes(message);
            SendBytes(sendBytes, endPoint);
        }

        public void Dispose()
        {
            _udp?.Close();
            _udp?.Dispose();
        }
    }
}
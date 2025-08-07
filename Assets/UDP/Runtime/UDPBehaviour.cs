using System;
using System.Net;
using UnityEngine;
using UnityEngine.Events;
#if ALCHEMY_SUPPORT
using Alchemy.Inspector;
#endif

namespace work.ctrl3d
{
    public class UDPBehaviour : MonoBehaviour
    {
        [Header("Settings")] [SerializeField] private int port = 5555;
        [SerializeField] private EncodingType encodingType = EncodingType.UTF8;
        [SerializeField] private bool autoListen = true;
        [SerializeField] private bool useLog = true;
        [SerializeField] private Color logColor;

        [Header("Events")] public UnityEvent<byte[], IPEndPoint> onBytesReceived;
        public UnityEvent<string, IPEndPoint> onStringReceived;

        public Action<byte[], IPEndPoint> OnBytesReceived;
        public Action<string, IPEndPoint> OnStringReceived;

        public int Port => port;
        public EncodingType EncodingType => encodingType;

        private UDP _udp;

        private void Awake()
        {
            if (autoListen) Listen();
        }

        private void OnStringReceivedHandler(string message, IPEndPoint endPoint)
        {
            onStringReceived?.Invoke(message, endPoint);
            OnStringReceived?.Invoke(message, endPoint);

            if (!useLog) return;
            Debug.Log($"UDP {endPoint.Address}:{endPoint.Port} -> {message}".WithColor(logColor));
        }

        private void OnBytesReceivedHandler(byte[] bytes, IPEndPoint endPoint)
        {
            onBytesReceived?.Invoke(bytes, endPoint);
            OnBytesReceived?.Invoke(bytes, endPoint);
        }


#if ALCHEMY_SUPPORT
        [Button, HorizontalGroup("Buttons")]
#endif
        public void Listen()
        {
            _udp = new UDP(port, encodingType.ToEncoding());

            _udp.OnStringReceived += OnStringReceivedHandler;
            _udp.OnBytesReceived += OnBytesReceivedHandler;

            if (!useLog) return;
            Debug.Log($"<b>[{name}]</b> Listening for UDP packets on port {port}".WithColor(logColor));
        }

#if ALCHEMY_SUPPORT
        [Button, HorizontalGroup("Buttons")]
#endif
        public void Close()
        {
            _udp.OnStringReceived -= OnStringReceivedHandler;
            _udp.OnBytesReceived -= OnBytesReceivedHandler;

            if (!useLog) return;
            Debug.Log($"<b>[{name}]</b> UDP Listener socket closed.".WithColor(logColor));


            Disconnect();
        }


#if ALCHEMY_SUPPORT
        [Button]
#endif
        public void SendString(string message, string ipAddress, int port)
        {
            if (_udp != null)
            {
                _udp.SendString(message, new IPEndPoint(IPAddress.Parse(ipAddress), port));
            }
            else
            {
                Debug.LogError($"<b>[{name}]</b> UDP is not listening.");
            }
        }

        public void SendBytes(byte[] sendBytes, string ipAddress, int port)
        {
            if (_udp != null)
            {
                _udp.SendBytes(sendBytes, new IPEndPoint(IPAddress.Parse(ipAddress), port));
            }
            else
            {
                Debug.LogError($"<b>[{name}]</b> UDP is not listening.");
            }
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        private void Disconnect()
        {
            if (_udp == null) return;
            _udp.Dispose();
            _udp = null;
        }
    }
}
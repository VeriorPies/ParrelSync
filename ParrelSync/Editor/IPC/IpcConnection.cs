using System;
using System.Net;

namespace ParrelSync.Ipc
{
    internal class IpcConnection : IEquatable<IpcConnection>, IDisposable
    {
        public EndPoint Endpoint => _tcpConnection.EndPoint;

        private readonly TcpConnection _tcpConnection;

        public IpcConnection(TcpConnection tcpConnection)
        {
            _tcpConnection = tcpConnection;
        }

        public void Send<T>(T message) where T : struct, IIpcMessage
        {
            QueuedIpcMessage serializedMessage = IpcMessages<T>.serializationMethod(message);
            _tcpConnection.Send(serializedMessage);
        }

        public bool Equals(IpcConnection other)
        {
            if (other is null)
            {
                return false;
            }

            return Endpoint.Equals(other.Endpoint);
        }

        public override bool Equals(object other)
        {
            if (other is IpcConnection otherIpc)
            {
                return this.Equals(otherIpc);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Endpoint.GetHashCode();
        }

        public override string ToString()
        {
            return _tcpConnection.EndPoint.ToString();
        }

        public void Dispose()
        {
            _tcpConnection.Dispose();
        }
    }

    internal class IpcConnectionToClient : IpcConnection
    {
        public IpcConnectionToClient(TcpConnection tcpConnection) : base(tcpConnection) { }
    }

    internal class IpcConnectionToServer : IpcConnection
    {
        public IpcConnectionToServer(TcpConnection tcpConnection) : base(tcpConnection) { }
    }
}

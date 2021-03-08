using System;

namespace NetMQ
{
    /// <summary>
    /// This enum-type specifies socket transport events (TCP and IPC only).
    /// </summary>
    [Flags]
    public enum SocketEvents
    {
        /// <summary>
        /// Socket got connected
        /// </summary>
        Connected = 1,
        
        /// <summary>
        /// Connect delayed
        /// </summary>
        ConnectDelayed = 2,
        
        /// <summary>
        /// Connect Retried
        /// </summary>
        ConnectRetried = 4,

        /// <summary>
        /// Socket is listening
        /// </summary>
        Listening = 8,
        
        /// <summary>
        /// Socket bind failed
        /// </summary>
        BindFailed = 16,

        /// <summary>
        /// Peer is accepted
        /// </summary>
        Accepted = 32,
        
        /// <summary>
        /// Accept failed
        /// </summary>
        AcceptFailed = 64,

        /// <summary>
        /// Socket is closed
        /// </summary>
        Closed = 128,
        
        /// <summary>
        /// Failed to close socket
        /// </summary>
        CloseFailed = 256,

        /// <summary>
        /// Socket disconnected
        /// </summary>
        Disconnected = 512,

        /// <summary>
        /// Handshake failed
        /// </summary>
        HandshakeFailedNoDetail = 1024,

        /// <summary>
        /// Handshake failed the protocol
        /// </summary>
        HandshakeFailedProtocol = 2048,

        /// <summary>
        /// Handshake failed the authentication
        /// </summary>
        HandshakeFailedAuth = 4096,

        /// <summary>
        /// Handshake succeeded
        /// </summary>
        HandshakeSucceeded = 8192,

        /// <summary>
        /// Listen to all events
        /// </summary>
        All = Connected | ConnectDelayed |
              ConnectRetried | Listening |
              BindFailed | Accepted |
              AcceptFailed | Closed |
              CloseFailed | Disconnected | 
              HandshakeFailedNoDetail | HandshakeFailedProtocol | 
              HandshakeFailedAuth | HandshakeSucceeded
    }
}
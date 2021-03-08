using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMQ.Core.Mechanisms
{
    class PlainClientMechanism : Mechanism
    {
        private enum State
        {
            SENDING_HELLO,
            WAITING_FOR_WELCOME,
            SENDING_INITIATE,
            WAITING_FOR_READY,
            ERROR_COMMAND_RECEIVED,
            READY
        }

        private State state;

        public PlainClientMechanism(SessionBase session, Options options) : base(session, options)
        {
            Console.WriteLine("Creating Plain Client Mechanism");
            state = State.SENDING_HELLO;
        }

        public override MechanismStatus Status
        {
            get
            {
                if (state == State.READY)
                {
                    return MechanismStatus.Ready;
                }
                else if (state == State.ERROR_COMMAND_RECEIVED)
                {
                    return MechanismStatus.Error;
                }
                else
                {
                    return MechanismStatus.Handshaking;
                }
            }
        }

        public override PushMsgResult ProcessHandshakeCommand(ref Msg msg)
        {
            Console.WriteLine("Processing Handshake Command with internal state: " + state);
            Console.Write("Received Message: ");
            PrintMessage(msg);
            int dataSize = msg.Size;
            PushMsgResult result;
            if (state == State.WAITING_FOR_WELCOME)
            {
                result = ProcessWelcome(msg);
            }
            else if (IsCommand("READY", ref msg))
            {
                result = ProcessReady(msg);
            }
            else if (IsCommand("ERROR", ref msg))
            {
                result = ProcessError(msg);
            }
            else
            {
                //  Temporary support for security debugging
                Console.WriteLine("PLAIN Client I: invalid handshake command");
                result = PushMsgResult.Error;
            }
            Console.Write("Processed command: ");
            PrintMessage(msg);
            Console.WriteLine("Internal state after generating next command: " + state);
            return result;
        }

        public override PullMsgResult NextHandshakeCommand(ref Msg msg)
        {
            Console.WriteLine("Creating next Handshake Command with internal state: " + state);
            PullMsgResult result;
            switch (state)
            {
                case State.SENDING_HELLO:
                    result = ProduceHello(ref msg);
                    if (result == PullMsgResult.Ok)
                    {
                        state = State.WAITING_FOR_WELCOME;
                    }
                    break;
                case State.SENDING_INITIATE:
                    result = ProduceInitiate(ref msg);
                    if (result == PullMsgResult.Ok)
                    {
                        state = State.WAITING_FOR_READY;
                    }
                    break;
                default:
                    result = PullMsgResult.Empty;
                    break;

            }
            Console.Write("Sending command: ");
            PrintMessage(msg);
            Console.WriteLine("Internal state after creating next command: " + state);
            return result;
        }

        private PullMsgResult ProduceHello(ref Msg msg)
        {
            Console.WriteLine("Producing Hello");
            String plainUsername = Options.PlainUsername;
            String plainPassword = Options.PlainPassword;
            string command = "HELLO";

            // Console.WriteLine("Putting Hello");
            int commandSize = 1 + command.Length + 1 + plainUsername.Length + 1 + plainPassword.Length;
            msg.InitPool(commandSize);


            // msg.Put((byte)command.Length, 0);
            msg.Put((byte) 5, 0);

            msg.Put(Encoding.ASCII, command, 1);
            // Console.WriteLine("Putting Username");
            msg.Put((byte)plainUsername.Length, 1 + command.Length);
            msg.Put(Encoding.ASCII, plainUsername, 1 + command.Length + 1);
            // Console.WriteLine("Putting Password");
            msg.Put((byte)plainPassword.Length, 1 + command.Length + 1 + plainUsername.Length);
            msg.Put(Encoding.ASCII, plainPassword, 1 + command.Length + 1 + plainUsername.Length + 1);

            // Console.WriteLine("Initialized: " + msg.IsInitialised);

            return PullMsgResult.Ok;
        }

        private PullMsgResult ProduceInitiate(ref Msg msg)
        {
            Console.WriteLine("Producing Initiate");
            string initiativeString = "INITIATE";
            //  Add mechanism string
            // int commandSize = 1 + initiativeString.Length + GetPropertyLength(ZmtpPropertySocketType, socketType.Length);
            string socketTypeName = "Socket-Type";
            string socketTypeValueName = GetSocketName(Options.SocketType);

            MakeCommandWithBasicProperties(ref msg, "INITIATE");

            // int commandSize = 1 + initiativeString.Length + 1 + socketTypeName.Length + 1 + socketTypeValueName.Length;
            // msg.InitPool(commandSize);

            // Console.WriteLine("Adding Command Name");
            // msg.Put((byte)initiativeString.Length, 0);
            // msg.Put(Encoding.ASCII, initiativeString, 1);

            // Console.WriteLine("Adding Property Name");
            // msg.Put((byte)socketTypeName.Length, 1 + initiativeString.Length);
            // msg.Put(Encoding.ASCII, socketTypeName, 1 + initiativeString.Length + 1);

            // Console.WriteLine("Adding Value");
            // msg.Put((byte)socketTypeValueName.Length, 1 + initiativeString.Length + 1 + socketTypeName.Length);
            // msg.Put(Encoding.ASCII, socketTypeValueName, 1 + initiativeString.Length + 1 + socketTypeName.Length + 1);


            // msg.InitPool(commandSize);
            // msg.Put((byte)initiativeString.Length);
            // msg.Put(Encoding.ASCII, initiativeString, 1);

            //  Add socket type property
            // AddProperty(msg, ZmtpPropertySocketType, socketType);

            //  Add identity property
            // if (Options.SocketType == ZmqSocketType.Req || Options.SocketType == ZmqSocketType.Dealer || Options.SocketType == ZmqSocketType.Router)
            // {
            // .NotNull(Options.Identity);
            // AddProperty(msg, ZmtpPropertyIdentity, Options.Identity);
            // }

            return PullMsgResult.Ok;
        }

        private PushMsgResult ProcessWelcome(Msg msg)
        {
            Console.WriteLine("Processing Welcome");
            if (state != State.WAITING_FOR_WELCOME)
            {
                return PushMsgResult.Error;
            }
            if (!IsCommand("WELCOME", ref msg))
            {
                Console.WriteLine("Isn't Welcome :(");
                return PushMsgResult.Error;
            }
            state = State.SENDING_INITIATE;
            return PushMsgResult.Ok;
        }

        private PushMsgResult ProcessReady(Msg msg)
        {
            Console.WriteLine("Process Ready");
            if (state != State.WAITING_FOR_READY)
            {
                return PushMsgResult.Error;
            }
            if (!ParseMetadata(msg.Slice(ReadyCommandName.Length + 1)))
                return PushMsgResult.Error;

            state = State.READY;
            return PushMsgResult.Ok;
        }

        private PushMsgResult ProcessError(Msg msg)
        {
            Console.WriteLine("Process Error");
            if (state != State.WAITING_FOR_WELCOME && state != State.WAITING_FOR_READY)
            {
                Session.Socket.EventHandshakeFailedProtocol(Session.GetAddress, ErrorCode.ProtocolNotSupported);
                return PushMsgResult.Error;
            }
            state = State.ERROR_COMMAND_RECEIVED;
            return PushMsgResult.Error;
        }

        public override void Dispose()
        {

            Console.WriteLine("DISPOSING Plain Client Mechanism.\n");
        }
    }
}

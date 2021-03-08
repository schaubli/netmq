using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMQ.Core.Mechanisms
{
    class PlainServerMechanism: Mechanism
    {
        private enum State
        {
            WAITING_FOR_HELLO,
            SENDING_WELCOME,
            WAITING_FOR_INITIATE,
            SENDING_READY,
            WAITING_FOR_ZAP_REPLY,
            SENDING_ERROR,
            ERROR_COMMAND_SENT,
            READY
        }

        private State state;

        public PlainServerMechanism(SessionBase session, Options options) : base(session, options)
        {
            Console.WriteLine("Creating Plain Server Mechanism");
            state = State.WAITING_FOR_HELLO;
        }

        public override MechanismStatus Status
        {
            get
            {
                if (state == State.READY)
                {
                    return MechanismStatus.Ready;
                }
                else if (state == State.ERROR_COMMAND_SENT)
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
            PushMsgResult result;
            switch (state)
            {
                case State.WAITING_FOR_HELLO:
                    result = ProcessHello(ref msg);
                    if (result == PushMsgResult.Ok)
                    {
                        state = State.SENDING_WELCOME;
                    }
                    break;
                case State.WAITING_FOR_INITIATE:
                    result = ProcessInitiate(ref msg);
                    if (result == PushMsgResult.Ok)
                    {
                        state = State.SENDING_READY;
                    }
                    break;
                default:
                    result = PushMsgResult.Error;
                    break;

            }
            Console.Write("Processed command: ");
            PrintMessage(msg);
            Console.WriteLine("Internal state after generating next command: " + state);
            return result;
        }

        private PushMsgResult ProcessHello(ref Msg msg)
        {
            PushMsgResult result = PushMsgResult.Ok;
            if (!IsCommand("WELCOME", ref msg))
            {
                Console.WriteLine("Wrong command received for state " + state);
                return PushMsgResult.Error;
            }


            // TODO: Check username & password
            
            return result;
        }

        private PushMsgResult ProcessInitiate(ref Msg msg)
        {
            PushMsgResult result = PushMsgResult.Ok;
            if (!IsCommand("INITIATE", ref msg))
            {
                Console.WriteLine("Wrong command received for state " + state);
                return PushMsgResult.Error;
            }
            return result;
        }

        public override PullMsgResult NextHandshakeCommand(ref Msg msg)
        {
            Console.WriteLine("Creating next Handshake Command with internal state: " + state);
            PullMsgResult result;
            switch (state)
            {
                case State.SENDING_WELCOME:
                    result = ProduceWelcome(ref msg);
                    if (result == PullMsgResult.Ok)
                    {
                        state = State.WAITING_FOR_INITIATE;
                    }
                    break;
                case State.SENDING_READY:
                    result = ProduceReady(ref msg);
                    if (result == PullMsgResult.Ok)
                    {
                        state = State.READY;
                    }
                    break;
                case State.SENDING_ERROR:
                    result = ProduceError(ref msg);
                    if (result == PullMsgResult.Ok)
                    {
                        state = State.READY;
                    }
                    break;
                default:
                    result = PullMsgResult.Error;
                    break;

            }
            Console.Write("Sending command: ");
            PrintMessage(msg);
            Console.WriteLine("Internal state after creating next command: " + state);
            return result;
        }

        private PullMsgResult ProduceWelcome(ref Msg msg)
        {

            string command = "WELCOME";
            int commandSize = 1 + command.Length;

            msg.InitPool(commandSize);
            msg.Put((byte)command.Length, 0);
            msg.Put(Encoding.ASCII, command, 1);
            return PullMsgResult.Ok;
        }

        private PullMsgResult ProduceError(ref Msg msg)
        {
            throw new NotImplementedException();
        }

        private PullMsgResult ProduceReady(ref Msg msg)
        {
            string command = "READY";
            int commandSize = 1 + command.Length;

            msg.InitPool(commandSize);
            msg.Put((byte)command.Length, 0);
            msg.Put(Encoding.ASCII, command, 1);
            return PullMsgResult.Ok;
        }

        public override void Dispose()
        {
            Console.WriteLine("DISPOSING Plain Server Mechanism.\n");
        }
    }
}

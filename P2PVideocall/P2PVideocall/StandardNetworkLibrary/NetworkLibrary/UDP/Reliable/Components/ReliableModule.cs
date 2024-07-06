﻿using System;
using System.Net;

namespace NetworkLibrary.UDP.Reliable.Components
{
    internal class ReliableModule
    {
        SenderModule sender = new SenderModule();
        ReceiverModule receiver = new ReceiverModule();
        public Action<ReliableModule, byte[], int, int> OnReceived;
        public Action<ReliableModule, byte[], int, int> OnSend;
        internal IPEndPoint Endpoint;

        public ReliableModule(IPEndPoint adress)
        {
            Endpoint = adress;
            sender.SendRequested += OutputDataBytesToSend;

            receiver.SendFeedback += OutputFeedbacks;
            receiver.OnMessageReceived += OutputMessageReceived;
        }

        private void OutputMessageReceived(byte[] buffer, int offset, int count)
        {
            OnReceived?.Invoke(this, buffer, offset, count);
        }

        private void OutputDataBytesToSend(byte[] buffer, int offset, int count)
        {
            OnSend?.Invoke(this, buffer, offset, count);
        }
        private void OutputFeedbacks(byte[] buffer, int offset, int count)
        {
            OnSend?.Invoke(this, buffer, offset, count);
        }

        public void HandleBytes(byte[] buffer, int offset, int count)
        {
            try
            {
                switch (buffer[offset])
                {
                    case SenderModule.Ack:
                        sender.HandleFeedback(buffer, offset);
                        break;
                    case SenderModule.AckList:
                        sender.HandleFeedback(buffer, offset);
                        break;
                    case SenderModule.Nack:
                        sender.HandleFeedback(buffer, offset);
                        break;
                    case SenderModule.ProbeResult:
                        sender.HandleFeedback(buffer, offset);
                        break;
                    case SenderModule.SynAck:
                        sender.HandleFeedback(buffer, offset);
                        break;
                    case SenderModule.Head:
                        receiver.HandleBytes(buffer, offset, count);
                        break;
                    case SenderModule.Chunk:
                        receiver.HandleBytes(buffer, offset, count);
                        break;
                    case SenderModule.Probe1:
                        receiver.HandleBytes(buffer, offset, count);
                        break;
                    case SenderModule.Probe2:
                        receiver.HandleBytes(buffer, offset, count);
                        break;
                    default:
                        Console.WriteLine("Reliable Udp Module could not parse incming message OPCode");
                        break;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to Read Low Level Message" + ex.Message);
            }
        }

        public void Send(byte[] buffer, int offset, int count)
        {
            sender.ProcessBytesToSend(buffer, offset, count);
        }
        public void Send(Segment first, Segment second)
        {
            sender.ProcessBytesToSend(first, second);
        }

        internal void Close()
        {
            OnReceived = null;
            OnSend = null;

            sender.SendRequested = null;
            receiver.SendFeedback = null;
            receiver.OnMessageReceived = null;
            sender.ShutDown();
            receiver.ShutDown();
        }
    }
}

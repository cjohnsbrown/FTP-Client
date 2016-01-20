/*
 * File: ClientControl.cs
 * Author: Cameron Johnson-Brown
 * Description: Parses user inputs 
 */
using System;
using System.Text.RegularExpressions;

public class ClientControl {


	public const string PROMPT = "FTP> ";

	// Information to parse commands
	public readonly string[] COMMANDS = { "ascii",
		"binary",
		"cd",
		"cdup",
		"debug",
		"dir",
		"get",
		"help",
		"passive",
		"put",
		"pwd",
		"quit",
		"user" };

	public const int ASCII = 0;
	public const int BINARY = 1;
	public const int CD = 2;
	public const int CDUP = 3;
	public const int DEBUG = 4;
	public const int DIR = 5;
	public const int GET = 6;
	public const int HELP = 7;
	public const int PASSIVE = 8;
	public const int PUT = 9;
	public const int PWD = 10;
	public const int QUIT = 11;
	public const int USER = 12;

	// Help message

	public readonly String[] HELP_MESSAGE = {
		"ascii      --> Set ASCII transfer type",
		"binary     --> Set binary transfer type",
		"cd <path>  --> Change the remote working directory",
		"cdup       --> Change the remote working directory to the",
		"               parent directory (i.e., cd ..)",
		"debug      --> Toggle debug mode",
		"dir        --> List the contents of the remote directory",
		"get path   --> Get a remote file",
		"help       --> Displays this text",
		"passive    --> Toggle passive/active mode",
		"put path   --> Transfer the specified file to the server",
		"pwd        --> Print the working directory on the server",
		"quit       --> Close the connection to the server and terminate",
		"user login --> Specify the user name (will prompt for password" };

	private ClientModel model;


	public ClientControl(ClientModel model) {
   		this.model = model;	
	}

	/*
	 * Reads in and parses inputs until EOF or quit are entered
	 */
	public void run() {
		bool eof = false;
		String input = null;

		Console.Write("Name (anonymous): ");
		input = Console.ReadLine();
		if (input.Equals(""))
			input = "anonymous";
		model.login(input);

		do {
			try {
				Console.Write( PROMPT );
				input = Console.ReadLine();
			}
			catch ( Exception e ) {
				eof = true;
			}

			// Keep going if we have not hit end of file
			if ( !eof && input.Length > 0 ) {
				int cmd = -1;
				string[] argv = Regex.Split(input, "\\s+");

				// What command was entered?
				for ( int i = 0; i < COMMANDS.Length && cmd == -1; i++ ) {
					if ( COMMANDS[ i ].Equals( argv[ 0 ], StringComparison.CurrentCultureIgnoreCase ) ) {
						cmd = i;
					}
				}

				// Execute the command
				switch( cmd ) {
					case ASCII:
						model.ascii();
						break;

					case BINARY:
						model.binary();
						break;

					case CD:
						if (argv.Length != 2)
							Console.WriteLine("Usage: cd <path>");
						else 
							model.cd(argv[1]);
						break;

					case CDUP:
						model.cdup();
						break;

					case DEBUG:
						model.debugToggle();
						break;

					case DIR:
						model.dir();
						break;

					case GET:
						if (argv.Length != 2)
							Console.WriteLine("Usage: get <filename>");
						else
							model.get(argv[1]);
						break;

					case HELP:
						for ( int i = 0; i < HELP_MESSAGE.Length; i++ ) {
							Console.WriteLine( HELP_MESSAGE[ i ] );
						}
						break;

					case PASSIVE:
						model.passiveToggle();	
						break;

					case PWD:
						model.pwd();
						break;

					case QUIT:
						eof = true;
						break;

					case USER:
						if (argv.Length != 2)
							Console.WriteLine("Usage: user <name>");
						else 
							model.login(argv[1]);
						break;

					default:
						Console.WriteLine( "Invalid command" );
						break;
				}
			}
		} while ( !eof );
		model.Close();
	}


}	


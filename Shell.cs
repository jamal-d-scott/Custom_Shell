/*
 * Jamal D. Scott
 * Personal Learning Project
 * C# Shell implimentation
 * Completed: 4/3/17
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace Custom_Shell
{
    class Program
    {
        // list[] holds the name of all acceptable commands.
        private static String[] list = {"cd", "ls", "batch", "alias", "prompt", "quit", "help", "clear", "format", "history", "rma", "rpa", "fortune"};
        //adjusted[] will hold any user given aliases.
        private static String[] adjusted = new String[1024];
        //inputCommand[] will be the buffer for user input.
        private static String[] inputCommand = new String[1024];
        //usrEntries[] will keep track of all commands entered by the user for the 'history command'.
        private static String[] usrEntries = new String[1024];
        //fortune[] will hold x ammount of quotes from the fortune file.
        private static string[] fortune;
        //prompt is the user entry display text.
        private static String prompt;
        private static int arrIndex = 0;
        private static int usrEntriesIndex = 0;
        private static Boolean aliasCall = false;
        private static String currentDirectory;

        static void Main(string[] args)
        {
            //loads the fortune into the array before user command execution.
            load_fortune();

            currentDirectory = Directory.GetCurrentDirectory();
            String parseString = "";
            prompt = "$>:";

            while (parseString != "quit")
            {
                inputCommand = new String[1024];
                Console.Write(prompt + " ");
                parseString = Console.ReadLine();
                if (usrEntriesIndex == 1024)
                    usrEntriesIndex = 0;
                else
                {
                    usrEntries[usrEntriesIndex] = parseString;
                    usrEntriesIndex++;
                }
                parseInput(parseString);
                readCommands();
                arrIndex = 0;
            }
        }

        //Seperates the line of user input into commands, and phrases. [command][phrase1][phrase2][etc..]
        static void parseInput(String input)
        {
            String parsed = "";
            char c;

            for (int i = 0; i < input.Length; i++)
            {
                c = input[i];
                parsed += c;

                //Delimiters for words
                if (c == ' ' || c == '\n' || c == '\r' || i == input.Length - 1)
                {
                    parsed = parsed.Trim();
                    inputCommand[arrIndex] = parsed;
                    arrIndex++;
                    parsed = "";
                }
            }
        }

        //Determines which command to execute. This information is always parsed to inputCommand[0].
        static void readCommands()
        {
            if (string.IsNullOrEmpty(inputCommand[0]))
            {
                return;
            }

            switch (inputCommand[0])
            {
                case "cd":
                    lsh_changeDirectory();
                    break;
                case "ls":
                    lsh_list();
                    break;
                case "batch":
                    lsh_batch();
                    break;
                case "prompt":
                    lsh_prompt();
                    break;
                case "quit":
                    lsh_quit();
                    break;
                case "alias":
                    lsh_alias();
                    break;
                case "help":
                    lsh_help();
                    break;
                case "clear":
                    lsh_clear();
                    break;
                case "history":
                    lsh_history();
                    break;
                case "format":
                    lsh_format();
                    break;
                case "rma":
                    lsh_rma();
                    break;
                case "rpa":
                    lsh_rpa();
                    break;
                case "la":
                    lsh_la();
                    break;
                case "fortune":
                    lsh_fortune();
                    break;
                case "rdoc":
                    lsh_rdoc();
                    break;
                case " ":
                    break;
                case "":
                    break;
                default:
                    //Checks to see if we are being called from the alias method.
                    if (checkAlias() && aliasCall == true)
                    {
                        Console.WriteLine("Checking");
                        readCommands();
                        aliasCall = false;
                    }
                    else
                        Console.WriteLine("\nCommand: '" + inputCommand[0] + "' is not supported or mapped.\n");
                    break;
            }
        }

        //Command to read and display a document.
        static void lsh_rdoc()
        {
            String text;
            text = System.IO.File.ReadAllText(@"C:\Users\scottj\Documents\Visual Studio 2013\Projects\Custom Shell\code.txt");
            Console.WriteLine(text);
        }

        //Command to read commands from a file.
        static void lsh_batch()
        {
            String location = inputCommand[1];
            string text = "";

            //If a file is not specified, read from the default file.
            if (string.IsNullOrEmpty(inputCommand[1]))
            {
                text = System.IO.File.ReadAllText(@"C:\Users\scottj\Documents\Visual Studio 2013\Projects\Custom Shell\batch_file.txt");
            }
            else
            {
                try
                {
                    text = System.IO.File.ReadAllText(location);
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine("Could not find batch file at the directory: " + location);
                    return;
                }

            }

            //Adds a delimiter on the end of the string to know when to stop reading.
            text = text + "\n";
            char c;
            int index = 0;
            string phrase = "";

            for (int i = 0; i < text.Length; i++)
            {
                c = text[i];

                //Since the file only allows one command per line, a new line signals the end of a command.
                //We store this command and execute it.
                if (c == '\n' || c == '\r')
                {
                    inputCommand[index] = phrase.Trim();
                    readCommands();
                    phrase = "";
                    index = 0;
                }
                else
                {
                    phrase += c;
                    if (c == ' ')
                    {
                        //Spaces mean there's multiple parts to a command, we store these parts into different parts of the buffer.
                        inputCommand[index] = phrase.Trim();
                        phrase = "";
                        index++;
                    }
                }
            }
        }

        //Command to display random quotes!
        static void load_fortune()
        {
            //Starts a new background thread to load the fortune while a user can execute commands.
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                string text = System.IO.File.ReadAllText(@"C:\Users\scottj\Documents\Visual Studio 2013\Projects\Custom Shell\fortunes");
                int count = 0;
                char c;
                //Determines how many quotes there will be.
                //quotes are separated by the '%' character.
                for (int i = 0; i < text.Length; i++)
                {
                    c = text[i];
                    if (c == '%') count++;
                }
                //Adds one onto the size for the last quote that isn't delimited by the '%' character.
                count += 1;

                //Sets the size of the array to hold the total number of quotes.
                fortune = new string[count];
                int fortuneIndex = 0;
                String phrase = "";

                //Adds the quote onto the array.
                for (int i = 0; i < text.Length; i++)
                {
                    c = text[i];

                    if (c == '%')
                    {
                        fortune[fortuneIndex++] = phrase;
                        phrase = "";
                    }
                    else
                        phrase += c;
                }
            }).Start();
        }

        //Picks a random spot in the array to print out a quote.
        static void lsh_fortune()
        {
            Random rnd = new Random();
            int number = rnd.Next(0, fortune.Length);
            Console.WriteLine(fortune[number]);
        }

        //Command to display all commands and their aliases (if given.)
        static void lsh_la()
        {
            Console.WriteLine("\nPrinting out a list of all commands and their aliases:");
            for (int i = 0; i < list.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(adjusted[i]))
                    Console.WriteLine("Command: " + list[i] + " Alias: ");
                else
                    Console.WriteLine("Command: " + list[i] + " Alias: " + adjusted[i]);
            }
            Console.WriteLine("\n");
        }

        //Command to remove an alias.
        static void lsh_rma()
        {
            if (string.IsNullOrEmpty(inputCommand[1]))
                return;
            if (!adjusted.Contains(inputCommand[1]))
                Console.WriteLine("Error: Alias " + inputCommand[1] + " does not exist.");
            else
            {
                adjusted[Array.IndexOf(adjusted, inputCommand[1])] = "";
                Console.WriteLine("Alias: " + inputCommand[1] + " has been removed.");
            }

        }

        //Command to replace an alias.
        static void lsh_rpa()
        {
            if (string.IsNullOrEmpty(inputCommand[1]))
                return;
            if (!adjusted.Contains(inputCommand[1]))
                Console.WriteLine("Error: Alias " + inputCommand[1] + " does not exist.");
            else
            {
                Console.WriteLine("Alias: " + inputCommand[1] + " has been replaced as: " + inputCommand[2]);
                adjusted[Array.IndexOf(adjusted, inputCommand[1])] = inputCommand[2];

            }

        }

        //Command to display all of the user entered commands.
        static void lsh_history()
        {
            Console.WriteLine("Printing out a list of the commands that you've typed: ");
            Console.WriteLine("______________");
            for (int i = 0; i < usrEntries.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(usrEntries[i]))
                {
                    Console.WriteLine(usrEntries[i]);
                    Console.WriteLine("-----------");
                }
            }
            Console.WriteLine("______________");
            Console.WriteLine("\n");
        }

        //Command to clear up the console.
        static void lsh_clear()
        {
            for (int i = 0; i <= 100; i++)
            {
                Console.WriteLine("\n");
            }
        }

        //Command that displays all of the available commands.
        static void lsh_help()
        {
            Console.WriteLine("\nHere is a list of all supported commands:");
            for (int i = 0; i < list.Length; i++)
            {
                Console.WriteLine(list[i]);
            }
            Console.WriteLine("\n");
        }

        //Command to change the current directory.
        static void lsh_changeDirectory()
        {
            currentDirectory = inputCommand[1];
            if (string.IsNullOrEmpty(currentDirectory))
            {
                Console.WriteLine("\nError; ommitted input.");
                currentDirectory = Directory.GetCurrentDirectory();
                return;
            }
            try
            {
                Environment.CurrentDirectory = currentDirectory;
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("\nCould not find directory: " + currentDirectory);
                currentDirectory = Directory.GetCurrentDirectory();
                return;
            }
        }

        //Command to list all of the files in the current directory.
        static void lsh_list()
        {
            String[] allfiles = System.IO.Directory.GetFiles(currentDirectory, "*", System.IO.SearchOption.TopDirectoryOnly);

            for (int i = 0; i < allfiles.Length; i++)
            {
                Console.WriteLine(allfiles[i]);
            }
        }

        //Checks to see if the alias exists, if it does, associate the alias with its command based on array index
        //Set the actual command into the command slot in the buffer, and then execute that command.
        static Boolean checkAlias()
        {
            int i = Array.IndexOf(adjusted, inputCommand[0]);
            if (i == -1)
                return false;

            //Sets the associated command into the command slot.
            inputCommand[0] = list[i].Trim();
            aliasCall = true;
            return true;
        }

        //Command that sets an alias for a command/
        static void lsh_alias()
        {
            if (Array.IndexOf(adjusted, inputCommand[1]) != -1)
            {
                Console.WriteLine("Cannot make an alias for an alias.");
                return;
            }

            if (list.Contains(inputCommand[2]))
            {
                Console.WriteLine("Cannot used an existing command as an alias.");
                return;
            }

            if (!list.Contains(inputCommand[1]))
            {
                Console.WriteLine("Command: " + inputCommand[1] + " does not exist.");
                return;
            }

            int i = Array.IndexOf(list, inputCommand[1]);
            adjusted[i] = inputCommand[2];
            Console.WriteLine(inputCommand[1] + " has now been set to: " + adjusted[i]);
        }

        //Command that changes the display prompt.
        static void lsh_prompt()
        {
            if (string.IsNullOrWhiteSpace(inputCommand[1]))
            {
                Console.WriteLine("Function 'prompt *prompt rename here* may not be blank!");
                prompt = "$>:";
            }
            else
                prompt = inputCommand[1];
        }

        //Command that explains command functionality and the format of the command (how to type it.)
        static void lsh_format()
        {
            Console.WriteLine("\nType a command to view the format of that command.");
            Console.WriteLine("The format of the command omits the initial '$>:'.");
            Console.WriteLine("*THIS* represents user specific input.");
            Console.WriteLine("Type 'exit' to exit the format program");
            Console.WriteLine("Type 'help' to view a list of commands: ");

            String readCommand = "";

            while (readCommand != "exit")
            {
                Console.Write(prompt + " ");
                readCommand = Console.ReadLine();

                switch (readCommand)
                {
                    case "cd":
                        Console.WriteLine("\ncd allows the user to change the current directory that the shell is working in.");
                        Console.WriteLine("FORMAT: $>: cd C:/*USER*/*PATH*");
                        break;
                    case "ls":
                        Console.WriteLine("\nls displays a list of files and folders in a directory.");
                        Console.WriteLine("FORMAT: $>: ls");
                        break;
                    case "batch":
                        Console.WriteLine("\nRead commands that are stored in a file. If not file specified, the default is used.");
                        Console.WriteLine("FORMAT: $>: batch *FileName*");
                        break;
                    case "prompt":
                        Console.WriteLine("\nprompt changes the input display text: '$>:' to what the user specified.");
                        Console.WriteLine("FORMAT: $>: prompt *CHANGE-TO*");
                        break;
                    case "quit":
                        Console.WriteLine("\nquit causes the shell to terminate.");
                        Console.WriteLine("FORMAT: $>: quit");
                        break;
                    case "alias":
                        Console.WriteLine("\nalias allows the user to give a command an alternate name. You cannot make an alias for an alias.");
                        Console.WriteLine("FORMAT: $>: alias *PRE-EXISTING COMMAND* *CHANGE-TO*");
                        break;
                    case "help":
                        lsh_help();
                        Console.WriteLine("If you want to exit the format helper, enter 'exit'.");
                        break;
                    case "clear":
                        Console.WriteLine("\nclear removes all text from the console.");
                        Console.WriteLine("FORMAT: $>: clear");
                        break;
                    case "history":
                        Console.WriteLine("\nhistory displays a full list of the commands that the user has entered.");
                        Console.WriteLine("FORMAT: $>: history");
                        break;
                    case "format":
                        Console.WriteLine("\nformat displays information about a command and the format that the command should be submitted in.");
                        Console.WriteLine("FORMAT: $>: format");
                        break;
                    case "rma":
                        Console.WriteLine("\nrma emoves a user created alias if one exists.");
                        Console.WriteLine("FORMAT: $>: rma *ALIAS NAME*");
                        break;
                    case "rpa":
                        Console.WriteLine("\nrpa replaces an existing alias with a user specified alias.");
                        Console.WriteLine("FORMAT: $>: rpa *ALIAS NAME* *CHANGE-TO*");
                        break;
                    case "la":
                        Console.WriteLine("\nla lists all commands and their aliases. If none exists *NONE* is outputted.");
                        Console.WriteLine("FORMAT: $>: la");
                        break;
                    case "fortune":
                        Console.WriteLine("\nGet a quote!.");
                        Console.WriteLine("FORMAT: $>: fortune");
                        break;
                    case "exit":
                        Console.WriteLine("Exiting the format helper.");
                        break;
                    default:
                        Console.WriteLine("\nCommand: '" + readCommand + "' not recognized.");
                        break;
                }
            }
            Console.WriteLine("\n");
        }

        //Command to exit the shell.
        static void lsh_quit()
        {
            Console.WriteLine("Quitting");
        }
    }
}

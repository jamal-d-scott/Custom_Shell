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
        private static String[] list = {"cd", "ls", "batch", "alias", "prompt", "quit", "help", "clear", "format", "history", "rma", "rpa", "fortune"};
        private static String[] adjusted = new String[1024];
        private static String[] inputCommand = new String[1024];
        private static String[] usrEntries = new String[1024];
        private static string[] fortune;
        private static String prompt;
        private static int arrIndex = 0;
        private static int usrEntriesIndex = 0;
        private static Boolean aliasCall = false;
        private static String currentDirectory;

        static void Main(string[] args)
        {
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

        static void parseInput(String input)
        {
            String parsed = "";
            char c;

            for (int i = 0; i < input.Length; i++)
            {
                c = input[i];
                parsed += c;

                if (c == ' ' || c == '\n' || c == '\r' || i == input.Length - 1)
                {
                    parsed = parsed.Trim();
                    inputCommand[arrIndex] = parsed;
                    arrIndex++;
                    parsed = "";
                }
            }
        }

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

        static void lsh_rdoc()
        {
            String text;
            text = System.IO.File.ReadAllText(@"U:\Private\Custom Shell\code.txt");
            Console.WriteLine(text);
        }

        static void lsh_batch()
        {
            String location = inputCommand[1];
            string text = "";
            if (string.IsNullOrEmpty(inputCommand[1]))
            {
                text = System.IO.File.ReadAllText(@"U:\Private\Custom Shell\batch_file.txt");
            }
            else
            {
                try
                {
                    text = System.IO.File.ReadAllText(location);
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("Could not find batch file at the directory: " + location);
                    return;
                }

            }

            text = text + "\n";
            char c;
            int index = 0;
            string phrase = "";

            for (int i = 0; i < text.Length; i++)
            {
                c = text[i];
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
                        inputCommand[index] = phrase.Trim();
                        phrase = "";
                        index++;
                    }
                }
            }
        }

        static void load_fortune()
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                string text = System.IO.File.ReadAllText(@"U:\Private\Custom Shell\fortunes");
                int count = 0;
                char c;
                for (int i = 0; i < text.Length; i++)
                {
                    c = text[i];
                    if (c == '%') count++;
                }
                count += 1;

                fortune = new string[count];
                int fortuneIndex = 0;
                String phrase = "";

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

        static void lsh_fortune()
        {
            Random rnd = new Random();
            int number = rnd.Next(0, fortune.Length);
            Console.WriteLine(fortune[number]);
        }

        static void lsh_la()
        {
            Console.WriteLine("\nPrinting out a list of all commands and their aliases:");
            for (int i = 0; i < list.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(adjusted[i]))
                    Console.WriteLine("Command: " + list[i] + " Alias: *NONE*");
                else
                    Console.WriteLine("Command: " + list[i] + " Alias: " + adjusted[i]);
            }
            Console.WriteLine("\n");
        }

        static void lsh_rma()
        {
            if (string.IsNullOrEmpty(inputCommand[1]))
                return;
            if (!adjusted.Contains(inputCommand[1]))
                Console.WriteLine("Error: Alias " + inputCommand[1] + " does not exist.");
            else
            {
                adjusted[Array.IndexOf(adjusted, inputCommand[1])] = "";
                Console.WriteLine("Alias: " + inputCommand[1] + " has been removed for the command: " + list[Array.IndexOf(list, inputCommand[0])]);
            }

        }

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

        static void lsh_clear()
        {
            for (int i = 0; i <= 100; i++)
            {
                Console.WriteLine("\n");
            }
        }

        static void lsh_help()
        {
            Console.WriteLine("\nHere is a list of all supported commands:");
            for (int i = 0; i < list.Length; i++)
            {
                Console.WriteLine(list[i]);
            }
            Console.WriteLine("\n");
        }

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

        static void lsh_list()
        {
            String[] allfiles = System.IO.Directory.GetFiles(currentDirectory, "*", System.IO.SearchOption.TopDirectoryOnly);

            for (int i = 0; i < allfiles.Length; i++)
            {
                Console.WriteLine(allfiles[i]);
            }
        }

        static Boolean checkAlias()
        {
            int i = Array.IndexOf(adjusted, inputCommand[0]);
            if (i == -1)
                return false;

            inputCommand[0] = list[i].Trim();
            aliasCall = true;
            return true;
        }

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

        static void lsh_quit()
        {
            Console.WriteLine("Quitting");
        }
    }
}

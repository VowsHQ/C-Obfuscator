using Protector.Protections;
using System;
using System.Drawing;
using System.Threading;
using static kryption.Main.Program;
using Console = Colorful.Console;

namespace kryption.Main
{
    internal class Program
    {
        static int selectedIndex = 0;

        static string[] options =
        {
            "Start",
            "Exit"
        };


        static void Main(string[] args)
        {
            Console.CursorVisible = false;

            while (true)
            {
                DrawMenu();

                var key = System.Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow || key == ConsoleKey.W)
                {
                    selectedIndex--;
                    if (selectedIndex < 0)
                        selectedIndex = options.Length - 1;
                }
                else if (key == ConsoleKey.DownArrow || key == ConsoleKey.S)
                {
                    selectedIndex++;
                    if (selectedIndex >= options.Length)
                        selectedIndex = 0;
                }
                else if (key == ConsoleKey.Enter)
                {
                    HandleSelection();
                }
                else if (key == ConsoleKey.Escape)
                {
                    break; 
                }
            }
        }

        static void DrawMenu()
        {
            Console.Clear();
            Logo.PrintLogo();




            for (int i = 0; i < options.Length; i++)
            {
                if (i == selectedIndex)
                {
                    Console.Write("│ » ", Color.Orange);
                    Console.WriteLine(options[i], Color.White);
                }
                else
                {
                    Console.Write(" ");
                    Console.WriteLine(options[i], Color.Gray);
                }
            }
        }

        static void HandleSelection()
        {
            Console.Clear();

            switch (selectedIndex)
            {
                case 0:
                    Logo.PrintLogo();
                    Console.Write("[", Color.GhostWhite);
                    Colorful.Console.Write("»", Color.Orange);
                    Console.Write("] Please enter the path to your program. (Not exe)", Color.GhostWhite);
                    Console.WriteLine();
                    Colorful.Console.Write(" » ", Color.Orange);
                    string folder = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(folder))
                    {
                        Logo.PrintLogo();
                        Console.Write("[", Color.GhostWhite);
                        Colorful.Console.Write("!404!", Color.Orange);
                        Console.Write("] Folder path cannot be empty", Color.GhostWhite);
                        Console.ReadKey();
                        break;
                    }

                    if (!System.IO.Directory.Exists(folder))
                    {
                        Logo.PrintLogo();
                        Console.Write("[", Color.GhostWhite);
                        Colorful.Console.Write("404", Color.Orange);
                        Console.Write("] Folder does not exist", Color.GhostWhite);
                        Console.ReadKey();
                        break;
                    }

                    if (Encryption.ObfuscateFolder(folder))
                    {
                        Logo.PrintLogo();
                        Console.Write("[", Color.GhostWhite);
                        Colorful.Console.Write("!", Color.Orange);
                        Console.Write("] Obfuscation Complete", Color.GhostWhite);
                    }
                    else
                    {
                        Logo.PrintLogo();
                        Console.Write("[", Color.GhostWhite);
                        Colorful.Console.Write("!404!", Color.Orange);
                        Console.Write("] Obfuscation failed", Color.GhostWhite);
                    }

                    Console.ReadKey();
                    break;

                case 1:
                    Logo.PrintLogo();
                    Console.Write("[", Color.GhostWhite);
                    Colorful.Console.Write("!", Color.Orange);
                    Console.Write("] Bye Bye", Color.GhostWhite);
                    Thread.Sleep(1500);
                    Environment.Exit(0);
                    break;

            }
        }
    }
}
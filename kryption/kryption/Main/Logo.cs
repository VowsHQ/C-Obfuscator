using System;
using System.ComponentModel;
using System.Drawing;

namespace kryption.Main
{
    internal class Logo
    {
        public static void PrintLogo()
        {
            {
                Colorful.Console.Clear();
                Colorful.Console.Title = "Crypter / C# Obfuscator / By Vows";
                Colorful.Console.WriteLineFormatted("", Color.GhostWhite);
                Colorful.Console.WriteLineFormatted("                                          {0}         ", Color.FromArgb(byte.MaxValue, 255, 48, 0), Color.FromArgb(87, 20, 20), "█", "║");
                Colorful.Console.WriteLineFormatted("             ▓  ▒    ▒  ▒  ░      ▓       {0}      ▒  ▒", Color.FromArgb(byte.MaxValue, 255, 55, 0), Color.FromArgb(87, 20, 20), "█", "║");
                Colorful.Console.WriteLineFormatted("             ▓{0}{0}▒   {0}▒{0}{0}▒ {0}░  {0}  {0}▓{0}{0}   {0}{0}{0}{0}{0}   {0}▒{0}{0}▒", Color.FromArgb(byte.MaxValue, 255, 64, 0), Color.FromArgb(87, 20, 20), "█", "╔", "═", "╝", "╚", "║", "║");
                Colorful.Console.WriteLineFormatted("            ▓{0}  ▓   {0}{0}  {0} ▓▒ ▒▓  {0}▓ ▓{0}    {0}     {0}{0}  {0}            {1} [{2}/{3}]", Color.FromArgb(byte.MaxValue, 255, 70, 0), Color.FromArgb(87, 20, 20), "█", "Menu Keys", "W", "S");
                Colorful.Console.WriteLineFormatted("            {0}░      {0}     ▒{0} {0}▒  {0}   {0}    {0}     {0}                {1}", Color.FromArgb(byte.MaxValue, 255, 78, 0), Color.FromArgb(87, 20, 20), "█", "Made by Vows", "╔", "═", "╝", "╚", "║");
                Colorful.Console.WriteLineFormatted("            {0}       {0}      {0} {0}   {0}   {0}    {0}     {0}                {1}", Color.FromArgb(byte.MaxValue, 255,84, 0), Color.FromArgb(87, 20, 20), "█", "github.com/VowsHQ", "╔", "╝", "╚", "║");
                Colorful.Console.WriteLineFormatted("            ▓{0}  ▓   {0}      ▓{0}▒   {0}▓ ▓{0}    {0}░    {0}    ", Color.FromArgb(byte.MaxValue, 255,90, 0), Color.FromArgb(87, 20, 20), "█", "═", "╝", "║");
                Colorful.Console.WriteLineFormatted("             ▓{0}{0}▒   {0}      ▒{0}    {0}▓{0}{0}     ▒{0}{0}   {0}    ", Color.FromArgb(byte.MaxValue, 255,100,0), Color.FromArgb(87, 20, 20), "█", "═", "╝", "║");
                Colorful.Console.WriteLineFormatted("                           ▒{0}    {0}                   ", Color.FromArgb(byte.MaxValue, 255,107, 0), Color.FromArgb(87, 20, 20), "█", "═", "╝", "║");
                Colorful.Console.WriteLineFormatted("                           {0}▒    {0}                   ", Color.FromArgb(byte.MaxValue, 255,114, 0), Color.FromArgb(87, 20, 20), "█", "═", "╝", "║");
                Colorful.Console.WriteLineFormatted("                          {0}{0}     {0}                   ", Color.FromArgb(byte.MaxValue, 255,120, 0), Color.FromArgb(87, 20, 20), "█", "═", "╝", "║");
                Colorful.Console.Write("", Color.GhostWhite);
                Colorful.Console.WriteLine();

            }
        }
    }
}
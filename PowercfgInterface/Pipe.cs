﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace PowercfgInterface
{
    public static class Pipe
    {
        public static string? Run(string command,string[] args)
        {
            string? output;
            using (Process process = new Process())
            {
                process.StartInfo.FileName = command;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.Arguments = String.Join(" ",args);
                try
                {
                    process.Start();

                    StreamReader reader = process.StandardOutput;
                    output = reader.ReadToEnd();

                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    Console.Write(ex.ToString());
                    output = null;
                }
            }
            return output;
        }

        public async static Task<string?> RunAsync(string command, string[] args)
        {
            string? output= await Task.Run(() =>
            {
                return Run(command, args);
            });
            return output;
        }
    }


}
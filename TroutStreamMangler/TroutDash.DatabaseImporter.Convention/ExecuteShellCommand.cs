﻿using System;
using System.Diagnostics;
using System.IO;

namespace TroutDash.DatabaseImporter.Convention
{
    public static class ExecuteShellCommand
    {
        public static void  ExecuteProcess(string command, DirectoryInfo defaultDirectory = null)
        {
            var process = new Process();
            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = "cmd.exe",
                Arguments = "/C " + command,
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            if (defaultDirectory != null)
            {
                startInfo.WorkingDirectory = defaultDirectory.FullName;
            }

            process.StartInfo = startInfo;
            process.Start();
            string stdoutx = process.StandardOutput.ReadToEnd();
            string stderrx = process.StandardError.ReadToEnd();   
            process.WaitForExit(80000);
        }

        public static void ExecuteSql(FileInfo sql, string databaseName, string hostName, string userName)
        {
            Console.WriteLine("Starting to execute sql named " + sql.Name);
            const string commandTemplate = @"psql -q -d {0} -f {1} --host={2} --username={3}";
            string command = String.Format(commandTemplate, databaseName, sql.FullName, hostName, userName);
            ExecuteProcess(command);
        }
    }
}
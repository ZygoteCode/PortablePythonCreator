using System;
using System.IO;
using System.Net;
using System.IO.Compression;
using System.Diagnostics;
using System.Collections.Generic;

class Program
{
    private static string pythonVersion = "";

    public static void Main(string[] args)
    {
        Console.Title = "PortablePythonCreator | Made by https://github.com/ZygoteCode/";

        while (pythonVersion == "")
        {
            Console.Write("Please, insert a valid Python version: ");
            pythonVersion = Console.ReadLine();
        }

        Console.WriteLine("Creating your portable Python build, please wait a while.");
        string[] dirs = { "dist", "out", "download" };

        foreach (string dir in dirs)
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }

        pythonVersion = pythonVersion.Split('+')[0];

        GetPython();
        GetPip();
        InvokePythonImportPatch();
        InvokeReOrgFiles();

        Directory.CreateDirectory(".\\out");
        ZipFile.CreateFromDirectory($".\\dist\\Python-{pythonVersion}", $".\\out\\portable-python-{pythonVersion}-windows-amd64.zip");

        Directory.Delete(".\\dist", true);
        Directory.Delete(".\\download", true);
        Process.Start(".\\out");

        Console.WriteLine("Succesfully created your portable Python build.");
        Console.WriteLine("Press the ENTER key in order to exit from the program.");
        Console.ReadLine();
    }

    static void GetPython()
    {
        string downloadDir = ".\\download";

        if (!Directory.Exists(downloadDir))
        {
            Directory.CreateDirectory(downloadDir);
        }

        string url = $"https://www.python.org/ftp/python/{pythonVersion}/python-{pythonVersion}-embed-amd64.zip";
        string output = Path.Combine(downloadDir, "Python.zip");

        if (!File.Exists(output))
        {
            WebClient webClient = new WebClient();
            webClient.DownloadFile(url, output);

            ZipFile.ExtractToDirectory(output, $".\\dist\\Python-{pythonVersion}");
        }
    }

    static void GetPip()
    {
        string pipScriptPath = $".\\dist\\Python-{pythonVersion}\\get-pip.py";

        if (!File.Exists(pipScriptPath))
        {
            string url = "https://bootstrap.pypa.io/get-pip.py";

            WebClient webClient = new WebClient();
            webClient.DownloadFile(url, pipScriptPath);

            Process.Start($".\\dist\\Python-{pythonVersion}\\Python.exe", pipScriptPath).WaitForExit();
        }
    }

    static void InvokePythonImportPatch()
    {
        string filePath = "";

        foreach (string file in Directory.GetFiles($".\\dist\\Python-{pythonVersion}"))
        {
            if (Path.GetExtension(file).ToLower().Equals("._pth"))
            {
                filePath = file;
                break;
            }
        }

        string[] lines = File.ReadAllLines(filePath);

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("#import site"))
            {
                lines[i] = lines[i].Replace("#import site", "import site");
            }

            if (lines[i].Contains("python"))
            {
                lines[i] = lines[i].Replace("python", "Lib\\site-packages\\python");
            }
        }

        File.WriteAllLines(filePath, lines);
        File.WriteAllText($".\\dist\\Python-{pythonVersion}\\sitecustomize.py", PortablePythonCreator.Properties.Resources.sitecustomize);
    }

    static void InvokeReOrgFiles()
    {
        string[] keepFiles = { "python.cat", "python.exe", "python38.dll", "pythonw.exe" };

        Directory.CreateDirectory($".\\dist\\Python-{pythonVersion}\\Lib");
        Directory.CreateDirectory($".\\dist\\Python-{pythonVersion}\\Lib\\site-packages");

        foreach (string file in Directory.GetFiles($".\\dist\\Python-{pythonVersion}"))
        {
            bool exists = false;

            foreach (string keepFile in keepFiles)
            {
                if (Path.GetFileName(file).ToLower().Equals(keepFile.ToLower()) || Path.GetExtension(file).ToLower().Equals("._pth"))
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                File.Copy(file, $".\\dist\\Python-{pythonVersion}\\Lib\\site-packages\\{Path.GetFileName(file)}");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sys = Cosmos.System;

namespace SharpOS
{
    public class Kernel : Sys.Kernel
    {
        string env_vars = "TEST:A test value;AUTHOR:Michael VanOverbeek";

        string current_directory = "0:\\";


        protected override void BeforeRun()
        {
            
            FS = new Sys.FileSystem.CosmosVFS();
            Sys.FileSystem.VFS.VFSManager.RegisterVFS(FS);
            FS.Initialize();
            Console.WriteLine("Scanning filesystems...");
            Console.Clear();
            Console.WriteLine("Welcome to SharpOS.");
            Console.WriteLine("Environment Variables:");
            Console.WriteLine(env_vars);
            Console.WriteLine("System dir separator: " + Sys.FileSystem.VFS.VFSManager.GetDirectorySeparatorChar());
        }

        bool running = true;
        public Cosmos.System.FileSystem.CosmosVFS FS = null;
        protected override void Run()
        {
            while (running)
            {
                try
                {
                    Console.Write(current_directory + "> ");
                    string input = Console.ReadLine();
                    InterpretCMD(input);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Exception thrown: " + e.Message);
                }
            }
        }

        public void InterpretCMD(string input)
        {
            string[] args = input.Split(' ');
            if (input.StartsWith("shutdown"))
            {
                Console.Clear();
                Console.WriteLine("It is safe to shut down your system.");
                running = false;
            }
            else if(input.StartsWith("test_crash"))
            {
                throw new Exception("Test crash.");
            }
            else if (input.StartsWith("reboot"))
            {
                Sys.Power.Reboot();
            }
            else if (input.StartsWith("echo "))
            {
                try
                {
                    Console.WriteLine(input.Remove(0, 5));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("echo: " + ex.Message);
                }
            }
            else if (input.StartsWith("dir"))
            {
                Console.WriteLine("Type\tName");
                foreach (var dir in Directory.GetDirectories(current_directory))
                {
                    Console.WriteLine("<DIR>\t" + dir);
                }
                foreach (var dir in Directory.GetFiles(current_directory))
                {
                    string[] sp = dir.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                    Console.WriteLine(sp[sp.Length - 1] + "\t" + dir);
                }

            }
            else if(input.StartsWith("test_write"))
            {
                var f = File.Create(@"0:\\TestFile.txt");
                f.Close();
                File.WriteAllText(@"0:\TestFile.txt", "Test\nAnother Test");
                Console.WriteLine("Created file!");
            }
            else if(input.StartsWith("mkdir "))
            {
                string dir = input.Remove(0, 6);
                Directory.CreateDirectory(@"0:\\apple");
                
            }
            else if(input.StartsWith("cd "))
            {
                var newdir = input.Remove(0, 3);
                if(dir_exists(newdir))
                {
                    Directory.SetCurrentDirectory(current_directory);
                    current_directory = current_directory + newdir + "\\";
                }
                else
                {
                    if(newdir == "..")
                    {
                        var dir = FS.GetDirectory(current_directory);
                        string p = dir.mParent.mName;
                        Console.WriteLine(p);
                    }
                }
            }
            else if (input.StartsWith("print "))
            {
                string file = input.Remove(0, 6);
                    if (File.Exists(file))
                    {
                        Console.WriteLine(File.ReadAllText(file));
                    }
                    else
                    {
                        Console.WriteLine("print: File doesn't exist.");
                    }
                
            }
            else if (input.StartsWith("lsvol"))
            {
                var vols = FS.GetVolumes();
                Console.WriteLine("Name\tSize\tParent");
                foreach (var vol in vols)
                {
                    Console.WriteLine(vol.mName + "\t" + vol.mSize + "\t" + vol.mParent);
                }
            }
            else
            {
                Console.WriteLine("Invalid Command.");
            }
             
        }

        public bool dir_exists(string path)
        {
            bool val = true;
            foreach (var dir in Directory.GetDirectories(path))
            {
                val = (path == dir);
            }
            return val;
        }
    }
}

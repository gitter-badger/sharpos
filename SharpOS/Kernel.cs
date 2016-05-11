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

        const string kernel_version = "0.0.1";
        const string kernel_flavour = "Earth";

        protected override void BeforeRun()
        {
            env_vars += ";VERSION:" + kernel_version + ";KERNEL:" + kernel_flavour;
            FS = new Sys.FileSystem.CosmosVFS();
            Sys.FileSystem.VFS.VFSManager.RegisterVFS(FS);
            FS.Initialize();
            Console.WriteLine("Scanning filesystems...");
            Console.Clear();
            Console.WriteLine("Welcome to SharpOS.");
            InterpretCMD("$VERSION");
            InterpretCMD("$KERNEL");
            InterpretCMD("$AUTHOR");
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
            string lower = input.ToLower(); //so commands are not case-sensitive use this
            if (lower.StartsWith("shutdown"))
            {
                Console.Clear();
                Console.WriteLine("It is safe to shut down your system.");
                running = false;
            }
            else if(lower.StartsWith("$"))
            {
                string[] evars = split_str(env_vars, ";");
                foreach(string kv in evars)
                {
                    string[] var = split_str(kv, ":");
                    if(var[0].ToLower() == lower.Remove(0, 1))
                    {
                        Console.WriteLine(var[0] + " = " + var[1]);
                    }
                }
            }
            else if(lower.StartsWith("test_crash"))
            {
                throw new Exception("Test crash.");
            }
            else if (lower.StartsWith("reboot"))
            {
                Sys.Power.Reboot();
            }
            else if (lower.StartsWith("echo "))
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
            else if (lower.StartsWith("dir"))
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
            else if(lower.StartsWith("test_write"))
            {
                var f = File.Create(@"0:\\TestFile.txt");
                f.Close();
                File.WriteAllText(@"0:\TestFile.txt", "Test\nAnother Test");
                Console.WriteLine("Created file!");
            }
            else if(lower.StartsWith("mkdir "))
            {
                string dir = input.Remove(0, 6);
                Directory.CreateDirectory(@"0:\\apple");
                
            }
            else if(lower.StartsWith("cd "))
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
            else if (lower.StartsWith("print "))
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
            else if (lower.StartsWith("lsvol"))
            {
                var vols = FS.GetVolumes();
                Console.WriteLine("Name\tSize\tParent");
                foreach (var vol in vols)
                {
                    Console.WriteLine(vol.mName + "\t" + vol.mSize + "\t" + vol.mParent);
                }
            }
            else if(lower.StartsWith("scr "))
            {
                string p = input.Remove(0, 4);
                Interpret_Script(current_directory + p);
            }
            else
            {
                Console.WriteLine("Invalid Command.");
            }
             
        }

        public void Interpret_Script(string path)
        {
            string[] lines = File.ReadAllLines(path);
            foreach(string line in lines)
            {
                InterpretCMD(line);
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

        public string[] split_str(string subject, string split)
        {
            return subject.Split(new[] { split }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}

using CommandLine;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace DiffCopy
{
    class Program
    {
        public class Options
        {
            [Option('s', "sourcedir", Required = true, HelpText = "The path to the original directory.")]
            public string SourceDir { get; set; }

            [Option('n', "newdir", Required = true, HelpText = "The path to the modified directory.")]
            public string NewDir { get; set; }

            [Option('o', "outputdir", Required = true, HelpText = "The path to the output directory.")]
            public string OutDir { get; set; }
        }


        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed(o => Copy(o.SourceDir, o.NewDir, o.OutDir));
        }

        private static void Copy(string sourceDir, string newDir, string outDir)
        {
            // First of all, check files in the root
            foreach (var newFile in Directory.GetFiles(newDir))
            {
                var fileName = Path.GetFileName(newFile);
                var sourceFile = Path.Combine(sourceDir, fileName);
                var outFile = Path.Combine(outDir, fileName);

                // If the file did not exist before or if it was modified, write it
                if (!File.Exists(sourceFile) || WasModified(sourceFile, newFile))
                {
                    if (!Directory.Exists(outDir))
                    {
                        Directory.CreateDirectory(outDir);
                    }

                    File.Copy(newFile, outFile, true);
                }
            }

            // Then for all directories, call recursively
            foreach (var newSubdir in Directory.GetDirectories(newDir))
            {
                var dirName = Path.GetFileName(newSubdir);
                var sourceSubdir = Path.Combine(sourceDir, dirName);
                var outSubdir = Path.Combine(outDir, dirName);

                // If the directory did not exist before, copy it entirely
                if (!Directory.Exists(sourceSubdir))
                {
                    DirectoryCopy(newSubdir, outSubdir, true);
                }
                else
                {
                    Copy(sourceSubdir, newSubdir, outSubdir);
                }
            }
        }

        private static bool WasModified(string sourceFile, string newFile)
        {
            using var md5 = MD5.Create();

            // Calc hash of source file
            using var sourceStream = File.OpenRead(sourceFile);
            var sourceHash = md5.ComputeHash(sourceStream);

            // Calc hash of new file
            using var newStream = File.OpenRead(newFile);
            var newHash = md5.ComputeHash(newStream);

            return !sourceHash.SequenceEqual(newHash);
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
    }
}

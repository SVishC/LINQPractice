using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WorWoLINQ
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = @"D:\MyStuffs\Personal\Doc";
            SortWithoutLINQ(filePath);
            Console.WriteLine("**********");
            SortWithLINQ(filePath);
            Console.WriteLine("**********");

            DirectoryInfo directory = new DirectoryInfo(filePath);
            FileInfo[] files = directory.GetFiles();
            SortWithXtensionMethods(files);
        }

        private static void SortWithXtensionMethods(FileInfo[] files)
        {
            files.Sort();
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"{files[i].Name,-60}:{files[i].Length,10:N0}");
            }
        }

        private static void SortWithoutLINQ(string filePath)
        {
            DirectoryInfo directory = new DirectoryInfo(filePath);
            FileInfo[] files = directory.GetFiles();
            Array.Sort(files, new FileComparer());
            //foreach (FileInfo file in files)
            //{
            //    Console.WriteLine($"{file.Name,-60}:{file.Length,10 :N0}");
            //}
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"{files[i].Name,-60}:{files[i].Length,10:N0}");
            }
        }

        private static void SortWithLINQ(string filePath)
        {
            DirectoryInfo directory = new DirectoryInfo(filePath);
            FileInfo[] files = directory.GetFiles();
            //var query = from file in files orderby file.Length descending select file;
            var query = files.OrderByDescending(f => f.Length).Take(5);
            
                
            foreach (var file in query)
            {
                Console.WriteLine($"{file.Name,-60}:{file.Length,10:N0}");
            }
        }
    }

    internal class FileComparer : IComparer<FileInfo>
    {
        public FileComparer()
        {
        }

        public int Compare(FileInfo x, FileInfo y)
        {
            return y.Length.CompareTo(x.Length);
        }


    }

    public static class FilesSort
    {
        public static FileInfo[] Sort(this FileInfo[] files)
        {
            Array.Sort(files, new FileComparer());
            return files;
        }
    }
}

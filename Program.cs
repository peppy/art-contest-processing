using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Humanizer;
using SharpCompress.Archives.Zip;
using SixLabors.ImageSharp;

namespace art_contest_processing
{
    class Program
    {
        static void Main(string[] args)
        {
            var zip = ZipArchive.Open(args[0]);
            System.Console.WriteLine($"Found {zip.Entries.Count} entries");

            var nameGen = new NameGen(zip.Entries.Count);

            foreach (var entry in zip.Entries)
            {
                var match = Regex.Match(entry.ToString(), "(.*) \\((.*)\\)/.*");

                string username = match.Groups[1].Value;
                int userId = int.Parse(match.Groups[2].Value);
                string entryName = nameGen.GetNext();
                var image = Image.Identify(entry.OpenEntryStream());
                handleEntry(image, username, userId, entryName);

            }
        }

        private static void handleEntry(IImageInfo image, string username, int userId, string entryName)
        {
            Console.WriteLine($"Entry by {username} (user id {userId}) assigned \"{entryName}\"");
            Console.WriteLine($"Image dimensions are {image.Width} x {image.Height}");
        }
    }

    public class NameGen
    {
        Queue<string> names = new Queue<string>();

        public NameGen(int count)
        {
            var emotions = File.ReadAllLines("emotions.txt").ToList();
            emotions.Shuffle();

            var animals = new List<string>();
            animals.AddRange(File.ReadAllLines("birds.txt"));
            animals.AddRange(File.ReadAllLines("lizards.txt"));
            animals.AddRange(File.ReadAllLines("mammals.txt"));
            animals.AddRange(File.ReadAllLines("reptiles.txt"));
            animals.Shuffle();

            for (int i = 0; i < count; i++)
                names.Enqueue($"{emotions[i]} {animals[i]}");
        }

        public string GetNext() => names.Dequeue().Humanize(LetterCasing.Title);
    }

    public static class Extensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            Random rnd = new Random();
            while (n > 1)
            {
                int k = (rnd.Next(0, n) % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}

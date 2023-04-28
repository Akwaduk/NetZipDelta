using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace NetZipDelta
{
    public static class ZipDelta
    {
        public static void GetDeltaFile(string FileOneZip, string FileTwoZip, string OutputDirectory, string OutputZipFileName)
        {
            List<ZipArchiveEntry> OldFiles = new List<ZipArchiveEntry>();
            List<ZipArchiveEntry> NewFiles = new List<ZipArchiveEntry>();
            List<ZipArchiveEntry> FilesToKeep = new List<ZipArchiveEntry>();
            List<ZipArchiveEntry> FilesToDelete = new List<ZipArchiveEntry>();

            List<int> MatchedOldIndexes = new List<int>();

            using (ZipArchive archive = ZipFile.OpenRead(FileOneZip))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    OldFiles.Add(entry);
                }


                using (ZipArchive archive2 = ZipFile.OpenRead(FileTwoZip))
                {
                    foreach (ZipArchiveEntry entry in archive2.Entries)
                    {
                        NewFiles.Add(entry);
                    }


                    var MaxListSize = OldFiles.Count;

                    if (NewFiles.Count > OldFiles.Count)
                    {
                        MaxListSize = NewFiles.Count;
                    }

                    var j = 0;

                    for (var i = 0; i < OldFiles.Count; i++)
                    {
                        j = i;
                        if (j >= NewFiles.Count)
                        {
                            j = NewFiles.Count - 1;
                        }

                        bool lap = false;

                        while (OldFiles[i].FullName.ToLower() != NewFiles[j].FullName.ToLower())
                        {
                            // End of array, check if we need to go back around
                            if (j == NewFiles.Count - 1)
                            {
                                if (lap == true)
                                {
                                    j = 0;
                                    FilesToDelete.Add(OldFiles[i]);
                                    break; // No matches
                                }
                                else
                                {
                                    lap = true;
                                    j = 0;
                                }
                            }

                            j = j + 1;
                        }

                        if (OldFiles[i].FullName.ToLower() == NewFiles[j].FullName.ToLower())
                        {
                            if (NewFileHasBeenUpdated(OldFiles[i], NewFiles[j]))
                            {
                                FilesToKeep.Add(NewFiles[j]);
                                MatchedOldIndexes.Add(j);
                            }
                            else
                            {
                                MatchedOldIndexes.Add(j);
                            }
                        }
                    }

                    for (var i = 0; i < NewFiles.Count; i++)
                    {
                        // Add lingering new files
                        if (MatchedOldIndexes.Contains(i) == false)
                        {
                            FilesToKeep.Add(NewFiles[i]);
                        }
                    }

                    foreach (ZipArchiveEntry entry in FilesToKeep)
                    {
                        var length = entry.FullName.LastIndexOf('/');
                        if (length == -1)
                        {
                            length = 0;
                        }

                        var directory = entry.FullName.Substring(0, length);
                        DirectoryInfo di = Directory.CreateDirectory(OutputDirectory + "\\" + directory);
                        var directoryName = OutputDirectory + "\\" + directory;
                        var filename = entry.Name;

                        if (string.IsNullOrEmpty(filename) != true && File.Exists(di + "\\" + filename) == false)
                        {
                            entry.ExtractToFile(directoryName + "\\" + filename);
                        }
                    }

                    foreach (ZipArchiveEntry entry in FilesToDelete)
                    {
                        var length = entry.FullName.LastIndexOf('/');
                        if (length == -1)
                        {
                            length = 0;
                        }

                        var directory = entry.FullName.Substring(0, length);
                        DirectoryInfo di = Directory.CreateDirectory(OutputDirectory + "\\" + directory);
                        var directoryName = OutputDirectory + "\\" + directory;
                        var filename = entry.Name;

                        if (string.IsNullOrEmpty(filename) != true && File.Exists(di + "\\" + filename) == false)
                        {
                            entry.ExtractToFile(directoryName + "\\" + "_DEL_" + filename);
                        }
                    }

                    ZipFile.CreateFromDirectory(OutputDirectory, OutputZipFileName);
                    Directory.Delete(OutputDirectory, true);
                }
            }
        }

        /// <summary>
        /// Determines if a file has been updated in the newer zip file. Checks date modified, archive length, then compares bytes
        /// </summary>
        /// <param name="OldArchiveEntry">What the zip archive file used to be</param>
        /// <param name="NewArchiveEntry">What the zip archive file is now</param>
        /// <returns></returns>
        private static bool NewFileHasBeenUpdated(ZipArchiveEntry OldArchiveEntry, ZipArchiveEntry NewArchiveEntry)
        {
            var FileHasBeenUpdated = true;
            // Knockout easy tests first
            if (OldArchiveEntry.LastWriteTime == NewArchiveEntry.LastWriteTime)
            {
                // Same File
                FileHasBeenUpdated = false;
            }
            if (OldArchiveEntry.Length != NewArchiveEntry.Length)
            {
                // Different File
                FileHasBeenUpdated = true;
            }

            using (StreamReader OldReader = new StreamReader(OldArchiveEntry.Open()))
            {
                var OldBytes = OldReader.ReadToEnd();
                using (StreamReader NewReader = new StreamReader(NewArchiveEntry.Open()))
                {
                    var NewBytes = NewReader.ReadToEnd();
                    if (OldBytes == NewBytes)
                    {
                        FileHasBeenUpdated = false;
                    }
                }
            }

            // Probably a different file
            return FileHasBeenUpdated;
        }

        static void CompareZipFiles(string fileOneZip, string fileTwoZip)
        {
            List<ZipArchiveEntry> oldFiles = new List<ZipArchiveEntry>();
            List<ZipArchiveEntry> newFiles = new List<ZipArchiveEntry>();

            using (ZipArchive archive = ZipFile.OpenRead(fileOneZip))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    oldFiles.Add(entry);
                }
            }

            using (ZipArchive archive = ZipFile.OpenRead(fileTwoZip))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    newFiles.Add(entry);
                }
            }

            Console.WriteLine("Comparing files between two zip archives:");

            foreach (ZipArchiveEntry oldEntry in oldFiles)
            {
                ZipArchiveEntry newEntry = newFiles.Find(x => x.FullName == oldEntry.FullName);

                if (newEntry == null)
                {
                    Console.WriteLine($"File '{oldEntry.FullName}' is missing in the second zip file.");
                    continue;
                }

                if (oldEntry.Length != newEntry.Length)
                {
                    Console.WriteLine($"File '{oldEntry.FullName}' has different sizes between the two zip files.");
                    continue;
                }

                using (Stream oldEntryStream = oldEntry.Open())
                using (Stream newEntryStream = newEntry.Open())
                {
                    if (!StreamsAreEqual(oldEntryStream, newEntryStream))
                    {
                        Console.WriteLine($"File '{oldEntry.FullName}' has different content between the two zip files.");
                    }
                }
            }
        }

        static bool StreamsAreEqual(Stream stream1, Stream stream2)
        {
            const int bufferSize = 1024 * sizeof(long);
            byte[] buffer1 = new byte[bufferSize];
            byte[] buffer2 = new byte[bufferSize];

            while (true)
            {
                int count1 = stream1.Read(buffer1, 0, bufferSize);
                int count2 = stream2.Read(buffer2, 0, bufferSize);

                if (count1 != count2)
                    return false;

                if (count1 == 0)
                    return true;

                if (!CompareByteArrays(buffer1, buffer2, count1))
                    return false;
            }
        }

        static bool CompareByteArrays(byte[] buffer1, byte[] buffer2, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (buffer1[i] != buffer2[i])
                    return false;
            }

            return true;
        }
    }
}
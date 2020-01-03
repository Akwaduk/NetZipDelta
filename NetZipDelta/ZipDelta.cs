using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace NetZipDelta
{
    public static class ZipDelta
    {
        public static void GetDiffFile(string FileOnePath, string FileTwoPath, string OutputFile)
        {
            // Debug
            FileOnePath = "C:\\Users\\e4451\\Desktop\\";
            FileTwoPath = "C:\\Users\\e4451\\Desktop\\";
            var NewZipPath = "C:\\Users\\e4451\\Desktop\\";
            var FileOneZip = FileOnePath + "Core-1806.zip";
            var FileTwoZip = FileTwoPath + "Core-1908.zip";
            var NewZip = NewZipPath + "NewZip.zip";

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

                        while (OldFiles[i].FullName != NewFiles[j].FullName)
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

                        if (OldFiles[i].FullName == NewFiles[j].FullName)
                        {
                            if (NewFileHasBeenUpdated(OldFiles[i], NewFiles[j]))
                            {
                                FilesToKeep.Add(NewFiles[j]);
                                MatchedOldIndexes.Add(j);
                            } else
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
                        DirectoryInfo di = Directory.CreateDirectory(NewZip + "\\" + directory);

                        var filename = entry.Name;
                        if (string.IsNullOrEmpty(filename) != true && File.Exists(di + "\\" + filename) == false)
                        {
                            entry.ExtractToFile(di + "\\" + filename);
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
                        DirectoryInfo di = Directory.CreateDirectory(NewZip + "\\" + directory);

                        var filename = entry.Name;
                        if (string.IsNullOrEmpty(filename) != true && File.Exists(di + "\\" + filename) == false)
                        {
                            entry.ExtractToFile(di + "\\" + "_DEL_" + filename);
                        }
                    }
                }
            }

            var test = "";
        }

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
    }
}
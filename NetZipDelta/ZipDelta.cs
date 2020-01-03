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
            FileOnePath = "C:\\Users\\akwad\\Desktop\\";
            FileTwoPath = "C:\\Users\\akwad\\Desktop\\";
            var NewZipPath = "C:\\Users\\akwad\\Desktop\\";
            var FileOneZip = FileOnePath + "BackUp.zip";
            var FileTwoZip = FileTwoPath + "BackUp2.zip";            
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

                    for (var i = 0; i < MaxListSize; i++)
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
                            if (OldFiles[i].LastWriteTime == NewFiles[j].LastWriteTime)
                            {
                                MatchedOldIndexes.Add(j);
                                // Same file                        
                            }
                            else
                            {
                                FilesToKeep.Add(NewFiles[j]);
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
                        var length = entry.FullName.IndexOf('/');
                        var directory = entry.FullName.Substring(0, length);
                        DirectoryInfo di = Directory.CreateDirectory(NewZip + "\\" + directory);

                        var filename = entry.Name;
                        entry.ExtractToFile(di + "\\" + filename);
                    }

                    foreach (ZipArchiveEntry entry in FilesToDelete)
                    {
                        var length = entry.FullName.IndexOf('/');
                        var directory = entry.FullName.Substring(0, length);
                        DirectoryInfo di = Directory.CreateDirectory(NewZip + "\\" + directory);

                        var filename = entry.Name;
                        entry.ExtractToFile(di + "\\" + "_DEL_" + filename);
                    }
                }
            }
                        
            var test = "";
        }
    }
}
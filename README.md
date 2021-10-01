# NetZipDelta
Compares 2 zip files and spits out files that have been modified in the newer zip file

Example of functionality can be found in the CLI project. You pass through files locations and the names of the new zip file as well as the location it's to be saved.

The comparison does the following
  1. Loads all files into a list of ZipArchiveEntry classes
  2. Goes through each item in the first list comparing file names with the second list
  3. If there are no matches of name and file path, it adds it to the deletion list because it was removed from the second zip
  4. If there is a name match, it checks if the new file is the same by the following
      - If they were modified on the same date time, it assumes they're the same file
      - If they have different byte length, it assumes the file has been modified
      - If they have the same byte length, and different write times, it compares byte by byte; and if the bytes don't match it assumes the file has been updated
      
 5. It then prefixes all of the deleted files with _DEL_, and retains all of the modified files and places them into a zip file.

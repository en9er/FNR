using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FNR
{
    internal class FileHandler
    {
        public double progress = 0;
        public string currentFile;
        public event Action<double> ProgressChanged;
        public event Action<string> FileChanged;
        private void OnProgressChanged(double progress)
        {
            var eh = ProgressChanged;
            if (eh != null)
            {
                eh(progress);
            }
        }
        private void OnFileChanged(string filename)
        {
            var eh = FileChanged;
            if (eh != null)
            {
                eh(filename);
            }
        }

        public ObservableCollection<string> getDirectoryFiles(string targetDirectory, SearchType searchType, string[] excludeMasks, string mask = "*")
        {
            progress = 0;
            OnProgressChanged(progress);
            if (Directory.Exists(targetDirectory))
            {
                ObservableCollection<string> files = new ObservableCollection<string>();
                if (SearchType.NotRecursive == searchType)
                {
                    progress = 0;
                    string[] fileEntries = Directory.GetFiles(targetDirectory, mask);
                    double step = 100.0 / fileEntries.Length;
                    foreach (string fileName in fileEntries)
                    {
                        Console.WriteLine(fileName);
                        currentFile = fileName;
                        OnFileChanged(fileName);
                        files.Add(fileName);
                        progress += step;
                        OnProgressChanged(progress);
                    }

                    foreach (string emsk in excludeMasks)
                    {
                        if (emsk != "" && emsk != null)
                        {
                            var filesToExclude = Directory.GetFiles(targetDirectory, emsk);
                            foreach (var file in filesToExclude)
                            {
                                int index = files.IndexOf(file);
                                if (index != -1)
                                {
                                    files.RemoveAt(index);
                                }
                            }
                        }
                    }
                }
                else
                {
                    string[] fileEntries = Directory.GetFiles(targetDirectory, mask, SearchOption.AllDirectories);
                    double step = 100.0 / fileEntries.Length;
                    foreach (string fileName in fileEntries)
                    {
                        Console.WriteLine(fileName);
                        currentFile = fileName;
                        OnFileChanged(currentFile);
                        files.Add(fileName);
                        progress += step;
                        OnProgressChanged(progress);
                    }
                    foreach (string emsk in excludeMasks)
                    {
                        if (emsk != "" && emsk != null)
                        {
                            var filesToExclude = Directory.GetFiles(targetDirectory, emsk, SearchOption.AllDirectories);
                            foreach(var file in filesToExclude)
                            {
                                int index = files.IndexOf(file);
                                if (index != -1)
                                {
                                    files.RemoveAt(index);
                                }
                            }
                        }
                    }

                }
                return files;
            }
            else
            {
                return new ObservableCollection<string>();
            }
        }

        public ObservableCollection<string> findMatches(string findText, ObservableCollection<string> filenames)
        {
            progress = 0;
            OnProgressChanged(progress);
            ObservableCollection<string> result = new ObservableCollection<string>();
            double step = 100.0 / filenames.Count;
            foreach (string filename in filenames)
            {
                currentFile = filename;
                OnFileChanged(currentFile);
                //var path = filename.Replace("\\", "/");
                ObservableCollection<string> data = getFileData(filename, "");
                foreach(string str in data)
                {
                    if(str.Contains(findText))
                    {
                        result.Add(filename);
                        progress += step;
                        OnProgressChanged(progress);
                        break;
                    }
                }
            }
            return result;
        }

        public ObservableCollection<string> getFileData(string filename, string findString)
        {
            currentFile = filename;
            OnFileChanged(currentFile);
            if (filename == null || filename == "")
                return new ObservableCollection<string>();
            progress = 0;
            OnProgressChanged(progress);
            var path = filename.Replace("\\", "/");
            if (findString == "")
            {
                ObservableCollection<string> res = new ObservableCollection<string>();
                using (StreamReader sr = new StreamReader(filename))
                {
                    Console.WriteLine(File.GetAttributes(filename));
                    while (sr.EndOfStream == false)
                    {
                        var line = sr.ReadLine();
                        res.Add(line);
                        progress = ((double)sr.BaseStream.Position / sr.BaseStream.Length) * 100;
                        OnProgressChanged(progress);
                    }
                }
                return res;
            }
            else
            {
                ObservableCollection<string> res = new ObservableCollection<string>();
                using (StreamReader sr = new StreamReader(filename, System.Text.Encoding.ASCII))
                {
                    while (sr.EndOfStream == false)
                    {
                        var line = sr.ReadLine();
                        if (line.Contains(findString))
                            res.Add(line);
                        progress = ((double)sr.BaseStream.Position / sr.BaseStream.Length) * 100;
                        OnProgressChanged(progress);
                    }
                }
                return res;
            }
        }

        public void replaceString(string filename, string targetStr, string newString)
        {
            currentFile = filename;
            OnFileChanged(currentFile);
            if(targetStr != "")
            {
                ObservableCollection<string> newFileData = new ObservableCollection<string>();
                using (StreamReader sr = new StreamReader(filename))
                {
                    Console.WriteLine(File.GetAttributes(filename));
                    while (sr.EndOfStream == false)
                    {
                        var line = sr.ReadLine();
                        if (line.Contains(targetStr))
                            newFileData.Add(line.Replace(targetStr, newString));
                        progress = ((double)sr.BaseStream.Position / sr.BaseStream.Length) * 100;
                        OnProgressChanged(progress);
                    }
                }
                File.Create(filename).Close();
                File.WriteAllLines(filename, newFileData);
                
            }
        }
    }
}

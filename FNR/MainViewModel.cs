using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace FNR
{
    enum SearchType { Recursive = 1, NotRecursive}
    internal class MainViewModel : INotifyPropertyChanged
    {
        FileHandler _fileHandler = new FileHandler();
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand IBrowseCommand { get; }
        public ICommand IFindCommand { get; }
        public ICommand IReplaceCommand { get; }
        private string _path = "";
        private bool _searchTypeRecursive = false;
        private ObservableCollection<string> _filenames = new ObservableCollection<string>();
        private string _findText = "";
        private string _replaceText = "";
        private string _selectedFilename = "";
        private double _progress = 0;
        private ObservableCollection<string> _previewData = new ObservableCollection<string>();
        private string _currentFile;
        private string _mask = "";
        private string[] _excludeMask = new string[10];
        private bool _useMask = false;
        private bool _useExcludeMask = true;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool SearchTypeRecursive 
        {
            get { return _searchTypeRecursive; }
            set 
            { 
                if(_searchTypeRecursive != value)
                {
                    _searchTypeRecursive = value;
                }
                FilePath = _path;
                OnPropertyChanged(nameof(SearchTypeRecursive));
            }
        }

        public string FindText
        {
            get { return _findText; }
            set
            {
                Console.WriteLine(value);
                _findText = value;
                OnPropertyChanged(nameof(FindText));
            }
        }

        public string ReplaceText
        {
            get { return _replaceText; }
            set
            {
                Console.WriteLine(value);
                _replaceText = value;
                OnPropertyChanged(nameof(ReplaceText));
            }
        }


        public int FilesCount
        {
            get { return _filenames.Count; }
        }

        public string FilePath
        {
            get { return _path; }
            set
            {
                _path = value;
                FileNames = _fileHandler.getDirectoryFiles(FilePath, SearchTypeRecursive? SearchType.Recursive: SearchType.NotRecursive, UseExcludeMask? _excludeMask: new string[0]{}, UseMask? Mask:"*");
                OnPropertyChanged(nameof(FilePath));
            }
        }

        public string SelectedFilename
        {
            get 
            {
                return _selectedFilename; 
            }
            set
            {
                _selectedFilename = value;
                Task.Run(() =>
                {
                    GetFileData();
                });
                OnPropertyChanged(nameof(SelectedFilename));
                OnPropertyChanged(nameof(CurrentFilename));    
            }
        }
        private void GetFileData()
        {
            PreviewFile = _fileHandler.getFileData(SelectedFilename, FindText);
            OnPropertyChanged(nameof(PreviewFile));
        }
        public ObservableCollection<string> PreviewFile
        {
            get 
            { 
                if(SelectedFilename != "" && SelectedFilename != null)
                {
                    return _previewData;
                }
                else
                {
                    return new ObservableCollection<string>();
                }
            }
            set
            {
                if (_previewData == value)
                    return;
                _previewData = value;
                OnPropertyChanged(nameof(PreviewFile));
            }
        }

        public ObservableCollection<string> FileNames
        {
            get { return _filenames; }
            set
            {
                if(_filenames != value)
                {
                    _filenames = value;
                }
                OnPropertyChanged(nameof(FileNames));
                OnPropertyChanged(nameof(FilesCount));
            }
        }
        public MainViewModel()
        {
            _excludeMask[0] = "*.exe";
            _excludeMask[1] = "*.dll";
            _fileHandler.ProgressChanged += ProgressChanged;
            _fileHandler.FileChanged += FileChanged;
            IBrowseCommand = new RelayCommand<string>(x =>
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.Description = "Choose directory";
                dialog.ShowNewFolderButton = true;
                DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    FilePath = dialog.SelectedPath;
                }
            });

            IFindCommand = new RelayCommand<string>(x =>
            {
                if(FindText == "")
                {

                }
                else
                {
                    if (FilePath == "")
                    {
                        IBrowseCommand.Execute(null);
                    }
                    Task.Run(() =>
                    {
                        FileNames = _fileHandler.findMatches(FindText, FileNames);
                        SelectedFilename = SelectedFilename;
                    });
                }
            });

            IReplaceCommand = new RelayCommand<string>(x =>
            {
                foreach(string filename in FileNames)
                {
                    _fileHandler.replaceString(filename, FindText, ReplaceText);
                }
            });
        }

        public string CurrentFilename
        {
            get { return _currentFile; }
            set
            {
                Console.WriteLine(value);
                _currentFile = value;
                OnPropertyChanged(nameof(CurrentFilename));
            }
        }

        public string Mask
        {
            get
            {
                if (!UseMask)
                {
                    _mask = "*";
                }
                return _mask; 
            }
            set
            {
                _mask = value;
                FilePath = _path;
                OnPropertyChanged(nameof(Mask));
            }
        }

        public string ExcludeMask
        {
            get
            {
                string res = "";
                foreach(string mask in _excludeMask)
                {
                    if(!String.IsNullOrEmpty(mask))
                        res += mask + ", ";
                }
                return res;
            }
            set
            {
                var masks = new ObservableCollection<string>();
                _excludeMask = value.Replace(" ", "").Split(',');
                FilePath = _path;
                OnPropertyChanged(nameof(ExcludeMask));
            }
        }

        private void ProgressChanged(double progress)
        {
            Progress = progress;
        }

        private void FileChanged(string filename)
        {
            CurrentFilename = filename;
        }

        public double Progress
        {
            get { return Math.Round(_progress); }
            set
            {
                _progress = value;
                if(_progress > 100)
                    _progress = 100;
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(ProgressPercent));
            }
        }
        public string ProgressPercent
        {
            get { return Progress.ToString() + "%"; }
        }

        public bool UseMask
        {
            get { return _useMask;}
            set
            {
                _useMask = value;
                FilePath = _path;
                OnPropertyChanged(nameof(UseMask));
            }
        }

        public bool UseExcludeMask
        {
            get { return _useExcludeMask; }
            set
            {
                _useExcludeMask = value;
                FilePath = _path;
                OnPropertyChanged(nameof(UseExcludeMask));
                OnPropertyChanged(nameof(ExcludeMask));
            }
        }
    }
}

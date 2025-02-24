using System.ComponentModel;
using System;
using System.IO;


namespace EasyLib.Models
{
    public class PriorityModel : INotifyPropertyChanged
    {
        private bool _isDocx;
        private bool _isPdf;
        private bool _isTxt;
        private bool _isJpg;

        public bool IsDocx
        {
            get => _isDocx;
            set
            {
                if (_isDocx != value)
                {
                    _isDocx = value;
                    OnPropertyChanged(nameof(IsDocx));
                }
            }
        }

        public bool IsPdf
        {
            get => _isPdf;
            set
            {
                if (_isPdf != value)
                {
                    _isPdf = value;
                    OnPropertyChanged(nameof(IsPdf));
                }
            }
        }

        public bool IsTxt
        {
            get => _isTxt;
            set
            {
                if (_isTxt != value)
                {
                    _isTxt = value;
                    OnPropertyChanged(nameof(IsTxt));
                }
            }
        }

        public bool IsJpg
        {
            get => _isJpg;
            set
            {
                if (_isJpg != value)
                {
                    _isJpg = value;
                    OnPropertyChanged(nameof(IsJpg));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

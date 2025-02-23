using System;
using System.IO;
using System.ComponentModel;

namespace EasyLib.Models
{
    public class EncoursModel : INotifyPropertyChanged
    {
        private int _id;
        private long _progress;
        private string name;
        private bool _enCoursbool;

        public int ID
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(ID)); }
        }
        public string Name
        {
            get => name;
            set { name = value; OnPropertyChanged(nameof(Name)); }
        }

        public long Progress
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(nameof(Progress)); }
        }

public bool EnCoursbool
    {
        get { return _enCoursbool; }
        set
        {
            if (_enCoursbool != value)
            {
                _enCoursbool = value;
                OnPropertyChanged(nameof(EnCoursbool)); // Notifie la vue que la propriété a changé
            }
        }
    }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public EncoursModel(int id, string name)
        {
            ID = id;
            Name = name;
            Progress = 0;
            EnCoursbool = false;
        }


        public void UpdateEnCoursboole(bool newValue)
        {
            EnCoursbool = newValue;  

        }

        public void UpdateProgress(long newValue)
        {
            Progress = newValue;  

        }

    }
}

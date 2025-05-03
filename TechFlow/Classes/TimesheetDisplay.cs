using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TechFlow.Classes
{
    public class TimesheetDisplay : INotifyPropertyChanged
    {
        private string _day;
        private string _january = string.Empty;
        private string _february = string.Empty;
        private string _march = string.Empty;
        private string _april = string.Empty;
        private string _may = string.Empty;
        private string _june = string.Empty;
        private string _july = string.Empty;
        private string _august = string.Empty;
        private string _september = string.Empty;
        private string _october = string.Empty;
        private string _november = string.Empty;
        private string _december = string.Empty;

        public string Day
        {
            get => _day;
            set
            {
                if (_day != value)
                {
                    _day = value;
                    OnPropertyChanged();
                }
            }
        }

        public string January
        {
            get => _january;
            set
            {
                if (_january != value)
                {
                    _january = value;
                    OnPropertyChanged();
                }
            }
        }

        public string February
        {
            get => _february;
            set
            {
                if (_february != value)
                {
                    _february = value;
                    OnPropertyChanged();
                }
            }
        }

        public string March
        {
            get => _march;
            set
            {
                if (_march != value)
                {
                    _march = value;
                    OnPropertyChanged();
                }
            }
        }

        public string April
        {
            get => _april;
            set
            {
                if (_april != value)
                {
                    _april = value;
                    OnPropertyChanged();
                }
            }
        }

        public string May
        {
            get => _may;
            set
            {
                if (_may != value)
                {
                    _may = value;
                    OnPropertyChanged();
                }
            }
        }

        public string June
        {
            get => _june;
            set
            {
                if (_june != value)
                {
                    _june = value;
                    OnPropertyChanged();
                }
            }
        }

        public string July
        {
            get => _july;
            set
            {
                if (_july != value)
                {
                    _july = value;
                    OnPropertyChanged();
                }
            }
        }

        public string August
        {
            get => _august;
            set
            {
                if (_august != value)
                {
                    _august = value;
                    OnPropertyChanged();
                }
            }
        }

        public string September
        {
            get => _september;
            set
            {
                if (_september != value)
                {
                    _september = value;
                    OnPropertyChanged();
                }
            }
        }

        public string October
        {
            get => _october;
            set
            {
                if (_october != value)
                {
                    _october = value;
                    OnPropertyChanged();
                }
            }
        }

        public string November
        {
            get => _november;
            set
            {
                if (_november != value)
                {
                    _november = value;
                    OnPropertyChanged();
                }
            }
        }

        public string December
        {
            get => _december;
            set
            {
                if (_december != value)
                {
                    _december = value;
                    OnPropertyChanged();
                }
            }
        }

        public void SetWorkType(int monthIndex, string statusCode)
        {
            switch (monthIndex)
            {
                case 0: January = statusCode; break;
                case 1: February = statusCode; break;
                case 2: March = statusCode; break;
                case 3: April = statusCode; break;
                case 4: May = statusCode; break;
                case 5: June = statusCode; break;
                case 6: July = statusCode; break;
                case 7: August = statusCode; break;
                case 8: September = statusCode; break;
                case 9: October = statusCode; break;
                case 10: November = statusCode; break;
                case 11: December = statusCode; break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
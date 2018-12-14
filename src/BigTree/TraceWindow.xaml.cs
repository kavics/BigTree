﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BigTree
{
    /// <summary>
    /// Interaction logic for TraceWindow.xaml
    /// </summary>
    public partial class TraceWindow : Window, INotifyPropertyChanged
    {
        public TraceWindow()
        {
            InitializeComponent();
            mainGrid.DataContext = this;
        }

        private int _iteration; //UNDONE: move to the Trace window
        public int Iteration
        {
            get { return _iteration; }
            set
            {
                if (value != _iteration)
                {
                    _iteration = value;
                    OnPropertyChanged(nameof(Iteration));
                }
            }
        }

        private const float ForceEndPoint = 0.08f;
        private string _forceMax; //UNDONE: move to the Trace window
        public string ForceMax
        {
            get { return _forceMax; }
            set
            {
                if (value != _forceMax)
                {
                    _forceMax = value;
                    OnPropertyChanged(nameof(ForceMax));
                }
            }
        }

        private DateTime _startTime;
        private string _measuringTimeText; //UNDONE: move to the Trace window
        public string MeasuringTimeText
        {
            get { return _measuringTimeText; }
            set
            {
                if (value != _measuringTimeText)
                {
                    _measuringTimeText = value;
                    OnPropertyChanged(nameof(MeasuringTimeText));
                }
            }
        }

        private string _calcTime; //UNDONE: move to the Trace window
        public string CalcTime
        {
            get { return _calcTime; }
            set
            {
                if (value != _calcTime)
                {
                    _calcTime = value;
                    OnPropertyChanged(nameof(CalcTime));
                }
            }
        }

        private string _drawTime; //UNDONE: move to the Trace window
        public string DrawTime
        {
            get { return _drawTime; }
            set
            {
                if (value != _drawTime)
                {
                    _drawTime = value;
                    OnPropertyChanged(nameof(DrawTime));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private bool _exit;
        internal void Exit()
        {
            _exit = true;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_exit)
            {
                e.Cancel = true;
                this.Hide();
            }
        }
    }
}

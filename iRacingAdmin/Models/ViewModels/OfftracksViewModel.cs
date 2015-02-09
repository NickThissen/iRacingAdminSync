using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Alchemy;
using iRacingAdmin.Classes;
using iRacingAdmin.Models.Drivers;
using iRacingAdmin.Sync;
using iRacingAdmin.Views;
using iRacingSimulator;
using Swordfish.NET.Charts;

namespace iRacingAdmin.Models.ViewModels
{
    public class OfftracksViewModel : ViewModelBase
    {
        private MainViewModel _mainModel;
        private ChartControl _chart;

        private readonly Dictionary<int, DriverOfftrackLine> _lines;

        private DispatcherTimer _updateTimer;

        public OfftracksViewModel(MainViewModel mainModel)
        {
            _colors = new Stack<Color>(new Color[]
                                       {
                                           Colors.White,
                                           Colors.Green,
                                           Colors.Pink,
                                           Colors.Orange,
                                           Colors.LightBlue,
                                           Colors.Gold,
                                           Colors.Fuchsia,
                                           Colors.LimeGreen,
                                           Colors.Blue,
                                           Colors.Red,
                                       });
            _lines = new Dictionary<int, DriverOfftrackLine>();
            _mainModel = mainModel;
            _limitsModel = new OfftrackLimitsViewModel(this);
        }

        private OfftrackLimitsViewModel _limitsModel;
        public OfftrackLimitsViewModel LimitsModel { get { return _limitsModel; } }

        public enum ChartUpdateTypes
        {
            None = 0,
            Slow = 1,
            Fast
        }

        public ChartUpdateTypes ChartUpdateType
        {
            get { return _chartUpdateType; }
            set
            {
                _chartUpdateType = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("ShouldNotUpdate");
                this.OnPropertyChanged("ShouldUpdateSlow");
                this.OnPropertyChanged("ShouldUpdateFast");
                this.SetUpdateTimer();
            }
        }

        public bool ShouldNotUpdate
        {
            get { return this.ChartUpdateType == ChartUpdateTypes.None; }
            set
            {
                if (value) this.ChartUpdateType = ChartUpdateTypes.None;
            }
        }

        public bool ShouldUpdateSlow
        {
            get { return this.ChartUpdateType == ChartUpdateTypes.Slow; }
            set { if (value) this.ChartUpdateType = ChartUpdateTypes.Slow; }
        }

        public bool ShouldUpdateFast
        {
            get { return this.ChartUpdateType == ChartUpdateTypes.Fast; }
            set { if (value) this.ChartUpdateType = ChartUpdateTypes.Fast; }
        }

        public List<DriverOfftrackLine> DriverLines { get { return _lines.Values.ToList(); } } 

        public void SetChart(ChartControl chart)
        {
            _chart = chart;
            this.ResetChart();
        }

        public void SetUpdateTimer()
        {
            if (_updateTimer != null && _updateTimer.IsEnabled) _updateTimer.Stop();

            if (ShouldNotUpdate) return;

            var time = this.ChartUpdateType == ChartUpdateTypes.Fast
                ? TimeSpan.FromSeconds(0.1)
                : TimeSpan.FromSeconds(2);
            _updateTimer = new DispatcherTimer(time, DispatcherPriority.Background,
                (o, e) => UpdateLines(), _mainModel.View.Dispatcher);
            _updateTimer.Start();
        }

        public void OnSyncStateUpdated()
        {
            this.UpdateLines();

            // Check for offtrack limit
            this.LimitsModel.CheckLimits();
        }

        public void OnSelectedDriversChanged()
        {
            this.UpdateLines();
        }

        //public void ClearOfftracks()
        //{
        //    SyncManager.Instance.SendStateUpdate(SyncCommandHelper.ClearOfftracks());

        //    foreach (var driver in Simulator.Instance.Drivers.ToList())
        //    {
        //        driver.Driver.OfftrackHistory.Clear();
        //    }
        //    this.UpdateLines();
        //}

        public void NotifyOfftrackLimit(DriverOfftrackLimit limit)
        {
            _mainModel.NotifyOfftrackLimit(limit);
        }

        public void UpdateLines()
        {
            if (_chart == null) return;

            this.ResetLines();
            this.ResetChart();

            // Keep existing drivers in plot and add new drivers
            foreach (var driver in _mainModel.DriverList.SelectedDrivers)
            {
                var line = GetLine(driver);
                _chart.AddPrimitive(line.Line);
            }

            // Remove unselected drivers
            var untaken = _lines.Values.Where(l => !l.Taken).ToArray();
            foreach (var line in untaken)
            {
                ResetColor(line.Line.LineColor);
                _lines.Remove(line.Driver.Driver.Id);
            }

            this.OnPropertyChanged("DriverLines");
            _chart.RedrawPlotLines();
        }

        private DriverOfftrackLine GetLine(DriverContainer driver)
        {
            DriverOfftrackLine line;

            if (!_lines.ContainsKey(driver.Driver.Id))
            {
                line = new DriverOfftrackLine();
                line.Driver = driver;
                line.Line = _chart.CreateXY();
                line.Line.IsHitTest = true;
                line.Line.LineThickness = 2;
                line.Line.LineColor = GetColor();
                //line.Line.LegendColor = line.Line.LineColor;
                //line.Line.Label = driver.Driver.ShortName;
                //line.Line.LegendLabel = new ColorLabel(driver.Driver.ShortName, Colors.White);
                
                _lines.Add(driver.Driver.Id, line);
            }
            else
            {
                line = _lines[driver.Driver.Id];
            }

            this.SetOfftrackPoints(line);

            line.Taken = true;
            return line;
        }

        private void ResetLines()
        {
            foreach (var line in _lines.Values)
            {
                line.Taken = false;
            }
        }

        private Stack<Color> _colors;
        private ChartUpdateTypes _chartUpdateType;

        private Color GetColor()
        {
            if (_colors.Count > 0) return _colors.Pop();
            return Colors.Red;
        }

        private void ResetColor(Color color)
        {
            _colors.Push(color);
        }

        private void SetOfftrackPoints(DriverOfftrackLine line)
        {
            line.Line.Points.Clear();
            int count = 0;
            foreach (var offtrack in line.Driver.Driver.OfftrackHistory.Offtracks.OrderBy(o => o.StartTime))
            {
                var x = offtrack.StartTime;
                line.Line.AddPoint(x, count);
                line.Line.AddPoint(x, ++count);
            }

            if (Connection.Instance.IsRunning && !Connection.Instance.IsSimulated)
            {
                // add final point on current time
                line.Line.AddPoint(Simulator.Instance.Telemetry.SessionTime.Value, count);
            }
        }

        private void ResetChart()
        {
            _chart.Reset();
            _chart.XAxisTitle = "Time";
            _chart.YAxisTitle = "Offtrack count";
        }

        #region Commands

        private ICommand _viewLimitsCommand;
        public ICommand ViewLimitsCommand
        {
            get { return _viewLimitsCommand ?? (_viewLimitsCommand = new RelayCommand(ViewLimitsExecute)); }
        }

        private ICommand _clearOfftracksCommand;
        public ICommand ClearOfftracksCommand
        {
            get
            {
                return _clearOfftracksCommand ??
                       (_clearOfftracksCommand = new RelayCommand(ClearOfftracksExecute, CanExecuteClearOfftracks));
            }
        }

        private void ViewLimitsExecute(object param)
        {
            _limitsModel.Show();
        }

        private void ClearOfftracksExecute(object param)
        {
            if (CanExecuteClearOfftracks(param))
            {
                if (MessageWindow.Show("Confirm",
                    "Are you sure you want to clear all offtracks for every driver for this session? There is no way to recover the offtracks.",
                    MessageBoxButton.YesNoCancel, Brushes.Tomato) == MessageBoxResult.Yes)
                {
                    this.ClearOfftracks();
                }
            }
            else
            {
                MessageWindow.Show("Not authorized", "Only the server host can clear the offtracks.",
                    MessageBoxButton.OK);
            }
        }

        private bool CanExecuteClearOfftracks(object param)
        {
            return SyncManager.Instance.Status == SyncManager.ConnectionStatus.Disconnected
                   || (SyncManager.Instance.User != null && SyncManager.Instance.User.IsHost);
        }

        #endregion
    }
}

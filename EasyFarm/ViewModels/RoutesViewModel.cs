
/*///////////////////////////////////////////////////////////////////
<EasyFarm, general farming utility for FFXI.>
Copyright (C) <2013 - 2014>  <Zerolimits>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
*/
///////////////////////////////////////////////////////////////////

using EasyFarm.Classes;
using FFACETools;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace EasyFarm.ViewModels
{
    public class RoutesViewModel : ViewModelBase
    {
        DispatcherTimer WaypointRecorder = new DispatcherTimer();
        FFACE.Position LastPosition = new FFACE.Position();
        
        public RoutesViewModel(ref GameEngine Engine, IEventAggregator eventAggregator) : 
            base(ref Engine, eventAggregator) 
        {
            WaypointRecorder.Tick += new EventHandler(RouteRecorder_Tick);
            WaypointRecorder.Interval = new TimeSpan(0, 0, 1);

            ClearRouteCommand = new DelegateCommand(ClearRoute);
            RecordRouteCommand = new DelegateCommand<Object>(RecordRoute);
            SaveCommand = new DelegateCommand(SaveRoute);
            LoadCommand = new DelegateCommand(LoadRoute);
        }

        public ObservableCollection<Waypoint> Route
        {
            get { return _engine.UserSettings.Waypoints; }
            set { SetProperty(ref this._engine.UserSettings.Waypoints, value); }
        }

        public ICommand RecordRouteCommand { get; set; }

        public ICommand ClearRouteCommand { get; set; }

        public ICommand SaveCommand { get; set; }

        public ICommand LoadCommand { get; set; }

        public void ClearRoute()
        {
            Route = new ObservableCollection<Waypoint>();
        }

        public void RecordRoute(Object RecordButton)
        {
            if (!WaypointRecorder.IsEnabled)
            {
                WaypointRecorder.Start();
                (RecordButton as Button).Content = "Recording!";
            }
            else
            {
                WaypointRecorder.Stop();
                (RecordButton as Button).Content = "Record";
            }
        }

        private void SaveRoute()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = Directory.GetCurrentDirectory();
            sfd.DefaultExt = ".wpl";
            sfd.Filter = "Waypoint lists (.wpl)|*.wpl";

            if (sfd.ShowDialog() == true)
            {
                Utilities.Serialize(sfd.SafeFileName, Route);
            }
        }

        private void LoadRoute()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Directory.GetCurrentDirectory();
            ofd.DefaultExt = ".wpl";
            ofd.Filter = "Waypoint lists (.wpl)|*.wpl";
            
            if (ofd.ShowDialog() == true)
            {
                Route = Utilities.Deserialize(ofd.SafeFileName, Route);
            }
        }

        void RouteRecorder_Tick(object sender, EventArgs e)
        {
            var Point = _engine.Session.Instance.Player.Position;

            if (!Point.Equals(LastPosition))
            {
                _engine.UserSettings.Waypoints.Add(new Waypoint(Point));
                LastPosition = Point;
            }
        }
    }
}
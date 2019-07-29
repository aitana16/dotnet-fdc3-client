﻿using OpenFin.FDC3.Channels;
using OpenFin.FDC3.Context;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenFin.FDC3.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private bool contextChanging = false;
        private Connection connection;
        SecondWindow win2 = new SecondWindow();       
        public MainWindow()
        {
            InitializeComponent();                      

            FDC3.InitializationComplete = DesktopAgent_Initialized;
#if DEBUG
            FDC3.Initialize($"{System.IO.Directory.GetCurrentDirectory()}\\app.json");
#else
            FDC3.Initialize();
#endif
        }

        private async void DesktopAgent_Initialized()
        {
            connection = await ConnectionManager.CreateConnectionAsync("mainwin");
            connection.AddContextHandler(ContextChanged);

            var channels = await connection.GetDesktopChannelsAsync();            

            await Dispatcher.InvokeAsync(async () =>
            {
                tbAppId.Text = FDC3.Uuid;

                foreach (var channel in channels)
                {
                    if (channel.ChannelType == ChannelType.Desktop)
                    {
                        ChannelListComboBox.Items.Add(new ComboBoxItem() { Content = channel.Name, Tag = channel });
                    }
                }

                ChannelListComboBox.SelectedValue = "Global";              
            });

            connection.AddContextHandler(ContextChanged);
        }

        private void ContextChanged(ContextBase obj)
        {
            Dispatcher.Invoke(() =>
            {
                contextChanging = true;
                TickerComboBox.SelectedValue = obj.Name;
                contextChanging = false;
            });
        }

        private async void TickerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!contextChanging)
            {
                var newTicker = TickerComboBox.SelectedValue as string;
                var context = new SecurityContext()
                {
                    Name = newTicker,
                    Id = new Dictionary<string, string>()
                    {
                        { "default", newTicker }
                    }
                };

                try
                {

                    await connection.BroadcastAsync(context);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        DesktopChannel newChannel; 

        private async void ChannelListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            newChannel = (ChannelListComboBox.SelectedItem as ComboBoxItem).Tag as DesktopChannel;

            var newColor = Color.FromRgb(
                (byte)(newChannel.Color >> 16),
                (byte)((newChannel.Color >> 8) & 0xFF),
                (byte)(newChannel.Color & 0xFF));

            Dispatcher.Invoke(() =>
            {
                ChannelColorEllipse.Fill = new SolidColorBrush() { Color = newColor };
            });

            try
            {                
                await newChannel.JoinAsync();                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void BtnLaunch_Click(object sender, RoutedEventArgs e)
        {
            win2.Show();
        }
    }
}
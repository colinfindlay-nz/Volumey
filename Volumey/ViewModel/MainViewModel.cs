﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Interop;
using log4net;
using Microsoft.Xaml.Behaviors.Core;
using Volumey.DataProvider;
using Volumey.Helper;
using Volumey.ViewModel.Settings;

namespace Volumey.ViewModel
{
	public sealed class MainViewModel : INotifyPropertyChanged
	{
		public ICommand ClosingCommand { get; }
		public ICommand TrayMixerCommand { get; }
		public ICommand TraySettingsCommand { get; }
		public ICommand TrayExitCommand { get; }
		public ICommand TrayClickCommand { get; }
		public ICommand SoundControlPanelCommand { get; }
		public ICommand SoundSettingsCommand { get; }
		
		private int selectedTabIndex;
		public int SelectedTabIndex
		{
			get => selectedTabIndex;
			set
			{
				selectedTabIndex = value;
				OnPropertyChanged();
			}
		}
		
		private bool windowIsVisible;
		public bool WindowIsVisible
		{
			get => windowIsVisible;
			set
			{
				windowIsVisible = value;
				OnPropertyChanged();
			}
		}

        public event Action OpenCommandEvent;

		public MainViewModel()
		{
			this.WindowIsVisible = !Startup.StartMinimized;
			this.ClosingCommand = new ActionCommand(OnWindowClosing);
			this.TrayMixerCommand = new ActionCommand(() => OpenCommand(tabIndex: 0));
			this.TraySettingsCommand = new ActionCommand(() => OpenCommand(tabIndex: 1));
			this.TrayClickCommand = new ActionCommand(() => OpenCommand(tabIndex: this.SelectedTabIndex));
			this.TrayExitCommand = new ActionCommand(OnExit);
			this.SoundControlPanelCommand = new ActionCommand(SystemSoundUtilities.StartSoundControlPanel);
			this.SoundSettingsCommand = new ActionCommand(SystemSoundUtilities.StartSoundSettings);
			App.Current.SessionEnding += (sender, args) => OnExit();

			HotkeysControl.OpenHotkeyPressed += OnOpenHotkeyPressed;
			ComponentDispatcher.ThreadFilterMessage += OnThreadFilterMessage;
		}

		private async void OnExit()
		{
			await SettingsProvider.SaveSettings().ConfigureAwait(true);
			LogManager.Shutdown();
			HotkeysControl.Dispose();
			App.Current.Shutdown();
		}

		private void OpenCommand(int tabIndex)
		{
			if (this.SelectedTabIndex != tabIndex)
				this.SelectedTabIndex = tabIndex;

			if (!WindowIsVisible)
				WindowIsVisible = true;
			else
			{
				//bring the window to the foreground if it's already visible 
				this.OpenCommandEvent?.Invoke();
			}
		}
		
		private void OnThreadFilterMessage(ref MSG msg, ref bool handled)
		{
			//"The operating system communicates with your application window by passing messages to it.
			// A message is simply a numeric code that designates a particular event."
			//When a user attempts to launch the second instance of the app
			//it sends a window message of type WM_SHOWME to all top-level windows in the system.
			//This event handler checks every window message for this type of message
			//to bring the window of the first instance to top or make it visible
			//if it was minimized to tray

			if(msg.message == Startup.WM_SHOWME)
				this.OpenCommand(this.SelectedTabIndex);
		}
		
		private void OnOpenHotkeyPressed()
		{
			this.OpenCommand(0);
		}
		
		private void OnWindowClosing(object e)
		{
            if (e is CancelEventArgs args)
            {
                args.Cancel = true;
                this.WindowIsVisible = false;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
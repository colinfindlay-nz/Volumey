﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Input;
using log4net;
using MahApps.Metro.Controls;

namespace Volumey.DataProvider
{
	[Serializable]
	public enum AppTheme
	{
		Dark,
		Light
	};
	
	[Serializable]
	public class AppSettings
	{
		private readonly HotkeysAppSettings hotkeysSettings = new HotkeysAppSettings();

		public HotkeysAppSettings HotkeysSettings => hotkeysSettings;
		
		private AppTheme currentAppTheme = AppTheme.Light;
		public AppTheme CurrentAppTheme
		{
			get => currentAppTheme;
			set => currentAppTheme = value;
		}

		private int volumeStep = 1;
		public int VolumeStep
		{
			get => volumeStep;
			set => volumeStep = value;
		}

		private string appLanguage;
		public string AppLanguage
		{
			get => appLanguage;
			set => appLanguage = value;
		}

		private string systemLanguage;
		public string SystemLanguage
		{
			get => systemLanguage;
			set => systemLanguage = value;
		}

		private string systemSoundsName;
		public string SystemSoundsName
		{
			get => systemSoundsName;
			set => systemSoundsName = value;
		}

		[OptionalField]
		private bool volumeLimitIsOn;
		public bool VolumeLimitIsOn
		{
			get => volumeLimitIsOn;
			set => volumeLimitIsOn = value;
		}

		[OptionalField]
		private int volumeLimit = 50;
		public int VolumeLimit
		{
			get => volumeLimit;
			set => volumeLimit = value;
		}

		private bool userHasRated;
		public bool UserHasRated
		{
			get => userHasRated;
			set => userHasRated = value;
		}
		
		private DateTime firstLaunchDate;
		public DateTime FirstLaunchDate
		{
			get => firstLaunchDate;
			set => firstLaunchDate = value;
		}

		private int launchCount = 1;
		public int LaunchCount
		{
			get => launchCount;
			set => launchCount = value;
		}
		
		[Serializable]
		public class HotkeysAppSettings
		{
			private string MusicAppName { get; set; }

			private bool VolumeUpIsEmpty => VolumeUpKey == Key.None && VolumeUpModifiers == ModifierKeys.None;
			private bool VolumeDownIsEmpty => VolumeDownKey == Key.None && VolumeDownModifiers == ModifierKeys.None;

			private Key VolumeUpKey;
			private ModifierKeys VolumeUpModifiers;

			private Key VolumeDownKey;
			private ModifierKeys VolumeDownModifiers;

			private Key OpenMixerKey;
			private ModifierKeys OpenMixerModifiers;

			internal HotKey DeviceVolumeUp
			{
				get
				{
					if(VolumeUpKey != Key.None || VolumeUpModifiers != ModifierKeys.None)
						return new HotKey(VolumeUpKey, VolumeUpModifiers);
					return null;
				}
				set
				{
					if(value == null)
					{
						this.VolumeUpKey = Key.None;
						this.VolumeUpModifiers = ModifierKeys.None;
					}
					else
					{
						this.VolumeUpKey = value.Key;
						this.VolumeUpModifiers = value.ModifierKeys;
					}
				}
			}

			internal HotKey DeviceVolumeDown
			{
				get
				{
					if(VolumeDownKey != Key.None || VolumeDownModifiers != ModifierKeys.None)
						return new HotKey(VolumeDownKey, VolumeDownModifiers);
					return null;
				}
				set
				{
					if(value == null)
					{
						this.VolumeDownKey = Key.None;
						this.VolumeDownModifiers = ModifierKeys.None;
					}
					else
					{
						this.VolumeDownKey = value.Key;
						this.VolumeDownModifiers = value.ModifierKeys;
					}
				}
			}

			internal HotKey OpenMixer
			{
				get
				{
					if(OpenMixerKey != Key.None || OpenMixerModifiers != ModifierKeys.None)
						return new HotKey(OpenMixerKey, OpenMixerModifiers);
					return null;
				}
				set
				{
					if(value == null)
					{
						this.OpenMixerKey = Key.None;
						this.OpenMixerModifiers = ModifierKeys.None;
					}
					else
					{
						this.OpenMixerKey = value.Key;
						this.OpenMixerModifiers = value.ModifierKeys;
					}
				}
			}

			[OptionalField]
			private Dictionary<string, (SerializableHotkey up, SerializableHotkey down)> serializableRegisteredSessions;

			internal ObservableConcurrentDictionary<string, Tuple<HotKey, HotKey>> GetRegisteredSessions()
			{
				var dictionary = new ObservableConcurrentDictionary<string, Tuple<HotKey, HotKey>>();
				if(this.serializableRegisteredSessions == null)
				{
					this.serializableRegisteredSessions = new Dictionary<string, (SerializableHotkey, SerializableHotkey)>();
					return dictionary;
				}
				if(this.serializableRegisteredSessions.Count == 0)
					return dictionary;
				foreach(var (key, (up, down)) in this.serializableRegisteredSessions)
				{
					dictionary.Add(key, new Tuple<HotKey, HotKey>(up.ToHotKey(), down.ToHotKey()));
				}
				return dictionary;
			}

			internal void AddRegisteredSession(string sessionName, Tuple<HotKey, HotKey> hotkeys)
			{
				if(sessionName == null || hotkeys.Item1 == null || hotkeys.Item2 == null)
					return;
				this.serializableRegisteredSessions.Add(sessionName, hotkeys.ToTuple());
			}

			internal void RemoveRegisteredSession(string sessionName)
			{
				if(string.IsNullOrEmpty(sessionName))
					return;
				this.serializableRegisteredSessions.Remove(sessionName);
			}

			[OnDeserialized]
			private void OnDeserialized(StreamingContext context)
			{
				//convert values from the old version of settings class to the new one
				if(this.MusicAppName != null && !VolumeUpIsEmpty && !VolumeDownIsEmpty)
				{
					this.serializableRegisteredSessions ??= new Dictionary<string, (SerializableHotkey, SerializableHotkey)>();
					this.serializableRegisteredSessions.Add(MusicAppName, (new SerializableHotkey(this.VolumeUpKey, this.VolumeUpModifiers),
						                                         new SerializableHotkey(this.VolumeDownKey, this.VolumeDownModifiers)));
					LogManager.GetLogger(typeof(AppSettings)).Info($"Converted old hotkeys to a new version. App: [{this.MusicAppName}] +vol: [{this.VolumeUpKey}] -vol: [{this.VolumeDownKey}]");
					this.MusicAppName = null;
					this.VolumeUpKey = VolumeDownKey = Key.None;
					this.VolumeUpModifiers = VolumeDownModifiers = ModifierKeys.None;
				}
			}
		}

		[Serializable]
		internal readonly struct SerializableHotkey
		{
			private readonly Key _key;
			private readonly ModifierKeys _modifierKeys;
			internal HotKey ToHotKey() => new HotKey(this._key, this._modifierKeys);

			internal SerializableHotkey(Key key, ModifierKeys modifierKeys)
			{
				_key = key;
				_modifierKeys = modifierKeys;
			}
		}
	}
}
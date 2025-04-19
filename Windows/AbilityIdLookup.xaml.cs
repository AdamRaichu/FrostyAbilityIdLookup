using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Frosty.Controls;
using Frosty.Core;
using Frosty.Core.Windows;
using FrostySdk;
using FrostySdk.IO;

namespace AbilityIdLookupPlugin.Windows
{
    public partial class AbilityIdLookupWindow : FrostyDockableWindow
    {
        private Dictionary<uint, string> idToNameMap = new Dictionary<uint, string>();
        public bool failed = false;

        public AbilityIdLookupWindow()
        {
            FrostyTaskWindow.Show("Loading Ability Id Cache", "Loading Ability Id Cache...", (task) =>
            {
                task.Update("Loading Cache", 0);
                if (LoadCache(task))
                {
                    task.Update("Cache Loaded", 100);
                }
                else
                {
                    task.Update("Cache Load Failed", 100);
                    failed = true;
                }
            });
            InitializeComponent();
            if (failed)
            {
                Close();
            }
        }

        private void IdInput_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                AbilityPath.Text = idToNameMap.TryGetValue(Convert.ToUInt32(IdInput.Text, CultureInfo.InvariantCulture), out string name) ? name : "Not Found (Added asset or modified id?)";
            }
            catch (FormatException)
            {
                AbilityPath.Text = "Invalid Id (make sure you only put in numbers)";
            }
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private bool LoadCache(FrostyTaskWindow task)
        {
            string cacheFilePath = AbilityIdGenerateCacheMenuExtension.CacheFilePath;
            if (!File.Exists(cacheFilePath))
            {
                FrostyMessageBox.Show("Cache file not found. Please generate the cache first through \"Tools>Generate Cache>Ability Id Cache\".");
                return false;
            }
            using (NativeReader reader = new NativeReader(new FileStream(cacheFilePath, FileMode.Open)))
            {
                int version = reader.ReadInt();
                if (version != AbilityIdGenerateCacheMenuExtension.version)
                {
                    FrostyMessageBox.Show($"Cache file version mismatch. Please generate the cache again. (file: {version}, expected: {AbilityIdGenerateCacheMenuExtension.version})");
                    return false;
                }
                int count = reader.ReadInt();
                for (int i = 0; i < count; i++)
                {
                    uint id = reader.ReadUInt();
                    string name = reader.ReadNullTerminatedString();
                    idToNameMap[id] = name;
                }
            }
            return true;
        }
    }
}
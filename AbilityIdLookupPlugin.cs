using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using AbilityIdLookupPlugin.Windows;
using Frosty.Controls;
using Frosty.Core;
using Frosty.Core.Controls;
using Frosty.Core.Windows;
using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;

namespace AbilityIdLookupPlugin
{
    public class AbilityIdGenerateCacheMenuExtension : MenuExtension
    {
        public override string TopLevelMenuName => "Tools";
        public override string SubLevelMenuName => "Generate Cache";
        public override string MenuItemName => "Ability Id Cache";
        public override ImageSource Icon => new ImageSourceConverter().ConvertFromString("pack://application:,,,/FrostyEditor;component/Images/Database.png") as ImageSource;

        public static string CacheDirectory = System.AppDomain.CurrentDomain.BaseDirectory + @"Plugins\Caches\";
        public static string CacheFilePath = CacheDirectory + Enum.GetName(typeof(ProfileVersion), ProfilesLibrary.DataVersion) + "_AbilityIdPlugin_Cache.cache";
        public static int version = 0x00000001;

        public override RelayCommand MenuItemClicked => new RelayCommand((o) =>
        {
            FrostyTaskWindow.Show("Creating Cache", "Enumerating EBX Files...", (task) =>
            {
                CreateCache(task);
            });
        });

        private void CreateCache(FrostyTaskWindow task)
        {
            AssetManager AM = App.AssetManager;
            List<EbxAssetEntry> ebxFiles = AM.EnumerateEbx(type: "PlayerAbilityAsset").ToList();
            int index = 0;
            int count = ebxFiles.Count;
            List<UInt32> ids = new List<UInt32>();
            List<string> paths = new List<string>();
            ebxFiles.ForEach((abilityEntry) =>
            {
                if (abilityEntry.IsAdded) {
                    App.Logger.Log("Skipping " + abilityEntry.Name + " as it is an added asset.");
                    return;
                }

                EbxAsset abilityAsset = AM.GetEbx(abilityEntry);

                task.Update($"Processing ({index + 1}/{count}): {abilityEntry.Filename}", index / count * 50);
                dynamic root = abilityAsset.RootObject;
                ids.Add(root.Identifier);
                paths.Add(abilityEntry.Name);
                index++;
            });
            using (NativeWriter writer = new NativeWriter(new FileStream(CacheFilePath, FileMode.Create)))
            {
                writer.Write(version);
                writer.Write(ids.Count);
                for (int i = 0; i < ids.Count; i++)
                {
                    writer.Write(ids[i]);
                    writer.WriteNullTerminatedString(paths[i]);
                }
            }
        }
    }

    public class ViewAbilityIdLookupWindow : MenuExtension
    {
        public override ImageSource Icon => new ImageSourceConverter().ConvertFromString("pack://application:,,,/AbilityIdLookupPlugin;component/Images/Hexagon.png") as ImageSource;

        public override string TopLevelMenuName => "View";
        public override string MenuItemName => "Ability Id Lookup";

        public override RelayCommand MenuItemClicked => new RelayCommand(o =>
        {
            AbilityIdLookupWindow window = new AbilityIdLookupWindow();
            if (!window.cacheReadFailed)
                window.Show();
        });
    }
}
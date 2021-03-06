﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Catel.Collections;
using PresetMagician.Core.Models;
using PresetMagician.Utils;
using Type = PresetMagician.Core.Models.Type;

namespace PresetMagician.VendorPresetParser.u_he
{
    // ReSharper disable once InconsistentNaming
    public abstract class u_he : AbstractVendorPresetParser
    {
        private readonly Regex parsingRegex = new Regex(@"^(?<type>.*):(\r\n|\r|\n)'(?<value>.*)'",
            RegexOptions.Multiline | RegexOptions.Compiled);

        public override void Init()
        {
            var factoryPath = GetPresetDirectory(GetDataDirectoryName(), GetProductName(), false);
            var userPath = GetPresetDirectory(GetDataDirectoryName(), GetProductName(), false);
            BankLoadingNotes =
                $"Factory Presets are loaded from {factoryPath}, User Presets are loaded from {userPath}. " +
                "Sub-folders define the bank.";
            base.Init();
        }

        public override int GetNumPresets()
        {
            var count = H2PScanBanks(GetDataDirectoryName(), GetProductName(), false, false).GetAwaiter().GetResult();
            count += H2PScanBanks(GetDataDirectoryName(), GetProductName(), true, false).GetAwaiter().GetResult();

            return base.GetNumPresets() + count;
        }

        public override async Task DoScan()
        {
            await H2PScanBanks(GetDataDirectoryName(), GetProductName(), false, true);
            await H2PScanBanks(GetDataDirectoryName(), GetProductName(), true, true);
            await base.DoScan();
        }

        protected abstract string GetProductName();

        protected virtual string GetDataDirectoryName()
        {
            return GetProductName() + ".data";
        }

        protected async Task<int> H2PScanBanks(string dataDirectoryName, string productName, bool userPresets,
            bool persist)
        {
            Logger.Debug(
                $"Begin H2PScanBanks with dataDirectoryName {dataDirectoryName} product name {productName} and userPresets {userPresets}");

            var rootDirectory = GetPresetDirectory(dataDirectoryName, productName, userPresets);
            Logger.Debug($"Parsing PresetDirectory {rootDirectory}");

            var directoryInfo = new DirectoryInfo(rootDirectory);

            if (!directoryInfo.Exists)
            {
                Logger.Debug($"Directory {rootDirectory} does not exist");
                return 0;
            }

            var bankName = BankNameFactory;
            if (userPresets)
            {
                bankName = BankNameUser;
            }

            var bank = RootBank.CreateRecursive(bankName);
            var count = await H2PScanBank(bank, directoryInfo, persist);
            Logger.Debug("End H2PScanBanks");

            return count;
        }

        private async Task<int> H2PScanBank(PresetBank bank, DirectoryInfo directory, bool persist)
        {
            var count = 0;

            foreach (var file in directory.EnumerateFiles("*.h2p"))
            {
                count++;
                if (!persist)
                {
                    continue;
                }

                await ProcessPreset(file, bank);
            }

            foreach (var subDirectory in directory.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
            {
                var subBank = bank.CreateRecursive(subDirectory.Name);
                count += await H2PScanBank(subBank, subDirectory, persist);
            }

            return count;
        }

        private async Task ProcessPreset(FileSystemInfo file, PresetBank bank)
        {
            var presetData = File.ReadAllBytes(file.FullName);
            var sourceFile = file.FullName;


            var preset = new PresetParserMetadata
            {
                PresetName = file.Name.Replace(".h2p", ""), Plugin = PluginInstance.Plugin, BankPath = bank.BankPath,
                SourceFile = sourceFile
            };

            var metadata = ExtractMetadata(Encoding.UTF8.GetString(presetData));

            if (metadata.ContainsKey("Author"))
            {
                preset.Author = metadata["Author"];
            }

            var comments = new List<string>();

            if (metadata.ContainsKey("Description") && metadata["Description"].Length > 0)
            {
                comments.Add(metadata["Description"]);
            }

            if (metadata.ContainsKey("Usage") && metadata["Usage"].Length > 0)
            {
                comments.Add(metadata["Usage"]);
            }

            preset.Comment = string.Join(Environment.NewLine, comments);

            if (metadata.ContainsKey("Categories") && metadata["Categories"].Length > 0)
            {
                preset.Types.AddRange(ExtractTypes(metadata["Categories"]));
            }

            if (metadata.ContainsKey("Features") && metadata["Features"].Length > 0)
            {
                preset.Characteristics.AddRange(ExtractModes(metadata["Features"]));
            }

            if (metadata.ContainsKey("Character") && metadata["Character"].Length > 0)
            {
                preset.Characteristics.AddRange(ExtractModes(metadata["Character"]));
            }

            await DataPersistence.PersistPreset(preset, presetData);
        }

        private IEnumerable<Type> ExtractTypes(string typesString)
        {
            var types = new List<Type>();
            var splitTypes = typesString.Split(',');

            foreach (var splitType in splitTypes)
            {
                var splitTypes2 = splitType.Trim().Split(':');
                if (splitTypes2.Length == 1)
                {
                    types.Add(new Type {TypeName = splitTypes2[0]});
                }
                else if (splitTypes.Length > 1)
                {
                    types.Add(new Type {TypeName = splitTypes2[0], SubTypeName = splitTypes2[1]});
                }
            }

            return types;
        }


        private List<Characteristic> ExtractModes(string modesString)
        {
            var modes = new List<Characteristic>();
            var splitModes = modesString.Split(',');

            foreach (var splitMode in splitModes)
            {
                modes.Add(new Characteristic {CharacteristicName = splitMode.Trim()});
            }

            return modes;
        }

        private Dictionary<string, string> ExtractMetadata(string presetData)
        {
            var result = new Dictionary<string, string>();

            var closingTagLocation = presetData.IndexOf("*/", StringComparison.InvariantCulture);

            if (closingTagLocation == -1)
            {
                // no metadata found, return
                return result;
            }

            var trimmedPresetData = presetData.Substring(0, closingTagLocation);

            foreach (Match match in parsingRegex.Matches(trimmedPresetData))
            {
                var type = match.Groups["type"].Value;
                var value = match.Groups["value"].Value;
                try
                {
                    result.Add(type, value);
                }
                catch (ArgumentException)
                {
                    Logger.Debug(
                        $"Unable to add metadata for type {type} with value {value} because {type} already exists.");
                }
            }

            return result;
        }

        private string GetDataDirectory(string dataDirectoryName)
        {
            var vstPluginsPath = Path.GetDirectoryName(PluginInstance.Plugin.DllPath);

            return Path.Combine(vstPluginsPath, dataDirectoryName);
        }

        private string GetPresetDirectory(string dataDirectoryName, string productName, bool userPresets)
        {
            dataDirectoryName = GetDataDirectory(dataDirectoryName);

            var shortCutDataDirectoryName = dataDirectoryName + ".lnk";

            string dataDirectory;

            var isShortcut = false;

            try
            {
                isShortcut = ShortcutUtils.IsShortcut(shortCutDataDirectoryName);
            }
            catch (IOException e)
            {
                Logger.Error(
                    $"Error while trying to resolve the shortcut {shortCutDataDirectoryName}: {e.GetType().FullName}: {e.Message}");
                Logger.Debug(e.StackTrace);
            }

            if (isShortcut)
            {
                dataDirectory = ShortcutUtils.ResolveShortcut(shortCutDataDirectoryName);
            }
            else
            {
                dataDirectory = dataDirectoryName;
            }

            if (dataDirectory != null)
            {
                return Path.Combine(dataDirectory, userPresets ? "UserPresets" : "Presets", productName);
            }

            Logger.Error($"Unable to find the data directory because {dataDirectoryName} is not a directory and no "+
                         $"shortcut {shortCutDataDirectoryName} exists.");
            return null;
        }
    }
}
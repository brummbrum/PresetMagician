﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using CannedBytes.Midi.Message;
using Catel.Data;
using Drachenkatze.PresetMagician.Utils;

namespace SharedModels
{
    public class Preset 
    {
        
        [Key] public string PresetId { get; set; } = Guid.NewGuid().ToString();

        [Index("Foo", IsUnique = true)]
        [ForeignKey("Plugin")]
        public int PluginId { get; set; }

        public Plugin Plugin
        {
            get { return _plugin; }
            set
            {
                _plugin = value;

                if (!string.IsNullOrEmpty(_bankPath))
                {
                    PresetBank = _plugin.RootBank.First().CreateRecursive(_bankPath);
                    _bankPath = null;
                }

            }

        }

        private Plugin _plugin;
        
        public bool IsDeleted { get; set; }
        
        public DateTime? LastExported { get; set; }

        
        public void SetPlugin (Plugin vst)
        {
            Plugin = vst;
        }

        public Preset()
        {
            PreviewNote = new MidiNoteName("C5");
        }

        [NotMapped]
        public PresetBank PresetBank { get; set; }

        private string _bankPath;
        
        public string BankPath
        {
            get
            {
                if (PresetBank != null)
                {
                    return PresetBank.BankPath;
                }
                  
                return "";
            }
            set
            {
                if (Plugin != null)
                {
                    PresetBank = Plugin.RootBank.First().CreateRecursive(value);
                }
                else
                {
                    _bankPath = value; 
                }
            }
        }

        [NotMapped]
        public byte[] PresetData
        {
            get { throw new NotImplementedException();}
            set { throw new NotImplementedException();}
        }

        public int PresetSize { get; set; }
        public int PresetCompressedSize { get; set; }

        public string PresetName { get; set; }

        [NotMapped]
        public MidiNoteName PreviewNote { get; set; }

        public int PreviewNoteNumber
        {
            get { return PreviewNote.NoteNumber; }
            set { PreviewNote.NoteNumber = value; }
        }

        public string Author { get;set; }
        public string Comment { get;set; }

        [Index("Foo", IsUnique = true)]
        public string SourceFile { get; set; }
        
        public ObservableCollection<ObservableCollection<string>> Types { get; set; } = new ObservableCollection<ObservableCollection<string>>();

        public ObservableCollection<string> Modes { get; set; } = new ObservableCollection<string>();
        
        public string PresetHash { get; set; }
        
     
    }
}
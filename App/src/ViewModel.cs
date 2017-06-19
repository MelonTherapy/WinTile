﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using App.Model;
using App.Model.Managers;
using App.Model.Managers.Strategies;
using App.Properties;
using App.Utils;
using PropertyChanged;
using Rect = App.Model.Rect;

namespace App
{
    [ImplementPropertyChanged]
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, args) => {};

        private readonly LayoutManager layoutManager = new LayoutManager();
        private bool _activeInEditor;
        private HotkeyManager hotkeyManager;

        private EditorWindowManager editorManager;
        private CompositeWindowManager windowManager;
        private HotkeyPair _selectedHotkeyPair;
        private Tile _selected;

        public ObservableCollection<Tile> Tiles
        {
            get => layoutManager.Layout.tiles;
            set => layoutManager.Layout.tiles = value;
        }

        public ObservableCollection<HotkeyPair> Hotkeys
        {
            get => layoutManager.Layout.hotkeys;
            set => layoutManager.Layout.hotkeys = value;
        }

        public IEnumerable<HotkeyType> HotkeyTypes
        {
            get
            {
                var used = Hotkeys.Select(h => h.Type);
                var types = Enum.GetValues(typeof(HotkeyType)).Cast<HotkeyType>();
                return types.Except(used);
            }
        }

        public string JsonLayout
        {
            get => layoutManager.Json;
            set
            {
                layoutManager.Json = value;

                var nativeWindowManager = new ConvertWindowManager(new User32Manager());
                editorManager = new EditorWindowManager(layoutManager.Layout.tiles);
                windowManager = new CompositeWindowManager(nativeWindowManager, editorManager);

                var move = new MoveStrategy(layoutManager.Layout.tiles, windowManager);
                var select = new SelectStrategy(layoutManager.Layout.tiles, windowManager);
                var extend = new ExtendStrategy(layoutManager.Layout.tiles, windowManager);
                var layout = new LayoutStrategy(layoutManager.Layout.tiles, windowManager);

                hotkeyManager = new HotkeyManager(layoutManager.Layout.hotkeys, new Dictionary<HotkeyType, Action<object>>
                {
                    {HotkeyType.MoveLeft, h1 => move.Left()},
                    {HotkeyType.MoveRight, h1 => move.Right()},
                    {HotkeyType.MoveUp, h1 => move.Up()},
                    {HotkeyType.MoveDown, h1 => move.Down()},

                    {HotkeyType.ExpandLeft, h1 => extend.Left()},
                    {HotkeyType.ExpandRight, h1 => extend.Right()},
                    {HotkeyType.ExpandUp, h1 => extend.Up()},
                    {HotkeyType.ExpandDown, h1 => extend.Down()},

                    {HotkeyType.LayoutLeft, h1 => layout.Left()},
                    {HotkeyType.LayoutRight, h1 => layout.Right()},
                    {HotkeyType.LayoutUp, h1 => layout.Up()},
                    {HotkeyType.LayoutDown, h1 => layout.Down()},

                    {HotkeyType.SelectLeft, h1 => select.Left()},
                    {HotkeyType.SelectRight, h1 => select.Right()},
                    {HotkeyType.SelectUp, h1 => select.Up()},
                    {HotkeyType.SelectDown, h1 => select.Down()}
                });
            }
        }

        public Tile Selected
        {
            get { return _selected; }
            set
            {
                _selected = value; 
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selected)+".Rect"));
            }
        }

        public int SelectedIndex { get; set; }

        public HotkeyPair SelectedHotkeyPair { private get; set; }
        public HotkeyType AddHotkeyType { get; set; }
        public Hotkey AddHotkeyHotkey{ get; set; }

        public ObservableCollection<Tile> Windows => editorManager.Windows;

        public bool ActiveInEditor
        {
            get => _activeInEditor;
            set
            {
                _activeInEditor = value;

                if (ActiveInEditor)
                    hotkeyManager.BindHotkeys();
                else
                    hotkeyManager.UnbindHotkeys();
            }
        }

        public void Load()
        {
            JsonLayout = Settings.Default.Layout ?? "{}";
        }

        public void BindHotkeys()
        {
            windowManager.Active = true;
            hotkeyManager.BindHotkeys();
        }

        public void UnbindHotkeys()
        {
            windowManager.Active = false;
            hotkeyManager.UnbindHotkeys();
        }

        public void AddTile()
        {
            var r = Selected?.Rect ?? new Rect();
            var rect = new Rect(r.Left, r.Top, r.Right, r.Bottom);
            Tiles.Add(new Tile(rect));
        }

        public void RemoveTile(Tile window)
        {
            Tiles.Remove(window);
        }
        public void AddWindow()
        {
            throw new NotImplementedException();
        }

        public void RemoveWindow(Tile window)
        {
            throw new NotImplementedException();
        }

        internal void Save()
        {
            Settings.Default.Layout = layoutManager.Json;
            Settings.Default.Save();
        }

        public void MoveSelectedUp()
        {
            var index = Tiles.IndexOf(Selected);
            if (index > 0)
                Tiles.Move(index, index - 1);
        }

        public void MoveSelectedDown()
        {
            var index = Tiles.IndexOf(Selected);
            if (index < Tiles.Count - 1)
                Tiles.Move(index, index + 1);
        }

        public void AddHotkey()
        {
            var htKey = new Hotkey(AddHotkeyHotkey.Key, AddHotkeyHotkey.Modifiers);
            var pair = new HotkeyPair(AddHotkeyType, htKey);
            Hotkeys.Add(pair);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HotkeyTypes)));
        }

        public void RemoveHotkey()
        {
            Hotkeys.Remove(SelectedHotkeyPair);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HotkeyTypes)));
        }
    }
}
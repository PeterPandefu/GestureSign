﻿using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using WindowsInput;
using GestureSign.Common.Localization;
using GestureSign.Common.Plugins;

namespace GestureSign.CorePlugins.MouseActions
{
    public class MouseActionsPlugin : IPlugin
    {
        #region Private Variables

        private MouseActionsUI _gui = null;
        private MouseActionsSettings _settings = null;

        #endregion

        #region Public Properties

        public string Name
        {
            get { return LocalizationProvider.Instance.GetTextValue("CorePlugins.MouseActions.Name"); }
        }

        public string Description
        {
            get { return GetDescription(); }
        }

        public object GUI
        {
            get { return _gui ?? (_gui = CreateGUI()); }
        }

        public bool ActivateWindowDefault
        {
            get { return false; }
        }

        public MouseActionsUI TypedGUI
        {
            get { return (MouseActionsUI)GUI; }
        }

        public string Category
        {
            get { return LocalizationProvider.Instance.GetTextValue("CorePlugins.MouseActions.Category"); }
        }

        public bool IsAction
        {
            get { return true; }
        }

        public object Icon => IconSource.Mouse;

        #endregion

        #region Public Methods

        public void Initialize()
        {

        }

        public bool Gestured(PointInfo actionPoint)
        {
            if (_settings == null)
                return false;

            InputSimulator simulator = new InputSimulator();
            try
            {
                var referencePoint = GetReferencePoint(_settings.ClickPosition, actionPoint);
                int buttonId = 1;
                if (_settings.MouseAction.ToString().Contains('2'))
                    buttonId = 2;
                switch (_settings.MouseAction)
                {
                    case MouseActions.HorizontalScroll:
                        simulator.Mouse.HorizontalScroll(_settings.ScrollAmount).Sleep(30);
                        return true;
                    case MouseActions.VerticalScroll:
                        simulator.Mouse.VerticalScroll(_settings.ScrollAmount).Sleep(30);
                        return true;
                    case MouseActions.MoveMouseTo:
                        Cursor.Position = _settings.MovePoint;
                        return true;
                    case MouseActions.MoveMouseBy:
                        referencePoint.Offset(_settings.MovePoint);
                        Cursor.Position = referencePoint;
                        break;
                    case MouseActions.XButton1Click:
                    case MouseActions.XButton2Click:
                        if (_settings.ClickPosition != ClickPositions.Original)
                            Cursor.Position = referencePoint;
                        simulator.Mouse.XButtonClick(buttonId).Sleep(30);
                        break;
                    case MouseActions.XButton1DoubleClick:
                    case MouseActions.XButton2DoubleClick:
                        if (_settings.ClickPosition != ClickPositions.Original)
                            Cursor.Position = referencePoint;
                        simulator.Mouse.XButtonDoubleClick(buttonId).Sleep(30);
                        break;
                    case MouseActions.XButton1Down:
                    case MouseActions.XButton2Down:
                        if (_settings.ClickPosition != ClickPositions.Original)
                            Cursor.Position = referencePoint;
                        simulator.Mouse.XButtonDown(buttonId).Sleep(30);
                        break;
                    case MouseActions.XButton1Up:
                    case MouseActions.XButton2Up:
                        if (_settings.ClickPosition != ClickPositions.Original)
                            Cursor.Position = referencePoint;
                        simulator.Mouse.XButtonUp(buttonId).Sleep(30);
                        break;
                    default:
                        {
                            if (_settings.ClickPosition != ClickPositions.Original)
                                Cursor.Position = referencePoint;

                            MethodInfo clickMethod = typeof(IMouseSimulator).GetMethod(_settings.MouseAction.ToString());
                            clickMethod.Invoke(simulator.Mouse, null);
                            Thread.Sleep(30);
                            break;
                        }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool Deserialize(string SerializedData)
        {
            return PluginHelper.DeserializeSettings(SerializedData, out _settings);
        }

        public string Serialize()
        {
            if (_gui != null)
                _settings = _gui.Settings;

            if (_settings == null)
                _settings = new MouseActionsSettings();

            return PluginHelper.SerializeSettings(_settings);
        }

        #endregion

        #region Private Methods

        private Point GetReferencePoint(ClickPositions position, PointInfo actionPoint)
        {
            Point referencePoint;
            switch (position)
            {
                case ClickPositions.LastUp:
                    referencePoint = actionPoint.Points.Last().Last();
                    break;
                case ClickPositions.LastDown:
                    referencePoint = actionPoint.Points.Last().First();
                    break;
                case ClickPositions.FirstUp:
                    referencePoint = actionPoint.Points.First().Last();
                    break;
                case ClickPositions.FirstDown:
                    referencePoint = actionPoint.Points.First().First();
                    break;
                default:
                    referencePoint = Cursor.Position;
                    break;
            }
            return referencePoint;

        }

        private MouseActionsUI CreateGUI()
        {
            MouseActionsUI newGUI = new MouseActionsUI();

            newGUI.Loaded += (o, e) =>
            {
                TypedGUI.Settings = _settings;
            };

            return newGUI;
        }

        private string GetDescription()
        {
            switch (_settings.MouseAction)
            {
                case MouseActions.HorizontalScroll:
                    return
                        String.Format(
                            LocalizationProvider.Instance.GetTextValue("CorePlugins.MouseActions.Description.HorizontalScroll"),
                            (_settings.ScrollAmount >= 0
                                ? LocalizationProvider.Instance.GetTextValue("CorePlugins.MouseActions.Description.Right")
                                : LocalizationProvider.Instance.GetTextValue("CorePlugins.MouseActions.Description.Left")),
                            Math.Abs(_settings.ScrollAmount));
                case MouseActions.VerticalScroll:
                    return
                        String.Format(
                            LocalizationProvider.Instance.GetTextValue("CorePlugins.MouseActions.Description.VerticalScroll"),
                            (_settings.ScrollAmount >= 0
                                ? LocalizationProvider.Instance.GetTextValue("CorePlugins.MouseActions.Description.Up")
                                : LocalizationProvider.Instance.GetTextValue("CorePlugins.MouseActions.Description.Down")),
                            Math.Abs(_settings.ScrollAmount));
                case MouseActions.MoveMouseBy:
                    return LocalizationProvider.Instance.GetTextValue("CorePlugins.MouseActions.Description.MoveMouseBy") + _settings.MovePoint;
                case MouseActions.MoveMouseTo:
                    return LocalizationProvider.Instance.GetTextValue("CorePlugins.MouseActions.Description.MoveMouseTo") + _settings.MovePoint;
            }

            var description = _settings.MouseAction.ToString();
            var button = MouseActionDescription.ButtonDescription[description.Split(MouseActionDescription.DescriptionDict.Keys.ToArray(), StringSplitOptions.RemoveEmptyEntries)[0]];
            var action = MouseActionDescription.DescriptionDict[description.Split(MouseActionDescription.ButtonDescription.Keys.ToArray(), StringSplitOptions.RemoveEmptyEntries)[0]];
            return string.Format("{0} {1} {2}", ClickPositionDescription.DescriptionDict[_settings.ClickPosition], action, button);
        }

        #endregion

        #region Host Control

        public IHostControl HostControl { get; set; }

        #endregion
    }
}

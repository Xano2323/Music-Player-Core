using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using System.Windows;
using System.Windows.Input;


namespace Music_Player.Utility
{
    public static class Logger
    {
        public enum MessageType
        {
            Information = 0x01,
            Success = 0x02,
            Warning = 0x04,
            Error = 0x08
        };
        private static readonly IDictionary<MessageType, SolidColorBrush> COLORS = new Dictionary<MessageType, SolidColorBrush>()
        {
            [MessageType.Information] = new SolidColorBrush() { Color = Color.FromArgb(255, 255, 255, 255) },
            [MessageType.Success] = new SolidColorBrush() { Color = Color.FromArgb(255, 143, 255, 101) },
            [MessageType.Warning] = new SolidColorBrush() { Color = Color.FromArgb(255, 249, 169, 28) },
            [MessageType.Error] = new SolidColorBrush() { Color = Color.FromArgb(255, 255, 100, 100) }
        };
        //public static readonly string LOGGER_TXT_PATH = @".\Logger.txt";
        public static TextBlock statusLogger_TextBlock;
        private static MessageType lastMessageColor = MessageType.Information;


        public static void InitializeLogger(Border loggerBorder)
        {
            statusLogger_TextBlock = loggerBorder.Child as TextBlock;

            if (!(statusLogger_TextBlock is TextBlock))
                throw new Exception($"Kontrolka status loggera musi być: TextBlock\nJest: {loggerBorder.Child.GetType()}");

            if (File.Exists(MainWindow.LoggerPath) && File.ReadAllBytes(MainWindow.LoggerPath).Length > 0)
                ShowStatus("");

            ShowStatus("Program initialized");

            loggerBorder.MouseDown += Status_TextBlock_MouseDown;
        }

        private static void Status_TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2)
            {
                string directory;

                try
                {
                    directory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                    if (directory.Contains(" (x86)"))
                        directory = directory.Replace(" (x86)", "");
                    System.Diagnostics.Process.Start(directory + @"\Notepad++\notepad++.exe", $"\"{MainWindow.LoggerPath}\"");
                }
                catch
                {
                    try
                    {
                        directory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                        System.Diagnostics.Process.Start(directory + @"\Notepad++\notepad++.exe", $"\"{MainWindow.LoggerPath}\"");
                    }
                    catch
                    {
                        ShowStatus($"Couldn't open logger status :)\n", Logger.MessageType.Error, saveToFile: false);
                    }
                }
            }
        }

        public static void ShowStatusWithMessageBox(object message, MessageType messageType = MessageType.Information, bool saveToFile = true, string errorTitle = "")
        {
            ShowStatus(message, messageType, saveToFile);
            MessageBox.Show(message.ToString(), errorTitle);
        }

        public static void ShowStatus(object message, MessageType messageType = MessageType.Information, bool saveToFile = true)
        {
            try
            {
                if (lastMessageColor != messageType)
                {
                    lastMessageColor = messageType;
                    statusLogger_TextBlock.Foreground = COLORS[messageType];
                }

                message = message.ToString().Replace("\n", " ");

                if (message.ToString() != string.Empty)
                    statusLogger_TextBlock.Text = message.ToString();

                if (saveToFile)
                    SaveToLoggerFile(message, messageType);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public static void SaveToLoggerFile(object message, MessageType messageType = MessageType.Information)
        {
            try
            {
                if (message.ToString() == string.Empty)
                {
                    File.AppendAllText(MainWindow.LoggerPath, Environment.NewLine);
                    return;
                }

                DateTime dateTime = DateTime.Now;
                string dateString = DateAndTime.GetNowDateAndTime();

                string text = $"{dateString} - {message}{Environment.NewLine}";

                File.AppendAllText(MainWindow.LoggerPath, text);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
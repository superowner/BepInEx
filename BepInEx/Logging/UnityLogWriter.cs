﻿using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.ConsoleUtil;
using UnityEngine;

namespace BepInEx.Logging
{
	/// <summary>
	/// Logs entries using Unity specific outputs.
	/// </summary>
    public class UnityLogWriter : BaseLogger
    {
		/// <summary>
		/// Writes a string specifically to the game output log.
		/// </summary>
		/// <param name="value">The value to write.</param>
        public void WriteToLog(string value)
        {
            UnityEngine.UnityLogWriter.WriteStringToUnityLog(value);
        }

        protected void InternalWrite(string value)
        {
            Console.Write(value);
            WriteToLog(value);
        }

	    /// <summary>
	    /// Logs an entry to the Logger instance.
	    /// </summary>
	    /// <param name="level">The level of the entry.</param>
	    /// <param name="entry">The textual value of the entry.</param>
        public override void Log(LogLevel level, object entry)
        {
            Kon.ForegroundColor = level.GetConsoleColor();
            base.Log(level, entry);
            Kon.ForegroundColor = ConsoleColor.Gray;
        }

        public override void WriteLine(string value) => InternalWrite($"{value}\r\n");
        public override void Write(char value) => InternalWrite(value.ToString());
        public override void Write(string value) => InternalWrite(value);

        /// <summary>
        /// Start listening to Unity's log message events and sending the messages to BepInEx logger.
        /// </summary>
        public static void ListenUnityLogs()
        {
            Type application = typeof(Application);
			
            EventInfo logEvent = application.GetEvent("logMessageReceived", BindingFlags.Public | BindingFlags.Static);
            if (logEvent != null)
            {
                logEvent.AddEventHandler(null, new Application.LogCallback(OnUnityLogMessageReceived));
            }
            else
            {
                MethodInfo registerLogCallback = application.GetMethod("RegisterLogCallback", BindingFlags.Public | BindingFlags.Static);
                registerLogCallback.Invoke(null, new object[] { new Application.LogCallback(OnUnityLogMessageReceived) });
            }
        }

        private static void OnUnityLogMessageReceived(string message, string stackTrace, LogType type)
        {
            LogLevel logLevel = LogLevel.Message;

            switch (type)
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    logLevel = LogLevel.Error;
                    break;
                case LogType.Warning:
                    logLevel = LogLevel.Warning;
                    break;
                case LogType.Log:
                default:
                    logLevel = LogLevel.Info;
                    break;
            }

            Logger.Log(logLevel, message);
            if (type == LogType.Exception)
            {
                Logger.Log(logLevel, $"Stack trace:\n{stackTrace}");
            }
        }
    }
}

namespace UnityEngine
{
    internal sealed class UnityLogWriter
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteStringToUnityLog(string s);
    }
}

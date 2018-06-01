﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
#if UNITY
using UnityEngine.Assertions.Must;
using Debug = UnityEngine.Debug;
#endif

namespace Flow.Impl
{
    /// <summary>
    /// Log system used by Models.
    /// </summary>
    public class Logger : ILogger
    {
        #region Public Fields
        public string LogPrefix { get; set; }
        public object LogSubject { get; set; }
        public int Verbosity { get; set; }
        public bool ShowSource { get; set; } = true;
        public bool ShowStack { get; set; } = true;

        public static string LogFileName;
        public static ELogLevel MaxLevel;
        #endregion

        #region Public Methods
        public Logger()
        {
            LogSubject = this;
        }

        public Logger(string pre, bool src, bool st)
            : this(pre)
        {
            ShowSource = src;
            ShowStack = st;
        }

        public Logger(string pre)
            : this()
        {
            LogPrefix = pre;
        }

        public static void Initialise()
        {
        }

        #endregion

        #region Protected Methods
        public void Info(string fmt, params object[] args)
        {
            Log(ELogLevel.Info, string.Format(fmt, args));
        }
        public void Warn(string fmt, params object[] args)
        {
            Log(ELogLevel.Warn, string.Format(fmt, args));
        }
        public void Error(string fmt, params object[] args)
        {
            Log(ELogLevel.Error, string.Format(fmt, args));
        }
        public void Verbose(int level, string fmt, params object[] args)
        {
            if (level > Verbosity)
                return;
            Log(ELogLevel.Verbose, string.Format(fmt, args));
        }
        #endregion

        #region Private

        void OutputLine(string text)
        {
            Console.WriteLine(text);
            Trace.WriteLine(text);
        }
        void Output(string text)
        {
            Console.Write(text);
            Trace.Write(text);
        }

        readonly string[] _logNames = { "Info", "Warn", "Error", "Verbose" };

        private void Log(ELogLevel level, string text)
        {
#if UNITY
            Action<string> log = Debug.Log;
#else
            Action<string> log = Console.WriteLine;
#endif
            if (level == ELogLevel.None)
                level = ELogLevel.Error;
#if TRACE
            Output(MakeEntry(level, text));
            var error = level == ELogLevel.Error;
            var showFrames = ShowStack || error;


            // HACK!
            showFrames = error;

            if (ShowSource || error)
            {
                OutputLine("");
                var lead = "\t\t";
                var st = new StackTrace(true);
                var foundTop = false;
                foreach (var fr in st.GetFrames())
                {
                    if (!foundTop)
                    {
                        var name = fr.GetMethod().Name;
                        if (_logNames.Contains(name))
                        {
                            foundTop = true;
                            continue;
                        }
                    }
                    if (!foundTop)
                        continue;

                    //if (!fr.HasSource())
                    //    break;
                    if (string.IsNullOrEmpty(fr.GetFileName()))
                        break;

                    OutputLine(
                        $"{lead}{fr.GetFileName()}({fr.GetFileLineNumber()},{fr.GetFileColumnNumber()}): from: {fr.GetMethod().Name}");
                    if (!showFrames)
                        break;
                    lead += "\t";
                }
            }
            else
            {
                OutputLine("");
            }
#else // TODO: use bitmasks as intended
            switch (level)
            {
                case ELogLevel.Info:
                    log = Debug.Log;
                    break;
                case ELogLevel.Warn:
                    log = Debug.LogWarning;
                    break;
                case ELogLevel.Error:
                    log = Debug.LogError;
                    break;
                case ELogLevel.Verbose:
                    log = Debug.Log;
                    break;
            }
            log(MakeEntry(level, text));
#endif
        }

        private string MakeEntry(ELogLevel level, string text)
        {
            text = text.Trim();
            var named = LogSubject as INamed;
            var name = named == null ? "" : named.Name;
            var dt = DateTime.Now - _startTime;
            var ms = dt.ToString(@"fff");
            var time = dt.ToString(@"mm\:ss\:") + ms;
            var prefix = string.IsNullOrEmpty(LogPrefix) ? "" : $"{LogPrefix}: ";
            var from = string.IsNullOrEmpty(name) ? "" : $" {name}:";
            var gen = LogSubject as IGenerator;
            var step = gen == null ? "" : $"#{gen.StepNumber}/{gen.Kernel.StepNumber}: ";
            var openTick = "`";
            switch (level)
            {
                case ELogLevel.None:
                    break;
                case ELogLevel.Info:
                    break;
                case ELogLevel.Warn:
                    openTick = "``";
                    break;
                case ELogLevel.Verbose:
                    openTick = "````";
                    break;
                case ELogLevel.Error:
                    openTick = "```";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
            // this trace includes the source type: it's a bit "verbose"
            //return $"{level}: {prefix}{time} {step}{LogSubject.GetType()}{from}\n\t{openTick}{text}`";
            return $"{level}: {prefix}{time} {step}{from}\n\t{openTick}{text}`";
        }
#endregion

#region Protected Fields
        protected ELogLevel _logLevel;

#endregion

        private static DateTime _startTime = DateTime.Now;
    }
}

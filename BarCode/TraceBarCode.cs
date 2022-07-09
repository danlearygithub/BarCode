using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarCode
{
   public static class TraceBarCode
   {
      public const int TraceId = 1;

      private static TraceSource _TraceSource = new TraceSource("BarCode");

      public static bool LogInfoEnable()
      {
         return (SourceLevel >= SourceLevels.Information);
      }
      public static bool LogVerboseEnable()
      {
         return (SourceLevel >= SourceLevels.Verbose);
      }
      private static bool LogWarningEnable()
      {
         return (SourceLevel >= SourceLevels.Warning);
      }
      private static bool LogErrorEnable()
      {
         return (SourceLevel >= SourceLevels.Error);
      }

      public static SourceLevels SourceLevel
      {
         get { return _TraceSource.Switch.Level; }
      }

      [Conditional("TRACE_VERBOSE")]
      public static void LogVerbose(string source, string message, params object[] data)
      {
         if (LogVerboseEnable())
         {
            _TraceSource.TraceData(TraceEventType.Verbose, TraceId, source, (data.Length == 0) ? message : string.Format(message, data));
         }
      }

      [Conditional("TRACE_INFO")]
      public static void LogInfo(string source, string message, params object[] data)
      {
         if (LogInfoEnable())
         {
            _TraceSource.TraceData(TraceEventType.Information, TraceId, source, (data.Length == 0) ? message : string.Format(message, data));
         }
      }

      [Conditional("TRACE")]
      public static void LogWarning(string source, string message, params object[] data)
      {
         _TraceSource.TraceData(TraceEventType.Warning, TraceId, source, (data.Length == 0) ? message : string.Format(message, data));
      }

      [Conditional("TRACE")]
      public static void LogError(string source, string message, params object[] data)
      {
         _TraceSource.TraceData(TraceEventType.Error, TraceId, source, (data.Length == 0) ? message : string.Format(message, data));
      }
   }
}

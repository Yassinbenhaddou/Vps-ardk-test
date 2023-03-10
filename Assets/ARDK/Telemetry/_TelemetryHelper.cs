using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.Protobuf;

using UnityEngine;

namespace Niantic.ARDK.Telemetry
{
  public static class _TelemetryHelper
  {
    public enum WayspotAnchorAction
    {
      Create,
      Restore,
      StatusUpdate
    }
    
    private static IARSession _session;
    
    public static void Start()
    {
      ARSessionFactory.SessionInitialized += LogEventForInitialization;
    }

    private static void LogEventForInitialization(AnyARSessionInitializedArgs args)
    {
      _session = args.Session;
      _TelemetryService.RecordEvent(new ARSessionEvent()
      {
        SessionState = ARSessionEvent.Types.State.Created,
        BatteryLevel = SystemInfo.batteryLevel,
        InstallMode = GetInstallMode(),
      });

      _session.Paused += LogEventOnSessionPaused;
      _session.Ran += LogEventForSessionRan;
      
      _session.Deinitialized += LogEventOnSessionClose;
      _session.SessionFailed += LogEventOnFailedSession;
    }

    private static void LogEventForSessionRan(ARSessionRanArgs args)
    {
      _TelemetryService.RecordEvent(
        new ARSessionEvent()
        {
          SessionState = ARSessionEvent.Types.State.Run,
          BatteryLevel = SystemInfo.batteryLevel,
          InstallMode = GetInstallMode(),
        });
    }

    private static void LogEventOnSessionPaused(ARSessionPausedArgs args)
    {
      _TelemetryService.RecordEvent(
        new ARSessionEvent()
        {
          SessionState = ARSessionEvent.Types.State.Pause,
          BatteryLevel = SystemInfo.batteryLevel,
          InstallMode = GetInstallMode(),
        });
    }

    private static void LogEventOnFailedSession(ARSessionFailedArgs args)
    {
      _TelemetryService.RecordEvent(
        new ARSessionEvent()
        {
          SessionState = ARSessionEvent.Types.State.Disposed,
          BatteryLevel = SystemInfo.batteryLevel,
          InstallMode = GetInstallMode(),
        });

      UnlinkSessionCloseEvents();
    }
    
    private static void LogEventOnSessionClose(ARSessionDeinitializedArgs args)
    {
      _TelemetryService.RecordEvent(new ARSessionEvent()
      {
        SessionState = ARSessionEvent.Types.State.Disposed,
        BatteryLevel = SystemInfo.batteryLevel,
        InstallMode = GetInstallMode(),
      });

      UnlinkSessionCloseEvents();
    }
    
    private static void UnlinkSessionCloseEvents()
    {
      _session.Paused -= LogEventOnSessionPaused;
      
      _session.Ran -= LogEventForSessionRan;
      
      _session.Deinitialized -= LogEventOnSessionClose;
      _session.SessionFailed -= LogEventOnFailedSession;
    }
    
    private static string GetInstallMode()
    {
      return $"install_mode:{Application.installMode.ToString()}";
    }
  }
}

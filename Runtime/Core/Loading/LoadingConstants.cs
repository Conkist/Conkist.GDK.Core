namespace Conkist.GDK
{
    /// <summary>
    /// Core GDK Constants.
    /// </summary>
    public static class Const
    {
        public static class OverlayName
        {
            public const string LoadingScreen = "LoadingScreen";
            public const string PopupOverlay = "PopupOverlay";
        }
    }

    /// <summary>
    /// Enum representing different types of loads.
    /// </summary>
    public enum LoadType
    {
        Hidden, Quick, FullScreen
    }

    /// <summary>
    /// Enum representing different loading states as a state machine.
    /// </summary>
    public enum LoadingStates
    {
        LoadStarted,
        BeforeEntryFade, EntryFade, AfterEntryFade,
        UnloadOriginScene, LoadDestinationScene, LoadProgressComplete, InterpolatedLoadProgressComplete, DestinationSceneActivation,
        BeforeExitFade, ExitFade,
        UnloadSceneLoader, LoadTransitionComplete
    }

    /// <summary>
    /// Exclusive to operations beyond local application.
    /// </summary>
    public enum DownloadOperationStatus
    {
        NotStarted,
        InProgress,
        Succeeded,
        Failed
    }
}

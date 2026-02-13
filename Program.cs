using System;
using Cornifer;

ApplicationConfiguration.Initialize();

Platform.Start(args);
var app = new App();

#if DEBUG
app.Run();

#else
    try
    {
        app.Run();
    }
    catch (Exception ex)
    {
        // Platform.DetachWindow();
        await Platform.MessageBox(
            $"Uncaught exception!\n" +
            $"After clicking Ok you will be prompted to save map state.\n" +
            $"Don't overwrite your existing state as it may be corrupted.\n" +
            $"Send this error when asking for help\n" +
            $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", "Cornifer has crashed!");
        // await Main.SaveStateAs();
        Platform.Stop();
        Environment.Exit(1);
    }
#endif
Platform.Stop();
Environment.Exit(0);
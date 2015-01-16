KlackTracks
===========

A simple key-logging application.

When the executable is started, it creates or opens a file named keylog.txt in the system's appdata folder. It writes the key pressed and the time it was pressed on a line, and flushes the stream. It also adds itself to the system's registry so it will be started once the system is booted; if this behavior is desired, the executable should be started in the path that it will remain until usage of the program is no longer needed or desired.

That's about it, really.

This was written so I could keep track of the keystrokes on my own computer and make a neat chart out of the data several months down the road. Obviously a program such as this could be used to do Bad Things, and this is me saying it should not be used to *do* Bad Things.

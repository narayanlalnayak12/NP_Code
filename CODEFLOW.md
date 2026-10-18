# SelectPhotos desktop code flow

The standalone Windows UI starts in:

- [SelectPhotos.Desktop/Program.cs](SelectPhotos.Desktop/Program.cs)

It initializes WinForms and opens the main form:

- [SelectPhotos.Desktop/Form1.cs](SelectPhotos.Desktop/Form1.cs)

The form owns the desktop workflow:

1. `FolderBrowserDialog` selects source and destination folders.
2. The source folder is scanned recursively for supported image files.
3. Previous and Next navigate the photo list; Select Photo toggles the current photo.
4. Selecting safely copies the original into the destination and records the managed copy.
5. Deselecting removes a managed copy only if its contents still match the source.
6. Export Selected reconciles selected files with the destination.
7. Settings and selection state are persisted locally as JSON.

The desktop project contains the runtime UI and photo operations. Folder paths and selections exist only for the current app session and are not restored on the next launch.

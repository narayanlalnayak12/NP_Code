# SelectPhotos

A local Windows desktop app for reviewing photos, selecting favourites, and copying the chosen originals to a destination folder.

## Requirements

- Windows 11
- .NET 9 SDK or later

## Run

From PowerShell:

```powershell
.\run.ps1
```

From CMD:

```cmd
run.bat
```

Or start it directly:

```powershell
dotnet run --project .\SelectPhotos.Desktop\SelectPhotos.Desktop.csproj
```

## Debug in VS Code

Select **SelectPhotos Desktop** in Run and Debug, then press F5. The C# extension is required.

## Use

1. Choose the source photo folder.
2. Choose a destination folder.
3. Navigate with Previous and Next, and use Select Photo to toggle a photo.
4. Use Export Selected to reconcile the selected copies.

Each launch starts without a selected folder or photo selections. Source files are never modified. When deselecting, the app removes a destination file only when its contents still match the source. Files already copied to the destination remain there after the app closes; the app does not clear that folder on startup.

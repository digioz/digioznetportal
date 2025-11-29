# DigiOz Portal Bulk Media Import

This console application allows bulk importing of pictures and videos into the DigiOz Portal.

## Features

- Import multiple pictures or videos at once
- Automatically generates thumbnails for pictures
- Generates video thumbnails using FFmpeg (when available)
- Assigns ownership to admin users
- Sets imported media as visible and approved
- Uses filename as description (optional)

## Prerequisites

### Required
- .NET 9.0 Runtime
- SQL Server database with DigiOz Portal schema
- Valid connection string in `appsettings.json`

### Optional (for video thumbnail generation)
- FFmpeg executables

## FFmpeg Setup

Video thumbnail generation requires FFmpeg. Without it, the app will still import videos but will use black placeholder thumbnails.

### Option 1: Download FFmpeg Portable

1. Download FFmpeg from: https://ffmpeg.org/download.html
   - For Windows: https://github.com/BtbN/FFmpeg-Builds/releases
   - Download the "ffmpeg-master-latest-win64-gpl.zip" file

2. Extract the ZIP file

3. Copy the extracted folder to one of these locations:
   - `[Application Directory]\ffmpeg\` (recommended)
   - `C:\ffmpeg\`

4. The folder structure should look like:
   ```
   ffmpeg/
   ??? bin/
   ?   ??? ffmpeg.exe
   ?   ??? ffprobe.exe
   ?   ??? ffplay.exe
   ??? ...
   ```

### Option 2: Install FFmpeg System-Wide

1. Download FFmpeg as described above
2. Extract it to a permanent location (e.g., `C:\Program Files\ffmpeg`)
3. Add the `bin` folder to your system PATH:
   - Right-click "This PC" ? Properties
   - Click "Advanced system settings"
   - Click "Environment Variables"
   - Under "System variables", find and edit "Path"
   - Add new entry: `C:\Program Files\ffmpeg\bin`
   - Click OK on all dialogs
   - Restart the console application

## Usage

1. Update `appsettings.json` with your database connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=digiozPortal9;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
     }
   }
   ```

2. Run the application:
   ```
   dotnet run
   ```
   Or double-click the executable: `digioz.Portal.BulkMediaImport.exe`

3. Follow the prompts:
   - Select media type (1=Pictures, 2=Videos)
   - Choose album from the list
   - Enter source directory containing media files
   - Enter base directory for saving media
   - Choose description option
   - Select admin user as owner (optional)

## Supported File Types

### Pictures
- `.jpg`, `.jpeg`, `.png`, `.gif`, `.bmp`, `.tif`, `.tiff`

### Videos
- `.mp4`, `.webm`, `.ogg`, `.mov`, `.avi`, `.mkv`, `.mpg`, `.mpeg`, `.wmv`

## Output Directories

### Pictures
- Full size: `[base directory]/Pictures/Full/`
- Thumbnails: `[base directory]/Pictures/Thumb/` (150x150px cropped)

### Videos
- Full size: `[base directory]/Videos/Full/`
- Thumbnails: `[base directory]/Videos/Thumb/` (150x150px cropped)

## Troubleshooting

### "Cannot find FFmpeg in PATH" Error

This warning means FFmpeg is not installed or not found. Videos will still be imported with placeholder thumbnails. Follow the FFmpeg setup instructions above to enable video thumbnail generation.

### Database Connection Errors

- Verify your connection string in `appsettings.json`
- Ensure SQL Server is running
- Check that the database exists
- Verify network connectivity to the database server

### Permission Errors

- Ensure you have write permissions to the output directories
- Run the application as administrator if needed

## Notes

- All imported media is set to `Visible=true` and `Approved=true`
- Files are renamed with GUIDs to prevent conflicts
- Original filenames are preserved in the description (if selected)
- Existing files in output directories are not overwritten
- Import progress is displayed in real-time

## License

Copyright © DigiOz Portal

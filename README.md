# SubtitleFileCleanerWeb
.Net backend server template that provides rest api for transforming subtitle files to text files while removing time codes.
## Supported subtitles
- Ass
- Sbv
- Srt
- Sub
- Vtt
## Features
- Remove subtitle tags from file
- Transform file text into one line
## Development
If you use Docker for development, be sure to provide Kestrel certificates.
They can be specified in the appsettings.Development.json file in parentheses instead of the <certificate password for docker> placeholder.
## Deployment
Before deploying the template, do not forget to look at the appsettings.json file in which you can change the following configuration settings:
- Cross-origin resource sharing!
- Maximum size of incoming file!
- File storage path
- SQLite connection string
- Logging levels

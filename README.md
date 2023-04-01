# Music Player Core

.Net 5.0: <br />
https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-5.0.408-windows-x64-installer

Doesn't work on Linux yet I may rewrite code on Avalonia in the future to support it.

It's mainly used by me so usage of this program may not be obvious. <br />
Create new playlist with "+" sign in the playlist section. <br />
Load playlist (double click or right click -> Load) then drag and drop music files on the right section. <br />
Playback speed is bugged right now so you have to wait couple seconds before music is hearable or just restart the music. <br />
Don't unselect music if you selected it with ctrl + a it will crash the program. <br />
Delete music in context menu doesn't work yet you can only remove music from playlist by deleting it in json file. <br />

Hotkeys: <br />
ctrl + alt + arrow down or up - changes volume <br />
ctrl + n - switches randomization to on/off <br />
ctrl + m - switches randomization style <br />
ctrl + < - plays previous music <br />
ctrl + > - plays next music <br />
ctrl + / - pauses/resumes music <br />
alt + < - plays previous music with reversed randomization option (if music is looped it will also switch back to normal mode and change music instead of repeating it) <br />
alt + > - plays next music with reversed randomization option (same as above) <br />
ctrl + f - opens search panel (second input is for filtering music rate)

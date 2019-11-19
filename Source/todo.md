# TODO

- Add option to omit ESC]c codes to advance cursor (for animation)
- BIN: Specify width when loading
- DONE: BUG: Fix issue setting aspect for xbin's
- Add logos to pablodraw startup
- Save to HTML
- Full screen 'viewer' mode - no toolbar/etc, auto-scroll
- Search & Replace TEXT and/or COLOUR
- Crash when connecting to old servers
- corrupt xbins?? 9px
- alt+a in linux doesn't allow BG to be set
- Pablo Server connection manager / network neighborhood / directory

- 16c browser doesn't work on mono 2.10 (bigint.parse missing, used by json.net)
	- use eto.parse?  Need to implement deserializing to objects..

- Ability to pre-define maximum canvas size/resolution (ideal for 80x23/80x25)
	- Set height same as width
- Optional rulers on the left side and top sides (for wide BIN/XBIN), ala Photoshop
- Persist the 'Enable Backups' setting, and add configuration (limit # of backups?)
- Limit # of backups - stevemkk
- Loading XBIN doesn't allow changing the aspect setting (should default based on font height??)
- Hot link url's
- Font Editor for XBIN ( & 512 chars?)
	- Font editor for XBIN charset
	- Support for double charset space in XBIN (512 characters)
- Saving bin at 160 wide doesn't save anything past 305th line
- Automated/timed interval saving (server + normal)
	- Automated/timed interval saving (incremental saves 0000-9999)
- server: auto save/backup enabling specific to this? or use one setting for both?
- Paste and Insert option (as opposed to Paste and Overwrite, eg. to prevent overcopying on shared canvases)
- add credits to docs (radman, others!)
- Ability to jump-to active cursor position of any user logged in
- Visual indicator in user window to indicate users actively/recently drawing
- Tabbed editing / ability to load multiple pages and switch between them via [Ctrl]+[Tab]
- Drag and drop file loading on window

- Option to resize and reposition chat window 
- Run on OS X 10.5 crashes - can't work due to xamarin launcher?
- Scrolling chat window causes it to jump when people are talking
	- Pause backscroll during active chat
- Tabbed editing / ability to load multiple pages and switch between them via [Ctrl]+[Tab]
- Open files when dragging them onto the pd window (windows, other)
- Server options:
	- Choose which functions/commands can be performed
	- Choose version to support (disables certain functions) - different list of available functions for different protocols?
	- Allow users to edit (+V by default, ops can demote)
	- IRC type functions.. read only, +V to draw +O to op/kick/ban?
	- Register & list pablo servers globally using web service
	- Aesthetic- ban user by nick or IP

	


3.3:
- BUG: BIN saving (or loading?) trims last line



3.2
- Disable "iCE Color off" escape code for blink mode/ or make declaration optional (or just use SAUCE to handle declaration)

 
## Longer Term
- Show an 80x25 (or different size) highlighted area
- Make preview window larger? Resize docked panes?
- Italics/Bold/Underline for AMIGA style
- Icons for file list (TreeView and/or icon support for listbox)
- ANSiMation


# DONE

3.2.1
-----
- Save as PNG on lion crashes
- Windows editing while connected to server
- Navigating archives doesn't work
- Server doesn't stay connected when running on mavericks


3.2
---

- Save backup files (.001.ans) (server + normal) - dialog for config/enabling?
- Press 'e' to erase after selecting a block
- Load ans, save as xbin (multiple times) reverts back to ans format
- Auto insert SAUCE if width <> 80, and enforce dimensions/type in sauce
- Add EOL to text-based files (RIP, ANSI, ASCII) when adding sauce
- Allow width of canvas greater than 512
- Set max width to 1024 or 2048??  performance??
- Crash with ctrl+up on linux - changes directory instead of colour



----------
- Drawing with mouse
 
- RIP Editing

- Networking

- Colour editor

- Preview Pane

- Character Set chooser/editor

- SAUCE editor
	- separate in-memory copy during edit mode
	- direct edit when viewing
	
	
	
	
	
	
Support
-----

	spinsane - first donator, kick ass dude
	bhaal - cool layout and generally cool guy
	sinisterx - infamous build 27
	rad-man - sweet ass ideas, keeping me busy!
	cleanah - for being patient about the .net version on linux!
	And everyone in EFNet IRC that got pissed at me because pablo didn't do something

	
	
	
	

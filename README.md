# xamlsync
Xaml Sync is a tool to synchronises a set of XAML resource directionaries. For example: A resource directionary defining English string but another one in French. the tool creates automatically the missing XAML entry to slave files from master files.

This tool is originally used to synchronize localized languages resource for The Hummingbird Project, and Themes definitions for Hummingbird UI Framework.

##usage
> xamlsync <masterFile.xaml> <slave1.xaml> <slave2.xaml> ...
Read entries from masterFile.xaml and add missing entries to slave1.xaml, slave2.xaml and so on.

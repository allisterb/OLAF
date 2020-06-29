@echo off
set TIKA_CONFIG=tika-config.xml
.\bin\x64\Debug\OLAF.Interfaces.CLI.exe --with-log-file --debug %*
set TIKA_CONFIG=
:end
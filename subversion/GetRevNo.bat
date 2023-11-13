echo off
: ワーキングフォルダ名を取得
setlocal
rem ツールのある実行ディレクトリ
set CurrentDir=%~dp0
cd %CurrentDir%
rem カレントディレクトリの一個上のフォルダ名を取得
cd ..
set x=%CD%
cd %CurrentDir%
rem ファイル名部分を取得する~nで末端ディレクトリを取得する
for /F "delims=" %%a in ('echo "%x%"') do set DIRNAME=%%~na
set /p NOLINEBREAK="hogehoge="< nul
svnversion ..\%DIRNAME%
endlocal

echo off
: ���[�L���O�t�H���_�����擾
setlocal
rem �c�[���̂�����s�f�B���N�g��
set CurrentDir=%~dp0
cd %CurrentDir%
rem �J�����g�f�B���N�g���̈��̃t�H���_�����擾
cd ..
set x=%CD%
cd %CurrentDir%
rem �t�@�C�����������擾����~n�Ŗ��[�f�B���N�g�����擾����
for /F "delims=" %%a in ('echo "%x%"') do set DIRNAME=%%~na
set /p NOLINEBREAK="hogehoge="< nul
svnversion ..\%DIRNAME%
endlocal

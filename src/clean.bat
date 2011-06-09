@echo off
echo Cleaning Solution

for /f %%i in ( 'dir /b /s obj' ) do rmdir /q /s %%i
for /f %%i in ( 'dir /b /s bin' ) do rmdir /q /s %%i

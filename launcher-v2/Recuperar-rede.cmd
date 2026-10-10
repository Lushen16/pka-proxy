@echo off
cd /d "%~dp0"
echo Execute este arquivo como administrador, depois de parar o tunel.
echo A recuperacao remove apenas os filtros persistentes do PKA V2.
PKA-Proxy.exe --guard-release
if errorlevel 1 (echo Falha: confirme que o motor esta parado e que este arquivo foi executado como administrador.) else (echo Bloqueio removido. Rede direta liberada.)
pause

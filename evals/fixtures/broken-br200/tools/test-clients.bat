@echo off
rem Usage: test-clients.bat <session name>
start "" Builds\Client\BR200.exe -batchmode -nographics -client -sessionName %1 -region eu -logFile client1.log
start "" Builds\Client\BR200.exe -batchmode -nographics -client -sessionName %1 -region eu -logFile client2.log

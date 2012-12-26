:NEXT0
FOR %%A IN (1 2 3 4 5) DO (
curl --retry 5 -F file=@Upload\Upload0\Temperature1.txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log
find "failed" upload.log
IF ERRORLEVEL 1 GOTO NEXT1)

:NEXT1
FOR %%A IN (1 2 3 4 5) DO (
curl --retry 5 -F file=@Upload\Upload0\Temperature2.txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log
find "failed" upload.log
IF ERRORLEVEL 1 GOTO NEXT2)

:NEXT2
FOR %%A IN (1 2 3 4 5) DO (
curl --retry 5 -F file=@Upload\Upload0\FlowRate12.txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log
find "failed" upload.log
IF ERRORLEVEL 1 GOTO NEXT3)

:NEXT3
FOR %%A IN (1 2 3 4 5) DO (
curl --retry 5 -F file=@Upload\Upload0\FlowRate11.txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log
find "failed" upload.log
IF ERRORLEVEL 1 GOTO NEXT4)

:NEXT4
FOR %%A IN (1 2 3 4 5) DO (
curl --retry 5 -F file=@Upload\Upload0\Airbox22.txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log
find "failed" upload.log
IF ERRORLEVEL 1 GOTO NEXT5)

:NEXT5
FOR %%A IN (1 2 3 4 5) DO (
curl --retry 5 -F file=@Upload\Upload0\Airbox24.txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log
find "failed" upload.log
IF ERRORLEVEL 1 GOTO NEXT6)

:NEXT6
FOR %%A IN (1 2 3 4 5) DO (
curl --retry 5 -F file=@Upload\Upload0\Airbox23.txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log
find "failed" upload.log
IF ERRORLEVEL 1 GOTO NEXT7)

:NEXT7
FOR %%A IN (1 2 3 4 5) DO (
curl --retry 5 -F file=@Upload\Upload0\Airbox21.txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log
find "failed" upload.log
IF ERRORLEVEL 1 GOTO NEXT8)

:NEXT8
FOR %%A IN (1 2 3 4 5) DO (
curl --retry 5 -F file=@Upload\Upload0\Sht751.txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log
find "failed" upload.log
IF ERRORLEVEL 1 GOTO NEXT9)

:NEXT9
FOR %%A IN (1 2 3 4 5) DO (
curl --retry 5 -F file=@Upload\Upload0\Sht752.txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log
find "failed" upload.log
IF ERRORLEVEL 1 GOTO NEXT10)

:NEXT10
FOR %%A IN (1 2 3 4 5) DO (
curl --retry 5 -F file=@Upload\Upload0\Sht753.txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log
find "failed" upload.log
IF ERRORLEVEL 1 GOTO NEXT11)

:NEXT11
FOR %%A IN (1 2 3 4 5) DO (
curl --retry 5 -F file=@Upload\Upload0\Sht754.txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log
find "failed" upload.log
IF ERRORLEVEL 1 GOTO NEXT12)

:NEXT12
FOR %%A IN (1 2 3 4 5) DO (
curl --retry 5 -F file=@Upload\Upload0\Sht755.txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log
find "failed" upload.log
IF ERRORLEVEL 1 GOTO NEXT13)

:NEXT13
FOR %%A IN (1 2 3 4 5) DO (
curl --retry 5 -F file=@Upload\Upload0\Sht756.txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log
find "failed" upload.log
IF ERRORLEVEL 1 GOTO NEXT14)

:NEXT14
FOR %%A IN (1 2 3 4 5) DO (
curl --retry 5 -F file=@Upload\Upload0\Sht757.txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log
find "failed" upload.log
IF ERRORLEVEL 1 GOTO NEXT15)

:NEXT15
FOR %%A IN (1 2 3 4 5) DO (
curl --retry 5 -F file=@Upload\Upload0\Sht758.txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log
find "failed" upload.log
IF ERRORLEVEL 1 GOTO NEXT16)

:NEXT16
FOR %%A IN (1 2 3 4 5) DO (
curl --retry 5 -F file=@Upload\Upload0\CO2flap34.txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log
find "failed" upload.log
IF ERRORLEVEL 1 GOTO NEXT17)

:NEXT17
FOR %%A IN (1 2 3 4 5) DO (
curl --retry 5 -F file=@Upload\Upload0\CO2flap33.txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log
find "failed" upload.log
IF ERRORLEVEL 1 GOTO NEXT18)

:NEXT18
FOR %%A IN (1 2 3 4 5) DO (
curl --retry 5 -F file=@Upload\Upload0\CO2flap31.txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log
find "failed" upload.log
IF ERRORLEVEL 1 GOTO NEXT19)

:NEXT19
FOR %%A IN (1 2 3 4 5) DO (
curl --retry 5 -F file=@Upload\Upload0\CO2flap32.txt http://cps.isc.ntu.edu.sg/smartcontainer/uploadfile/upload.php > upload.log
find "failed" upload.log
IF ERRORLEVEL 1 GOTO NEXT20)

:NEXT20
DEL /F /Q upload.log
set AMDPROFILERPATH=C:\Program Files (x86)\AMD\AMD APP Profiler\x86
set AMDPROFILEREXECUTABLE=sprofile.exe
set AMDPROFILER=%AMDPROFILERPATH%\%AMDPROFILEREXECUTABLE%

set PROFILEDAPPPATH=D:\Projects\Csharp\MiNI_AprioriAll\bin\x86
set PROFILEDAPPEXECUTABLE=AprioriAllLibBenchmark.exe
set PROFILEDAPP=%PROFILEDAPPPATH%\%PROFILEDAPPEXECUTABLE%

call echo %AMDAPPSDKROOT%

set PARAMS=apriori openCL repeats=1 input=Example2 support=0.1

REM call "%AMDPROFILER%" -o "%PROFILEDAPPPATH%\profiling_counters.csv" -w "%PROFILEDAPPPATH%" "%PROFILEDAPP%" "%PARAMS%"
call "%AMDPROFILER%" -o "%PROFILEDAPPPATH%\profiling_trace.atp" -t -T -w "%PROFILEDAPPPATH%" "%PROFILEDAPP%" "%PARAMS%"

call pause
exit /B 0

set AMDPROFILERPATH=C:\Program Files (x86)\AMD\AMD APP Profiler\x86

set AMDPROFILEREXECUTABLE=sprofile.exe

set AMDPROFILER=%AMDPROFILERPATH%\%AMDPROFILEREXECUTABLE%

set PROFILEDAPPPATH=D:\Projects\Csharp\MiNI_AprioriAll\bin\Release\Benchmark\x86

set PROFILEDAPPEXECUTABLE=AprioriAllLibBenchmark.exe

set PROFILEDAPP=%PROFILEDAPPPATH%\%PROFILEDAPPEXECUTABLE%

call echo %AMDAPPSDKROOT%

set input1=testsCount=1 openCL support=0.3 custCountMin=1 custCountMax=5 transactCount=3 itemCount=3 uniqueIds=20

set input2=testsCount=1 openCL support=0.1 input=Example2

call "%AMDPROFILER%" -o "%PROFILEDAPPPATH%\profiling_counters.csv" -w "%PROFILEDAPPPATH%" "%PROFILEDAPP%" 

call "%AMDPROFILER%" -o "%PROFILEDAPPPATH%\profiling_trace.atp" -t -T -w "%PROFILEDAPPPATH%" "%PROFILEDAPP%" "%input2%"

call pause

exit /B 0

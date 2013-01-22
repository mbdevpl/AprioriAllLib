REM call AprioriAllLibBenchmark testsCount=50 newEachTime openCL support=0.5 custCount=1 transactCount=1 itemCount=1 uniqueIds=1

REM call AprioriAllLibBenchmark testsCount=50 newEachTime support=0.5 custCount=1 transactCount=1 itemCount=1 uniqueIds=1

REM setlocal enableDelayedExpansion

REM set testConditions1=testsCount=4 support=0.4 transactCount=3 itemCount=5 uniqueIds=20

REM for /l %%x in (1, 1, 15) do (
	REM call AprioriAllLibBenchmark %testConditions1% custCount=%%x
REM )

REM for /l %%x in (1, 1, 15) do (
	REM call AprioriAllLibBenchmark %testConditions1% openCL custCount=%%x 
REM )

set testConditions2=testsCount=4 openCL serialized support=0.5 custCountMin=1 custCountMax=10 transactCount=5 itemCount=5 uniqueIds=14
call AprioriAllLibBenchmark %testConditions2% 

call pause

exit /B 0

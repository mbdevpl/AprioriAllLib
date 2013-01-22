REM call AprioriAllLibBenchmark testsCount=50 newEachTime openCL support=0.5 custCount=1 transactCount=1 itemCount=1 uniqueIds=1

REM call AprioriAllLibBenchmark testsCount=50 newEachTime support=0.5 custCount=1 transactCount=1 itemCount=1 uniqueIds=1

REM set testConditions1=testsCount=4 support=0.4 transactCount=3 itemCount=5 uniqueIds=20

set testConditions2=testsCount=3 openCL warmUp support=0.5 custCountMin=1 custCountMax=30 transactCount=5 itemCount=5 uniqueIds=20

call AprioriAllLibBenchmark %testConditions2% 

call pause

exit /B 0

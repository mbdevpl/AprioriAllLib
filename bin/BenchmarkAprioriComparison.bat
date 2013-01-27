call AprioriAllLibBenchmark apriori serialized openCL newEachTime repeats=5 input=Example6 support=0.6 customers=2;100;2 printProgress saveLatex

call pause
exit /B 0

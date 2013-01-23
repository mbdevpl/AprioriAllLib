Apriori and AprioriAll in C# {#mainpage}
============================

Authors: Karolina Baltyn and Mateusz Bysiek
of Faculty of Mathematics and Information Sciences
at Warsaw University of Technology

## License

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <http://www.gnu.org/licenses/>.

## How to use the application

Launch <code>/bin/Release/AprioriAll.bat</code> 
or <code>/bin/Release/Apriori.bat</code>
to see an example.

Launch <code>/bin/Release/AprioriAll.exe</code> 
or <code>/bin/Release/Apriori.exe</code>
with correct command line arguments to execute AprioriAll algorithm 
for an arbitrary input data. When the program is run without any options,
it displays information about available options.

### Examples

<code>AprioriAll dataset1.xml 0.2</code> runs AprioriAll algorithm with 
support 0.2 on given data set

<code>Apriori dataset2.xml 0.3</code> runs Apriori algorithm with support 
0.3 on given data set

Apriori and AprioriAll executables do not have any extra options at the moment.

The executables do not (yet) allow execution of parallel (OpenCL) version of the 
algorithm. It is still accessible via AprioriAllLib.dll and the benchmarking 
executable. It was tested on AMD GPU, but it is not efficient enough to be useful 
in practice. Current development focuses primarily on the parallel version, with 
only minor tweaks (if any) in the serialized.

<code>AprioriAllLibBenchmark testsCount=1 openCL support=0.1 input=Example2</code>

<code>AprioriAllLibBenchmark testsCount=4 openCL serialized support=0.5 custCountMin=1 custCountMax=10 transactCount=3 itemCount=3 uniqueIds=7</code>

<code>AprioriAllLibBenchmark testsCount=3 openCL warmUp support=0.5 custCountMin=1 custCountMax=30 transactCount=5 itemCount=5 uniqueIds=20</code>

Benchmark command-line options are currently not documented as they frequently 
change.

## Timeline

* 6 Nov 2012: project started
* 7 Dec 2012: version 1.0.0
* 15 Dec 2012: version 1.5.0
* 21 Jan 2013: version 2.0.0
* 23 Jan 2013: version 2.1.0

## Where is everything, i.e. structure of the repository

* */bin/Release* - binaries of applications and the library, compiled for .NET 4.0
* */bin/Release/Profiling/x86* - binaries of benchmark of the library, compiled for .NET 4.0 x86 with batch file for AMD APP profiler
* */doc/html* - documentation of source code
* */doc/pdf* - general documentation describing main principles of the algorithm and this application
* */src* - source code
* */src/Apriori* - console application that runs Apriori algorithm on user-defined input
* */src/AprioriAll* - console application that runs AprioriAll algorithm on user-defined input
* */src/AprioriAllLib* - library that implements the algorithms
* */src/AprioriAllLibTest* - unit tests, which check correctness of the implementation
* */src/ConsoleTest(...)* - console applications that run the specific algorithm only on specific test data

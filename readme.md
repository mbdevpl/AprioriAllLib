Apriori All algorithm implementation {#mainpage}
====================================

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

Launch <code>/bin/Release/AprioriAll.bat</code> to see an example.

Launch <code>/bin/Release/AprioriAll.exe</code> with correct command line arguments 
to execute AprioriAll algorithm for an arbitrary input data. When the program is run without any options,
it displays information about available options.

## Where is everything, i.e. structure of the repository

* */bin/Release* - binaries compiled for .NET 4.0
* */doc/pdf* - general documentation describing main principles of the algorithm and this application
* */doc/html* - documentation of source code
* */src* - source code
* */src/AprioriAllLib* - library that implements the algorithm
* */src/AprioriAllTest* - unit tests, which check correctness of the implementation
* */src/AprioriAllConsole* - console application which accepts user-defined input
* */src/AprioriAllConsoleTest* - a test console application that generates random input data and runs the algorithm on this data

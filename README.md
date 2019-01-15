# NR2K3Results_MVVM

##About

A simple WPF MVVM application that takes exported NR2003 results and outputs them to PDFs that look like the official results NASCAR uses.

NR2003 is a NASCAR game from 2003. It is highly open ended and is still "modded" to this day. One thing you can do is output the results of a race to an HTML file. However, the template for the HTML file was written in 2003 and it is atrociously terrible ot look at. 

However, this program is able to read the driver names from the .HTML file and fetch much more data about each driver from the game files. Once it does this, it can then output to a PDF, which looks just like the official results that NASCAR releases each weekend. This program was intended for use by online racing leagues, who love to make their leagues more realistic.

The user must input some initial data, but this data is then stored in a persistent SQLLite database on their own machine. After that, all they must provide each time they'd like to output a PDF is a result file.

You should be able to run the program by compiling it in Visual Studio 2017. However, it requires the game to be installed, so it may not be worth trying to run.

##Examples
Before and after examples can be found in the /Examples folder.
  - input_html.html is the raw, unedited HTML that was fed to the program.
  - input.png is a screenshot of the relevant data from the input HTML file that was used to generate the output PDF.
  - output.pdf is the end result of the program. 
  
##How could I expand this?



I apologize in advance for the lack of documentation on some of the code, I am still working on that.

# NR2K3 Results

## About

A simple WPF MVVM application that takes exported NR2003 results and outputs them to PDFs that look like the official results NASCAR uses.

NR2003 is a NASCAR game from 2003. It is highly open ended and is still "modded" to this day. One thing you can do is output the results of a race to an HTML file. However, the template for the HTML file was written in 2003 and it is atrociously terrible to look at. 

## How does it work?
This program is able to read the driver names from the .HTML file and fetch much more data about each driver from the game files. Once it does this, it can then output to a PDF, which looks just like the official results that NASCAR releases each weekend. This program was intended for use by online racing leagues, who love to make their leagues more realistic.

The user must input some initial data, but this data is then stored in a persistent SQLLite database on their own machine. After that, all they must provide each time they'd like to output a PDF is the file containing the race results.

You should be able to run the program by compiling it in Visual Studio 2017. However, it requires the game to be installed, so it may not be worth trying to run.

## Examples and Usage
Before and after examples can be found in the /Examples folder.
  - input_html.html is the raw, unedited HTML that was fed to the program.
  - input.png is a screenshot of the relevant data from the input HTML file that was used to generate the output PDF.
  - output.pdf is the end result of the program. 
  
## How could I expand this?
I have been busy with school lately and have not been able to work too much on this, but I have been thinking of how I could expand this.

However, I have been thinking about expanding the scope of this project (or roll the work into a new project) to be a full season manager.

### What does that mean?
Simply put, the program would track the progess of a racing season in the game. It would track the points standings after each race and provide statistics about the seasons such as most wins, most laps led, most laps completed, etc. The possibilities are endless, and I am still thinking about how far I would want to take this project.

## Known issues
I try to open up issues on github as I find them.

- This is not pure MVVM. Many error dialogs are contained in my view-models. Simply put, placing them in my view-moels. Future iterations of this project may move them to be fully MVVM.
- I apologize in advance for the lack of documentation on some of the code, I am still working on that.

## Credits
This software makes use of the [iTextSharp](https://github.com/itext/itextsharp) library, [MVVMLight](https://github.com/lbugnion/mvvmlight), and [HTML Agility Pack](https://github.com/zzzprojects/html-agility-pack).

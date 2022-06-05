<div id="top"></div>

<br />
<div align="center">

<h3 align="center">OFXNet</h3>
  <p align="center">
    OFXNet is a OFX parser and interpreter, written in C#. This has been ported from the orignal repo (<a href="https://github.com/jhollingworth/OFXSharp">OFXSharp</a>) and updated to the latest .Net version.
  </p>
</div>

<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>

<!-- ABOUT THE PROJECT -->
## About The Project

The project is a parser and interpreter for OFX files. OFX files can be ready by the library and converted to appropriate POCO objects for use by the consuming software.
<p align="right">(<a href="#top">back to top</a>)</p>

### Built With

* [.Net](https://dotnet.microsoft.com/en-us/)

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- USAGE EXAMPLES -->
## Usage
For basic usage the below sample should be enough to get you started.
```
OFXDocument ofxDocument = OFXDocumentParser.Import(new FileStream(@"PathToDocument.ofx", FileMode.Open));
```

Once the document has been readin to the POCO objects you can interact with the file as required.
```
ICollection<Transaction> transactions = ofXDocument.Transactions;
foreach (Transaction transaction in transactions)
  DoLogic(transaction);
```

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- LICENSE -->
## License

Distributed under the MIT License. See `LICENSE.txt` for more information.

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- ACKNOWLEDGMENTS -->
## Acknowledgments

I would like to acknowledge the original project from which this is forked, all previous work is credited to <a href="https://github.com/jhollingworth">jhollingworth</a>. 

<p align="right">(<a href="#top">back to top</a>)</p>
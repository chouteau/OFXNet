![Build and Test](https://github.com/LiamPietralla/OFXNet/actions/workflows/dotnet.yml/badge.svg) ![Nuget](https://img.shields.io/nuget/v/OFXNet.Parser)

# OFXNet

OFXNet is a OFX parser and interpreter, written in C#. This has been ported from the orignal repo [https://github.com/jhollingworth/OFXSharp"](OFXSharp) and updated to the latest .Net version.

## About The Project

The project is a parser and interpreter for OFX files. OFX files can be ready by the library and converted to appropriate POCO objects for use by the consuming software.

### Built With

* [.Net](https://dotnet.microsoft.com/en-us/)

## Usage
For basic usage the below sample should be enough to get you started.
```
OFXDocument ofxDocument = OFXDocumentParser.Import(new FileStream(@"PathToDocument.ofx", FileMode.Open));
```

Once the document has been read in to the POCO objects you can interact with the file as required.
```
ICollection<Transaction> transactions = ofXDocument.Transactions;
foreach (Transaction transaction in transactions)
  DoLogic(transaction);
```

## License

Distributed under the MIT License. See `LICENSE.txt` for more information.

## Acknowledgments

I would like to acknowledge the original project from which this is forked, all previous work is credited to [https://github.com/jhollingworth](jhollingworth). 
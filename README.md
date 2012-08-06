# Overview
C\# Mass Spectrometry Library (CSMSL) is a .NET-based framework for working with proteomic data. There are many tools available for MS analysis on the internet, but most are geared to end users and are challenging to adapt to a specific need. CSMSL looks to provide an easy-to-use, powerful, feature-rich library of .NET C\# objects and methods to enable even novice programmers the ability to analyze proteomic data quickly. Simplicity is key, calculating the mass of the peptide sequence "CSMSL" only requires the following two lines:
```csharp
Peptide peptide = new Peptide("CSMSL");
Console.WriteLine(peptide.Mass.Monoisotopic);
//produces : 539.20835516707
```
In addition to simply syntax, CSMSL is designed with performance in mind, allowing even computationally intensive calculations to be completed quickly. For [example](https://github.com/dbaileychess/CSMSL/blob/master/Examples/ExampleTrypticDigest.cs), a complete yeast database (6,627 proteins) can be loaded from a .fasta file, digested with trypsin (up to 3 missed cleavages) in under 3 seconds. If we include the calculation for the +1 m/z of each of the 913,740 resulting peptides, the total time only goes up to 9 seconds (this includes full chemical formula determination). While CSMSL is not expected to meet the performance of advanced compiled languages (e.g. C/C++, Fortran, etc...), its adequate performance plus simplicity of use are sure to be helpful in analyzing data in new and creative ways without significant overhead.

Future goals include providing native support for reading and writing common m/z formats (mzXML, .sqlite, etc...) as well as cross-vendor support.

CSMSL is seeking contributors to improve all aspects of this library. Whether you are a programmer or scientist, everyone can have helpful insights in expanding the scope of this library.

## Examples
Functional coding examples are a great way to dive into any programming language/library. We provide a number of example programs using CSMSL (under [Examples\\ExamplesCSMSL.csproj](https://github.com/dbaileychess/CSMSL/tree/master/Examples)) so that people can learn the tools and experiment with different features. Below are a subset of short code examples to get a flavor of options available.

### Chemical Formulas
---------------------
Chemical formulas are the foundation ofmany other classes in CSMSL (e.g. Peptide, Fragments, Proteins, Modifications, etc..) and are the main way that masses are calculated for those objects. Thorough understanding of chemical formulas and how they are used is important in using CSMSL effectively. 

Creation of a chemical formula is simple as well as accessing different properties of it, such as mass:
```csharp
ChemicalFormula formula1 = new ChemicalFormula("C2H3NO");
Console.WriteLine("Formula {0} mass is {1}", formula1, formula1.Mass.Monoisotopic);
// produces: "Formula C2H3NO mass is 57.02146372057"
```
We can modify the chemical formula by adding another chemical formula to it:
```csharp
ChemicalFormula formula2 = new ChemicalFormula("C2");
formula1.Add(formula2);
Console.WriteLine("Formula {0} mass is {1}", formula1, formula1.Mass.Monoisotopic);
// produces: "Formula C4H3NO mass is 81.02146372057"
```
Chemical formulas even support negative values even if it doesn't make physical sense:
```csharp
ChemicalFormula formula3 = new ChemicalFormula("C10");
formula1.Remove(formula3);
Console.WriteLine("Formula {0} mass is {1}", formula1, formula1.Mass.Monoisotopic);
// produces: "Formula C-6H3NO mass is -38.97853627943"
```
Data wise, formulas are a set of isotopes, allowing for the possibility of making them with isotopes that are not the most common (i.e. Carbon 13) using the following notation: \<Element Symbol\>{\<Mass Number\>}
```csharp
ChemicalFormula formula1 = new ChemicalFormula("C2H3NO");
ChemicalFormula formula2 = new ChemicalFormula("C{13}CH3NO");
Console.WriteLine("Formula {0} mass is {1}", formula1, formula1.Mass.Monoisotopic);
Console.WriteLine("Formula {0} mass is {1}", formula2, formula2.Mass.Monoisotopic);
// produces: "Formula C2H3NO mass is 57.02146372057"
//           "Formula C{13}CH3NO mass is 58.02481855837"
```
**Note:** `C{12} == C` since carbon 12 is the most abundant isotope of carbon.

Basic arithmetic (+, -, *) is also possible:
```csharp
ChemicalFormula formula3 = formula2 - formula1;
Console.WriteLine("Formula {0} mass is {1}", formula3, formula3.Mass.Monoisotopic);
// produces: "Formula C{13}C-1 mass is 1.0033548378"
ChemicalFormula formula4 = formula3 * 4;
Console.WriteLine("Formula {0} mass is {1}", formula4, formula4.Mass.Monoisotopic);
// produces: "Formula C{13}4C-4 mass is 4.0134193512"
```

### Peptides and Proteins
---------------------
Proteomics involves a lot of proteins and peptides and CSMSL offers extensive support in providing these constructs in a straightforward manner. 

Creating a peptide object is as easy as making a chemical formula:
```csharp
Peptide peptide1 = new Peptide("ACDEFGHIKLMNPQRSTVWY");
Console.WriteLine("Peptide {0} mass is {1}", peptide1. peptide1.Mass.Monoisotopic);
// produces: "Peptide ACDEFGHIKLMNPQRSTVWY mass is 2394.12490682513)"
```
Since a peptide is just a big chemical formula, the peptide also has a chemical formula (where it gets it mass from):
```csharp
Console.WriteLine("Peptide {0} formula is {1}", peptide1. peptide1.ChemicalFormula);
// produces: "Peptide ACDEFGHIKLMNPQRSTVWY formula is H159C107N29O30S2"
```
Peptides and proteins can be modified post translationally, CSMSL offers support for easy modification of residues and termini
```csharp
peptide1.SetModification(new ChemicalModification("Fe"), Terminus.C | Terminus.N);
Console.WriteLine("Peptide {0} formula is {1}", peptide1. peptide1.ChemicalFormula);
// produces: "Peptide [Fe]-ACDEFGHIKLMNPQRSTVWY-[Fe] formula is Fe2H157C107N29O29S2"
ChemicalModification oxMod = new ChemicalModification("O", "Oxidation");
peptide1.SetModification(oxMod, 'M');
// produces: "Peptide [Fe]-ACDEFGHIKLM[Oxidation]NPQRSTVWY-[Fe] formula is Fe2H159C107N29O30S2"
```


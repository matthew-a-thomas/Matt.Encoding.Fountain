# Matt.Encoding.Fountain
C# implementation of [Fountain Codes](https://en.wikipedia.org/wiki/Fountain_code)

![Build status](https://switchigan.visualstudio.com/_apis/public/build/definitions/9e65584e-ff3f-4616-b1ab-5227abae1502/10/badge "Build status")

## NuGet

[```Matt.Encoding.Fountain```](https://www.nuget.org/packages/Matt.Encoding.Fountain/)

## Overview

A [fountain code](https://en.wikipedia.org/wiki/Fountain_code) is a rateless forward error correction code. That means:
 * Data is split up into slices.
 * Only a subset of those slices are needed to get the data back.

## Practical applications

### Increase backup resiliency
 * Create 10 slices of your data, each holding 1/5 the size of your data.
 * Let any 4 of those be lost.
 * You'll probably still be able to get your data back!

### Increase users' download speeds
 * Make several of your servers serve slices from the same file.
   * No coordination between servers is required.
   * Just make sure you are not generating slices "systematically"--more on that below.
 * Let users download from all your servers at once to get slices extra fast.
 * After a user has collected enough slices, they'll be able to put them together and get your file!

## Examples

### Encoding slices

```csharp
// Start with some data
var data = new byte[]
{
  (byte)'C',
  (byte)'u',
  (byte)'t',
  (byte)'e',
  (byte)' ',
  (byte)'k',
  (byte)'i',
  (byte)'t',
  (byte)'t',
  (byte)'e',
  (byte)'n',
  (byte)'s',
};

// Slice up the data
var slices =
  SliceHelpers
    .CreateGenerator(
      data: data,
      sliceSize: 4,
      rngFactoryDelegate: () => new RandomAdapter(new Random()) // RandomAdapter is from https://www.nuget.org/packages/Matt.Random/
    )
    .Take(105) // How many slices do you want?
    .ToList();

// Put the slices somewhere
foreach (var slice in slices)
  await WriteToDiskAsync(slice);
```

### Decoding slices

```csharp
// Grab some slices
var random = new Random();
var slices =
  await ReadSlicesFromDiskAsync()
    .Where(_ => random.Next() % 2 == 0) // Oh no! We lost half the slices!
    .ToList();

// Set up a solver and let it know how large the original data is
var solver = new SliceSolver(data.Length);

// Tell the solver about the slices you've got
foreach (var slice in slices)
  await solver.RememberAsync(slice);

// Try solving for the original data
var solution = await solver.TrySolveAsync();
if (solution == null)
  throw new Exception("We need to gather more slices");

// Do something with the original data
await DoSomethingWithSolutionAsync(solution);
```

## Technicalities

### This isn't magic

All that's happening behind the scenes is that random subsets of the original data are being XOR'd together for each slice. This is basically how all fountain codes work. The [Luby Transform](https://en.wikipedia.org/wiki/Luby_transform_code) fountain code just draws its randomness from a distribution with engineered properties. [Raptor Codes](https://en.wikipedia.org/wiki/Raptor_code) just pre-process the original data so that it's more likely that decoding will work sooner with fewer slices.

### Decoding is not deterministic

The rate of success of decoding is probabilistic. If you use a slice size of 1/_k_ (so that there are _k_ slices), you'll _probably_ be able to decode your original data after collecting _n_ slices according to this equation:

[Probability of decoding n slices](https://raw.githubusercontent.com/matthew-a-thomas/Matt.Encoding.Fountain/master/Probability%20of%20decoding%20n%20slices.gif "Probability of decoding n slices")
